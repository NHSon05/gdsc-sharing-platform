using GdscSharingPlatform.IntegrationTests.Support;
using Xunit.Abstractions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GdscSharingPlatform.IntegrationTests.Roadmaps;

public sealed partial class RoadmapEndpointsTests : IAsyncLifetime
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"roadmap_api_{Guid.NewGuid():N}");
    private readonly WebApplicationFactory<Program> _factory;
    private readonly PostgresTestDatabase? _postgres = PostgresTestDatabase.FromEnvironment();
    private HttpClient _admin = null!;
    private HttpClient _member = null!;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public RoadmapEndpointsTests(ITestOutputHelper output)
    {
        var dbName = Guid.NewGuid().ToString();
        output.WriteLine(_postgres is null ? "Roadmap API provider: InMemory" : "Roadmap API provider: PostgreSQL");
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SeedAdmin:Enabled"] = "false", ["SeedMember:Enabled"] = "false",
                ["RoadmapStorage:RootPath"] = _root, ["RoadmapStorage:MaxFileBytes"] = "1024"
            }));
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    if (_postgres is null) options.UseInMemoryDatabase(dbName);
                    else options.UseNpgsql(_postgres.ConnectionString);
                });
            });
        });
    }
    public async Task InitializeAsync()
    {
        if (_postgres is not null) await _postgres.CreateAsync();
        _admin = await ClientAsync(RoleNames.Admin);
        _member = await ClientAsync(RoleNames.Member);
    }
    public async Task DisposeAsync()
    {
        try
        {
            _admin?.Dispose(); _member?.Dispose();
            await _factory.DisposeAsync();
        }
        finally
        {
            if (_postgres is not null) await _postgres.DropAsync();
            if (Directory.Exists(_root)) Directory.Delete(_root, true);
        }
    }
    private async Task<HttpClient> ClientAsync(string role, UserStatus status = UserStatus.Active)
    {
        var client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = new ApplicationUser { Id = Guid.NewGuid(), FullName = "Test author", Email = $"{Guid.NewGuid():N}@test.local" };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var jwt = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>().GenerateAccessToken(user.Id,
            user.Email, user.FullName, [role], null, status.ToString(), 1);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt.Token);
        return client;
    }
    private static async Task<T> ReadAsync<T>(HttpResponseMessage response, HttpStatusCode status = HttpStatusCode.OK)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == status, $"Expected {status}, got {response.StatusCode}: {body}");
        return JsonSerializer.Deserialize<T>(body, Json)!;
    }
    private async Task<RoadmapResponse> CreateRoadmapAsync(string slug = "frontend-test")
    {
        var categories = await ReadAsync<CategoryResponse[]>(await _admin.GetAsync("/api/admin/roadmap-categories"));
        return await ReadAsync<RoadmapResponse>(await _admin.PostAsJsonAsync("/api/admin/roadmaps",
            new RoadmapRequest(categories[0].Id, "Frontend Test", slug, "Summary", RoadmapLevel.Beginner)), HttpStatusCode.Created);
    }
    private async Task<NodeResponse> CreateNodeAsync(Guid id, string slug) => await ReadAsync<NodeResponse>(
        await _admin.PostAsJsonAsync($"/api/admin/roadmaps/{id}/nodes", new NodeRequest(slug, slug, PositionX: 120, Width: 240)), HttpStatusCode.Created);
    private static MultipartFormDataContent FileForm(string name = "guide.pdf", string type = "application/pdf", byte[]? data = null, bool title = true)
    {
        var form = new MultipartFormDataContent();
        if (title) form.Add(new StringContent("Guide"), "title");
        var file = new ByteArrayContent(data ?? Encoding.UTF8.GetBytes("%PDF-1.7\nTest content\n%%EOF"));
        file.Headers.ContentType = new MediaTypeHeaderValue(type);
        form.Add(file, "file", name);
        return form;
    }

    [Fact]
    public async Task RoadmapWorkflow_DefaultPublished_DraftHidden_ArchivedReadableAndCannotAdd()
    {
        var roadmap = await CreateRoadmapAsync();
        Assert.Equal(RoadmapStatus.Published, roadmap.Status);
        Assert.NotNull(roadmap.PublishedAtUtc);
        Assert.Equal(HttpStatusCode.NoContent, (await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{roadmap.Id}/status", new { status = "Draft" })).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _member.GetAsync($"/api/roadmaps/{roadmap.Slug}")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{roadmap.Id}/status", new { status = "Published" })).StatusCode);
        var a = await CreateNodeAsync(roadmap.Id, "html");
        var b = await CreateNodeAsync(roadmap.Id, "css");
        var edge = await ReadAsync<EdgeResponse>(await _admin.PostAsJsonAsync($"/api/admin/roadmaps/{roadmap.Id}/edges", new EdgeRequest(a.Id, b.Id)), HttpStatusCode.Created);
        await _admin.PostAsJsonAsync($"/api/admin/roadmap-nodes/{a.Id}/resources/links", new LinkResourceRequest("Guide", "https://example.com"));
        Assert.Equal(HttpStatusCode.NoContent, (await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{roadmap.Id}/status", new { status = "Published" })).StatusCode);
        var detail = await ReadAsync<RoadmapResponse>(await _member.GetAsync($"/api/roadmaps/{roadmap.Slug}"));
        Assert.Equal(2, detail.Nodes.Count); Assert.Single(detail.Edges);
        Assert.NotNull(detail.PublishedAtUtc);
        Assert.Equal(roadmap.PublishedAtUtc!.Value, detail.PublishedAtUtc.Value, TimeSpan.FromSeconds(1));
        var node = await ReadAsync<NodeDetailResponse>(await _member.GetAsync($"/api/roadmaps/{roadmap.Id}/nodes/{a.Id}"));
        Assert.Single(node.Resources); Assert.Equal(b.Id, Assert.Single(node.NextNodes).Id);
        await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{roadmap.Id}/status", new { status = "Archived" });
        Assert.Equal(HttpStatusCode.OK, (await _member.GetAsync($"/api/roadmaps/{roadmap.Slug}")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _admin.PostAsJsonAsync($"/api/admin/roadmaps/{roadmap.Id}/nodes", new NodeRequest("No", "no"))).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _admin.PostAsJsonAsync($"/api/admin/roadmaps/{roadmap.Id}/edges", new EdgeRequest(b.Id, a.Id, RoadmapRelationType.Optional))).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _admin.PostAsJsonAsync($"/api/admin/roadmap-nodes/{a.Id}/resources/links", new LinkResourceRequest("No", "https://example.com"))).StatusCode);
        await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{roadmap.Id}/nodes/{a.Id}/status", new { isActive = false });
        detail = await ReadAsync<RoadmapResponse>(await _member.GetAsync($"/api/roadmaps/{roadmap.Slug}"));
        Assert.Single(detail.Nodes); Assert.Empty(detail.Edges);
        Assert.Equal(HttpStatusCode.NotFound, (await _member.GetAsync($"/api/roadmaps/{roadmap.Id}/nodes/{a.Id}")).StatusCode);
        var adminDetail = await ReadAsync<RoadmapResponse>(await _admin.GetAsync($"/api/admin/roadmaps/{roadmap.Id}"));
        Assert.Equal(2, adminDetail.Nodes.Count); Assert.Equal(edge.Id, Assert.Single(adminDetail.Edges).Id);
    }

    [Fact]
    public async Task GraphValidationAndBatchPositions_RejectInvalidRequestsWithoutPartialUpdate()
    {
        var roadmap = await CreateRoadmapAsync();
        var a = await CreateNodeAsync(roadmap.Id, "a"); var b = await CreateNodeAsync(roadmap.Id, "b"); var c = await CreateNodeAsync(roadmap.Id, "c");
        var path = $"/api/admin/roadmaps/{roadmap.Id}/edges";
        Assert.Equal(HttpStatusCode.BadRequest, (await _admin.PostAsJsonAsync(path, new EdgeRequest(a.Id, a.Id))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await _admin.PostAsJsonAsync(path, new EdgeRequest(a.Id, b.Id))).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _admin.PostAsJsonAsync(path, new EdgeRequest(a.Id, b.Id))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await _admin.PostAsJsonAsync(path, new EdgeRequest(b.Id, c.Id))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await _admin.PostAsJsonAsync(path, new EdgeRequest(c.Id, a.Id))).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await _admin.PostAsJsonAsync(path, new EdgeRequest(c.Id, a.Id, RoadmapRelationType.Optional, RoadmapLineStyle.Dashed))).StatusCode);
        var other = await CreateRoadmapAsync("other"); var foreign = await CreateNodeAsync(other.Id, "foreign");
        Assert.Equal(HttpStatusCode.BadRequest, (await _admin.PostAsJsonAsync(path, new EdgeRequest(a.Id, foreign.Id))).StatusCode);
        var positions = $"/api/admin/roadmaps/{roadmap.Id}/nodes/positions";
        Assert.Equal(HttpStatusCode.BadRequest, (await _admin.PatchAsJsonAsync(positions, new NodePositionsRequest([new(a.Id, 999, 2), new(foreign.Id, 3, 4)]))).StatusCode);
        var detail = await ReadAsync<RoadmapResponse>(await _admin.GetAsync($"/api/admin/roadmaps/{roadmap.Id}"));
        Assert.Equal(120, detail.Nodes.Single(x => x.Id == a.Id).Position.X);
        Assert.Equal(HttpStatusCode.NoContent, (await _admin.PatchAsJsonAsync(positions, new NodePositionsRequest([new(a.Id, -123.5m, 4), new(b.Id, 456.5m, 8)]))).StatusCode);
        detail = await ReadAsync<RoadmapResponse>(await _admin.GetAsync($"/api/admin/roadmaps/{roadmap.Id}"));
        Assert.Equal(-123.5m, detail.Nodes.Single(x => x.Id == a.Id).Position.X);
        Assert.Equal(240, detail.Nodes.Single(x => x.Id == a.Id).Width);
    }

    [Fact]
    public async Task CategoriesSearchFiltersReorderAndValidation_AreConsistent()
    {
        var category = await ReadAsync<CategoryResponse>(await _admin.PostAsJsonAsync("/api/admin/roadmap-categories", new CategoryRequest("New category", "new-category")), HttpStatusCode.Created);
        var roadmap = await ReadAsync<RoadmapResponse>(await _admin.PostAsJsonAsync("/api/admin/roadmaps", new RoadmapRequest(category.Id, "Lộ trình React", "Lộ trình React", "Summary", RoadmapLevel.Advanced)), HttpStatusCode.Created);
        Assert.Equal("lo-trinh-react", roadmap.Slug);
        Assert.Equal(HttpStatusCode.Conflict, (await _admin.PostAsJsonAsync("/api/admin/roadmaps", new RoadmapRequest(category.Id, "Duplicate", roadmap.Slug, "Summary", RoadmapLevel.Beginner))).StatusCode);
        var page = await ReadAsync<PageResponse<RoadmapSummary>>(await _member.GetAsync($"/api/roadmaps?search=react&level=Advanced&categoryId={category.Id}&pageSize=1"));
        Assert.Equal(roadmap.Id, Assert.Single(page.Items).Id); Assert.Equal(1, page.TotalPages);
        await _admin.PatchAsJsonAsync($"/api/admin/roadmap-categories/{category.Id}/status", new { isActive = false });
        Assert.Equal(HttpStatusCode.OK, (await _member.GetAsync($"/api/roadmaps/{roadmap.Slug}")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await _admin.PostAsJsonAsync("/api/admin/roadmaps", new RoadmapRequest(category.Id, "No", "no", "Summary", RoadmapLevel.Beginner))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _admin.PatchAsJsonAsync("/api/admin/roadmaps/reorder", new ReorderRequest([roadmap.Id]))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await _admin.PatchAsJsonAsync("/api/admin/roadmaps/reorder", new ReorderRequest([roadmap.Id, roadmap.Id]))).StatusCode);
        var invalid = await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{roadmap.Id}/status", new { status = "Invalid" });
        var error = await ReadAsync<JsonElement>(invalid, HttpStatusCode.BadRequest);
        Assert.True(error.TryGetProperty("traceId", out _)); Assert.True(error.TryGetProperty("validationErrors", out _));
        invalid = await _admin.PostAsJsonAsync("/api/admin/roadmap-categories", new CategoryRequest("", ""));
        error = await ReadAsync<JsonElement>(invalid, HttpStatusCode.BadRequest);
        Assert.True(error.GetProperty("validationErrors").TryGetProperty("name", out _));
    }

    [Fact]
    public async Task FileWorkflow_ValidatesUploadsProtectsDownloadsAndRetainsOldFileOnFailure()
    {
        var roadmap = await CreateRoadmapAsync(); var node = await CreateNodeAsync(roadmap.Id, "files");
        var path = $"/api/admin/roadmap-nodes/{node.Id}/resources/files";
        using var form = FileForm();
        var resource = await ReadAsync<ResourceResponse>(await _admin.PostAsync(path, form), HttpStatusCode.Created);
        var downloaded = await _member.GetAsync(resource.DownloadUrl);
        Assert.Equal(HttpStatusCode.OK, downloaded.StatusCode);
        Assert.Equal("application/pdf", downloaded.Content.Headers.ContentType!.MediaType);
        Assert.Contains("guide.pdf", downloaded.Content.Headers.ContentDisposition!.ToString());
        Assert.Equal("nosniff", downloaded.Headers.GetValues("X-Content-Type-Options").Single());
        var oldBytes = await downloaded.Content.ReadAsByteArrayAsync();
        using var invalid = FileForm(data: Encoding.UTF8.GetBytes("Not a PDF"), title: false);
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, (await _admin.PostAsync($"/api/admin/roadmap-resources/{resource.Id}/replace-file", invalid)).StatusCode);
        Assert.Equal(oldBytes, await _member.GetByteArrayAsync(resource.DownloadUrl));
        Assert.Single(Directory.GetFiles(_root));
        using var oversized = FileForm(data: new byte[1025]);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, (await _admin.PostAsync(path, oversized)).StatusCode);
        using var wrongMime = FileForm(type: "image/png");
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, (await _admin.PostAsync(path, wrongMime)).StatusCode);
        using var missing = new MultipartFormDataContent(); missing.Add(new StringContent("Guide"), "title");
        Assert.Equal(HttpStatusCode.BadRequest, (await _admin.PostAsync(path, missing)).StatusCode);
        using var replacement = FileForm("new.pdf", data: Encoding.UTF8.GetBytes("%PDF-1.7 new"), title: false);
        await ReadAsync<ResourceResponse>(await _admin.PostAsync($"/api/admin/roadmap-resources/{resource.Id}/replace-file", replacement));
        Assert.Single(Directory.GetFiles(_root));
        await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{roadmap.Id}/status", new { status = "Archived" });
        Assert.Equal(HttpStatusCode.OK, (await _member.GetAsync(resource.DownloadUrl)).StatusCode);
        using var archiveUpload = FileForm();
        Assert.Equal(HttpStatusCode.Conflict, (await _admin.PostAsync(path, archiveUpload)).StatusCode);
        await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{roadmap.Id}/status", new { status = "Draft" });
        Assert.Equal(HttpStatusCode.NotFound, (await _member.GetAsync(resource.DownloadUrl)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _admin.GetAsync(resource.DownloadUrl)).StatusCode);
        await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{roadmap.Id}/status", new { status = "Published" });
        await _admin.DeleteAsync($"/api/admin/roadmap-resources/{resource.Id}");
        Assert.Equal(HttpStatusCode.NotFound, (await _member.GetAsync(resource.DownloadUrl)).StatusCode);
        Assert.Single(Directory.GetFiles(_root));
        Assert.Equal(HttpStatusCode.NotFound, (await _member.GetAsync($"/App_Data/roadmap-resources/{Path.GetFileName(Directory.GetFiles(_root)[0])}")).StatusCode);
    }

    [Fact]
    public async Task AdminCanUpdateMetadataEdgesResourcesAndReorderWholeCollections()
    {
        var roadmap = await CreateRoadmapAsync();
        var a = await CreateNodeAsync(roadmap.Id, "a"); var b = await CreateNodeAsync(roadmap.Id, "b");
        var updated = await ReadAsync<RoadmapResponse>(await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{roadmap.Id}",
            new RoadmapRequest(roadmap.Category.Id, "Updated roadmap", roadmap.Slug, "New summary", RoadmapLevel.Intermediate)));
        Assert.Equal("Updated roadmap", updated.Title);
        var updatedNode = await ReadAsync<NodeResponse>(await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{roadmap.Id}/nodes/{a.Id}",
            new NodeRequest("Updated node", "updated-node", RoadmapNodeType.Milestone, 40, 60, 320, LearningObjectives: "Build a page")));
        Assert.Equal(RoadmapNodeType.Milestone, updatedNode.NodeType);
        var edges = $"/api/admin/roadmaps/{roadmap.Id}/edges";
        var edge = await ReadAsync<EdgeResponse>(await _admin.PostAsJsonAsync(edges, new EdgeRequest(a.Id, b.Id)), HttpStatusCode.Created);
        edge = await ReadAsync<EdgeResponse>(await _admin.PatchAsJsonAsync($"{edges}/{edge.Id}",
            new EdgeRequest(b.Id, a.Id, RoadmapRelationType.Recommended, RoadmapLineStyle.Dashed, "Reference")));
        Assert.Equal(b.Id, edge.SourceNodeId);
        Assert.Equal(HttpStatusCode.NoContent, (await _admin.DeleteAsync($"{edges}/{edge.Id}")).StatusCode);
        Assert.Empty(await ReadAsync<EdgeResponse[]>(await _admin.GetAsync(edges)));
        var resourcePath = $"/api/admin/roadmap-nodes/{a.Id}/resources";
        var first = await ReadAsync<ResourceResponse>(await _admin.PostAsJsonAsync($"{resourcePath}/links", new LinkResourceRequest("First", "https://example.com/1")), HttpStatusCode.Created);
        var second = await ReadAsync<ResourceResponse>(await _admin.PostAsJsonAsync($"{resourcePath}/links", new LinkResourceRequest("Second", "https://example.com/2")), HttpStatusCode.Created);
        first = await ReadAsync<ResourceResponse>(await _admin.PatchAsJsonAsync($"/api/admin/roadmap-resources/{first.Id}", new UpdateResourceRequest("Edited", ExternalUrl: "https://example.com/edited")));
        Assert.Equal("https://example.com/edited", first.ExternalUrl);
        Assert.Equal(HttpStatusCode.BadRequest, (await _admin.PatchAsJsonAsync($"{resourcePath}/reorder", new ReorderRequest([first.Id]))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await _admin.PatchAsJsonAsync($"{resourcePath}/reorder", new ReorderRequest([second.Id, first.Id]))).StatusCode);
        var resources = await ReadAsync<ResourceResponse[]>(await _admin.GetAsync(resourcePath));
        Assert.Equal(second.Id, resources[0].Id); Assert.Equal(0, resources[0].SortOrder); Assert.Equal(1, resources[1].SortOrder);
        await _admin.PatchAsJsonAsync($"/api/admin/roadmap-resources/{second.Id}/status", new { isActive = false });
        var detail = await ReadAsync<NodeDetailResponse>(await _member.GetAsync($"/api/roadmaps/{roadmap.Id}/nodes/{a.Id}"));
        Assert.Equal(first.Id, Assert.Single(detail.Resources).Id);
        Assert.Equal("Build a page", detail.LearningObjectives);
    }

    [Fact]
    public async Task OversizedMultipartRequest_Returns413ProblemDetailsBeforeBinding()
    {
        var roadmap = await CreateRoadmapAsync(); var node = await CreateNodeAsync(roadmap.Id, "files");
        using var form = FileForm(data: new byte[22 * 1024 * 1024]);
        var error = await ReadAsync<JsonElement>(await _admin.PostAsync($"/api/admin/roadmap-nodes/{node.Id}/resources/files", form), HttpStatusCode.RequestEntityTooLarge);
        Assert.True(error.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task MemberCannotCallAnyAdminRoute_AnonymousCannotRead()
    {
        using var anonymous = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/roadmaps")).StatusCode);
        var id = Guid.NewGuid();
        (HttpMethod Method, string Path)[] requests =
        [
            (HttpMethod.Get, "roadmap-categories"), (HttpMethod.Post, "roadmap-categories"),
            (HttpMethod.Patch, $"roadmap-categories/{id}"), (HttpMethod.Patch, $"roadmap-categories/{id}/status"),
            (HttpMethod.Get, "roadmaps"), (HttpMethod.Get, $"roadmaps/{id}"), (HttpMethod.Post, "roadmaps"),
            (HttpMethod.Patch, $"roadmaps/{id}"), (HttpMethod.Patch, $"roadmaps/{id}/status"), (HttpMethod.Patch, "roadmaps/reorder"),
            (HttpMethod.Get, $"roadmaps/{id}/nodes"), (HttpMethod.Post, $"roadmaps/{id}/nodes"),
            (HttpMethod.Patch, $"roadmaps/{id}/nodes/{id}"), (HttpMethod.Patch, $"roadmaps/{id}/nodes/{id}/status"),
            (HttpMethod.Patch, $"roadmaps/{id}/nodes/positions"), (HttpMethod.Get, $"roadmaps/{id}/edges"),
            (HttpMethod.Post, $"roadmaps/{id}/edges"), (HttpMethod.Patch, $"roadmaps/{id}/edges/{id}"), (HttpMethod.Delete, $"roadmaps/{id}/edges/{id}"),
            (HttpMethod.Get, $"roadmap-nodes/{id}/resources"), (HttpMethod.Post, $"roadmap-nodes/{id}/resources/links"),
            (HttpMethod.Post, $"roadmap-nodes/{id}/resources/files"), (HttpMethod.Patch, $"roadmap-resources/{id}"),
            (HttpMethod.Post, $"roadmap-resources/{id}/replace-file"), (HttpMethod.Patch, $"roadmap-resources/{id}/status"),
            (HttpMethod.Delete, $"roadmap-resources/{id}"), (HttpMethod.Patch, $"roadmap-nodes/{id}/resources/reorder")
        ];
        foreach (var (method, path) in requests)
        {
            using var request = new HttpRequestMessage(method, $"/api/admin/{path}");
            var response = await _member.SendAsync(request);
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden, $"{method} {path}: {response.StatusCode}");
        }
    }

    [Fact]
    public async Task SwaggerDescribesRoadmapBodiesUploadsDownloadsAndAuthorization()
    {
        var document = await ReadAsync<JsonElement>(await _admin.GetAsync("/swagger/v1/swagger.json"));
        var paths = document.GetProperty("paths");
        foreach (var path in paths.EnumerateObject())
        {
            Assert.False(path.Value.TryGetProperty("put", out _), $"PUT is still exposed at {path.Name}.");
        }

        string[] membershipPatchPaths =
        [
            "/api/admin/departments/{departmentId}",
            "/api/admin/generations/{generationId}",
            "/api/admin/members/{userId}/department-memberships/{departmentMembershipId}",
            "/api/admin/members/{userId}/department-memberships/{departmentMembershipId}/roles"
        ];
        foreach (var path in membershipPatchPaths)
        {
            Assert.True(paths.GetProperty(path).TryGetProperty("patch", out _));
        }

        (string Path, string Tag)[] patchEndpoints =
        [
            ("/api/admin/roadmap-categories/{id}", "Admin Roadmap Categories"),
            ("/api/admin/roadmaps/{id}", "Admin Roadmaps"),
            ("/api/admin/roadmaps/reorder", "Admin Roadmaps"),
            ("/api/admin/roadmaps/{roadmapId}/nodes/{nodeId}", "Admin Roadmap Nodes"),
            ("/api/admin/roadmaps/{roadmapId}/nodes/positions", "Admin Roadmap Nodes"),
            ("/api/admin/roadmaps/{roadmapId}/edges/{edgeId}", "Admin Roadmap Edges"),
            ("/api/admin/roadmap-resources/{id}", "Admin Roadmap Resources"),
            ("/api/admin/roadmap-nodes/{nodeId}/resources/reorder", "Admin Roadmap Resources")
        ];
        foreach (var (path, tag) in patchEndpoints)
        {
            Assert.False(paths.GetProperty(path).TryGetProperty("put", out _));
            var patch = paths.GetProperty(path).GetProperty("patch");
            Assert.Equal(tag, patch.GetProperty("tags")[0].GetString());
            Assert.True(patch.TryGetProperty("security", out _));
        }

        Assert.True(paths.GetProperty("/api/admin/roadmaps").GetProperty("post").TryGetProperty("security", out _));
        var upload = paths.GetProperty("/api/admin/roadmap-nodes/{nodeId}/resources/files").GetProperty("post");
        Assert.True(upload.GetProperty("requestBody").GetProperty("content").TryGetProperty("multipart/form-data", out _));
        Assert.True(upload.GetProperty("responses").TryGetProperty("413", out _));
        Assert.True(upload.GetProperty("responses").TryGetProperty("415", out _));
        var download = paths.GetProperty("/api/roadmap-resources/{id}/download").GetProperty("get");
        Assert.Equal("binary", download.GetProperty("responses").GetProperty("200").GetProperty("content")
            .GetProperty("application/octet-stream").GetProperty("schema").GetProperty("format").GetString());
    }
}
