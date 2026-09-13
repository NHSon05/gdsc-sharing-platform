namespace GdscSharingPlatform.Infrastructure.Storage;

public sealed class RoadmapStorageOptions
{
    public const string SectionName = "RoadmapStorage";
    public const long MaximumFileBytes = 20 * 1024 * 1024;
    public const long MaximumRequestBytes = MaximumFileBytes + 1024 * 1024;
    public string RootPath { get; set; } = "App_Data/roadmap-resources";
    public long MaxFileBytes { get; set; } = MaximumFileBytes;
}
