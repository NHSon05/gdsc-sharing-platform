using GdscSharingPlatform.Domain.Enums;
using GdscSharingPlatform.Domain.Roadmaps;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GdscSharingPlatform.UnitTests.Infrastructure.Persistence;

public class RoadmapModelTests
{
    private static ApplicationDbContext CreateContext() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=model_only;Username=postgres")
            .Options);

    [Fact]
    public void AllRoadmapForeignKeys_RestrictDeletion()
    {
        using var context = CreateContext();
        Type[] types = [typeof(RoadmapCategory), typeof(Roadmap), typeof(RoadmapNode), typeof(RoadmapEdge), typeof(LearningResource)];
        foreach (var type in types)
        {
            var entity = context.Model.FindEntityType(type);
            Assert.NotNull(entity);
            Assert.Equal("gdsc", entity.GetSchema());
            Assert.All(entity.GetForeignKeys(), fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
        }
    }

    [Theory]
    [InlineData(typeof(RoadmapCategory), "Slug")]
    [InlineData(typeof(Roadmap), "Slug")]
    [InlineData(typeof(RoadmapNode), "RoadmapId,Slug")]
    [InlineData(typeof(RoadmapEdge), "RoadmapId,SourceNodeId,TargetNodeId,RelationType")]
    public void BusinessKeys_AreUnique(Type type, string properties)
    {
        using var context = CreateContext();
        var entity = context.Model.FindEntityType(type)!;
        Assert.Contains(entity.GetIndexes(), index => index.IsUnique && string.Join(',', index.Properties.Select(p => p.Name)) == properties);
    }

    [Theory]
    [InlineData(typeof(Roadmap), "Status,SortOrder")]
    [InlineData(typeof(Roadmap), "CategoryId,Status")]
    [InlineData(typeof(RoadmapNode), "RoadmapId,IsActive")]
    [InlineData(typeof(RoadmapEdge), "RoadmapId,IsActive")]
    [InlineData(typeof(LearningResource), "RoadmapNodeId,IsActive,SortOrder")]
    public void QueryIndexes_MatchDocumentedAccessPatterns(Type type, string properties)
    {
        using var context = CreateContext();
        Assert.Contains(context.Model.FindEntityType(type)!.GetIndexes(),
            index => string.Join(',', index.Properties.Select(p => p.Name)) == properties);
    }

    [Fact]
    public void EdgeForeignKeys_IncludeRoadmapForBothEndpoints()
    {
        using var context = CreateContext();
        var edge = context.Model.FindEntityType(typeof(RoadmapEdge))!;
        var fks = edge.GetForeignKeys().Where(fk => fk.PrincipalEntityType.ClrType == typeof(RoadmapNode)).ToList();
        Assert.Equal(2, fks.Count);
        Assert.All(fks, fk => Assert.Equal("RoadmapId", fk.Properties[0].Name));
        Assert.All(fks, fk => Assert.Equal(new[] { "RoadmapId", "Id" }, fk.PrincipalKey.Properties.Select(p => p.Name)));
    }

    [Fact]
    public void RoadmapDefault_DoesNotOverrideExplicitDraft()
    {
        using var context = CreateContext();
        var property = context.Model.FindEntityType(typeof(Roadmap))!.FindProperty(nameof(Roadmap.Status))!;
        Assert.Equal(RoadmapStatus.Published, property.GetDefaultValue());
        Assert.NotEqual(RoadmapStatus.Draft, property.Sentinel);
    }

    [Fact]
    public void MigrationSnapshot_MatchesCurrentModel()
    {
        using var context = CreateContext();
        Assert.False(context.Database.HasPendingModelChanges());
    }
}
