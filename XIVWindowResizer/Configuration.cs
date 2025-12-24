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

    public string ToDisplayString()
    {
        if(IsUnset)
            return LocalizationManager.Strings.Unset;

        string mods = string.Empty;
        if(Ctrl)
            mods += "Ctrl+";
        if(Shift)
            mods += "Shift+";
        if(Alt)
            mods += "Alt+";

        return $"{mods}{Key}";
    }
}

