using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace XIVWindowResizer.UI;

public class ConfigWindow : Window
{
    private static readonly IReadOnlyList<(string Label, VirtualKey Key)> KeyOptions = Enum
        .GetValues(typeof(VirtualKey))
        .Cast<VirtualKey>()
        .Where(k => k != VirtualKey.SHIFT && k != VirtualKey.CONTROL && k != VirtualKey.MENU && k != 0)
        .OrderBy(k => k.ToString())
        .Select(k => (k.ToString(), k))
        .ToList();

    private readonly Configuration _configuration;
    private readonly Action _saveConfiguration;
    private readonly Func<Size> _getCurrentSize;
    private readonly Func<Size> _getSavedSize;

    public ConfigWindow(
        Configuration configuration,
        Action saveConfiguration,
        Func<Size> getCurrentSize,
        Func<Size> getSavedSize,
        Dalamud.Plugin.Services.IKeyState keyState)
        : base("XIVWindowResizer 设置", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize)
    {
        _configuration = configuration;
        _saveConfiguration = saveConfiguration;
        _getCurrentSize = getCurrentSize;
        _getSavedSize = getSavedSize;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(340, 410),
            MaximumSize = new Vector2(340, 410)
        };
    }

    public override void Draw()
    {
        DrawToggles();
        ImGui.Separator();
        DrawCommandHelp();
        ImGui.Separator();
        DrawPresets();
        ImGui.Separator();
        DrawHotkeys();
        ImGui.Separator();
        DrawStatus();
    }

    private void DrawToggles()
    {
        bool enableHotkeys = _configuration.EnableHotkeys;
        if(ImGui.Checkbox("Enable hotkeys", ref enableHotkeys))
        {
            _configuration.EnableHotkeys = enableHotkeys;
            _saveConfiguration();
        }

        bool showChat = _configuration.ShowChatMessages;
        if(ImGui.Checkbox("Show chat message after execution", ref showChat))
        {
            _configuration.ShowChatMessages = showChat;
            _saveConfiguration();
        }
    }

    private void DrawCommandHelp()
    {
        ImGui.TextColored(new Vector4(0.7f, 0.8f, 1.0f, 1.0f), "Command overview");

        DrawCommandChip("/wresize set", "Set window size to specified resolution.\nExample: /wresize set 5120 2160");
        ImGui.SameLine(0, 20f);
        DrawCommandChip("/wresize reset", "Reset window size to startup size.\nThe size saved by the plugin when it was loaded or the last time the update command was used.");
        ImGui.SameLine(0, 20f);
        DrawCommandChip("/wresize update", "Record current window size as new startup size.\nUse this command with caution.");
    }

    private void DrawPresets()
    {
        if(ImGui.CollapsingHeader("Window size presets", ImGuiTreeNodeFlags.DefaultOpen))
        {
            DrawPresetTableRow("Preset A", _configuration.PresetA);
            DrawPresetTableRow("Preset B", _configuration.PresetB);
        }
    }

    private void DrawPresetTableRow(string label, ResolutionSelection selection)
    {
        string widthStr = selection.Width.ToString();
        string heightStr = selection.Height.ToString();
        var widthTextSize = ImGui.CalcTextSize(widthStr);
        var heightTextSize = ImGui.CalcTextSize(heightStr);
        float widthColumnWidth = Math.Max(50, widthTextSize.X + 8);
        float heightColumnWidth = Math.Max(50, heightTextSize.X + 8);

        if(ImGui.BeginTable($"table-{label}", 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("preset", ImGuiTableColumnFlags.WidthFixed, 200);
            ImGui.TableSetupColumn("width", ImGuiTableColumnFlags.WidthFixed, widthColumnWidth);
            ImGui.TableSetupColumn("multiply", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 20);
            ImGui.TableSetupColumn("height", ImGuiTableColumnFlags.WidthFixed, heightColumnWidth);

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            DrawPresetCombo(label, selection);

            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(-1);
            if(ImGui.InputText($"##width-{label}", ref widthStr, 10, ImGuiInputTextFlags.CharsDecimal))
            {
                if(int.TryParse(widthStr, out int width))
                {
                    selection.Width = Math.Max(1, width);
                    _saveConfiguration();
                }
            }
            if(ImGui.IsItemHovered())
                ImGui.SetTooltip("Width");

            ImGui.TableNextColumn();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetColumnWidth() - ImGui.CalcTextSize("×").X) * 0.5f);
            ImGui.Text("×");

            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(-1);
            if(ImGui.InputText($"##height-{label}", ref heightStr, 10, ImGuiInputTextFlags.CharsDecimal))
            {
                if(int.TryParse(heightStr, out int height))
                {
                    selection.Height = Math.Max(1, height);
                    _saveConfiguration();
                }
            }
            if(ImGui.IsItemHovered())
                ImGui.SetTooltip("Height");

            ImGui.EndTable();
        }
    }

    private void DrawPresetCombo(string label, ResolutionSelection selection)
    {
        var matched = ResolutionPresetCatalog.Match(selection.Width, selection.Height);
        string preview = $"{label} | {(matched?.Label ?? "Custom")}";

        ImGui.SetNextItemWidth(190);
        if(ImGui.BeginCombo($"##{label}", preview))
        {
            foreach(var preset in ResolutionPresetCatalog.Presets)
            {
                bool selected = matched != null && matched.Id == preset.Id;
                if(ImGui.Selectable(preset.Label, selected))
                {
                    selection.Width = preset.Width;
                    selection.Height = preset.Height;
                    _saveConfiguration();
                    matched = preset;
                }

                if(selected)
                    ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }
    }

    private void DrawHotkeys()
    {
        using var disabled = ImRaii.Disabled(!_configuration.EnableHotkeys);

        if(ImGui.CollapsingHeader("Hotkeys", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if(ImGui.BeginTable("hotkeys", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg))
            {
                ImGui.TableSetupColumn("binding", ImGuiTableColumnFlags.WidthFixed, 220);
                ImGui.TableSetupColumn("desc", ImGuiTableColumnFlags.WidthStretch);

                DrawHotkeyRow("Set resolution A", _configuration.HotkeyPresetA);
                DrawHotkeyRow("Set resolution B", _configuration.HotkeyPresetB);
                DrawHotkeyRow("Reset to startup size", _configuration.HotkeyReset);
                DrawHotkeyRow("Update to startup size", _configuration.HotkeyUpdate);

                ImGui.EndTable();
            }
        }
    }

    private void DrawHotkeyRow(string description, HotkeyBinding binding)
    {
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        DrawBindingControls(binding, description);

        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.TextWrapped(description);
    }

    private void DrawBindingControls(HotkeyBinding binding, string id)
    {
        if(ImGui.BeginTable($"bind-{id}", 5, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Ctrl");
            ImGui.TableSetupColumn("Alt");
            ImGui.TableSetupColumn("Shift");
            ImGui.TableSetupColumn("Reset");
            ImGui.TableSetupColumn("Key", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            bool ctrl = binding.Ctrl;
            ImGui.PushID($"ctrl-{id}");
            if(ImGui.Checkbox("##ctrl", ref ctrl))
            {
                binding.Ctrl = ctrl;
                _saveConfiguration();
            }
            if(ImGui.IsItemHovered())
                ImGui.SetTooltip("Ctrl");
            ImGui.PopID();

            ImGui.TableNextColumn();
            bool alt = binding.Alt;
            ImGui.PushID($"alt-{id}");
            if(ImGui.Checkbox("##alt", ref alt))
            {
                binding.Alt = alt;
                _saveConfiguration();
            }
            if(ImGui.IsItemHovered())
                ImGui.SetTooltip("Alt");
            ImGui.PopID();

            ImGui.TableNextColumn();
            bool shift = binding.Shift;
            ImGui.PushID($"shift-{id}");
            if(ImGui.Checkbox("##shift", ref shift))
            {
                binding.Shift = shift;
                _saveConfiguration();
            }
            if(ImGui.IsItemHovered())
                ImGui.SetTooltip("Shift");
            ImGui.PopID();

            ImGui.TableNextColumn();
            ImGui.AlignTextToFramePadding();
            ImGui.PushID($"reset-{id}");
            if(ImGui.Button("Reset"))
            {
                binding.Clear();
                _saveConfiguration();
            }
            if(ImGui.IsItemHovered())
                ImGui.SetTooltip("Clear current binding");
            ImGui.PopID();

            ImGui.TableNextColumn();
            string preview = binding.IsUnset ? "Unset" : binding.Key.ToString();
            ImGui.SetNextItemWidth(-1);
            ImGui.PushID($"key-{id}");
            if(ImGui.BeginCombo("##key", preview))
            {
                foreach(var (label, key) in KeyOptions)
                {
                    bool selected = binding.Key == key;
                    if(ImGui.Selectable(label, selected))
                    {
                        binding.Key = key;
                        _saveConfiguration();
                    }

                    if(selected)
                        ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }
            ImGui.PopID();

            ImGui.EndTable();
        }
    }

    private void DrawCommandChip(string text, string tooltip)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(text);
        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.Text(FontAwesomeIcon.InfoCircle.ToIconString());
        ImGui.PopFont();
        if(ImGui.IsItemHovered())
            ImGui.SetTooltip(tooltip);
    }

    private void DrawStatus()
    {
        var current = _getCurrentSize();
        var saved = _getSavedSize();

        ImGui.TextColored(new Vector4(0.7f, 0.8f, 1.0f, 1.0f), "Status");
        ImGui.Text($"Current window size: {current.Width} x {current.Height}");
        ImGui.Text($"Startup size: {saved.Width} x {saved.Height}");
    }
}

