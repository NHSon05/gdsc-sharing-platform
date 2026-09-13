using System.Net;
using System.Net.Http.Json;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Roadmaps.Models;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Infrastructure.Identity.Seeding;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GdscSharingPlatform.IntegrationTests.Roadmaps;

public sealed partial class RoadmapEndpointsTests
{
    [Fact]
    public async Task SeededRoadmaps_AdminEditsAreVisibleToMembersAndSurviveReseeding()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var author = await db.Users.FirstAsync();
            var role = await db.Roles.SingleAsync(x => x.Name == RoleNames.Admin);
            db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = author.Id, RoleId = role.Id });
            await db.SaveChangesAsync();
            await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedRoadmapsAsync();
        }

        var catalog = await ReadAsync<PageResponse<RoadmapSummary>>(await _member.GetAsync("/api/roadmaps"));
        Assert.Equal(6, catalog.TotalItems);
        var memberRoadmap = await ReadAsync<RoadmapResponse>(await _member.GetAsync("/api/roadmaps/frontend"));
        var adminRoadmap = await ReadAsync<RoadmapResponse>(await _admin.GetAsync($"/api/admin/roadmaps/{memberRoadmap.Id}"));
        Assert.Equal(memberRoadmap.Id, adminRoadmap.Id);
        Assert.Equal(5, memberRoadmap.Nodes.Count);
        Assert.Equal(4, memberRoadmap.Edges.Count);

        var request = new RoadmapRequest(memberRoadmap.Category.Id, "Frontend Club Curriculum", "frontend",
            "Updated by the club administrator", RoadmapLevel.Beginner);
        await ReadAsync<RoadmapResponse>(await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{memberRoadmap.Id}", request));
        var node = memberRoadmap.Nodes[0];
        await ReadAsync<NodeResponse>(await _admin.PatchAsJsonAsync($"/api/admin/roadmaps/{memberRoadmap.Id}/nodes/{node.Id}",
            new NodeRequest("Updated HTML lesson", node.Slug, Description: "Administrator lesson content", PositionX: 100, Width: 280)));
        Assert.Equal(HttpStatusCode.Forbidden,
            (await _member.PatchAsJsonAsync($"/api/admin/roadmaps/{memberRoadmap.Id}", request)).StatusCode);

        using (var scope = _factory.Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<DatabaseSeeder>().SeedRoadmapsAsync();

        var updated = await ReadAsync<RoadmapResponse>(await _member.GetAsync("/api/roadmaps/frontend"));
        Assert.Equal(request.Title, updated.Title);
        Assert.Equal(request.ShortDescription, updated.ShortDescription);
        Assert.Contains(updated.Nodes, x => x.Id == node.Id && x.Title == "Updated HTML lesson");
        Assert.Equal(6, (await ReadAsync<PageResponse<RoadmapSummary>>(await _member.GetAsync("/api/roadmaps"))).TotalItems);
    }
}
