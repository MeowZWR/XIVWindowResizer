using System;
using Dalamud.Configuration;
using Dalamud.Game.ClientState.Keys;

namespace XIVWindowResizer;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public bool EnableHotkeys { get; set; } = true;
    public bool ShowChatMessages { get; set; } = true;
    public bool PassHotkeysToGame { get; set; } = false;
    public bool ApplyRenderResolutionOnly { get; set; } = false;
    public bool LockPresetAspectRatio { get; set; } = false;
    public AspectRatioSelection PresetAspectRatio { get; set; } = AspectRatioSelection.Ratio16X9;
    public Language Language { get; set; } = Language.English;

    public ResolutionSelection PresetA { get; set; } = ResolutionSelection.Default4K();
    public ResolutionSelection PresetB { get; set; } = ResolutionSelection.Default4K();

    public HotkeyBinding HotkeyPresetA { get; set; } = new();
    public HotkeyBinding HotkeyPresetB { get; set; } = new();
    public HotkeyBinding HotkeyReset { get; set; } = new();
    public HotkeyBinding HotkeyUpdate { get; set; } = new();

    public int SavedWidth { get; set; }
    public int SavedHeight { get; set; }
}

[Serializable]
public enum AspectRatioSelection
{
    Ratio16X9 = 0,
    Ratio16X10 = 1,
    Ratio21X9 = 2,
    Ratio32X9 = 3
}

public static class AspectRatioHelper
{
    public static int CalculateHeight(int width, AspectRatioSelection selection)
    {
        var (widthRatio, heightRatio) = GetParts(selection);
        return Math.Max(1, (int)Math.Round(width * heightRatio / (double)widthRatio));
    }

    public static string GetLabel(AspectRatioSelection selection)
    {
        var (width, height) = GetParts(selection);
        return $"{width}:{height}";
    }

    public static (int Width, int Height) GetParts(AspectRatioSelection selection)
    {
        return selection switch
        {
            AspectRatioSelection.Ratio16X10 => (16, 10),
            AspectRatioSelection.Ratio21X9 => (21, 9),
            AspectRatioSelection.Ratio32X9 => (32, 9),
            _ => (16, 9)
        };
    }
}

[Serializable]
public class ResolutionSelection
{
    public int Width { get; set; }
    public int Height { get; set; }

    public ResolutionSelection()
    {
    }

    public ResolutionSelection(int width, int height)
    {
        Width = width;
        Height = height;
    }

    public static ResolutionSelection Default4K()
    {
        return new ResolutionSelection(3840, 2160);
    }

    public void ClampToMinimum(int minimum)
    {
        if(Width < minimum)
            Width = minimum;
        if(Height < minimum)
            Height = minimum;
    }
}

[Serializable]
public class HotkeyBinding
{
    public VirtualKey Key { get; set; }
    public bool Ctrl { get; set; }
    public bool Shift { get; set; }
    public bool Alt { get; set; }

    public bool IsUnset => Key == 0;

    public void Clear()
    {
        Key = 0;
        Ctrl = false;
        Shift = false;
        Alt = false;
    }
}

