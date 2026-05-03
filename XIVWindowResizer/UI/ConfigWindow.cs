using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace XIVWindowResizer.UI;

public class ConfigWindow : Window
{
    private static readonly Language[] LanguageOptions = Enum.GetValues(typeof(Language)).Cast<Language>().ToArray();
    private static readonly AspectRatioSelection[] AspectRatioOptions = Enum.GetValues(typeof(AspectRatioSelection)).Cast<AspectRatioSelection>().ToArray();
    private const string WindowId = "XIVWindowResizerConfig";
    private const float PresetComboWidth = 180f;
    private const float HotkeyInputWidth = 75f;
    private const float ToggleSwitchWidth = 32f;

    private readonly Configuration _configuration;
    private readonly Action _saveConfiguration;
    private readonly Func<Size> _getCurrentSize;
    private readonly Func<Size> _getSavedSize;
    private readonly Action<Language> _onLanguageChanged;
    private readonly IKeyState _keyState;
    private readonly Dictionary<VirtualKey, bool> _captureState = new();
    private LocalizationStrings L => LocalizationManager.Strings;
    private float Scale(float value) => value * ImGui.GetIO().FontGlobalScale;
    private Vector2 Scale(Vector2 value) => value * ImGui.GetIO().FontGlobalScale;

    public ConfigWindow(
        Configuration configuration,
        Action saveConfiguration,
        Func<Size> getCurrentSize,
        Func<Size> getSavedSize,
        Action<Language> onLanguageChanged,
        IKeyState keyState)
        : base($"{LocalizationManager.Strings.SettingsTitle}###{WindowId}", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize)
    {
        _configuration = configuration;
        _saveConfiguration = saveConfiguration;
        _getCurrentSize = getCurrentSize;
        _getSavedSize = getSavedSize;
        _onLanguageChanged = onLanguageChanged;
        _keyState = keyState;
    }

    public override void Draw()
    {
        ApplyLayoutSizing();
        WindowName = $"{L.SettingsTitle}###{WindowId}";
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
        float comboWidth = CalculateLanguageComboWidth();
        float labelWidth = ImGui.CalcTextSize(L.LanguageLabel).X;
        float langColumnWidth = labelWidth + ImGui.GetStyle().ItemSpacing.X + comboWidth;

        string topTableId = $"toggles-top-{ImGui.GetIO().FontGlobalScale:F3}";
        if(ImGui.BeginTable(topTableId, 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("left", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("lang", ImGuiTableColumnFlags.WidthFixed, langColumnWidth);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            bool enableHotkeys = _configuration.EnableHotkeys;
            if(ImGui.Checkbox(L.EnableHotkeys, ref enableHotkeys))
            {
                _configuration.EnableHotkeys = enableHotkeys;
                _saveConfiguration();
            }

            ImGui.TableNextColumn();
            DrawLanguageCombo(comboWidth);
            ImGui.EndTable();
        }

        ImGui.Spacing();

        string bottomTableId = $"toggles-bottom-{ImGui.GetIO().FontGlobalScale:F3}";
        if(ImGui.BeginTable(bottomTableId, 1, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("single", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            bool showChat = _configuration.ShowChatMessages;
            if(DrawWrappedCheckbox(L.ShowChatMessages, ref showChat))
            {
                _configuration.ShowChatMessages = showChat;
                _saveConfiguration();
            }

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            bool passHotkeys = _configuration.PassHotkeysToGame;
            if(DrawWrappedCheckbox(L.PassHotkeysToGame, ref passHotkeys))
            {
                _configuration.PassHotkeysToGame = passHotkeys;
                _saveConfiguration();
            }
            if(ImGui.IsItemHovered())
                ImGui.SetTooltip(L.PassHotkeysToGameTooltip);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            bool applyRenderResolutionOnly = _configuration.ApplyRenderResolutionOnly;
            if(DrawWrappedCheckbox(L.ApplyRenderResolutionOnly, ref applyRenderResolutionOnly))
            {
                _configuration.ApplyRenderResolutionOnly = applyRenderResolutionOnly;
                _saveConfiguration();
            }
            if(ImGui.IsItemHovered())
                ImGui.SetTooltip(L.ApplyRenderResolutionOnlyTooltip);

            ImGui.EndTable();
        }
    }

    private float CalculateLanguageComboWidth()
    {
        float maxLabel = LanguageOptions
            .Select(option => LocalizationManager.GetLanguageName(option))
            .Select(text => ImGui.CalcTextSize(text).X)
            .DefaultIfEmpty(0f)
            .Max();
        float padding = Scale(30f);
        return Math.Max(Scale(140f), maxLabel + padding);
    }

    private void DrawLanguageCombo(float comboWidth)
    {
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(L.LanguageLabel);
        if(ImGui.IsItemHovered())
            ImGui.SetTooltip(L.LanguageTooltip);

        ImGui.SameLine();
        ImGui.SetNextItemWidth(comboWidth);
        Language language = _configuration.Language;
        string preview = LocalizationManager.GetLanguageName(language);
        if(ImGui.BeginCombo("##language", preview))
        {
            foreach(var option in LanguageOptions)
            {
                bool selected = language == option;
                string label = LocalizationManager.GetLanguageName(option);
                if(ImGui.Selectable(label, selected))
                {
                    _configuration.Language = option;
                    _onLanguageChanged(option);
                    _saveConfiguration();
                }

                if(selected)
                    ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }
        if(ImGui.IsItemHovered())
            ImGui.SetTooltip(L.LanguageTooltip);
    }

    private bool DrawWrappedCheckbox(string label, ref bool value)
    {
        float wrapX = ImGui.GetCursorPosX() + ImGui.GetColumnWidth();
        ImGui.PushTextWrapPos(wrapX);
        bool changed = ImGui.Checkbox(label, ref value);
        ImGui.PopTextWrapPos();
        return changed;
    }

    private void DrawCommandHelp()
    {
        ImGui.TextColored(new Vector4(0.7f, 0.8f, 1.0f, 1.0f), L.CommandOverview);

        DrawCommandChip(L.CommandSetLabel, L.CommandSetTooltip);
        ImGui.SameLine(0, Scale(20f));
        DrawCommandChip(L.CommandResetLabel, L.CommandResetTooltip);
        ImGui.SameLine(0, Scale(20f));
        DrawCommandChip(L.CommandUpdateLabel, L.CommandUpdateTooltip);
    }

    private void DrawPresets()
    {
        if(ImGui.CollapsingHeader(L.WindowSizePresets, ImGuiTreeNodeFlags.DefaultOpen))
        {
            DrawAspectRatioControls();
            ImGui.Spacing();
            DrawPresetTableRow(L.PresetA, _configuration.PresetA);
            DrawPresetTableRow(L.PresetB, _configuration.PresetB);
        }
    }

    private void DrawAspectRatioControls()
    {
        bool lockAspectRatio = _configuration.LockPresetAspectRatio;
        if(DrawWrappedCheckbox(L.LockPresetAspectRatio, ref lockAspectRatio))
        {
            _configuration.LockPresetAspectRatio = lockAspectRatio;
            _saveConfiguration();
        }
        if(ImGui.IsItemHovered())
            ImGui.SetTooltip(L.LockPresetAspectRatioTooltip);

        ImGui.SameLine();
        using var disabled = ImRaii.Disabled(!_configuration.LockPresetAspectRatio);
        ImGui.SetNextItemWidth(-1);

        var aspectRatio = _configuration.PresetAspectRatio;
        string preview = AspectRatioHelper.GetLabel(aspectRatio);
        if(ImGui.BeginCombo("##preset-aspect-ratio", preview))
        {
            foreach(var option in AspectRatioOptions)
            {
                bool selected = aspectRatio == option;
                if(ImGui.Selectable(AspectRatioHelper.GetLabel(option), selected))
                {
                    _configuration.PresetAspectRatio = option;
                    _saveConfiguration();
                }

                if(selected)
                    ImGui.SetItemDefaultFocus();
            }

            ImGui.EndCombo();
        }
        if(ImGui.IsItemHovered())
            ImGui.SetTooltip(L.PresetAspectRatioTooltip);
    }

    private void DrawPresetTableRow(string label, ResolutionSelection selection)
    {
        string widthStr = selection.Width.ToString();
        string heightStr = GetEffectivePresetHeight(selection).ToString();
        var widthTextSize = ImGui.CalcTextSize(widthStr);
        var heightTextSize = ImGui.CalcTextSize(heightStr);
        float widthColumnWidth = Math.Max(Scale(50f), widthTextSize.X + Scale(8f));
        float heightColumnWidth = Math.Max(Scale(50f), heightTextSize.X + Scale(8f));

        string tableId = $"table-{label}-{ImGui.GetIO().FontGlobalScale:F3}";
        if(ImGui.BeginTable(tableId, 4, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("preset", ImGuiTableColumnFlags.WidthFixed, Scale(PresetComboWidth));
            ImGui.TableSetupColumn("width", ImGuiTableColumnFlags.WidthFixed, widthColumnWidth);
            ImGui.TableSetupColumn("multiply", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, Scale(20f));
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
                ImGui.SetTooltip(L.Width);

            ImGui.TableNextColumn();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetColumnWidth() - ImGui.CalcTextSize("×").X) * 0.5f);
            ImGui.Text("×");

            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(-1);
            using(var disabled = ImRaii.Disabled(_configuration.LockPresetAspectRatio))
            {
                if(ImGui.InputText($"##height-{label}", ref heightStr, 10, ImGuiInputTextFlags.CharsDecimal))
                {
                    if(int.TryParse(heightStr, out int height))
                    {
                        selection.Height = Math.Max(1, height);
                        _saveConfiguration();
                    }
                }
            }
            if(ImGui.IsItemHovered())
                ImGui.SetTooltip(_configuration.LockPresetAspectRatio ? L.HeightLockedByAspectRatio : L.Height);

            ImGui.EndTable();
        }
    }

    private void DrawPresetCombo(string label, ResolutionSelection selection)
    {
        var matched = ResolutionPresetCatalog.Match(selection.Width, selection.Height);
        string preview = $"{label} | {(matched?.Label ?? L.CustomPreset)}";

        ImGui.SetNextItemWidth(Scale(PresetComboWidth));
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

    private int GetEffectivePresetHeight(ResolutionSelection selection)
    {
        return _configuration.LockPresetAspectRatio
            ? AspectRatioHelper.CalculateHeight(selection.Width, _configuration.PresetAspectRatio)
            : selection.Height;
    }

    private void DrawHotkeys()
    {
        using var disabled = ImRaii.Disabled(!_configuration.EnableHotkeys);

        if(ImGui.CollapsingHeader(L.Hotkeys, ImGuiTreeNodeFlags.DefaultOpen))
        {
            string hotkeyTableId = $"hotkeys-{ImGui.GetIO().FontGlobalScale:F3}";
            if(ImGui.BeginTable(hotkeyTableId, 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg))
            {
                float bindingWidth = CalculateBindingColumnWidth();
                ImGui.TableSetupColumn("binding", ImGuiTableColumnFlags.WidthFixed, bindingWidth);
                ImGui.TableSetupColumn("desc", ImGuiTableColumnFlags.WidthStretch);

                DrawHotkeyRow(L.HotkeyPresetADesc, _configuration.HotkeyPresetA);
                DrawHotkeyRow(L.HotkeyPresetBDesc, _configuration.HotkeyPresetB);
                DrawHotkeyRow(L.HotkeyResetDesc, _configuration.HotkeyReset);
                DrawHotkeyUpdateRow();

                ImGui.EndTable();
            }
        }
    }

    private void DrawHotkeyRow(string description, HotkeyBinding binding, bool disableBindingControls = false)
    {
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        using(var _ = ImRaii.Disabled(disableBindingControls))
            DrawBindingControls(binding, description);

        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(description);
    }

    private void DrawHotkeyUpdateRow()
    {
        ImGui.TableNextRow();

        bool updateRiskAck = _configuration.HotkeyUpdateRiskAcknowledged;
        ImGui.TableNextColumn();
        using(var _ = ImRaii.Disabled(!updateRiskAck))
            DrawBindingControls(_configuration.HotkeyUpdate, L.HotkeyUpdateDesc);

        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(L.HotkeyUpdateDesc);
        ImGui.SameLine(0, ImGui.GetStyle().ItemInnerSpacing.X);
        ImGui.AlignTextToFramePadding();
        ImGui.PushID("hotkey-update-risk");
        if(DrawToggleSwitch(ref updateRiskAck))
        {
            _configuration.HotkeyUpdateRiskAcknowledged = updateRiskAck;
            _saveConfiguration();
        }

        if(ImGui.IsItemHovered())
            ImGui.SetTooltip(L.HotkeyUpdateRiskAcknowledgeTooltip);

        ImGui.PopID();
    }

    private bool DrawToggleSwitch(ref bool value)
    {
        float w = Scale(ToggleSwitchWidth);
        float h = ImGui.GetFrameHeight();
        Vector2 pos = ImGui.GetCursorScreenPos();
        bool clicked = ImGui.InvisibleButton("##toggle", new Vector2(w, h));
        if(clicked)
            value = !value;

        float rounding = h * 0.5f;
        uint trackCol = ImGui.GetColorU32(ImGuiCol.FrameBg);

        var drawList = ImGui.GetWindowDrawList();
        drawList.AddRectFilled(pos, new Vector2(pos.X + w, pos.Y + h), trackCol, rounding);

        float pad = Math.Max(1.5f, h * 0.16f);
        float knobRadius = Math.Max(1f, (h - pad * 2f) * 0.5f);
        float knobCenterX = value ? pos.X + w - pad - knobRadius : pos.X + pad + knobRadius;
        float knobCenterY = pos.Y + h * 0.5f;
        uint knobCol = value
            ? ImGui.GetColorU32(ImGuiCol.CheckMark)
            : ImGui.GetColorU32(ImGuiCol.TextDisabled);
        drawList.AddCircleFilled(new Vector2(knobCenterX, knobCenterY), knobRadius, knobCol);

        return clicked;
    }

    private void DrawBindingControls(HotkeyBinding binding, string id)
    {
        if(ImGui.BeginTable($"bind-{id}", 5, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn(L.Ctrl);
            ImGui.TableSetupColumn(L.Alt);
            ImGui.TableSetupColumn(L.Shift);
            ImGui.TableSetupColumn(L.ResetBindingButton);
            ImGui.TableSetupColumn(L.KeyLabel, ImGuiTableColumnFlags.WidthFixed, Scale(HotkeyInputWidth));

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
            if(ImGui.Button(L.ResetBindingButton))
            {
                binding.Clear();
                _saveConfiguration();
            }
            if(ImGui.IsItemHovered())
                ImGui.SetTooltip(L.ClearBindingTooltip);
            ImGui.PopID();

            ImGui.TableNextColumn();
            DrawHotkeyInput(binding, id);

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

        ImGui.TextColored(new Vector4(0.7f, 0.8f, 1.0f, 1.0f), L.Status);
        ImGui.Text(string.Format(L.CurrentWindowSize, current.Width, current.Height));
        ImGui.Text(string.Format(L.StartupSize, saved.Width, saved.Height));
    }

    private void DrawHotkeyInput(HotkeyBinding binding, string id)
    {
        string preview = binding.IsUnset ? L.Unset : binding.Key.ToString();
        ImGui.SetNextItemWidth(Scale(HotkeyInputWidth));
        ImGui.PushID($"key-{id}");
        ImGui.InputText("##key", ref preview, 32, ImGuiInputTextFlags.ReadOnly | ImGuiInputTextFlags.NoHorizontalScroll);
        if(ImGui.IsItemHovered())
            ImGui.SetTooltip(L.HotkeyInputTooltip);

        if(ImGui.IsItemActivated())
            _captureState.Clear();

        if(ImGui.IsItemActive())
        {
            if(TryCaptureKey(out var captured))
            {
                binding.Key = captured;
                _saveConfiguration();
            }

            _keyState.ClearAll(); // prevent game input while capturing

            if(ImGui.IsKeyPressed(ImGuiKey.Escape))
            {
                binding.Clear();
                _saveConfiguration();
            }
        }

        ImGui.PopID();
    }

    private bool TryCaptureKey(out VirtualKey keyPressed)
    {
        foreach(var key in _keyState.GetValidVirtualKeys().OrderBy(k => k.ToString()))
        {
            if(IsModifierKey(key))
                continue;

            bool isDown = _keyState.IsVirtualKeyValid(key) && _keyState[key];
            bool wasDown = _captureState.TryGetValue(key, out bool prev) && prev;
            _captureState[key] = isDown;

            bool pressedNow = ImGui.IsKeyPressed(ImGuiHelpers.VirtualKeyToImGuiKey(key), false) || (isDown && !wasDown);

            if(pressedNow)
            {
                keyPressed = key;
                return true;
            }
        }

        keyPressed = default;
        return false;
    }

    private static bool IsModifierKey(VirtualKey key)
    {
        return key is VirtualKey.CONTROL
            or VirtualKey.MENU
            or VirtualKey.SHIFT
            or VirtualKey.LCONTROL
            or VirtualKey.RCONTROL
            or VirtualKey.LMENU
            or VirtualKey.RMENU
            or VirtualKey.LSHIFT
            or VirtualKey.RSHIFT;
    }

    private float CalculateBindingColumnWidth()
    {
        var style = ImGui.GetStyle();
        float checkboxWidth = ImGui.GetFrameHeight();
        float resetButtonWidth = ImGui.CalcTextSize(L.ResetBindingButton).X + style.FramePadding.X * 2f;
        float inputWidth = Scale(HotkeyInputWidth);

        float perCellPadding = style.CellPadding.X * 2f;
        float innerTablePadding = perCellPadding * 5;
        float outerCellPadding = style.CellPadding.X * 2f;

        float total = checkboxWidth * 3
                     + resetButtonWidth
                     + inputWidth
                     + innerTablePadding
                     + outerCellPadding;

        float minimum = Scale(HotkeyInputWidth + 60f);
        return Math.Max(total, minimum);
    }

    private void ApplyLayoutSizing()
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(0f, 0f),
            MaximumSize = Scale(new Vector2(700f, float.MaxValue))
        };
    }
}

