using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Dalamud.Plugin;

namespace XIVWindowResizer;

public enum Language
{
    English = 0,
    Chinese = 1
}

public class LocalizationStrings
{
    public string SettingsTitle { get; set; } = "XIVWindowResizer Settings";
    public string EnableHotkeys { get; set; } = "Enable hotkeys";
    public string LanguageLabel { get; set; } = "Language";
    public string LanguageTooltip { get; set; } = "Choose display language";
    public string ShowChatMessages { get; set; } = "Show chat message after execution";
    public string PassHotkeysToGame { get; set; } = "Prevent hotkeys from reaching the game";
    public string PassHotkeysToGameTooltip { get; set; } = "When enabled, hotkeys are consumed by the plugin and not sent to the game.";
    public string ApplyRenderResolutionOnly { get; set; } = "Keep image inside window";
    public string ApplyRenderResolutionOnlyTooltip { get; set; } = "Only change the game's internal render resolution and leave the actual window size unchanged. This keeps oversized resolutions inside the current window.";
    public string LockPresetAspectRatio { get; set; } = "Lock preset aspect ratio";
    public string LockPresetAspectRatioTooltip { get; set; } = "When enabled, preset heights are temporarily adjusted to match the selected aspect ratio.";
    public string PresetAspectRatio { get; set; } = "Aspect ratio";
    public string PresetAspectRatioTooltip { get; set; } = "Choose the aspect ratio used to recalculate preset heights.";
    public string CommandOverview { get; set; } = "Command overview";
    public string CommandSetLabel { get; set; } = "/wresize set";
    public string CommandSetTooltip { get; set; } = "Set window size to specified resolution.\nExample: /wresize set 5120 2160";
    public string CommandResetLabel { get; set; } = "/wresize reset";
    public string CommandResetTooltip { get; set; } = "Reset window size to startup size.\nThe size saved by the plugin when it was loaded or the last time the update command was used.";
    public string CommandUpdateLabel { get; set; } = "/wresize update";
    public string CommandUpdateTooltip { get; set; } = "Record current window size as new startup size.\nUse this command with caution.";
    public string WindowSizePresets { get; set; } = "Window size presets";
    public string PresetA { get; set; } = "Preset A";
    public string PresetB { get; set; } = "Preset B";
    public string Width { get; set; } = "Width";
    public string Height { get; set; } = "Height";
    public string HeightLockedByAspectRatio { get; set; } = "Height is calculated from the selected aspect ratio.";
    public string CustomPreset { get; set; } = "Custom";
    public string Hotkeys { get; set; } = "Hotkeys";
    public string HotkeyPresetADesc { get; set; } = "Set resolution A";
    public string HotkeyPresetBDesc { get; set; } = "Set resolution B";
    public string HotkeyResetDesc { get; set; } = "Reset to startup size";
    public string HotkeyUpdateDesc { get; set; } = "Update to startup size";
    public string HotkeyUpdateRiskAcknowledgeTooltip { get; set; } =
        "This hotkey overwrites the saved “startup size” with the current window size. The previous value is not kept. You can still set an exact size with /wresize set or presets.";
    public string HotkeyUpdateHotkeyBlocked { get; set; } =
        "“Update startup size” hotkey is disabled. Open plugin settings, read the notice, and enable the checkbox to use it.";
    public string Ctrl { get; set; } = "Ctrl";
    public string Alt { get; set; } = "Alt";
    public string Shift { get; set; } = "Shift";
    public string ResetBindingButton { get; set; } = "Reset";
    public string ClearBindingTooltip { get; set; } = "Clear current binding";
    public string HotkeyInputTooltip { get; set; } = "Click and press a key.";
    public string KeyLabel { get; set; } = "Key";
    public string Unset { get; set; } = "Unset";
    public string Status { get; set; } = "Status";
    public string CurrentWindowSize { get; set; } = "Current window size: {0} x {1}";
    public string StartupSize { get; set; } = "Startup size: {0} x {1}";
    public string UnknownCommand { get; set; } = "Unknown command: {0}";
    public string UsageSet { get; set; } = "Usage: /wresize set <width> <height>";
    public string InvalidWidthHeight { get; set; } = "Invalid width or height";
    public string ResetToSavedSize { get; set; } = "Window size is reset to the saved startup size.";
    public string UpdatedSavedSize { get; set; } = "Updated saved window size to {0}x{1}";
    public string UpdateFailed { get; set; } = "Unable to update window size: {0}";
    public string SetWindowSizeSuccess { get; set; } = "Window size is set to {0}x{1}";
    public string SetWindowSizeFailed { get; set; } = "Unable to set window size: {0}";
    public string HelpMessage { get; set; } =
        "Open config window.\r\nUsage:\r\n/wresize set <width> <height> - Set window size.\r\n/wresize reset - Reset window size back to the original size.\r\n/wresize update - Update window size used by /wresize reset command. Use if you have changed game's screen resolution without restarting the game or reloading the plugin.";

    public LocalizationStrings Clone()
    {
        return new LocalizationStrings
        {
            SettingsTitle = SettingsTitle,
            EnableHotkeys = EnableHotkeys,
            LanguageLabel = LanguageLabel,
            LanguageTooltip = LanguageTooltip,
            ShowChatMessages = ShowChatMessages,
            PassHotkeysToGame = PassHotkeysToGame,
            PassHotkeysToGameTooltip = PassHotkeysToGameTooltip,
            ApplyRenderResolutionOnly = ApplyRenderResolutionOnly,
            ApplyRenderResolutionOnlyTooltip = ApplyRenderResolutionOnlyTooltip,
            LockPresetAspectRatio = LockPresetAspectRatio,
            LockPresetAspectRatioTooltip = LockPresetAspectRatioTooltip,
            PresetAspectRatio = PresetAspectRatio,
            PresetAspectRatioTooltip = PresetAspectRatioTooltip,
            CommandOverview = CommandOverview,
            CommandSetLabel = CommandSetLabel,
            CommandSetTooltip = CommandSetTooltip,
            CommandResetLabel = CommandResetLabel,
            CommandResetTooltip = CommandResetTooltip,
            CommandUpdateLabel = CommandUpdateLabel,
            CommandUpdateTooltip = CommandUpdateTooltip,
            WindowSizePresets = WindowSizePresets,
            PresetA = PresetA,
            PresetB = PresetB,
            Width = Width,
            Height = Height,
            HeightLockedByAspectRatio = HeightLockedByAspectRatio,
            CustomPreset = CustomPreset,
            Hotkeys = Hotkeys,
            HotkeyPresetADesc = HotkeyPresetADesc,
            HotkeyPresetBDesc = HotkeyPresetBDesc,
            HotkeyResetDesc = HotkeyResetDesc,
            HotkeyUpdateDesc = HotkeyUpdateDesc,
            HotkeyUpdateRiskAcknowledgeTooltip = HotkeyUpdateRiskAcknowledgeTooltip,
            HotkeyUpdateHotkeyBlocked = HotkeyUpdateHotkeyBlocked,
            Ctrl = Ctrl,
            Alt = Alt,
            Shift = Shift,
            ResetBindingButton = ResetBindingButton,
            ClearBindingTooltip = ClearBindingTooltip,
            HotkeyInputTooltip = HotkeyInputTooltip,
            KeyLabel = KeyLabel,
            Unset = Unset,
            Status = Status,
            CurrentWindowSize = CurrentWindowSize,
            StartupSize = StartupSize,
            UnknownCommand = UnknownCommand,
            UsageSet = UsageSet,
            InvalidWidthHeight = InvalidWidthHeight,
            ResetToSavedSize = ResetToSavedSize,
            UpdatedSavedSize = UpdatedSavedSize,
            UpdateFailed = UpdateFailed,
            SetWindowSizeSuccess = SetWindowSizeSuccess,
            SetWindowSizeFailed = SetWindowSizeFailed,
            HelpMessage = HelpMessage
        };
    }
}

public static class LocalizationManager
{
    private static readonly Dictionary<Language, string> LanguageFileCodes = new()
    {
        [Language.English] = "en",
        [Language.Chinese] = "zh"
    };

    private static LocalizationStrings _fallback = new();

    public static LocalizationStrings Strings { get; private set; } = new();
    public static Language CurrentLanguage { get; private set; } = Language.English;

    private static string _basePath = string.Empty;

    public static void Initialize(IDalamudPluginInterface pluginInterface, Language language)
    {
        _basePath = pluginInterface.AssemblyLocation.Directory?.FullName ?? string.Empty;
        SetLanguage(language);
    }

    public static void SetLanguage(Language language)
    {
        var path = GetFilePath(language);
        var loaded = LoadFromFile(path);
        Strings = MergeStrings(_fallback, loaded);
        CurrentLanguage = language;
    }

    public static string GetLanguageName(Language language)
    {
        return language switch
        {
            Language.Chinese => "简体中文",
            _ => "English"
        };
    }

    private static string GetFilePath(Language language)
    {
        if(string.IsNullOrEmpty(_basePath))
            return string.Empty;

        string code = LanguageFileCodes.TryGetValue(language, out string? fileCode)
            ? fileCode
            : "en";

        return Path.Combine(_basePath, "Localization", $"strings.{code}.json");
    }

    private static LocalizationStrings? LoadFromFile(string path)
    {
        if(string.IsNullOrEmpty(path) || !File.Exists(path))
            return null;

        try
        {
            string json = File.ReadAllText(path);
            var parsed = JsonSerializer.Deserialize<LocalizationStrings>(json);
            return parsed;
        }
        catch
        {
            return null;
        }
    }

    private static LocalizationStrings MergeStrings(LocalizationStrings defaults, LocalizationStrings? overrides)
    {
        var result = defaults.Clone();
        if(overrides == null)
            return result;

        result.SettingsTitle = Coalesce(result.SettingsTitle, overrides.SettingsTitle);
        result.EnableHotkeys = Coalesce(result.EnableHotkeys, overrides.EnableHotkeys);
        result.LanguageLabel = Coalesce(result.LanguageLabel, overrides.LanguageLabel);
        result.LanguageTooltip = Coalesce(result.LanguageTooltip, overrides.LanguageTooltip);
        result.ShowChatMessages = Coalesce(result.ShowChatMessages, overrides.ShowChatMessages);
        result.PassHotkeysToGame = Coalesce(result.PassHotkeysToGame, overrides.PassHotkeysToGame);
        result.PassHotkeysToGameTooltip = Coalesce(result.PassHotkeysToGameTooltip, overrides.PassHotkeysToGameTooltip);
        result.ApplyRenderResolutionOnly = Coalesce(result.ApplyRenderResolutionOnly, overrides.ApplyRenderResolutionOnly);
        result.ApplyRenderResolutionOnlyTooltip = Coalesce(result.ApplyRenderResolutionOnlyTooltip, overrides.ApplyRenderResolutionOnlyTooltip);
        result.LockPresetAspectRatio = Coalesce(result.LockPresetAspectRatio, overrides.LockPresetAspectRatio);
        result.LockPresetAspectRatioTooltip = Coalesce(result.LockPresetAspectRatioTooltip, overrides.LockPresetAspectRatioTooltip);
        result.PresetAspectRatio = Coalesce(result.PresetAspectRatio, overrides.PresetAspectRatio);
        result.PresetAspectRatioTooltip = Coalesce(result.PresetAspectRatioTooltip, overrides.PresetAspectRatioTooltip);
        result.CommandOverview = Coalesce(result.CommandOverview, overrides.CommandOverview);
        result.CommandSetLabel = Coalesce(result.CommandSetLabel, overrides.CommandSetLabel);
        result.CommandSetTooltip = Coalesce(result.CommandSetTooltip, overrides.CommandSetTooltip);
        result.CommandResetLabel = Coalesce(result.CommandResetLabel, overrides.CommandResetLabel);
        result.CommandResetTooltip = Coalesce(result.CommandResetTooltip, overrides.CommandResetTooltip);
        result.CommandUpdateLabel = Coalesce(result.CommandUpdateLabel, overrides.CommandUpdateLabel);
        result.CommandUpdateTooltip = Coalesce(result.CommandUpdateTooltip, overrides.CommandUpdateTooltip);
        result.WindowSizePresets = Coalesce(result.WindowSizePresets, overrides.WindowSizePresets);
        result.PresetA = Coalesce(result.PresetA, overrides.PresetA);
        result.PresetB = Coalesce(result.PresetB, overrides.PresetB);
        result.Width = Coalesce(result.Width, overrides.Width);
        result.Height = Coalesce(result.Height, overrides.Height);
        result.HeightLockedByAspectRatio = Coalesce(result.HeightLockedByAspectRatio, overrides.HeightLockedByAspectRatio);
        result.CustomPreset = Coalesce(result.CustomPreset, overrides.CustomPreset);
        result.Hotkeys = Coalesce(result.Hotkeys, overrides.Hotkeys);
        result.HotkeyPresetADesc = Coalesce(result.HotkeyPresetADesc, overrides.HotkeyPresetADesc);
        result.HotkeyPresetBDesc = Coalesce(result.HotkeyPresetBDesc, overrides.HotkeyPresetBDesc);
        result.HotkeyResetDesc = Coalesce(result.HotkeyResetDesc, overrides.HotkeyResetDesc);
        result.HotkeyUpdateDesc = Coalesce(result.HotkeyUpdateDesc, overrides.HotkeyUpdateDesc);
        result.HotkeyUpdateRiskAcknowledgeTooltip = Coalesce(result.HotkeyUpdateRiskAcknowledgeTooltip, overrides.HotkeyUpdateRiskAcknowledgeTooltip);
        result.HotkeyUpdateHotkeyBlocked = Coalesce(result.HotkeyUpdateHotkeyBlocked, overrides.HotkeyUpdateHotkeyBlocked);
        result.Ctrl = Coalesce(result.Ctrl, overrides.Ctrl);
        result.Alt = Coalesce(result.Alt, overrides.Alt);
        result.Shift = Coalesce(result.Shift, overrides.Shift);
        result.ResetBindingButton = Coalesce(result.ResetBindingButton, overrides.ResetBindingButton);
        result.ClearBindingTooltip = Coalesce(result.ClearBindingTooltip, overrides.ClearBindingTooltip);
        result.HotkeyInputTooltip = Coalesce(result.HotkeyInputTooltip, overrides.HotkeyInputTooltip);
        result.KeyLabel = Coalesce(result.KeyLabel, overrides.KeyLabel);
        result.Unset = Coalesce(result.Unset, overrides.Unset);
        result.Status = Coalesce(result.Status, overrides.Status);
        result.CurrentWindowSize = Coalesce(result.CurrentWindowSize, overrides.CurrentWindowSize);
        result.StartupSize = Coalesce(result.StartupSize, overrides.StartupSize);
        result.UnknownCommand = Coalesce(result.UnknownCommand, overrides.UnknownCommand);
        result.UsageSet = Coalesce(result.UsageSet, overrides.UsageSet);
        result.InvalidWidthHeight = Coalesce(result.InvalidWidthHeight, overrides.InvalidWidthHeight);
        result.ResetToSavedSize = Coalesce(result.ResetToSavedSize, overrides.ResetToSavedSize);
        result.UpdatedSavedSize = Coalesce(result.UpdatedSavedSize, overrides.UpdatedSavedSize);
        result.UpdateFailed = Coalesce(result.UpdateFailed, overrides.UpdateFailed);
        result.SetWindowSizeSuccess = Coalesce(result.SetWindowSizeSuccess, overrides.SetWindowSizeSuccess);
        result.SetWindowSizeFailed = Coalesce(result.SetWindowSizeFailed, overrides.SetWindowSizeFailed);
        result.HelpMessage = Coalesce(result.HelpMessage, overrides.HelpMessage);

        return result;
    }

    private static string Coalesce(string fallback, string? candidate)
    {
        return string.IsNullOrWhiteSpace(candidate) ? fallback : candidate;
    }
}

