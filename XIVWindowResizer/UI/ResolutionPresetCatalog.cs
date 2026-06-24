using System.Collections.Generic;
using System.Linq;

namespace XIVWindowResizer.UI;

public record ResolutionPreset(string Id, string Name, string AspectRatio, int Width, int Height)
{
    public string ResolutionLabel => $"{Width}×{Height}";
}

public static class ResolutionPresetCatalog
{
    private static readonly List<ResolutionPreset> PresetsInternal = new()
    {
        // 16:9
        new ResolutionPreset("fhd-1080p", "Full HD", "16:9", 1920, 1080),
        new ResolutionPreset("qhd-1440p", "QHD", "16:9", 2560, 1440),
        new ResolutionPreset("qhd-plus", "QHD+", "16:9", 3200, 1800),
        new ResolutionPreset("4k-uhd", "4K UHD", "16:9", 3840, 2160),
        new ResolutionPreset("5k", "5K", "16:9", 5120, 2880),
        new ResolutionPreset("8k-uhd", "8K UHD", "16:9", 7680, 4320),

        // 16:10
        new ResolutionPreset("wuxga", "WUXGA", "16:10", 1920, 1200),
        new ResolutionPreset("wqxga", "WQXGA", "16:10", 2560, 1600),
        new ResolutionPreset("wquxga", "WQUXGA", "16:10", 3840, 2400),

        // 21:9
        new ResolutionPreset("uw-fhd", "UW-FHD", "21:9", 2560, 1080),
        new ResolutionPreset("uw-qhd", "UW-QHD", "21:9", 3440, 1440),
        new ResolutionPreset("uw-4k", "UW-4K", "21:9", 3840, 1600),
        new ResolutionPreset("uw-5k", "UW-5K", "21:9", 5120, 2160),

        // 32:9
        new ResolutionPreset("dfhd", "DFHD", "32:9", 3840, 1080),
        new ResolutionPreset("dqhd", "DQHD", "32:9", 5120, 1440),
        new ResolutionPreset("duhd", "DUHD", "32:9", 7680, 2160),
    };

    public static IReadOnlyList<ResolutionPreset> Presets => PresetsInternal;

    public static ResolutionPreset? Match(int width, int height)
    {
        return PresetsInternal.FirstOrDefault(p => p.Width == width && p.Height == height);
    }

    public static string FormatResolution(int width, int height) => $"{width}×{height}";
}
