using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Memberships.Models;
using GdscSharingPlatform.Infrastructure.Persistence;
using GdscSharingPlatform.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GdscSharingPlatform.UnitTests.Infrastructure.Services;

public class GenerationServiceTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateGeneration_ValidRequest_ShouldCreate()
    {
        using var dbContext = CreateDbContext();
        var service = new GenerationService(dbContext, NullLogger<GenerationService>.Instance);

        var request = new CreateGenerationRequest(5, new DateOnly(2026, 9, 1), new DateOnly(2027, 8, 31));
        var result = await service.CreateGenerationAsync(request);

        Assert.Equal(5, result.Number);
        Assert.Equal("Gen 5", result.Name);
        Assert.True(result.IsActive);

        var inDb = await dbContext.ClubGenerations.SingleOrDefaultAsync(g => g.Id == result.Id);
        Assert.NotNull(inDb);
        Assert.Equal(5, inDb.Number);
    }

    [Fact]
    public async Task CreateGeneration_DuplicateNumber_ShouldThrowConflictException()
    {
        using var dbContext = CreateDbContext();
        var service = new GenerationService(dbContext, NullLogger<GenerationService>.Instance);

        await service.CreateGenerationAsync(new CreateGenerationRequest(5, null, null));

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateGenerationAsync(new CreateGenerationRequest(5, null, null)));
    }

    [Fact]
    public async Task DeactivateGeneration_ShouldSetIsActiveFalse()
    {
        using var dbContext = CreateDbContext();
        var service = new GenerationService(dbContext, NullLogger<GenerationService>.Instance);

        var created = await service.CreateGenerationAsync(new CreateGenerationRequest(5, null, null));

        await service.DeactivateGenerationAsync(created.Id);

        var inDb = await dbContext.ClubGenerations.SingleOrDefaultAsync(g => g.Id == created.Id);
        Assert.NotNull(inDb);
        Assert.False(inDb.IsActive);
    }
}
