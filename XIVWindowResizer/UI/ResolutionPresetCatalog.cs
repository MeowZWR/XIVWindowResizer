using System.Collections.Generic;
using System.Linq;

namespace XIVWindowResizer.UI;

public record ResolutionPreset(string Id, string Label, int Width, int Height);

public static class ResolutionPresetCatalog
{
    private static readonly List<ResolutionPreset> PresetsInternal = new()
    {
        new ResolutionPreset("4k", "4K (3840x2160)", 3840, 2160),
        new ResolutionPreset("8k", "8K (7680x4320)", 7680, 4320)
    };

    public static IReadOnlyList<ResolutionPreset> Presets => PresetsInternal;

    public static ResolutionPreset? Match(int width, int height)
    {
        return PresetsInternal.FirstOrDefault(p => p.Width == width && p.Height == height);
    }
}

