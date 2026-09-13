using GdscSharingPlatform.Application.Common.Security;
using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Roadmaps;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GdscSharingPlatform.Infrastructure.Identity.Seeding;

public sealed partial class DatabaseSeeder
{
    public async Task SeedRoadmapsAsync(CancellationToken cancellationToken = default)
    {
        // Use a real administrator for the required author FK; never create a synthetic account.
        var authorId = await (from user in _dbContext.Users
                              join assignment in _dbContext.UserRoles on user.Id equals assignment.UserId
                              join role in _dbContext.Roles on assignment.RoleId equals role.Id
                              where role.Name == RoleNames.Admin && user.Status == UserStatus.Active && !user.IsDeleted
                              orderby user.Id
                              select (Guid?)user.Id).FirstOrDefaultAsync(cancellationToken);
        if (authorId is null)
        {
            _logger.LogInformation("Roadmap seed deferred: no active administrator exists.");
            return;
        }

        var categories = await _dbContext.RoadmapCategories.Where(x => x.IsActive)
            .ToDictionaryAsync(x => x.Slug, cancellationToken);
        var existing = await _dbContext.Roadmaps.Select(x => new { x.Id, x.Slug }).ToListAsync(cancellationToken);
        var count = 0;
        foreach (var seed in DefaultRoadmaps)
        {
            // Stable IDs also protect administrator changes to the original slug.
            if (existing.Any(x => x.Id == seed.Id || x.Slug == seed.Slug)) continue;
            if (!categories.TryGetValue(seed.Slug, out var category)) continue;

            var roadmap = new Roadmap(category.Id, seed.Title, seed.Slug, seed.Summary,
                RoadmapLevel.Beginner, authorId.Value, seed.SortOrder)
            {
                Id = seed.Id,
                Description = seed.Summary,
                Prerequisites = "Basic computer skills and a willingness to practice."
            };
            RoadmapNode? previous = null;
            for (var index = 0; index < seed.Topics.Length; index++)
            {
                var topic = seed.Topics[index];
                var node = new RoadmapNode(roadmap.Id, topic.Title, topic.Slug,
                    positionX: 240, positionY: 80 + index * 160, width: 280, sortOrder: index)
                {
                    Description = topic.Objective,
                    LearningObjectives = topic.Objective
                };
                roadmap.Nodes.Add(node);
                if (previous is not null)
                    roadmap.Edges.Add(new RoadmapEdge(roadmap.Id, previous.Id, node.Id, sortOrder: index - 1));
                previous = node;
            }
            _dbContext.Roadmaps.Add(roadmap);
            count++;
        }

        if (count == 0) return;
        // One SaveChanges transaction persists complete graphs or rolls back every insert.
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Seeded {Count} published roadmaps with nodes and edges.", count);
    }

    private sealed record SeedTopic(string Title, string Slug, string Objective);
    private sealed record SeedRoadmap(Guid Id, string Title, string Slug, string Summary, int SortOrder, SeedTopic[] Topics);

    private static readonly SeedRoadmap[] DefaultRoadmaps =
    [
        new(new Guid("c7300001-6b87-4d48-9812-000000000001"), "Frontend", "frontend",
            "Build accessible, responsive web interfaces from HTML and CSS to interactive applications.", 0,
            [new("HTML & Accessibility", "html-accessibility", "Structure a page with semantic HTML, labels and keyboard navigation."),
             new("CSS & Responsive Layout", "css-responsive-layout", "Create layouts with Flexbox and Grid that adapt to different screen sizes."),
             new("JavaScript Fundamentals", "javascript-fundamentals", "Work with functions, arrays, objects, events and asynchronous code."),
             new("React & API Integration", "react-api-integration", "Compose components, manage UI state and display data from an API."),
             new("Frontend Project", "frontend-project", "Build and test an accessible application with loading, empty and error states.")]),
        new(new Guid("c7300001-6b87-4d48-9812-000000000002"), "Backend", "backend",
            "Learn to build authenticated APIs, persist data and operate a backend application.", 1,
            [new("Programming Fundamentals", "programming-fundamentals", "Practice types, control flow, functions and error handling."),
             new("HTTP & API Design", "http-api-design", "Design routes, request validation, status codes and consistent error responses."),
             new("Persistence & Transactions", "persistence-transactions", "Store application data with constraints and atomic transactions."),
             new("Authentication & Authorization", "authentication-authorization", "Protect API operations with identity, roles and resource permissions."),
             new("Backend Project", "backend-project", "Build an API with integration tests, structured logs and deployment configuration.")]),
        new(new Guid("c7300001-6b87-4d48-9812-000000000003"), "Database", "database",
            "Model relational data, write SQL queries and understand database reliability and performance.", 2,
            [new("Relational Modeling", "relational-modeling", "Identify entities, keys and relationships, then normalize a schema."),
             new("SQL Fundamentals", "sql-fundamentals", "Read and modify data with SQL while using parameterized queries."),
             new("Joins & Aggregation", "joins-aggregation", "Combine tables and calculate grouped results."),
             new("Indexes & Query Plans", "indexes-query-plans", "Read execution plans and choose indexes for actual query patterns."),
             new("Transactions & Recovery", "transactions-recovery", "Explore isolation, concurrent updates, backups and restore procedures.")]),
        new(new Guid("c7300001-6b87-4d48-9812-000000000004"), "Software Engineering", "software-engineering",
            "Practice the foundations of designing, testing and maintaining software as a team.", 3,
            [new("Git & Collaboration", "git-collaboration", "Use branches, commits, pull requests and code review to collaborate."),
             new("Requirements & Modeling", "requirements-modeling", "Translate user needs into acceptance criteria and domain models."),
             new("Design & Architecture", "design-architecture", "Separate responsibilities and define clear dependency boundaries."),
             new("Automated Testing", "automated-testing", "Choose unit and integration tests that verify meaningful behavior."),
             new("Delivery & Maintenance", "delivery-maintenance", "Automate checks, release changes and investigate operational failures.")]),
        new(new Guid("c7300001-6b87-4d48-9812-000000000005"), "Fullstack", "fullstack",
            "Connect a web interface, an API and a database into a complete application.", 4,
            [new("Web Foundations", "web-foundations", "Understand the browser, HTTP and the request-response lifecycle."),
             new("User Interface", "user-interface", "Build accessible forms and responsive pages with explicit UI states."),
             new("API & Database", "api-database", "Implement validated API operations backed by a relational database."),
             new("End-to-End Integration", "end-to-end-integration", "Connect authentication, API calls and client cache updates."),
             new("Fullstack Project", "fullstack-project", "Deliver an application with end-to-end tests and deployment configuration.")]),
        new(new Guid("c7300001-6b87-4d48-9812-000000000006"), "Artificial Intelligence", "artificial-intelligence",
            "Explore Python, data preparation and the foundations of evaluating machine learning models.", 5,
            [new("Python Fundamentals", "python-fundamentals", "Practice Python functions, collections and working with data files."),
             new("Math & Statistics", "math-statistics", "Review vectors, probability and descriptive statistics."),
             new("Data Preparation", "data-preparation", "Clean data and separate training, validation and test sets without leakage."),
             new("Machine Learning Basics", "machine-learning-basics", "Train simple models and compare them against a baseline."),
             new("Evaluation & AI Project", "evaluation-ai-project", "Choose evaluation metrics, inspect errors and document model limitations.")])
    ];
}
