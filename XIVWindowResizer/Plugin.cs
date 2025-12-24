using System;
using System.Collections.Generic;
using System.Drawing;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using XIVWindowResizer.Helpers;
using XIVWindowResizer.UI;

namespace XIVWindowResizer;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "XIVWindowResizer";
    private const string CommandName = "/wresize";

    [PluginService] private ICommandManager _commandManager { get; init; } = null!;
    [PluginService] private IChatGui _chatGui { get; init; } = null!;
    [PluginService] private IDalamudPluginInterface _pluginInterface { get; init; } = null!;
    [PluginService] private IFramework _framework { get; init; } = null!;
    [PluginService] private IKeyState _keyState { get; init; } = null!;

    private Size _originalWindowSize { get; set; }

    private WindowSizeHelper _windowSizeHelper { get; init; }
    private readonly WindowSystem _windowSystem;
    private readonly ConfigWindow _configWindow;
    private readonly Dictionary<VirtualKey, bool> _lastKeyState = new();
    private readonly List<HotkeyBinding> _hotkeyBindings;
    private Configuration _configuration;

    public Plugin()
    {
        _windowSizeHelper = new WindowSizeHelper(new WindowSearchHelper());
        _configuration = LoadConfiguration();
        EnsureSavedSize();
        _originalWindowSize = new Size(_configuration.SavedWidth, _configuration.SavedHeight);

        _hotkeyBindings = new List<HotkeyBinding>
        {
            _configuration.HotkeyPresetA,
            _configuration.HotkeyPresetB,
            _configuration.HotkeyReset,
            _configuration.HotkeyUpdate
        };

        _windowSystem = new WindowSystem(Name);
        _configWindow = new ConfigWindow(
            _configuration,
            SaveConfiguration,
            GetCurrentWindowSizeSafe,
            GetSavedSize,
            _keyState);

        _windowSystem.AddWindow(_configWindow);

        _pluginInterface.UiBuilder.Draw += DrawUI;
        _pluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;
        _pluginInterface.UiBuilder.OpenMainUi += OpenConfigUi;
        _framework.Update += OnFrameworkUpdate;

        _commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "打开设置界面。\r\nUsage:\r\n/wresize set <width> <height> - Set window size.\r\n/wresize reset - Reset window size back to the original size.\r\n/wresize update - Update window size used by /wresize reset command. Use if you have changed game's screen resolution without restarting the game or reloading the plugin."
        });
    }

    public void Dispose()
    {
        _framework.Update -= OnFrameworkUpdate;
        _pluginInterface.UiBuilder.Draw -= DrawUI;
        _pluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;
        _pluginInterface.UiBuilder.OpenMainUi -= OpenConfigUi;
        _commandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        string[] splitArgs = args.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if(splitArgs.Length == 0)
        {
            OpenConfigUi();
            return;
        }

        switch(splitArgs[0].ToLowerInvariant())
        {
            case "set":
                HandleSet(splitArgs);
                break;
            case "reset":
                ResetToSavedSize();
                break;
            case "update":
                UpdateSavedSize();
                break;
            default:
                PrintInChat($"Unknown command: {splitArgs[0]}");
                break;
        }
    }

    private void HandleSet(string[] splitArgs)
    {
        if(splitArgs.Length < 3)
        {
            PrintInChat("Usage: /wresize set <width> <height>");
            return;
        }

        if(!int.TryParse(splitArgs[1], out int width) || !int.TryParse(splitArgs[2], out int height))
        {
            PrintInChat("Invalid width or height");
            return;
        }

        ApplyWindowSize(width, height);
    }

    private void DrawUI()
    {
        _windowSystem.Draw();
    }

    private void OpenConfigUi()
    {
        _configWindow.IsOpen = !_configWindow.IsOpen;
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        if(!_configuration.EnableHotkeys)
        {
            SyncKeyStates();
            return;
        }

        ProcessHotkey(_configuration.HotkeyPresetA, () => ApplyPreset(_configuration.PresetA));
        ProcessHotkey(_configuration.HotkeyPresetB, () => ApplyPreset(_configuration.PresetB));
        ProcessHotkey(_configuration.HotkeyReset, ResetToSavedSize);
        ProcessHotkey(_configuration.HotkeyUpdate, UpdateSavedSize);

        SyncKeyStates();
    }

    private void ProcessHotkey(HotkeyBinding binding, Func<bool> action)
    {
        if(binding.IsUnset || binding.Key == 0)
            return;

        var key = binding.Key;
        bool isDown = _keyState[key];
        bool wasDown = _lastKeyState.TryGetValue(key, out bool previous) && previous;
        _lastKeyState[key] = isDown;

        if(!isDown || wasDown)
            return;

        if(binding.Ctrl && !_keyState[VirtualKey.CONTROL])
            return;
        if(binding.Shift && !_keyState[VirtualKey.SHIFT])
            return;
        if(binding.Alt && !_keyState[VirtualKey.MENU])
            return;

        action();
    }

    private void SyncKeyStates()
    {
        foreach(var binding in _hotkeyBindings)
        {
            if(binding.IsUnset || binding.Key == 0)
                continue;

            _lastKeyState[binding.Key] = _keyState[binding.Key];
        }
    }

    private bool ApplyPreset(ResolutionSelection selection)
    {
        return ApplyWindowSize(selection.Width, selection.Height);
    }

    private bool ResetToSavedSize()
    {
        return ApplyWindowSize(_configuration.SavedWidth, _configuration.SavedHeight, "Window size is reset to the saved startup size.");
    }

    private bool UpdateSavedSize()
    {
        try
        {
            var current = _windowSizeHelper.GetWindowSize();
            _configuration.SavedWidth = current.Width;
            _configuration.SavedHeight = current.Height;
            SaveConfiguration();

            _originalWindowSize = current;
            Notify($"Updated saved window size to {current.Width}x{current.Height}");
            return true;
        }
        catch(Exception ex)
        {
            Notify($"Unable to update window size: {ex.Message}");
            return false;
        }
    }

    private bool ApplyWindowSize(int width, int height, string? successMessage = null)
    {
        if(width <= 0 || height <= 0)
        {
            Notify("Invalid width or height");
            return false;
        }

        try
        {
            _windowSizeHelper.SetWindowSize(width, height);
            Notify(successMessage ?? $"Window size is set to {width}x{height}");
            return true;
        }
        catch(Exception ex)
        {
            Notify($"Unable to set window size: {ex.Message}");
            return false;
        }
    }

    private Size GetCurrentWindowSizeSafe()
    {
        try
        {
            return _windowSizeHelper.GetWindowSize();
        }
        catch
        {
            return new Size(_configuration.SavedWidth, _configuration.SavedHeight);
        }
    }

    private Size GetSavedSize()
    {
        return new Size(_configuration.SavedWidth, _configuration.SavedHeight);
    }

    private void Notify(string message)
    {
        if(_configuration.ShowChatMessages)
            PrintInChat(message);
    }

    private void PrintInChat(string message)
    {
        var xivChat = new XivChatEntry
        {
            Message = message
        };

        _chatGui.Print(xivChat);
    }

    private Configuration LoadConfiguration()
    {
        var config = _pluginInterface.GetPluginConfig() as Configuration;
        if(config != null)
            return config;

        config = new Configuration();
        _pluginInterface.SavePluginConfig(config);
        return config;
    }

    private void EnsureSavedSize()
    {
        if(_configuration.SavedWidth > 0 && _configuration.SavedHeight > 0)
            return;

        var initial = _windowSizeHelper.GetWindowSize();
        _configuration.SavedWidth = initial.Width;
        _configuration.SavedHeight = initial.Height;
        SaveConfiguration();
    }

    private void SaveConfiguration()
    {
        _pluginInterface.SavePluginConfig(_configuration);
    }
}

