using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Memberships.Interfaces;
using GdscSharingPlatform.Application.Features.Memberships.Models;
using GdscSharingPlatform.Domain.Memberships;
using GdscSharingPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GdscSharingPlatform.Infrastructure.Services;

public sealed class GenerationService : IGenerationService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<GenerationService> _logger;

    public GenerationService(
        ApplicationDbContext dbContext,
        ILogger<GenerationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<GenerationDto> CreateGenerationAsync(
        CreateGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var numberExists = await _dbContext.ClubGenerations
            .AnyAsync(g => g.Number == request.Number, cancellationToken);

        if (numberExists)
        {
            throw new ConflictException($"Generation number {request.Number} already exists.");
        }

        var generation = new ClubGeneration(request.Number, request.StartDate, request.EndDate);

        _dbContext.ClubGenerations.Add(generation);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created generation {GenerationId} - Gen {Number}", generation.Id, generation.Number);

        return MapToDto(generation);
    }

    public async Task<GenerationDto> UpdateGenerationAsync(
        Guid generationId,
        UpdateGenerationRequest request,
        CancellationToken cancellationToken = default)
    {
        var generation = await _dbContext.ClubGenerations
            .SingleOrDefaultAsync(g => g.Id == generationId, cancellationToken);

        if (generation is null)
        {
            throw new NotFoundException(nameof(ClubGeneration), generationId);
        }

        generation.Update(generation.Number, request.StartDate, request.EndDate);

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
            {
                generation.Activate();
            }
            else
            {
                generation.Deactivate();
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated generation {GenerationId} - Gen {Number}", generation.Id, generation.Number);

        return MapToDto(generation);
    }

    public async Task DeactivateGenerationAsync(
        Guid generationId,
        CancellationToken cancellationToken = default)
    {
        var generation = await _dbContext.ClubGenerations
            .SingleOrDefaultAsync(g => g.Id == generationId, cancellationToken);

        if (generation is null)
        {
            throw new NotFoundException(nameof(ClubGeneration), generationId);
        }

        generation.Deactivate();
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deactivated generation {GenerationId} - Gen {Number}", generation.Id, generation.Number);
    }

    private static GenerationDto MapToDto(ClubGeneration g) =>
        new(g.Id, g.Number, g.Name, g.StartDate, g.EndDate, g.IsActive);
}
