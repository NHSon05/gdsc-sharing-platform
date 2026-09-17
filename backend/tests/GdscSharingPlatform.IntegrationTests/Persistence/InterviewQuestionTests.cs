using System.Text.Json;
using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Common.Interfaces;
using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Application.Features.Interviews;
using GdscSharingPlatform.Domain.Interviews;
using GdscSharingPlatform.Infrastructure.Persistence;
using GdscSharingPlatform.Infrastructure.Services;
using GdscSharingPlatform.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net;
using System.Net.Http.Json;

namespace GdscSharingPlatform.IntegrationTests.Persistence;

public sealed class InterviewQuestionTests
{
    [PostgresFact]
    public async Task MigrationAndQueries_PreserveJsonAndEnforceVisibility()
    {
        var postgres = PostgresTestDatabase.FromEnvironment()!;
        await postgres.CreateAsync();
        try
        {
            await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(postgres.ConnectionString).Options);
            await db.Database.MigrateAsync();
            Assert.False(db.Database.HasPendingModelChanges());
            var published = NewQuestion("published", "PostgreSQL JSONB", false);
            var draft = NewQuestion("draft", "Pending review", true);
            db.AddRange(published, draft);
            await db.SaveChangesAsync();
            db.ChangeTracker.Clear();
            Assert.Equal("PostgreSQL JSONB", (await db.InterviewQuestions.SingleAsync(q => q.Id == published.Id)).Question);

            // Simulate adopting a populated table that was seeded before this migration.
            await db.Database.ExecuteSqlRawAsync("DELETE FROM \"__EFMigrationsHistory\" WHERE \"MigrationId\" = '20260916034314_AddInterviewQuestions'");
            await db.Database.MigrateAsync();
            Assert.Equal(2, await db.InterviewQuestions.CountAsync());
            var reader = new InterviewQuestionService(db, new User(RoleNames.Member));
            var page = await reader.ListAsync(new() { Department = "backend", Topic = "postgresql", Search = "jsonb", Level = "basic" }, false, default);
            Assert.Equal(published.Id, Assert.Single(page.Items).Id);
            Assert.Equal(1, page.Total);
            Assert.Empty((await reader.ListAsync(new() { Department = "frontend" }, false, default)).Items);
            Assert.Empty((await reader.ListAsync(new() { Page = 2, PageSize = 1 }, false, default)).Items);
            var detail = await reader.GetAsync(published.Id, false, default);
            Assert.Equal("Sample", detail.Data.GetProperty("answer").GetProperty("summary")[0].GetString());
            await Assert.ThrowsAsync<NotFoundException>(() => reader.GetAsync(draft.Id, false, default));
            await Assert.ThrowsAsync<ForbiddenAccessException>(() => reader.ListAsync(new(), true, default));
            await Assert.ThrowsAsync<System.ComponentModel.DataAnnotations.ValidationException>(() => reader.ListAsync(new() { PageSize = 101 }, false, default));
            var admin = new InterviewQuestionService(db, new User(RoleNames.Admin));
            Assert.Equal(2, (await admin.ListAsync(new(), true, default)).Total);
            Assert.True((await admin.GetAsync(draft.Id, true, default)).Data.GetProperty("needsReview").GetBoolean());

            var legacyUser = new GdscSharingPlatform.Infrastructure.Identity.ApplicationUser
            { Id = Guid.NewGuid(), FullName = "Existing seed user", Email = "admin@gdsc.com", NormalizedEmail = "ADMIN@GDSC.COM" };
            db.Add(legacyUser);
            await db.SaveChangesAsync();

            await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
                { ["SeedAdmin:Enabled"] = "false", ["SeedMember:Enabled"] = "false" }));
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                    services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
                    services.AddDbContext<ApplicationDbContext>(o => o.UseNpgsql(postgres.ConnectionString));
                });
            });
            using var client = factory.CreateClient();
            Assert.True(await db.Users.AnyAsync(u => u.Id == legacyUser.Id));
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/interview-questions")).StatusCode);
            using var scope = factory.Services.CreateScope();
            var tokens = scope.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
            string Token(string role) => tokens.GenerateAccessToken(Guid.NewGuid(), "interview@test.local", "Tester", [role], null, "Active", 1).Token;
            client.DefaultRequestHeaders.Authorization = new("Bearer", Token(RoleNames.Member));
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/v1/interview-questions?department=backend&topic=postgresql")).StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/v1/interview-questions?pageSize=101")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/v1/interview-questions/{draft.Id}")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/admin/interview-questions")).StatusCode);
            client.DefaultRequestHeaders.Authorization = new("Bearer", Token(RoleNames.Admin));
            var response = await client.GetAsync($"/api/v1/admin/interview-questions/{draft.Id}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(draft.Id, (await response.Content.ReadFromJsonAsync<InterviewQuestionDetail>())!.Id);
        }
        finally { await postgres.DropAsync(); }
    }
    private static InterviewQuestion NewQuestion(string status, string question, bool review)
    {
        var id = Guid.NewGuid();
        return new() { Id = id, Status = status, Data = JsonSerializer.Serialize(new {
            id, question, slug = id.ToString(), level = "basic", access = "free", needsReview = review,
            departments = new[] { "backend" }, topics = new[] { "postgresql" }, answer = new { summary = new[] { "Sample" } }
        }) };
    }
    private sealed class User(string role) : ICurrentUserService
    {
        public Guid? UserId => Guid.Empty;
        public string? Email => null;
        public IReadOnlyCollection<string> Roles => [role];
        public bool IsAuthenticated => true;
    }
    private sealed class PostgresFactAttribute : FactAttribute
    {
        public PostgresFactAttribute()
        {
            if (PostgresTestDatabase.FromEnvironment() is null) Skip = "Requires ROADMAP_TEST_POSTGRES";
        }
    }
}
