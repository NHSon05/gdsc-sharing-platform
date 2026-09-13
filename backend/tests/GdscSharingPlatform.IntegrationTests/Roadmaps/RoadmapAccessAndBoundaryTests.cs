using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GdscSharingPlatform.IntegrationTests.Roadmaps;

public sealed partial class RoadmapEndpointsTests
{
    [Fact]
    public async Task DownloadAccessChecksEveryParentStateAndMissingFile()
    {
        var roadmap = await CreateRoadmapAsync(); var node = await CreateNodeAsync(roadmap.Id, "download");
        using var form = FileForm();
        var resource = await ReadAsync<ResourceResponse>(await _admin.PostAsync($"/api/admin/roadmap-nodes/{node.Id}/resources/files", form), HttpStatusCode.Created);
        using var anonymous = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync(resource.DownloadUrl)).StatusCode);
        foreach (var status in Enum.GetValues<RoadmapStatus>())
        {
            await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{roadmap.Id}/nodes/{node.Id}/status", new { isActive = true });
            Assert.Equal(HttpStatusCode.NoContent, (await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{roadmap.Id}/status", new { status })).StatusCode);
            foreach (var nodeActive in new[] { true, false })
            foreach (var resourceActive in new[] { true, false })
            {
                await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{roadmap.Id}/nodes/{node.Id}/status", new { isActive = nodeActive });
                await _admin.PatchAsJsonAsync($"/api/admin/roadmap-resources/{resource.Id}/status", new { isActive = resourceActive });
                var expected = status != RoadmapStatus.Draft && nodeActive && resourceActive ? HttpStatusCode.OK : HttpStatusCode.NotFound;
                Assert.Equal(expected, (await _member.GetAsync(resource.DownloadUrl)).StatusCode);
                Assert.Equal(HttpStatusCode.OK, (await _admin.GetAsync(resource.DownloadUrl)).StatusCode);
            }
        }
        File.Delete(Assert.Single(Directory.GetFiles(_root)));
        var error = await ReadAsync<JsonElement>(await _admin.GetAsync(resource.DownloadUrl), HttpStatusCode.NotFound);
        Assert.True(error.TryGetProperty("traceId", out _));
        Assert.DoesNotContain(_root, error.ToString());
    }

    [Fact]
    public async Task FileDownloadSupportsByteRangesWithoutExposingStorageMetadata()
    {
        var roadmap = await CreateRoadmapAsync(); var node = await CreateNodeAsync(roadmap.Id, "range");
        using var form = FileForm();
        var response = await _admin.PostAsync($"/api/admin/roadmap-nodes/{node.Id}/resources/files", form);
        var json = await ReadAsync<JsonElement>(response, HttpStatusCode.Created);
        Assert.False(json.TryGetProperty("storageKey", out _)); Assert.False(json.TryGetProperty("storedFileName", out _));
        var url = json.GetProperty("downloadUrl").GetString();
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Range = new RangeHeaderValue(0, 4);
        var download = await _member.SendAsync(request);
        Assert.Equal(HttpStatusCode.PartialContent, download.StatusCode);
        Assert.Equal("%PDF-", await download.Content.ReadAsStringAsync());
        Assert.Equal(0, download.Content.Headers.ContentRange!.From);
        Assert.True(download.Headers.CacheControl!.NoStore);
        Assert.True(download.Headers.CacheControl.Private);
    }

    [Fact]
    public async Task MemberPaginationIncludesArchivedButCannotRevealDraftUsingFilters()
    {
        var first = await CreateRoadmapAsync("first");
        var archived = await CreateRoadmapAsync("archived");
        var draft = await CreateRoadmapAsync("draft");
        await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{archived.Id}/status", new { status = "Archived" });
        await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{draft.Id}/status", new { status = "Draft" });
        await _admin.PatchAsJsonAsync("/api/admin/roadmaps/reorder", new ReorderRequest([archived.Id, draft.Id, first.Id]));
        var page1 = await ReadAsync<PageResponse<RoadmapSummary>>(await _member.GetAsync("/api/roadmaps?pageSize=1"));
        var page2 = await ReadAsync<PageResponse<RoadmapSummary>>(await _member.GetAsync("/api/roadmaps?page=2&pageSize=1"));
        Assert.Equal(2, page1.TotalItems); Assert.Equal(2, page1.TotalPages);
        Assert.Equal(archived.Id, Assert.Single(page1.Items).Id); Assert.Equal(first.Id, Assert.Single(page2.Items).Id);
        var hidden = await ReadAsync<PageResponse<RoadmapSummary>>(await _member.GetAsync("/api/roadmaps?status=Draft"));
        Assert.Empty(hidden.Items); Assert.Equal(0, hidden.TotalItems);
        Assert.Equal(HttpStatusCode.NotFound, (await _member.GetAsync("/api/roadmaps/draft")).StatusCode);
        var beyond = await ReadAsync<PageResponse<RoadmapSummary>>(await _member.GetAsync("/api/roadmaps?page=3&pageSize=1"));
        Assert.Empty(beyond.Items); Assert.Equal(2, beyond.TotalItems);
        var noMatch = await ReadAsync<PageResponse<RoadmapSummary>>(await _member.GetAsync("/api/roadmaps?search=nonexistent"));
        Assert.Empty(noMatch.Items);
    }

    [Fact]
    public async Task AdminPaginationIncludesEveryStatusAndCanFilterDrafts()
    {
        var published = await CreateRoadmapAsync("admin-published");
        var archived = await CreateRoadmapAsync("admin-archived");
        var draft = await CreateRoadmapAsync("admin-draft");
        await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{archived.Id}/status", new { status = "Archived" });
        await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{draft.Id}/status", new { status = "Draft" });
        await _admin.PatchAsJsonAsync("/api/admin/roadmaps/reorder", new ReorderRequest([draft.Id, archived.Id, published.Id]));
        var page = await ReadAsync<PageResponse<RoadmapSummary>>(await _admin.GetAsync("/api/admin/roadmaps?pageSize=2"));
        Assert.Equal(3, page.TotalItems); Assert.Equal(2, page.TotalPages);
        Assert.Collection(page.Items,
            item => { Assert.Equal(draft.Id, item.Id); Assert.Equal(RoadmapStatus.Draft, item.Status); },
            item => { Assert.Equal(archived.Id, item.Id); Assert.Equal(RoadmapStatus.Archived, item.Status); });
        var drafts = await ReadAsync<PageResponse<RoadmapSummary>>(await _admin.GetAsync("/api/admin/roadmaps?status=Draft"));
        Assert.Equal(draft.Id, Assert.Single(drafts.Items).Id);
        Assert.Equal(RoadmapStatus.Draft, (await ReadAsync<RoadmapResponse>(await _admin.GetAsync($"/api/admin/roadmaps/{draft.Id}"))).Status);
    }

    [Fact]
    public async Task InactiveOrUnrecognizedRolesCannotReadOrManageRoadmaps()
    {
        foreach (var role in new[] { RoleNames.Admin, RoleNames.Member })
        {
            using var banned = await ClientAsync(role, UserStatus.Banned);
            Assert.Equal(HttpStatusCode.Forbidden, (await banned.GetAsync("/api/roadmaps")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await banned.GetAsync("/api/admin/roadmaps")).StatusCode);
        }
        using var unknown = await ClientAsync("UnrecognizedRole");
        Assert.Equal(HttpStatusCode.Forbidden, (await unknown.GetAsync("/api/roadmaps")).StatusCode);
        using var anonymous = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/admin/roadmaps")).StatusCode);
    }

    [Fact]
    public async Task FailedResourceReorderAndWrongParentRoutesLeaveDataUnchanged()
    {
        var first = await CreateRoadmapAsync("first"); var other = await CreateRoadmapAsync("other");
        var a = await CreateNodeAsync(first.Id, "a"); var foreign = await CreateNodeAsync(other.Id, "foreign");
        var resourcePath = $"/api/admin/roadmap-nodes/{a.Id}/resources";
        var resource = await ReadAsync<ResourceResponse>(await _admin.PostAsJsonAsync(resourcePath + "/links", new LinkResourceRequest("Guide", "https://example.com")), HttpStatusCode.Created);
        var otherResource = await ReadAsync<ResourceResponse>(await _admin.PostAsJsonAsync($"/api/admin/roadmap-nodes/{foreign.Id}/resources/links", new LinkResourceRequest("Other", "https://example.com")), HttpStatusCode.Created);
        Assert.Equal(HttpStatusCode.BadRequest, (await _admin.PatchAsJsonAsync(resourcePath + "/reorder", new ReorderRequest([resource.Id, otherResource.Id]))).StatusCode);
        Assert.Equal(0, Assert.Single(await ReadAsync<ResourceResponse[]>(await _admin.GetAsync(resourcePath))).SortOrder);
        Assert.Equal(HttpStatusCode.NotFound, (await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{first.Id}/nodes/{foreign.Id}", new NodeRequest("Wrong", "wrong"))).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _member.GetAsync($"/api/roadmaps/{first.Id}/nodes/{foreign.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await _admin.PatchAsJsonAsync($"/api/admin/roadmap-resources/{resource.Id}", new UpdateResourceRequest("No URL"))).StatusCode);
        using var replacement = FileForm(title: false);
        Assert.Equal(HttpStatusCode.Conflict, (await _admin.PostAsync($"/api/admin/roadmap-resources/{resource.Id}/replace-file", replacement)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _member.GetAsync($"/api/roadmap-resources/{resource.Id}/download")).StatusCode);
    }

    [Fact]
    public async Task CategoryUpdatesPreserveRoadmapsAndInactiveCategoryCannotReceiveNewRoadmaps()
    {
        var created = await ReadAsync<CategoryResponse>(await _admin.PostAsJsonAsync("/api/admin/roadmap-categories", new CategoryRequest("Custom", "custom")), HttpStatusCode.Created);
        var updated = await ReadAsync<CategoryResponse>(await _admin.PatchAsJsonAsync($"/api/admin/roadmap-categories/{created.Id}", new CategoryRequest("Edited", "edited", "Description", SortOrder: 7)));
        Assert.Equal(created.Id, updated.Id); Assert.Equal("edited", updated.Slug);
        Assert.Equal(HttpStatusCode.Conflict, (await _admin.PostAsJsonAsync("/api/admin/roadmap-categories", new CategoryRequest("EDITED", "other-slug"))).StatusCode);
        await _admin.PatchAsJsonAsync($"/api/admin/roadmap-categories/{created.Id}/status", new { isActive = false });
        var categories = await ReadAsync<CategoryResponse[]>(await _member.GetAsync("/api/roadmap-categories"));
        Assert.DoesNotContain(categories, x => x.Id == created.Id);
        await _admin.PatchAsJsonAsync($"/api/admin/roadmap-categories/{created.Id}/status", new { isActive = true });
        Assert.Contains(await ReadAsync<CategoryResponse[]>(await _member.GetAsync("/api/roadmap-categories")), x => x.Id == created.Id);
    }
}
