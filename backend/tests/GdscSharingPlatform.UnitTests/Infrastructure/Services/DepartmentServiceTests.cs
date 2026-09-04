using GdscSharingPlatform.Application.Common.Exceptions;
using GdscSharingPlatform.Application.Features.Memberships.Models;
using GdscSharingPlatform.Infrastructure.Persistence;
using GdscSharingPlatform.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GdscSharingPlatform.UnitTests.Infrastructure.Services;

public class DepartmentServiceTests
{
    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task CreateDepartment_ValidRequest_ShouldCreate()
    {
        using var dbContext = CreateDbContext();
        var service = new DepartmentService(dbContext, NullLogger<DepartmentService>.Instance);

        var request = new CreateDepartmentRequest("Software", "software", "Description", "#000000", "icon", 1);
        var result = await service.CreateDepartmentAsync(request);

        Assert.Equal("Software", result.Name);
        Assert.Equal("software", result.Slug);
        Assert.True(result.IsActive);

        var inDb = await dbContext.Departments.SingleOrDefaultAsync(d => d.Id == result.Id);
        Assert.NotNull(inDb);
        Assert.Equal("SOFTWARE", inDb.Code);
    }

    [Fact]
    public async Task CreateDepartment_DuplicateName_ShouldThrowConflictException()
    {
        using var dbContext = CreateDbContext();
        var service = new DepartmentService(dbContext, NullLogger<DepartmentService>.Instance);

        await service.CreateDepartmentAsync(new CreateDepartmentRequest("Software", "software", null, null, null, 1));

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateDepartmentAsync(new CreateDepartmentRequest("SOFTWARE", "software-2", null, null, null, 2)));
    }

    [Fact]
    public async Task CreateDepartment_DuplicateSlug_ShouldThrowConflictException()
    {
        using var dbContext = CreateDbContext();
        var service = new DepartmentService(dbContext, NullLogger<DepartmentService>.Instance);

        await service.CreateDepartmentAsync(new CreateDepartmentRequest("Software", "software", null, null, null, 1));

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateDepartmentAsync(new CreateDepartmentRequest("Engineering", "software", null, null, null, 2)));
    }

    [Fact]
    public async Task DeactivateDepartment_ShouldSetIsActiveFalse()
    {
        using var dbContext = CreateDbContext();
        var service = new DepartmentService(dbContext, NullLogger<DepartmentService>.Instance);

        var created = await service.CreateDepartmentAsync(new CreateDepartmentRequest("Software", "software", null, null, null, 1));

        await service.DeactivateDepartmentAsync(created.Id);

        var inDb = await dbContext.Departments.SingleOrDefaultAsync(d => d.Id == created.Id);
        Assert.NotNull(inDb);
        Assert.False(inDb.IsActive);
        Assert.NotNull(inDb.DeletedAt);
    }

    [Fact]
    public async Task ActivateDepartment_ShouldSetIsActiveTrue()
    {
        using var dbContext = CreateDbContext();
        var service = new DepartmentService(dbContext, NullLogger<DepartmentService>.Instance);

        var created = await service.CreateDepartmentAsync(new CreateDepartmentRequest("Software", "software", null, null, null, 1));
        await service.DeactivateDepartmentAsync(created.Id);

        var activated = await service.ActivateDepartmentAsync(created.Id);

        Assert.True(activated.IsActive);
        var inDb = await dbContext.Departments.SingleOrDefaultAsync(d => d.Id == created.Id);
        Assert.NotNull(inDb);
        Assert.True(inDb.IsActive);
        Assert.Null(inDb.DeletedAt);
    }
}
