using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Sharing;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Memberships;
using GdscSharingPlatform.Infrastructure.Identity;
using GdscSharingPlatform.Infrastructure.Persistence;
using GdscSharingPlatform.IntegrationTests.Support;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GdscSharingPlatform.IntegrationTests.Sharing;

public sealed class SharingEndpointsTests : IAsyncLifetime
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), $"sharing_test_{Guid.NewGuid():N}");
    private readonly PostgresTestDatabase? _postgres = PostgresTestDatabase.FromEnvironment();
    private readonly WebApplicationFactory<Program> _factory;
    private HttpClient _owner = null!, _admin = null!, _other = null!;
    private Guid _ownerId, _otherId;
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    public SharingEndpointsTests()
    {
        var dbName = Guid.NewGuid().ToString();
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
                    if (_postgres is null) options.UseInMemoryDatabase(dbName); else options.UseNpgsql(_postgres.ConnectionString);
                });
            });
        });
    }
    public async Task InitializeAsync()
    {
        if (_postgres is not null) await _postgres.CreateAsync();
        (_admin, _) = await ClientAsync(RoleNames.Admin);
        (_owner, _ownerId) = await ClientAsync(RoleNames.Member);
        (_other, _otherId) = await ClientAsync(RoleNames.Member);
    }
    public async Task DisposeAsync()
    {
        _owner?.Dispose(); _other?.Dispose(); _admin?.Dispose();
        await _factory.DisposeAsync();
        if (_postgres is not null) await _postgres.DropAsync();
        if (Directory.Exists(_root)) Directory.Delete(_root, true);
    }
    private async Task<(HttpClient, Guid)> ClientAsync(string role)
    {
        var client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = new ApplicationUser { Id = Guid.NewGuid(), FullName = "Sharing tester", Email = $"{Guid.NewGuid():N}@test.local" };
        db.Add(user); await db.SaveChangesAsync();
        var token = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>()
            .GenerateAccessToken(user.Id, user.Email, user.FullName, [role], null, UserStatus.Active.ToString(), 1);
        client.DefaultRequestHeaders.Authorization = new("Bearer", token.Token);
        return (client, user.Id);
    }
    private static async Task<T> Read<T>(HttpResponseMessage response, HttpStatusCode status = HttpStatusCode.OK)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == status, $"Expected {status}, got {response.StatusCode}: {body}");
        return JsonSerializer.Deserialize<T>(body, Json)!;
    }
    private static Task<HttpResponseMessage> Mutate(HttpClient client, HttpMethod method, string path, long version, object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.IfMatch.Add(new EntityTagHeaderValue($"\"{version}\""));
        if (body is not null) request.Content = JsonContent.Create(body, options: Json);
        return client.SendAsync(request);
    }
    private ContentRequest Content(string slug = "sharing-test") => new("Sharing test", slug, "Summary", "# Original markdown", [], [_otherId]);
    private ScheduleRequest Schedule(IReadOnlyList<PresenterRequest>? presenters = null) => new("Sharing session", SharingType.TechTalk,
        DeliveryMode.Online, new(2026, 10, 1, 14, 0, 0), new(2026, 10, 1, 15, 0, 0), "Asia/Ho_Chi_Minh",
        AudienceScope.AllMembers, presenters ?? [], [], [], [], MeetingUrl: "https://meet.example.com/private-token");

    [Fact]
    public async Task ContentWorkflow_DraftPrivate_ContributorRejectedOnly_PublishedOwnerEdits_StaleFails()
    {
        var request = Content();
        var content = await Read<ContentResponse>(await _owner.PostAsJsonAsync("/api/sharing/contents", request, Json), HttpStatusCode.Created);
        var id = content.Content.Id;
        Assert.Equal(_ownerId, content.Authors.Single(x => x.Role == SharingAuthorRole.Owner).UserId);
        Assert.Equal(HttpStatusCode.NotFound, (await _other.GetAsync($"/api/sharing/contents/mine/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _other.GetAsync($"/api/sharing/contents/{id}/resources")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Mutate(_other, HttpMethod.Patch, $"/api/sharing/contents/{id}", 0, request)).StatusCode);
        content = await Read<ContentResponse>(await Mutate(_owner, HttpMethod.Post, $"/api/sharing/contents/{id}/submit", 0));
        Assert.Equal(SharingContentStatus.PendingReview, content.Content.Status);
        Assert.Equal(HttpStatusCode.BadRequest, (await Mutate(_owner, HttpMethod.Patch, $"/api/sharing/contents/{id}", 1, request)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/contents/{id}/reject", 1, new ReviewRequest(""))).StatusCode);
        content = await Read<ContentResponse>(await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/contents/{id}/reject", 1, new ReviewRequest("Add examples")));
        content = await Read<ContentResponse>(await Mutate(_other, HttpMethod.Patch, $"/api/sharing/contents/{id}", 2, request with { BodyMarkdown = "# Revised" }));
        Assert.Equal(3, content.Content.Version);
        Assert.Equal(HttpStatusCode.Forbidden, (await Mutate(_other, HttpMethod.Post, $"/api/sharing/contents/{id}/submit", 3)).StatusCode);
        content = await Read<ContentResponse>(await Mutate(_owner, HttpMethod.Post, $"/api/sharing/contents/{id}/submit", 3));
        content = await Read<ContentResponse>(await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/contents/{id}/approve", 4));
        Assert.Null(content.ReviewNote);
        var publishedAt = content.Content.PublishedAtUtc;
        Assert.Equal(HttpStatusCode.PreconditionFailed, (await Mutate(_owner, HttpMethod.Post, $"/api/sharing/contents/{id}/withdraw", 4)).StatusCode);
        content = await Read<ContentResponse>(await Mutate(_owner, HttpMethod.Patch, $"/api/sharing/contents/{id}", 5, request with { BodyMarkdown = "# Published edit", ContributorUserIds = [] }));
        Assert.Equal(SharingContentStatus.Published, content.Content.Status);
        Assert.Equal(publishedAt, content.Content.PublishedAtUtc);
        Assert.Single(content.Authors);
        Assert.Equal("# Published edit", (await Read<ContentResponse>(await _other.GetAsync("/api/sharing/contents/sharing-test"))).BodyMarkdown);
        content = await Read<ContentResponse>(await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/contents/{id}/archive", 6));
        content = await Read<ContentResponse>(await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/contents/{id}/return-to-draft", 7));
        Assert.Equal(SharingContentStatus.Draft, content.Content.Status);
        using var scope = _factory.Services.CreateScope();
        var audit = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Set<SharingAuditEntry>().Where(x => x.EntityId == id).ToListAsync();
        Assert.Contains(audit, x => x.Action == "Approve"); Assert.Contains(audit, x => x.Action == "RemoveAuthor");
        Assert.All(audit, x => Assert.False(string.IsNullOrWhiteSpace(x.TraceId)));
    }

    [Fact]
    public async Task ScheduleWorkflow_SelfAssignment_AdminAssignment_OverlapAndImmutableHistory()
    {
        var request = Schedule();
        var schedule = await Read<ScheduleResponse>(await _owner.PostAsJsonAsync("/api/sharing/schedules", request, Json), HttpStatusCode.Created);
        Assert.Equal(_ownerId, Assert.Single(schedule.Presenters).UserId);
        Assert.Equal(7, schedule.StartsAtUtc.Hour);
        Assert.Equal(HttpStatusCode.NotFound, (await _other.GetAsync($"/api/sharing/schedules/{schedule.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _owner.PostAsJsonAsync("/api/sharing/schedules", Schedule([new(_otherId, PresenterRole.Speaker)]), Json)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await Mutate(_other, HttpMethod.Patch, $"/api/sharing/schedules/{schedule.Id}", 0, request)).StatusCode);
        schedule = await Read<ScheduleResponse>(await Mutate(_owner, HttpMethod.Patch, $"/api/sharing/schedules/{schedule.Id}", 0, request with { Title = "Edited" }));
        schedule = await Read<ScheduleResponse>(await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/schedules/{schedule.Id}/publish", 1));
        Assert.Equal(request.MeetingUrl, (await Read<ScheduleResponse>(await _other.GetAsync($"/api/sharing/schedules/{schedule.Id}"))).MeetingUrl);
        Assert.Equal(HttpStatusCode.Conflict, (await _admin.PostAsJsonAsync("/api/admin/sharing/schedules", Schedule([new(_ownerId, PresenterRole.Host)]), Json)).StatusCode);
        var adjacent = Schedule([new(_ownerId, PresenterRole.Host)]) with { StartsAtLocal = request.EndsAtLocal, EndsAtLocal = request.EndsAtLocal.AddHours(1) };
        Assert.Equal(HttpStatusCode.Created, (await _admin.PostAsJsonAsync("/api/admin/sharing/schedules", adjacent, Json)).StatusCode);
        schedule = await Read<ScheduleResponse>(await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/schedules/{schedule.Id}/start", 2));
        schedule = await Read<ScheduleResponse>(await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/schedules/{schedule.Id}/complete", 3));
        Assert.Equal(HttpStatusCode.BadRequest, (await Mutate(_admin, HttpMethod.Patch, $"/api/admin/sharing/schedules/{schedule.Id}/presenters", 4, new PresentersRequest([]))).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Mutate(_admin, HttpMethod.Delete, $"/api/admin/sharing/schedules/{schedule.Id}", 4)).StatusCode);
    }

    [Fact]
    public async Task SelectedAudience_FiltersListsDetailAndLinkedDraft_WithoutLeakingMeetingUrl()
    {
        Guid generationId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var generation = new ClubGeneration(99); generationId = generation.Id;
            db.Add(generation); db.Add(new ClubMembership(_otherId, generationId)); await db.SaveChangesAsync();
        }
        var content = await Read<ContentResponse>(await _owner.PostAsJsonAsync("/api/sharing/contents", Content(), Json), HttpStatusCode.Created);
        var request = Schedule([new(_ownerId, PresenterRole.Speaker)]) with
        { AudienceScope = AudienceScope.SelectedAudience, GenerationIds = [generationId], ContentIds = [content.Content.Id] };
        var schedule = await Read<ScheduleResponse>(await _admin.PostAsJsonAsync("/api/admin/sharing/schedules", request, Json), HttpStatusCode.Created);
        Assert.Single((await Read<ScheduleResponse>(await _owner.GetAsync($"/api/sharing/schedules/{schedule.Id}"))).Contents);
        Assert.Equal(HttpStatusCode.NotFound, (await _other.GetAsync($"/api/sharing/schedules/{schedule.Id}")).StatusCode);
        schedule = await Read<ScheduleResponse>(await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/schedules/{schedule.Id}/publish", 0));
        var memberView = await Read<ScheduleResponse>(await _other.GetAsync($"/api/sharing/schedules/{schedule.Id}"));
        Assert.Empty(memberView.Contents); Assert.NotNull(memberView.MeetingUrl);
        var page = await Read<SharingPage<ScheduleResponse>>(await _other.GetAsync("/api/sharing/schedules"));
        Assert.Single(page.Items);
        var (outside, _) = await ClientAsync(RoleNames.Member);
        using (outside)
        {
            Assert.Equal(HttpStatusCode.NotFound, (await outside.GetAsync($"/api/sharing/schedules/{schedule.Id}")).StatusCode);
            Assert.Empty((await Read<SharingPage<ScheduleResponse>>(await outside.GetAsync("/api/sharing/schedules"))).Items);
        }
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await db.ClubMemberships.SingleAsync(x => x.UserId == _otherId)).End(); await db.SaveChangesAsync();
        }
        Assert.Equal(HttpStatusCode.NotFound, (await _other.GetAsync($"/api/sharing/schedules/{schedule.Id}")).StatusCode);
    }

    [Fact]
    public async Task Files_ParentVersion_PrivateDraft_Validation_ReplaceAndSoftDelete()
    {
        var content = await Read<ContentResponse>(await _owner.PostAsJsonAsync("/api/sharing/contents", Content(), Json), HttpStatusCode.Created);
        var path = $"/api/sharing/contents/{content.Content.Id}/resources/files";
        var resource = await Read<ResourceMutationResponse>(await Upload(path, 0, "%PDF-1.7 original", true), HttpStatusCode.Created);
        Assert.Equal(1, resource.Version);
        var download = $"/api/sharing/resources/{resource.Resource.Id}/download";
        Assert.Equal(HttpStatusCode.NotFound, (await _other.GetAsync(download)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await _owner.GetAsync(download)).StatusCode);
        Assert.Equal(HttpStatusCode.PreconditionFailed, (await Upload(path, 0, "%PDF-1.7 stale", true)).StatusCode);
        var replace = $"/api/sharing/resources/{resource.Resource.Id}/replace-file";
        Assert.Equal(HttpStatusCode.UnsupportedMediaType, (await Upload(replace, 1, "bad signature", false)).StatusCode);
        Assert.Contains("original", await _owner.GetStringAsync(download));
        resource = await Read<ResourceMutationResponse>(await Upload(replace, 1, "%PDF-1.7 replacement", false));
        Assert.Single(Directory.GetFiles(_root));
        Assert.Equal(HttpStatusCode.NoContent, (await Mutate(_owner, HttpMethod.Delete, $"/api/sharing/resources/{resource.Resource.Id}", 2)).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _owner.GetAsync(download)).StatusCode);
        Assert.Single(Directory.GetFiles(_root));
    }
    private async Task<HttpResponseMessage> Upload(string path, long version, string bytes, bool title)
    {
        using var form = new MultipartFormDataContent();
        if (title) form.Add(new StringContent("Guide"), "title");
        var file = new ByteArrayContent(Encoding.UTF8.GetBytes(bytes)); file.Headers.ContentType = new("application/pdf");
        form.Add(file, "file", "guide.pdf");
        using var request = new HttpRequestMessage(HttpMethod.Post, path) { Content = form };
        request.Headers.IfMatch.Add(new($"\"{version}\"")); return await _owner.SendAsync(request);
    }

    [Fact]
    public async Task AssociationUpdates_PersistNewAuthorsTagsPresentersAndRejectStaleChanges()
    {
        var request = Content() with { ContributorUserIds = [] };
        var content = await Read<ContentResponse>(await _owner.PostAsJsonAsync("/api/sharing/contents", request, Json), HttpStatusCode.Created);
        var tag = await Read<TagResponse>(await _admin.PostAsJsonAsync("/api/admin/sharing/tags", new TagRequest("Backend", "backend", "#00AAFF"), Json), HttpStatusCode.Created);
        request = request with { ContributorUserIds = [_otherId], TagIds = [tag.Id] };
        content = await Read<ContentResponse>(await Mutate(_owner, HttpMethod.Patch, $"/api/sharing/contents/{content.Content.Id}", 0, request));
        Assert.Equal(2, content.Authors.Count); Assert.Single(content.Tags);
        var schedule = await Read<ScheduleResponse>(await _admin.PostAsJsonAsync("/api/admin/sharing/schedules", Schedule(), Json), HttpStatusCode.Created);
        schedule = await Read<ScheduleResponse>(await Mutate(_admin, HttpMethod.Patch, $"/api/admin/sharing/schedules/{schedule.Id}/presenters", 0,
            new PresentersRequest([new(_ownerId, PresenterRole.Host), new(_otherId, PresenterRole.Speaker)])));
        Assert.Equal(2, schedule.Presenters.Count);
        schedule = await Read<ScheduleResponse>(await Mutate(_admin, HttpMethod.Patch, $"/api/admin/sharing/schedules/{schedule.Id}/contents", 1,
            new ScheduleContentsRequest([content.Content.Id])));
        Assert.Single(schedule.Contents);
        Assert.Equal(HttpStatusCode.PreconditionFailed, (await Mutate(_admin, HttpMethod.Patch, $"/api/admin/sharing/schedules/{schedule.Id}/presenters", 0,
            new PresentersRequest([]))).StatusCode);
        schedule = await Read<ScheduleResponse>(await Mutate(_admin, HttpMethod.Patch, $"/api/admin/sharing/schedules/{schedule.Id}/presenters", 2,
            new PresentersRequest([new(_otherId, PresenterRole.Speaker, 3)])));
        Assert.Equal(3, Assert.Single(schedule.Presenters).SortOrder);
    }

    [Fact]
    public async Task ContentListIncludesAuthorsAndTags_WithoutExposingDraftsOrBreakingPagination()
    {
        var tag = await Read<TagResponse>(await _admin.PostAsJsonAsync("/api/admin/sharing/tags",
            new TagRequest("Backend", "backend", "#00AAFF"), Json), HttpStatusCode.Created);
        for (var i = 0; i < 3; i++)
        {
            var request = Content($"list-{i}") with { TagIds = [tag.Id] };
            var content = await Read<ContentResponse>(await _owner.PostAsJsonAsync("/api/sharing/contents", request, Json), HttpStatusCode.Created);
            if (i == 2) continue;
            await Read<ContentResponse>(await Mutate(_owner, HttpMethod.Post, $"/api/sharing/contents/{content.Content.Id}/submit", 0));
            await Read<ContentResponse>(await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/contents/{content.Content.Id}/approve", 1));
        }
        var url = $"/api/sharing/contents?tagId={tag.Id}&authorId={_ownerId}&pageSize=1&sort=oldest";
        var first = await Read<SharingPage<ContentSummary>>(await _other.GetAsync(url));
        var item = Assert.Single(first.Items);
        Assert.Equal(2, first.TotalCount);
        Assert.Equal(2, item.Authors.Count);
        Assert.Equal(_ownerId, item.Authors.Single(x => x.Role == SharingAuthorRole.Owner).UserId);
        Assert.All(item.Authors, x => Assert.Equal("Sharing tester", x.FullName));
        Assert.Equal(tag.Id, Assert.Single(item.Tags).Id);
        var second = await Read<SharingPage<ContentSummary>>(await _other.GetAsync(url + "&page=2"));
        Assert.NotEqual(item.Id, Assert.Single(second.Items).Id);
        Assert.All(first.Items.Concat(second.Items), x => Assert.Equal(SharingContentStatus.Published, x.Status));
    }

    [Fact]
    public async Task InvalidEndTimeReportsCorrectFieldOnCreateAndUpdate()
    {
        var request = Schedule() with
        {
            TimeZoneId = "America/New_York", StartsAtLocal = new(2026, 11, 1, 0, 30, 0),
            EndsAtLocal = new(2026, 11, 1, 1, 30, 0)
        };
        var createError = await Read<JsonElement>(await _owner.PostAsJsonAsync("/api/sharing/schedules", request, Json), HttpStatusCode.BadRequest);
        Assert.True(createError.GetProperty("validationErrors").TryGetProperty("endsAtLocal", out _));
        var schedule = await Read<ScheduleResponse>(await _owner.PostAsJsonAsync("/api/sharing/schedules", Schedule(), Json), HttpStatusCode.Created);
        var updateError = await Read<JsonElement>(await Mutate(_owner, HttpMethod.Patch, $"/api/sharing/schedules/{schedule.Id}", 0, request), HttpStatusCode.BadRequest);
        Assert.True(updateError.GetProperty("validationErrors").TryGetProperty("endsAtLocal", out _));
        Assert.Equal(0, (await Read<ScheduleResponse>(await _owner.GetAsync($"/api/sharing/schedules/{schedule.Id}"))).Version);
    }

    [Fact]
    public async Task DepartmentAudience_UsesActiveDepartmentMembership_AndGenerationOrDepartment()
    {
        Guid generationId, departmentId, departmentMembershipId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var generation = new ClubGeneration(98);
            var department = new GdscSharingPlatform.Domain.Departments.Department
            { Code = "PHASE7", Name = "Testing", Slug = "phase7" };
            var membership = new ClubMembership(_otherId, generation.Id);
            var departmentMembership = new DepartmentMembership(membership.Id, department.Id);
            generationId = generation.Id; departmentId = department.Id; departmentMembershipId = departmentMembership.Id;
            db.AddRange(generation, department, membership, departmentMembership);
            await db.SaveChangesAsync();
        }
        var request = Schedule([new(_ownerId, PresenterRole.Speaker)]) with
        { AudienceScope = AudienceScope.SelectedAudience, DepartmentIds = [departmentId] };
        var schedule = await Read<ScheduleResponse>(await _admin.PostAsJsonAsync("/api/admin/sharing/schedules", request, Json), HttpStatusCode.Created);
        schedule = await Read<ScheduleResponse>(await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/schedules/{schedule.Id}/publish", schedule.Version));
        Assert.NotNull((await Read<ScheduleResponse>(await _other.GetAsync($"/api/sharing/schedules/{schedule.Id}"))).MeetingUrl);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            (await db.Set<DepartmentMembership>().SingleAsync(x => x.Id == departmentMembershipId)).End();
            await db.SaveChangesAsync();
        }
        Assert.Equal(HttpStatusCode.NotFound, (await _other.GetAsync($"/api/sharing/schedules/{schedule.Id}")).StatusCode);
        Assert.Empty((await Read<SharingPage<ScheduleResponse>>(await _other.GetAsync("/api/sharing/schedules"))).Items);
        schedule = await Read<ScheduleResponse>(await Mutate(_admin, HttpMethod.Patch, $"/api/admin/sharing/schedules/{schedule.Id}/audience",
            schedule.Version, new AudienceRequest(AudienceScope.SelectedAudience, [generationId], [departmentId])));
        Assert.NotNull((await Read<ScheduleResponse>(await _other.GetAsync($"/api/sharing/schedules/{schedule.Id}"))).MeetingUrl);
    }

    [Fact]
    public async Task CancelledScheduleReleasesOverlap_ButCannotRestartOrEdit()
    {
        var request = Schedule([new(_ownerId, PresenterRole.Speaker)]);
        var schedule = await Read<ScheduleResponse>(await _admin.PostAsJsonAsync("/api/admin/sharing/schedules", request, Json), HttpStatusCode.Created);
        Assert.Equal(HttpStatusCode.Forbidden, (await Mutate(_owner, HttpMethod.Post, $"/api/admin/sharing/schedules/{schedule.Id}/publish", 0)).StatusCode);
        schedule = await Read<ScheduleResponse>(await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/schedules/{schedule.Id}/publish", 0));
        Assert.Equal(HttpStatusCode.BadRequest, (await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/schedules/{schedule.Id}/cancel", 1, new CancelScheduleRequest(" "))).StatusCode);
        schedule = await Read<ScheduleResponse>(await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/schedules/{schedule.Id}/cancel", 1, new CancelScheduleRequest("Presenter unavailable")));
        Assert.Equal(SharingScheduleStatus.Cancelled, schedule.Status);
        Assert.Equal("Presenter unavailable", schedule.CancellationReason);
        Assert.Equal(HttpStatusCode.BadRequest, (await Mutate(_admin, HttpMethod.Post, $"/api/admin/sharing/schedules/{schedule.Id}/start", 2)).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await Mutate(_admin, HttpMethod.Patch, $"/api/admin/sharing/schedules/{schedule.Id}", 2, request)).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await _admin.PostAsJsonAsync("/api/admin/sharing/schedules", request, Json)).StatusCode);
    }

    [Fact]
    public async Task PendingReviewLocksResources_WithdrawRestoresEditing_AndOversizeUploadLeavesNoFile()
    {
        var content = await Read<ContentResponse>(await _owner.PostAsJsonAsync("/api/sharing/contents", Content(), Json), HttpStatusCode.Created);
        var id = content.Content.Id;
        var path = $"/api/sharing/contents/{id}/resources/files";
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, (await Upload(path, 0, "%PDF-1.7" + new string('x', 1024), true)).StatusCode);
        Assert.True(!Directory.Exists(_root) || Directory.GetFiles(_root).Length == 0);
        await Read<ContentResponse>(await Mutate(_owner, HttpMethod.Post, $"/api/sharing/contents/{id}/submit", 0));
        Assert.Equal(HttpStatusCode.BadRequest, (await Upload(path, 1, "%PDF-1.7 locked", true)).StatusCode);
        var withdrawn = await Read<ContentResponse>(await Mutate(_owner, HttpMethod.Post, $"/api/sharing/contents/{id}/withdraw", 1));
        Assert.Equal(SharingContentStatus.Draft, withdrawn.Content.Status);
        Assert.Equal(HttpStatusCode.Created, (await Upload(path, 2, "%PDF-1.7 editable", true)).StatusCode);
    }

    [Fact]
    public async Task Tags_AdminCanEditAndDeactivate_MembersCannotManageOrAssignInactiveTags()
    {
        var request = new TagRequest("Testing", "testing", "#112233");
        Assert.Equal(HttpStatusCode.Forbidden, (await _owner.PostAsJsonAsync("/api/admin/sharing/tags", request, Json)).StatusCode);
        var tag = await Read<TagResponse>(await _admin.PostAsJsonAsync("/api/admin/sharing/tags", request, Json), HttpStatusCode.Created);
        tag = await Read<TagResponse>(await _admin.PatchAsJsonAsync($"/api/admin/sharing/tags/{tag.Id}", request with { Name = "Updated" }, Json));
        Assert.Equal("Updated", tag.Name);
        Assert.Contains(await Read<List<TagResponse>>(await _owner.GetAsync("/api/sharing/tags")), x => x.Id == tag.Id);
        tag = await Read<TagResponse>(await _admin.PatchAsJsonAsync($"/api/admin/sharing/tags/{tag.Id}/status", new TagStatusRequest(false), Json));
        Assert.False(tag.IsActive);
        Assert.DoesNotContain(await Read<List<TagResponse>>(await _owner.GetAsync("/api/sharing/tags")), x => x.Id == tag.Id);
        Assert.Contains(await Read<List<TagResponse>>(await _admin.GetAsync("/api/admin/sharing/tags")), x => x.Id == tag.Id && !x.IsActive);
        Assert.Equal(HttpStatusCode.BadRequest, (await _owner.PostAsJsonAsync("/api/sharing/contents", Content() with { TagIds = [tag.Id] }, Json)).StatusCode);
    }

    [Fact]
    public async Task ApiContracts_ValidationErrors_AdminAuthorization_SwaggerAndSubmitRateLimit()
    {
        using var anonymous = _factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/sharing/contents")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await _owner.GetAsync("/api/admin/sharing/contents")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await _owner.PostAsJsonAsync("/api/sharing/contents", Content() with { TagIds = null! }, Json)).StatusCode);
        var content = await Read<ContentResponse>(await _owner.PostAsJsonAsync("/api/sharing/contents", Content(), Json), HttpStatusCode.Created);
        var missing = await _owner.PatchAsJsonAsync($"/api/sharing/contents/{content.Content.Id}", Content(), Json);
        var error = await Read<JsonElement>(missing, HttpStatusCode.BadRequest); Assert.True(error.TryGetProperty("traceId", out _));
        var doc = await Read<JsonElement>(await _admin.GetAsync("/swagger/v1/swagger.json"));
        string[] patchPaths =
        [
            "/api/sharing/contents/{id}", "/api/sharing/schedules/{id}",
            "/api/admin/sharing/schedules/{id}", "/api/admin/sharing/schedules/{id}/presenters",
            "/api/admin/sharing/schedules/{id}/contents", "/api/admin/sharing/schedules/{id}/audience",
            "/api/admin/sharing/tags/{id}", "/api/sharing/resources/{id}",
            "/api/sharing/contents/{contentId}/resources/reorder"
        ];
        foreach (var path in patchPaths)
        {
            var operations = doc.GetProperty("paths").GetProperty(path);
            Assert.True(operations.TryGetProperty("patch", out _), $"PATCH missing at {path}");
            Assert.False(operations.TryGetProperty("put", out _), $"PUT still exposed at {path}");
        }
        using var obsoletePut = new HttpRequestMessage(HttpMethod.Put, $"/api/sharing/contents/{content.Content.Id}")
        { Content = JsonContent.Create(Content(), options: Json) };
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await _owner.SendAsync(obsoletePut)).StatusCode);
        var patch = doc.GetProperty("paths").GetProperty("/api/sharing/contents/{id}").GetProperty("patch");
        Assert.Contains(patch.GetProperty("parameters").EnumerateArray(), p => p.GetProperty("name").GetString() == "If-Match");
        for (var i = 0; i < 5; i++) await Mutate(_owner, HttpMethod.Post, $"/api/sharing/contents/{content.Content.Id}/submit", 999);
        var limited = await Mutate(_owner, HttpMethod.Post, $"/api/sharing/contents/{content.Content.Id}/submit", 999);
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.NotNull(limited.Headers.RetryAfter);
    }
}
