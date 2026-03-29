using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Ben10Mod.Content.Transformations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Interface;

public sealed class PaletteByteSlider : UIElement {
    private bool _dragging;
    private int _value;

    public string Label { get; set; } = string.Empty;
    public Color AccentColor { get; set; } = Color.White;
    public bool IsInteractive { get; set; } = true;
    public Func<int, string> ValueFormatter { get; set; }
    public event Action<int> ValueChanged;

    public int Value => _value;

    public void SetValue(int value, bool invoke = true) {
        int clamped = Utils.Clamp(value, 0, 255);
        if (_value == clamped)
            return;

        _value = clamped;
        if (invoke)
            ValueChanged?.Invoke(_value);
    }

    public override void LeftMouseDown(UIMouseEvent evt) {
        base.LeftMouseDown(evt);
        if (!IsInteractive)
            return;

        _dragging = true;
        UpdateValueFromMouse(evt.MousePosition.X);
    }

    public override void LeftMouseUp(UIMouseEvent evt) {
        base.LeftMouseUp(evt);
        _dragging = false;
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        if (_dragging) {
            if (!Main.mouseLeft)
                _dragging = false;
            else
                UpdateValueFromMouse(Main.MouseScreen.X);
        }
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        CalculatedStyle dims = GetDimensions();
        Texture2D pixel = TextureAssets.MagicPixel.Value;

        Color border = IsInteractive ? new Color(110, 140, 160) : new Color(70, 80, 90);
        Color background = IsInteractive ? new Color(24, 30, 38, 215) : new Color(16, 18, 22, 180);
        Color fill = IsInteractive ? AccentColor : Color.Lerp(AccentColor, Color.Black, 0.5f);

        Rectangle outer = dims.ToRectangle();
        spriteBatch.Draw(pixel, outer, background);
        spriteBatch.Draw(pixel, new Rectangle(outer.X, outer.Y, outer.Width, 2), border);
        spriteBatch.Draw(pixel, new Rectangle(outer.X, outer.Bottom - 2, outer.Width, 2), border);
        spriteBatch.Draw(pixel, new Rectangle(outer.X, outer.Y, 2, outer.Height), border);
        spriteBatch.Draw(pixel, new Rectangle(outer.Right - 2, outer.Y, 2, outer.Height), border);

        Utils.DrawBorderString(spriteBatch, Label, new Vector2(dims.X + 10f, dims.Y + 6f),
            new Color(220, 230, 240), 0.8f);
        string valueText = ValueFormatter?.Invoke(_value) ?? _value.ToString();
        Utils.DrawBorderString(spriteBatch, valueText, new Vector2(dims.X + dims.Width - 10f, dims.Y + 6f),
            Color.White, 0.8f, 1f, 0f);

        Rectangle track = new Rectangle(outer.X + 10, outer.Y + 28, outer.Width - 20, 10);
        spriteBatch.Draw(pixel, track, new Color(10, 12, 16));

        int fillWidth = (int)Math.Round(track.Width * (_value / 255f));
        if (_value > 0 && fillWidth < 1)
            fillWidth = 1;

        if (fillWidth > 0)
            spriteBatch.Draw(pixel, new Rectangle(track.X, track.Y, fillWidth, track.Height), fill);

        int handleX = track.X + (int)Math.Round(track.Width * (_value / 255f));
        Rectangle handle = new Rectangle(handleX - 4, track.Y - 4, 8, track.Height + 8);
        spriteBatch.Draw(pixel, handle, Color.White);
    }

    private void UpdateValueFromMouse(float mouseX) {
        CalculatedStyle dims = GetDimensions();
        float startX = dims.X + 10f;
        float width = Math.Max(1f, dims.Width - 20f);
        float percent = MathHelper.Clamp((mouseX - startX) / width, 0f, 1f);
        SetValue((int)Math.Round(percent * 255f));
    }
}

public sealed class PaletteChannelButton : UIPanel {
    private readonly Func<Color> _resolveColor;
    private readonly Func<bool> _resolveSelected;

    public PaletteChannelButton(string label, Func<Color> resolveColor, Func<bool> resolveSelected) {
        Label = label ?? string.Empty;
        _resolveColor = resolveColor;
        _resolveSelected = resolveSelected;
        PaddingTop = 0f;
        PaddingBottom = 0f;
        PaddingLeft = 0f;
        PaddingRight = 0f;
    }

    public string Label { get; }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        BackgroundColor = _resolveSelected()
            ? new Color(28, 44, 62, 235)
            : new Color(18, 24, 30, 215);
        BorderColor = _resolveSelected()
            ? new Color(120, 220, 170)
            : new Color(85, 100, 115);

        base.DrawSelf(spriteBatch);

        CalculatedStyle dims = GetDimensions();
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Rectangle swatch = new Rectangle((int)dims.X + 10, (int)dims.Y + 9, 22, 22);
        Color swatchColor = _resolveColor();

        spriteBatch.Draw(pixel, swatch, swatchColor);
        spriteBatch.Draw(pixel, new Rectangle(swatch.X, swatch.Y, swatch.Width, 1), Color.Black);
        spriteBatch.Draw(pixel, new Rectangle(swatch.X, swatch.Bottom - 1, swatch.Width, 1), Color.Black);
        spriteBatch.Draw(pixel, new Rectangle(swatch.X, swatch.Y, 1, swatch.Height), Color.Black);
        spriteBatch.Draw(pixel, new Rectangle(swatch.Right - 1, swatch.Y, 1, swatch.Height), Color.Black);

        Utils.DrawBorderString(spriteBatch, Label, new Vector2(dims.X + 42f, dims.Y + 8f), Color.White, 0.82f);
    }
}

public sealed class DraggableUIPanel : UIPanel {
    private bool _dragging;
    private Vector2 _dragOffset;

    public DraggableUIPanel() {
        PaddingTop = 0f;
        PaddingBottom = 0f;
        PaddingLeft = 0f;
        PaddingRight = 0f;
    }

    public float HandleHeight { get; set; } = 28f;
    public string DragLabel { get; set; } = "Move";

    public override void LeftMouseDown(UIMouseEvent evt) {
        base.LeftMouseDown(evt);
        if (!IsWithinHandle(evt.MousePosition))
            return;

        CalculatedStyle dims = GetDimensions();
        _dragging = true;
        _dragOffset = evt.MousePosition - new Vector2(dims.X, dims.Y);
    }

    public override void LeftMouseUp(UIMouseEvent evt) {
        base.LeftMouseUp(evt);
        _dragging = false;
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);

        if (!_dragging)
            return;

        if (!Main.mouseLeft) {
            _dragging = false;
            return;
        }

        CalculatedStyle dims = GetDimensions();
        float maxLeft = Math.Max(0f, Main.screenWidth - dims.Width);
        float maxTop = Math.Max(0f, Main.screenHeight - dims.Height);
        Vector2 topLeft = Main.MouseScreen - _dragOffset;
        Left.Set(MathHelper.Clamp(topLeft.X, 0f, maxLeft), 0f);
        Top.Set(MathHelper.Clamp(topLeft.Y, 0f, maxTop), 0f);
        Recalculate();
        Main.LocalPlayer.mouseInterface = true;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        base.DrawSelf(spriteBatch);

        CalculatedStyle dims = GetDimensions();
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Rectangle handle = new((int)dims.X + 2, (int)dims.Y + 2, Math.Max(0, (int)dims.Width - 4), Math.Max(0, (int)HandleHeight - 2));
        if (handle.Width <= 0 || handle.Height <= 0)
            return;

        spriteBatch.Draw(pixel, handle, new Color(26, 34, 44, 230));
        spriteBatch.Draw(pixel, new Rectangle(handle.X, handle.Bottom - 1, handle.Width, 1), new Color(78, 92, 110));
        Utils.DrawBorderString(spriteBatch, DragLabel, new Vector2(dims.X + 12f, dims.Y + 6f), new Color(220, 230, 240), 0.72f);
    }

    private bool IsWithinHandle(Vector2 mousePosition) {
        CalculatedStyle dims = GetDimensions();
        return mousePosition.X >= dims.X &&
               mousePosition.X <= dims.X + dims.Width &&
               mousePosition.Y >= dims.Y &&
               mousePosition.Y <= dims.Y + HandleHeight;
    }
}

public sealed class CustomNameTextInputPanel : UIElement {
    private readonly object _innerInputObject;
    private readonly UIElement _innerInputElement;
    private readonly FieldInfo _focusField;
    private readonly FieldInfo _currentStringField;
    private readonly MethodInfo _setTextMethod;
    private bool _suppressEvents;

    public CustomNameTextInputPanel(string placeholderText) {
        Type inputType = ResolveInputType();
        _innerInputObject = Activator.CreateInstance(inputType,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            args: new object[] { placeholderText },
            culture: null);
        _innerInputElement = (UIElement)_innerInputObject;
        _focusField = inputType.GetField("Focused", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _currentStringField = inputType.GetField("CurrentString", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) ??
                              inputType.GetField("_currentString", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        _setTextMethod = inputType.GetMethod("SetText", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
            binder: null, types: new[] { typeof(string) }, modifiers: null);

        _innerInputElement.Left.Set(0f, 0f);
        _innerInputElement.Top.Set(0f, 0f);
        _innerInputElement.Width.Set(0f, 1f);
        _innerInputElement.Height.Set(0f, 1f);
        Append(_innerInputElement);

        BindInnerEvent(inputType, "OnTextChange", nameof(HandleInnerTextChanged));
        BindInnerEvent(inputType, "OnUnfocus", nameof(HandleInnerUnfocus));
    }

    public int MaxLength { get; set; } = OmnitrixPlayer.MaxCustomTransformationNameLength;
    public bool IsInteractive { get; set; } = true;
    public event Action<string> TextChanged;
    public event Action<string> Submitted;
    public bool IsFocused => _focusField != null && (bool)(_focusField.GetValue(_innerInputObject) ?? false);
    public string Text => (_currentStringField?.GetValue(_innerInputObject) as string) ?? string.Empty;

    public void SetText(string text, bool invoke = true) {
        string sanitizedText = SanitizeText(text);
        if (string.Equals(Text, sanitizedText, StringComparison.Ordinal))
            return;

        _suppressEvents = true;
        _setTextMethod?.Invoke(_innerInputObject, new object[] { sanitizedText });
        _currentStringField?.SetValue(_innerInputObject, sanitizedText);
        _suppressEvents = false;

        if (invoke)
            TextChanged?.Invoke(Text);
    }

    public void SetFocused(bool focused) {
        _focusField?.SetValue(_innerInputObject, focused && IsInteractive);
        if (focused && IsInteractive)
            Main.clrInput();
    }

    public override void Update(GameTime gameTime) {
        IgnoresMouseInteraction = !IsInteractive;
        _innerInputElement.IgnoresMouseInteraction = !IsInteractive;
        if (!IsInteractive && IsFocused)
            _focusField?.SetValue(_innerInputObject, false);

        base.Update(gameTime);
    }

    private string SanitizeText(string text) {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        string sanitizedText = text
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Replace('\t', ' ');

        if (sanitizedText.Length > MaxLength)
            sanitizedText = sanitizedText[..MaxLength];

        return sanitizedText;
    }

    private static Type ResolveInputType() {
        Assembly uiAssembly = typeof(UIElement).Assembly;
        Type inputType = uiAssembly.GetType("Terraria.ModLoader.UI.UIFocusInputTextField", throwOnError: false) ??
                         uiAssembly.GetType("Terraria.ModLoader.UI.UIInputTextField", throwOnError: false);
        if (inputType == null)
            throw new InvalidOperationException("Unable to locate the internal tModLoader text input control.");

        return inputType;
    }

    private void BindInnerEvent(Type inputType, string eventName, string handlerMethodName) {
        EventInfo eventInfo = inputType.GetEvent(eventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        MethodInfo handlerMethod = GetType().GetMethod(handlerMethodName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (eventInfo == null || handlerMethod == null)
            return;

        Delegate handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, this, handlerMethod);
        eventInfo.AddEventHandler(_innerInputObject, handler);
    }

    private void HandleInnerTextChanged(object sender, EventArgs args) {
        if (_suppressEvents)
            return;

        string sanitizedText = SanitizeText(Text);
        if (!string.Equals(Text, sanitizedText, StringComparison.Ordinal))
            SetText(sanitizedText, invoke: false);

        TextChanged?.Invoke(Text);
    }

    private void HandleInnerUnfocus(object sender, EventArgs args) {
        if (!_suppressEvents)
            Submitted?.Invoke(Text);
    }
}

public class TransformationPaletteScreen : UIState {
    private const float HeaderPanelWidth = 980f;
    private const float HeaderPanelHeight = 142f;
    private const float FloatingListPanelWidth = 336f;
    private const float FloatingDetailPanelWidth = 606f;
    private const float FloatingPanelHeight = 556f;
    private const float FloatingPanelMargin = 24f;
    private const float FloatingPanelGap = 18f;
    private const float FloatingPanelContentTop = 36f;
    private const float FloatingPanelInnerMargin = 8f;

    private enum CustomizationTab {
        Palette,
        Costumes,
        CustomNames
    }

    private sealed class PaletteClipboardEntry {
        public string ChannelId { get; init; } = string.Empty;
        public int ChannelIndex { get; init; }
        public Color Color { get; init; }
        public byte Hue { get; init; }
        public byte Saturation { get; init; }
        public bool Enabled { get; init; }
    }

    private sealed class PaletteClipboardState {
        public string SourceTransformationId { get; init; } = string.Empty;
        public string SourceDisplayName { get; init; } = string.Empty;
        public List<PaletteClipboardEntry> Entries { get; } = new();
    }

    private sealed class CostumeListEntry {
        public string CostumeId { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string SourceLabel { get; init; } = string.Empty;
        public bool IsDefault { get; init; }
    }

    private static PaletteClipboardState s_paletteClipboard;

    internal static void ClearSharedState() {
        s_paletteClipboard = null;
    }

    private DraggableUIPanel mainPanel;
    private DraggableUIPanel selectionPanel;
    private DraggableUIPanel detailPanel;
    private UIText titleText;
    private UIText targetText;
    private UIText statusText;
    private UITextPanel<string> paletteTabButton;
    private UITextPanel<string> costumesTabButton;
    private UITextPanel<string> customNamesTabButton;
    private UIPanel paletteListPanel;
    private UIPanel costumeListPanel;
    private UIPanel customNameListPanel;
    private UIPanel paletteDetailPanel;
    private UIPanel costumeDetailPanel;
    private UIPanel customNameDetailPanel;
    private UIList channelList;
    private UIScrollbar channelScrollbar;
    private UIText selectedChannelText;
    private UIList sliderList;
    private UIScrollbar sliderScrollbar;
    private PaletteByteSlider redSlider;
    private PaletteByteSlider greenSlider;
    private PaletteByteSlider blueSlider;
    private PaletteByteSlider hueSlider;
    private PaletteByteSlider saturationSlider;
    private UITextPanel<string> copyPaletteButton;
    private UITextPanel<string> pastePaletteButton;
    private UITextPanel<string> paletteToggleButton;
    private UITextPanel<string> applyButton;
    private UITextPanel<string> resetChannelButton;
    private UITextPanel<string> resetAllButton;
    private UIText palettePresetHintText;
    private readonly List<UITextPanel<string>> palettePresetButtons = new();
    private UIList costumeList;
    private UIScrollbar costumeScrollbar;
    private UIText selectedCostumeText;
    private UIText costumeSourceText;
    private UIText costumeDescriptionText;
    private UIText costumeHintText;
    private UITextPanel<string> useDefaultCostumeButton;
    private UIList customNameList;
    private UIScrollbar customNameScrollbar;
    private UIText selectedNameText;
    private UIText originalNameText;
    private UIText customNamePreviewText;
    private UIText customNameHintText;
    private CustomNameTextInputPanel customNameInput;
    private UITextPanel<string> applyNameButton;
    private UITextPanel<string> resetNameButton;

    private CustomizationTab _activeTab = CustomizationTab.Palette;
    private string _currentTransformationId = string.Empty;
    private string _currentChannelSignature = string.Empty;
    private string _currentChannelEnabledSignature = string.Empty;
    private string _currentPaletteCostumeId = string.Empty;
    private string _currentCustomNameSignature = string.Empty;
    private string _currentCostumeSignature = string.Empty;
    private string _selectedChannelId = string.Empty;
    private string _selectedCustomNameTransformationId = string.Empty;
    private string _loadedCustomNameValue = string.Empty;
    private bool _selectedChannelPaletteEnabled = true;
    private bool _suppressSliderCallbacks;
    private bool _hasPendingPaletteChanges;
    private bool _suppressCustomNameCallbacks;
    private bool _hasPendingCustomNameChanges;
    private readonly List<TransformationPaletteChannel> _activeChannels = new();
    private readonly List<CostumeListEntry> _availableCostumeEntries = new();
    private readonly List<string> _availableCustomNameTransformationIds = new();
    private readonly Dictionary<string, Color> _pendingColors = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, byte> _pendingHueValues = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, byte> _pendingSaturationValues = new(StringComparer.OrdinalIgnoreCase);
    private bool _layoutRefreshPending = true;
    private int _lastLayoutScreenWidth = -1;
    private int _lastLayoutScreenHeight = -1;

    public override void OnInitialize() {
        mainPanel = new DraggableUIPanel {
            DragLabel = "Customization"
        };
        mainPanel.Width.Set(HeaderPanelWidth, 0f);
        mainPanel.Height.Set(HeaderPanelHeight, 0f);
        Append(mainPanel);

        selectionPanel = new DraggableUIPanel {
            DragLabel = "Move List"
        };
        selectionPanel.Width.Set(FloatingListPanelWidth, 0f);
        selectionPanel.Height.Set(FloatingPanelHeight, 0f);
        Append(selectionPanel);

        detailPanel = new DraggableUIPanel {
            DragLabel = "Move Details"
        };
        detailPanel.Width.Set(FloatingDetailPanelWidth, 0f);
        detailPanel.Height.Set(FloatingPanelHeight, 0f);
        Append(detailPanel);

        titleText = new UIText("Alien Customization", 1.35f);
        titleText.Left.Set(24f, 0f);
        titleText.Top.Set(38f, 0f);
        mainPanel.Append(titleText);

        paletteTabButton = CreateActionButton("Palette", 552f, 34f, (_, _) => SetActiveTab(CustomizationTab.Palette), width: 120f);
        costumesTabButton = CreateActionButton("Costumes", 682f, 34f,
            (_, _) => SetActiveTab(CustomizationTab.Costumes), width: 120f);
        customNamesTabButton = CreateActionButton("Custom Names", 812f, 34f,
            (_, _) => SetActiveTab(CustomizationTab.CustomNames), width: 140f);
        mainPanel.Append(paletteTabButton);
        mainPanel.Append(costumesTabButton);
        mainPanel.Append(customNamesTabButton);

        targetText = new UIText("No transformation selected", 1f);
        targetText.Left.Set(24f, 0f);
        targetText.Top.Set(78f, 0f);
        mainPanel.Append(targetText);

        statusText = new UIText("Pick a transformation with palette masks to begin.", 0.9f);
        statusText.Left.Set(24f, 0f);
        statusText.Top.Set(104f, 0f);
        mainPanel.Append(statusText);

        paletteListPanel = new UIPanel();
        paletteListPanel.Width.Set(320f, 0f);
        paletteListPanel.Height.Set(512f, 0f);
        paletteListPanel.HAlign = 0.5f;
        paletteListPanel.Left.Set(0f, 0f);
        paletteListPanel.Top.Set(FloatingPanelContentTop, 0f);
        paletteListPanel.PaddingTop = 0f;
        paletteListPanel.PaddingBottom = 0f;
        paletteListPanel.PaddingLeft = 0f;
        paletteListPanel.PaddingRight = 0f;
        selectionPanel.Append(paletteListPanel);

        UIText channelsHeader = new UIText("Palette Parts", 1.05f);
        channelsHeader.Left.Set(16f, 0f);
        channelsHeader.Top.Set(12f, 0f);
        paletteListPanel.Append(channelsHeader);

        channelList = new UIList();
        channelList.Width.Set(-30f, 1f);
        channelList.Height.Set(-52f, 1f);
        channelList.Left.Set(10f, 0f);
        channelList.Top.Set(40f, 0f);
        channelList.ListPadding = 8f;
        paletteListPanel.Append(channelList);

        channelScrollbar = new UIScrollbar();
        channelScrollbar.Height.Set(-52f, 1f);
        channelScrollbar.Left.Set(-20f, 1f);
        channelScrollbar.Top.Set(40f, 0f);
        paletteListPanel.Append(channelScrollbar);
        channelList.SetScrollbar(channelScrollbar);

        paletteDetailPanel = new UIPanel();
        paletteDetailPanel.Width.Set(590f, 0f);
        paletteDetailPanel.Height.Set(512f, 0f);
        paletteDetailPanel.HAlign = 0.5f;
        paletteDetailPanel.Left.Set(0f, 0f);
        paletteDetailPanel.Top.Set(FloatingPanelContentTop, 0f);
        paletteDetailPanel.PaddingTop = 0f;
        paletteDetailPanel.PaddingBottom = 0f;
        paletteDetailPanel.PaddingLeft = 0f;
        paletteDetailPanel.PaddingRight = 0f;
        detailPanel.Append(paletteDetailPanel);

        selectedChannelText = new UIText("No part selected", 1.05f);
        selectedChannelText.Left.Set(16f, 0f);
        selectedChannelText.Top.Set(18f, 0f);
        selectedChannelText.Width.Set(276f, 0f);
        paletteDetailPanel.Append(selectedChannelText);

        copyPaletteButton = CreateActionButton("Copy Palette", 304f, 12f, (_, _) => CopyCurrentPalette(), width: 124f);
        pastePaletteButton = CreateActionButton("Paste Palette", 436f, 12f, (_, _) => PastePalette(), width: 138f);
        paletteDetailPanel.Append(copyPaletteButton);
        paletteDetailPanel.Append(pastePaletteButton);

        UIPanel sliderPanel = new UIPanel();
        sliderPanel.Left.Set(16f, 0f);
        sliderPanel.Top.Set(62f, 0f);
        sliderPanel.Width.Set(558f, 0f);
        sliderPanel.Height.Set(286f, 0f);
        sliderPanel.PaddingTop = 8f;
        sliderPanel.PaddingBottom = 8f;
        sliderPanel.PaddingLeft = 8f;
        sliderPanel.PaddingRight = 8f;
        paletteDetailPanel.Append(sliderPanel);

        sliderList = new UIList();
        sliderList.Left.Set(0f, 0f);
        sliderList.Top.Set(0f, 0f);
        sliderList.Width.Set(-24f, 1f);
        sliderList.Height.Set(0f, 1f);
        sliderList.ListPadding = 8f;
        sliderPanel.Append(sliderList);

        sliderScrollbar = new UIScrollbar();
        sliderScrollbar.Left.Set(-20f, 1f);
        sliderScrollbar.Top.Set(0f, 0f);
        sliderScrollbar.Height.Set(0f, 1f);
        sliderPanel.Append(sliderScrollbar);
        sliderList.SetScrollbar(sliderScrollbar);

        redSlider = CreateColorSlider("Red", new Color(225, 80, 80), value => value.ToString());
        greenSlider = CreateColorSlider("Green", new Color(90, 220, 120), value => value.ToString());
        blueSlider = CreateColorSlider("Blue", new Color(90, 155, 245), value => value.ToString());
        hueSlider = CreateColorSlider("Hue", new Color(110, 210, 255), FormatHueValue);
        saturationSlider = CreateColorSlider("Saturation", new Color(255, 210, 120), FormatSaturationValue);
        sliderList.Add(redSlider);
        sliderList.Add(greenSlider);
        sliderList.Add(blueSlider);
        sliderList.Add(hueSlider);
        sliderList.Add(saturationSlider);

        paletteToggleButton = CreateActionButton("Use Original", 16f, 360f, (_, _) => TogglePaletteEnabled(), width: 132f);
        applyButton = CreateActionButton("Apply Changes", 156f, 360f, (_, _) => ApplyPendingColors(), width: 132f);
        resetChannelButton = CreateActionButton("Reset Part", 296f, 360f, (_, _) => ResetSelectedPendingColor(), width: 132f);
        resetAllButton = CreateActionButton("Reset All", 436f, 360f, (_, _) => ResetAllPendingColors(), width: 138f);
        paletteDetailPanel.Append(paletteToggleButton);
        paletteDetailPanel.Append(applyButton);
        paletteDetailPanel.Append(resetChannelButton);
        paletteDetailPanel.Append(resetAllButton);

        palettePresetHintText = new UIText(
            "Left click loads a preset. Right click saves the current colours and mask toggles.",
            0.74f);
        palettePresetHintText.Left.Set(16f, 0f);
        palettePresetHintText.Top.Set(402f, 0f);
        palettePresetHintText.Width.Set(558f, 0f);
        paletteDetailPanel.Append(palettePresetHintText);

        const float presetButtonWidth = 180f;
        const float presetButtonSpacing = 9f;
        for (int presetIndex = 0; presetIndex < OmnitrixPlayer.PalettePresetSlotCount; presetIndex++) {
            int capturedPresetIndex = presetIndex;
            UITextPanel<string> presetButton = CreateActionButton($"Preset {presetIndex + 1}", 16f + presetIndex * (presetButtonWidth + presetButtonSpacing),
                442f, (_, _) => LoadPalettePreset(capturedPresetIndex), width: presetButtonWidth);
            presetButton.OnRightClick += (_, _) => SavePalettePreset(capturedPresetIndex);
            palettePresetButtons.Add(presetButton);
            paletteDetailPanel.Append(presetButton);
        }

        costumeListPanel = new UIPanel();
        costumeListPanel.Width.Set(320f, 0f);
        costumeListPanel.Height.Set(512f, 0f);
        costumeListPanel.HAlign = 0.5f;
        costumeListPanel.Left.Set(0f, 0f);
        costumeListPanel.Top.Set(FloatingPanelContentTop, 0f);
        costumeListPanel.PaddingTop = 0f;
        costumeListPanel.PaddingBottom = 0f;
        costumeListPanel.PaddingLeft = 0f;
        costumeListPanel.PaddingRight = 0f;
        selectionPanel.Append(costumeListPanel);

        UIText costumeListHeader = new UIText("Available Costumes", 1.05f);
        costumeListHeader.Left.Set(16f, 0f);
        costumeListHeader.Top.Set(12f, 0f);
        costumeListPanel.Append(costumeListHeader);

        costumeList = new UIList();
        costumeList.Width.Set(-30f, 1f);
        costumeList.Height.Set(-52f, 1f);
        costumeList.Left.Set(10f, 0f);
        costumeList.Top.Set(40f, 0f);
        costumeList.ListPadding = 8f;
        costumeListPanel.Append(costumeList);

        costumeScrollbar = new UIScrollbar();
        costumeScrollbar.Height.Set(-52f, 1f);
        costumeScrollbar.Left.Set(-20f, 1f);
        costumeScrollbar.Top.Set(40f, 0f);
        costumeListPanel.Append(costumeScrollbar);
        costumeList.SetScrollbar(costumeScrollbar);

        costumeDetailPanel = new UIPanel();
        costumeDetailPanel.Width.Set(590f, 0f);
        costumeDetailPanel.Height.Set(512f, 0f);
        costumeDetailPanel.HAlign = 0.5f;
        costumeDetailPanel.Left.Set(0f, 0f);
        costumeDetailPanel.Top.Set(FloatingPanelContentTop, 0f);
        costumeDetailPanel.PaddingTop = 0f;
        costumeDetailPanel.PaddingBottom = 0f;
        costumeDetailPanel.PaddingLeft = 0f;
        costumeDetailPanel.PaddingRight = 0f;
        detailPanel.Append(costumeDetailPanel);

        selectedCostumeText = new UIText("Default Costume", 1.05f);
        selectedCostumeText.Left.Set(18f, 0f);
        selectedCostumeText.Top.Set(16f, 0f);
        costumeDetailPanel.Append(selectedCostumeText);

        useDefaultCostumeButton = CreateActionButton("Use Default Look", 406f, 12f,
            (_, _) => SelectCostume(string.Empty), width: 166f);
        costumeDetailPanel.Append(useDefaultCostumeButton);

        costumeSourceText = new UIText("Source: Base Ben10Mod look", 0.9f);
        costumeSourceText.Left.Set(18f, 0f);
        costumeSourceText.Top.Set(48f, 0f);
        costumeDetailPanel.Append(costumeSourceText);

        costumeDescriptionText = new UIText("Select a costume to apply it to the current transformation.", 0.9f) {
            IsWrapped = true
        };
        costumeDescriptionText.Left.Set(18f, 0f);
        costumeDescriptionText.Top.Set(84f, 0f);
        costumeDescriptionText.Width.Set(554f, 0f);
        costumeDetailPanel.Append(costumeDescriptionText);

        costumeHintText = new UIText(
            "Costumes can come from Ben10Mod or addon mods. Palette colours save separately for each costume.",
            0.84f) {
            IsWrapped = true
        };
        costumeHintText.Left.Set(18f, 0f);
        costumeHintText.Top.Set(238f, 0f);
        costumeHintText.Width.Set(554f, 0f);
        costumeDetailPanel.Append(costumeHintText);

        customNameListPanel = new UIPanel();
        customNameListPanel.Width.Set(320f, 0f);
        customNameListPanel.Height.Set(512f, 0f);
        customNameListPanel.HAlign = 0.5f;
        customNameListPanel.Left.Set(0f, 0f);
        customNameListPanel.Top.Set(FloatingPanelContentTop, 0f);
        customNameListPanel.PaddingTop = 0f;
        customNameListPanel.PaddingBottom = 0f;
        customNameListPanel.PaddingLeft = 0f;
        customNameListPanel.PaddingRight = 0f;
        selectionPanel.Append(customNameListPanel);

        UIText customNameListHeader = new UIText("Transformations", 1.05f);
        customNameListHeader.Left.Set(16f, 0f);
        customNameListHeader.Top.Set(12f, 0f);
        customNameListPanel.Append(customNameListHeader);

        customNameList = new UIList();
        customNameList.Width.Set(-30f, 1f);
        customNameList.Height.Set(-52f, 1f);
        customNameList.Left.Set(10f, 0f);
        customNameList.Top.Set(40f, 0f);
        customNameList.ListPadding = 8f;
        customNameListPanel.Append(customNameList);

        customNameScrollbar = new UIScrollbar();
        customNameScrollbar.Height.Set(-52f, 1f);
        customNameScrollbar.Left.Set(-20f, 1f);
        customNameScrollbar.Top.Set(40f, 0f);
        customNameListPanel.Append(customNameScrollbar);
        customNameList.SetScrollbar(customNameScrollbar);

        customNameDetailPanel = new UIPanel();
        customNameDetailPanel.Width.Set(590f, 0f);
        customNameDetailPanel.Height.Set(512f, 0f);
        customNameDetailPanel.HAlign = 0.5f;
        customNameDetailPanel.Left.Set(0f, 0f);
        customNameDetailPanel.Top.Set(FloatingPanelContentTop, 0f);
        customNameDetailPanel.PaddingTop = 0f;
        customNameDetailPanel.PaddingBottom = 0f;
        customNameDetailPanel.PaddingLeft = 0f;
        customNameDetailPanel.PaddingRight = 0f;
        detailPanel.Append(customNameDetailPanel);

        selectedNameText = new UIText("No transformation selected", 1.05f);
        selectedNameText.Left.Set(18f, 0f);
        selectedNameText.Top.Set(16f, 0f);
        customNameDetailPanel.Append(selectedNameText);

        originalNameText = new UIText("Original Name: --", 0.92f);
        originalNameText.Left.Set(18f, 0f);
        originalNameText.Top.Set(48f, 0f);
        customNameDetailPanel.Append(originalNameText);

        UIPanel customNamePreviewPanel = new UIPanel();
        customNamePreviewPanel.Left.Set(18f, 0f);
        customNamePreviewPanel.Top.Set(82f, 0f);
        customNamePreviewPanel.Width.Set(554f, 0f);
        customNamePreviewPanel.Height.Set(118f, 0f);
        customNamePreviewPanel.PaddingTop = 0f;
        customNamePreviewPanel.PaddingBottom = 0f;
        customNamePreviewPanel.PaddingLeft = 0f;
        customNamePreviewPanel.PaddingRight = 0f;
        customNameDetailPanel.Append(customNamePreviewPanel);

        UIText previewLabel = new UIText("Current Display", 0.95f);
        previewLabel.Left.Set(14f, 0f);
        previewLabel.Top.Set(12f, 0f);
        customNamePreviewPanel.Append(previewLabel);

        customNamePreviewText = new UIText("No name selected", 1.2f);
        customNamePreviewText.Left.Set(14f, 0f);
        customNamePreviewText.Top.Set(48f, 0f);
        customNamePreviewPanel.Append(customNamePreviewText);

        UIText inputLabel = new UIText("Custom Name", 0.95f);
        inputLabel.Left.Set(18f, 0f);
        inputLabel.Top.Set(222f, 0f);
        customNameDetailPanel.Append(inputLabel);

        customNameInput = new CustomNameTextInputPanel("Leave blank to use the original alien name") {
            MaxLength = OmnitrixPlayer.MaxCustomTransformationNameLength
        };
        customNameInput.Left.Set(18f, 0f);
        customNameInput.Top.Set(250f, 0f);
        customNameInput.Width.Set(554f, 0f);
        customNameInput.Height.Set(52f, 0f);
        customNameInput.TextChanged += _ => OnCustomNameInputChanged();
        customNameInput.Submitted += _ => CommitSelectedCustomName();
        customNameDetailPanel.Append(customNameInput);

        customNameHintText = new UIText("Custom names save with your player data. Press Enter, Apply Name, or close the screen to save.",
            0.88f);
        customNameHintText.Left.Set(18f, 0f);
        customNameHintText.Top.Set(318f, 0f);
        customNameHintText.Width.Set(540f, 0f);
        customNameHintText.IsWrapped = true;
        customNameDetailPanel.Append(customNameHintText);

        applyNameButton = CreateActionButton("Apply Name", 18f, 442f, (_, _) => CommitSelectedCustomName(), width: 170f);
        resetNameButton = CreateActionButton("Use Original Name", 198f, 442f,
            (_, _) => ResetSelectedCustomName(), width: 190f);
        customNameDetailPanel.Append(applyNameButton);
        customNameDetailPanel.Append(resetNameButton);

        UITextPanel<string> closeButton = CreateActionButton("Close", 786f, 82f, (_, _) => {
            CommitPendingColors();
            CommitSelectedCustomName();
            ModContent.GetInstance<UISystem>().HideMyUI();
            Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().showingUI = false;
        }, width: 170f);
        mainPanel.Append(closeButton);

        ResetWindowLayout();
        SetActiveTab(_activeTab, refreshState: false);
        UpdateTabButtonState();
    }

    public override void OnActivate() {
        base.OnActivate();
        if (mainPanel == null)
            return;
        _layoutRefreshPending = true;
        _lastLayoutScreenWidth = -1;
        _lastLayoutScreenHeight = -1;
        ResetWindowLayout();
        RefreshPaletteContext(force: true);
        RefreshCostumeContext(force: true);
        RefreshCustomNameContext(force: true);
        SetActiveTab(_activeTab, refreshState: true);
    }

    public override void OnDeactivate() {
        CommitPendingColors();
        CommitSelectedCustomName();
        customNameInput?.SetFocused(false);
        base.OnDeactivate();
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);
        if (mainPanel == null)
            return;

        RefreshWindowLayoutIfNeeded();
        RefreshPaletteContext(force: false);
        RefreshCostumeContext(force: false);
        RefreshCustomNameContext(force: false);

        if (mainPanel.ContainsPoint(Main.MouseScreen) ||
            selectionPanel.ContainsPoint(Main.MouseScreen) ||
            detailPanel.ContainsPoint(Main.MouseScreen))
            Main.LocalPlayer.mouseInterface = true;
    }

    private void RefreshWindowLayoutIfNeeded() {
        int screenWidth = Main.screenWidth;
        int screenHeight = Main.screenHeight;
        if (screenWidth <= 0 || screenHeight <= 0)
            return;

        bool screenChanged = screenWidth != _lastLayoutScreenWidth || screenHeight != _lastLayoutScreenHeight;
        if (!_layoutRefreshPending && !screenChanged)
            return;

        ResetWindowLayout();
        _lastLayoutScreenWidth = screenWidth;
        _lastLayoutScreenHeight = screenHeight;
        _layoutRefreshPending = false;
    }

    private void SetActiveTab(CustomizationTab tab, bool refreshState = true) {
        if (_activeTab != tab) {
            CommitSelectedCustomName();
            customNameInput?.SetFocused(false);
            _activeTab = tab;
        }

        SetPanelVisibility(paletteListPanel, tab == CustomizationTab.Palette);
        SetPanelVisibility(costumeListPanel, tab == CustomizationTab.Costumes);
        SetPanelVisibility(customNameListPanel, tab == CustomizationTab.CustomNames);
        SetPanelVisibility(paletteDetailPanel, tab == CustomizationTab.Palette);
        SetPanelVisibility(costumeDetailPanel, tab == CustomizationTab.Costumes);
        SetPanelVisibility(customNameDetailPanel, tab == CustomizationTab.CustomNames);

        selectionPanel.DragLabel = tab switch {
            CustomizationTab.Palette => "Palette Parts",
            CustomizationTab.Costumes => "Costume List",
            CustomizationTab.CustomNames => "Name Targets",
            _ => "Selection"
        };
        detailPanel.DragLabel = tab switch {
            CustomizationTab.Palette => "Palette Controls",
            CustomizationTab.Costumes => "Costume Details",
            CustomizationTab.CustomNames => "Name Details",
            _ => "Details"
        };

        UpdateTabButtonState();

        if (!refreshState)
            return;

        RefreshPaletteContext(force: true);
        RefreshCostumeContext(force: true);
        RefreshCustomNameContext(force: true);
    }

    private void UpdateTabButtonState() {
        UpdateTabButtonVisual(paletteTabButton, _activeTab == CustomizationTab.Palette);
        UpdateTabButtonVisual(costumesTabButton, _activeTab == CustomizationTab.Costumes);
        UpdateTabButtonVisual(customNamesTabButton, _activeTab == CustomizationTab.CustomNames);
    }

    private void ResetWindowLayout() {
        float headerLeft = Math.Max(0f, (Main.screenWidth - HeaderPanelWidth) * 0.5f);
        float headerTop = FloatingPanelMargin;
        float floatingTop = MathHelper.Clamp(headerTop + HeaderPanelHeight + FloatingPanelGap,
            0f, Math.Max(0f, Main.screenHeight - FloatingPanelHeight));
        float listLeft = FloatingPanelMargin;
        float detailLeft = Math.Max(0f, Main.screenWidth - FloatingDetailPanelWidth - FloatingPanelMargin);

        if (detailLeft <= listLeft + FloatingListPanelWidth + FloatingPanelGap)
            detailLeft = Math.Min(
                Math.Max(0f, Main.screenWidth - FloatingDetailPanelWidth),
                listLeft + FloatingListPanelWidth + FloatingPanelGap
            );

        mainPanel.Left.Set(headerLeft, 0f);
        mainPanel.Top.Set(headerTop, 0f);
        mainPanel.Recalculate();

        selectionPanel.Left.Set(listLeft, 0f);
        selectionPanel.Top.Set(floatingTop, 0f);
        selectionPanel.Recalculate();

        detailPanel.Left.Set(detailLeft, 0f);
        detailPanel.Top.Set(floatingTop, 0f);
        detailPanel.Recalculate();
    }

    private static void SetPanelVisibility(UIElement panel, bool visible) {
        if (panel == null)
            return;

        panel.Left.Set(visible ? 0f : -2000f, 0f);
        panel.IgnoresMouseInteraction = !visible;
        panel.Recalculate();
    }

    private static void UpdateTabButtonVisual(UITextPanel<string> button, bool selected) {
        if (button == null)
            return;

        button.BackgroundColor = selected ? new Color(42, 84, 76) : new Color(40, 44, 54);
        button.BorderColor = selected ? new Color(140, 220, 170) : new Color(72, 78, 90);
    }

    private PaletteByteSlider CreateColorSlider(string label, Color accentColor, Func<int, string> valueFormatter) {
        PaletteByteSlider slider = new() {
            Label = label,
            AccentColor = accentColor,
            ValueFormatter = valueFormatter
        };
        slider.Width.Set(0f, 1f);
        slider.Height.Set(42f, 0f);
        slider.ValueChanged += _ => UpdatePendingColorFromSliders();
        return slider;
    }

    private static UITextPanel<string> CreateActionButton(string text, float left, float top,
        UIElement.MouseEvent action, float width = 170f) {
        UITextPanel<string> button = new(text, 0.92f, large: false) {
            Width = { Pixels = width },
            Height = { Pixels = 38f },
            Left = { Pixels = left },
            Top = { Pixels = top }
        };
        button.OnLeftClick += action;
        return button;
    }

    private void RefreshPaletteContext(bool force) {
        Player localPlayer = Main.LocalPlayer;
        if (localPlayer == null || Main.gameMenu || Main.myPlayer < 0 || Main.myPlayer >= Main.maxPlayers ||
            !localPlayer.active)
            return;

        OmnitrixPlayer omp = localPlayer.GetModPlayer<OmnitrixPlayer>();
        Transformation targetTransformation = omp.GetPaletteTargetTransformation();
        string targetTransformationId = targetTransformation?.FullID ?? string.Empty;
        string selectedCostumeId = targetTransformation == null
            ? string.Empty
            : omp.GetSelectedTransformationCostumeId(targetTransformation.FullID);
        IReadOnlyList<TransformationPaletteChannel> channels = targetTransformation?.GetPaletteChannels(omp)
            ?.Where(channel => channel != null && channel.IsValid)
            .ToArray() ?? Array.Empty<TransformationPaletteChannel>();
        string channelSignature = BuildChannelSignature(channels);
        string channelEnabledSignature = BuildChannelEnabledSignature(targetTransformation, channels, omp);
        bool channelContentChanged = force || targetTransformationId != _currentTransformationId ||
            selectedCostumeId != _currentPaletteCostumeId ||
            channelSignature != _currentChannelSignature;
        bool channelStateChanged = channelContentChanged || channelEnabledSignature != _currentChannelEnabledSignature;

        if (!channelContentChanged && !channelStateChanged)
            return;

        _currentTransformationId = targetTransformationId;
        _currentPaletteCostumeId = selectedCostumeId;
        _currentChannelSignature = channelSignature;
        _currentChannelEnabledSignature = channelEnabledSignature;

        if (channelContentChanged) {
            _activeChannels.Clear();
            _activeChannels.AddRange(channels);
            _pendingColors.Clear();
            _pendingHueValues.Clear();
            _pendingSaturationValues.Clear();
            _hasPendingPaletteChanges = false;

            if (targetTransformation != null) {
                for (int i = 0; i < _activeChannels.Count; i++) {
                    TransformationPaletteChannel channel = _activeChannels[i];
                    _pendingColors[channel.Id] = omp.GetPaletteColor(targetTransformation, channel.Id);
                    _pendingHueValues[channel.Id] = omp.GetPaletteHue(targetTransformation.FullID, channel.Id);
                    _pendingSaturationValues[channel.Id] = omp.GetPaletteSaturation(targetTransformation.FullID, channel.Id);
                }
            }

            if (_activeChannels.Count > 0) {
                _selectedChannelId = _activeChannels.Any(channel => channel.Id == _selectedChannelId)
                    ? _selectedChannelId
                    : _activeChannels[0].Id;
            }
            else {
                _selectedChannelId = string.Empty;
            }
        }

        _selectedChannelPaletteEnabled = IsPaletteChannelEnabled(_selectedChannelId);
        if (channelStateChanged)
            RebuildChannelButtons();

        if (_activeTab == CustomizationTab.Palette)
            UpdatePaletteHeaderState(omp, targetTransformation);

        if (channelContentChanged)
            LoadSelectedChannelIntoSliders();
    }

    private void UpdatePaletteHeaderState(OmnitrixPlayer omp, Transformation targetTransformation) {
        if (targetTransformation == null) {
            targetText.SetText("No active transformation context");
            statusText.SetText("Transform, or select an Omnitrix slot first, to customize palette parts.");
            selectedChannelText.SetText("No part selected");
            SetControlsInteractive(false);
            UpdatePaletteClipboardButtons(omp, null, interactive: false);
            UpdatePalettePresetButtons(omp, null, interactive: false);
            return;
        }

        string contextLabel = omp.IsTransformed ? "Current Form" : "Selected Form";
        targetText.SetText($"{contextLabel}: {targetTransformation.GetDisplayName(omp)}");

        if (_activeChannels.Count == 0) {
            statusText.SetText("This transformation has no custom mask parts configured.");
            selectedChannelText.SetText("No part selected");
            SetControlsInteractive(false);
            UpdatePaletteClipboardButtons(omp, targetTransformation, interactive: false);
            UpdatePalettePresetButtons(omp, targetTransformation, interactive: false);
            return;
        }

        statusText.SetText(_selectedChannelPaletteEnabled
            ? "Select a custom part, adjust the sliders, then apply your changes."
            : $"{GetSelectedChannelDisplayName()} is using the original texture. Hue and saturation still apply while custom RGB stays stored for later.");
        selectedChannelText.SetText(GetSelectedChannelDisplayName());
        SetControlsInteractive(true);
        UpdatePaletteClipboardButtons(omp, targetTransformation, interactive: true);
        UpdatePalettePresetButtons(omp, targetTransformation, interactive: true);
    }

    private void RefreshCostumeContext(bool force) {
        Player localPlayer = Main.LocalPlayer;
        if (localPlayer == null || Main.gameMenu || Main.myPlayer < 0 || Main.myPlayer >= Main.maxPlayers ||
            !localPlayer.active)
            return;

        OmnitrixPlayer omp = localPlayer.GetModPlayer<OmnitrixPlayer>();
        Transformation targetTransformation = omp.GetPaletteTargetTransformation();
        string selectedCostumeId = targetTransformation == null
            ? string.Empty
            : omp.GetSelectedTransformationCostumeId(targetTransformation.FullID);
        List<CostumeListEntry> costumeEntries = BuildCostumeEntries(targetTransformation);
        string costumeSignature = BuildCostumeSignature(targetTransformation, costumeEntries, selectedCostumeId);
        bool costumeContentChanged = force || costumeSignature != _currentCostumeSignature;

        if (!costumeContentChanged && _activeTab != CustomizationTab.Costumes)
            return;

        if (costumeContentChanged) {
            _currentCostumeSignature = costumeSignature;
            _availableCostumeEntries.Clear();
            _availableCostumeEntries.AddRange(costumeEntries);
            RebuildCostumeButtons();
        }

        if (_activeTab == CustomizationTab.Costumes)
            UpdateCostumeHeaderState(omp, targetTransformation);
    }

    private void UpdateCostumeHeaderState(OmnitrixPlayer omp, Transformation targetTransformation) {
        if (targetTransformation == null) {
            targetText.SetText("No active transformation context");
            statusText.SetText("Transform, or select an Omnitrix slot first, to choose a costume.");
            selectedCostumeText.SetText("Default Look");
            costumeSourceText.SetText("Source: --");
            costumeDescriptionText.SetText("Costumes let addon mods replace the look of a transformation without replacing the transformation itself.");
            costumeHintText.SetText("When installed, addon costumes will show up here and keep their own palette settings.");
            UpdateCostumeButtonState(interactive: false);
            return;
        }

        CostumeListEntry selectedEntry = GetSelectedCostumeEntry(targetTransformation, omp);
        string selectedDisplayName = selectedEntry?.DisplayName ?? "Default Look";
        targetText.SetText($"Costumes: {targetTransformation.GetDisplayName(omp)}");
        statusText.SetText(selectedEntry is { IsDefault: true }
            ? "Using the default look. Select a costume to apply an alternate appearance for this transformation."
            : "This costume is active now. Palette colours and toggles save separately for each costume.");
        selectedCostumeText.SetText(selectedDisplayName);
        costumeSourceText.SetText($"Source: {selectedEntry?.SourceLabel ?? "Base Ben10Mod look"}");
        costumeDescriptionText.SetText(string.IsNullOrWhiteSpace(selectedEntry?.Description)
            ? "This costume swaps the transformation's visuals while leaving its moveset and gameplay intact."
            : selectedEntry.Description);
        costumeHintText.SetText(_availableCostumeEntries.Count <= 1
            ? "No alternate costumes are installed for this transformation yet. Addon costumes will appear here automatically."
            : "Click a costume to apply it immediately. The palette tab will switch to that costume's own saved colours.");
        UpdateCostumeButtonState(interactive: true);
    }

    private void UpdateCostumeButtonState(bool interactive) {
        if (useDefaultCostumeButton == null)
            return;

        useDefaultCostumeButton.BackgroundColor = interactive ? new Color(78, 66, 42) : new Color(40, 44, 54);
        useDefaultCostumeButton.BorderColor = interactive ? new Color(225, 192, 118) : new Color(62, 68, 80);
    }

    private List<CostumeListEntry> BuildCostumeEntries(Transformation targetTransformation) {
        List<CostumeListEntry> entries = new();
        if (targetTransformation == null)
            return entries;

        entries.Add(new CostumeListEntry {
            CostumeId = string.Empty,
            DisplayName = "Default Look",
            Description = $"Use the base {targetTransformation.GetDisplayName(Main.LocalPlayer?.GetModPlayer<OmnitrixPlayer>())} appearance.",
            SourceLabel = $"{targetTransformation.Mod.Name} default appearance",
            IsDefault = true
        });

        IReadOnlyList<TransformationCostume> costumes = TransformationCostumeLoader.GetForTransformation(targetTransformation.FullID);
        for (int i = 0; i < costumes.Count; i++) {
            TransformationCostume costume = costumes[i];
            if (costume == null)
                continue;

            entries.Add(new CostumeListEntry {
                CostumeId = costume.FullID,
                DisplayName = string.IsNullOrWhiteSpace(costume.DisplayName) ? costume.CostumeName : costume.DisplayName,
                Description = costume.Description,
                SourceLabel = costume.Mod.Name,
                IsDefault = false
            });
        }

        return entries;
    }

    private static string BuildCostumeSignature(Transformation targetTransformation, IReadOnlyList<CostumeListEntry> entries,
        string selectedCostumeId) {
        if (targetTransformation == null)
            return string.Empty;

        StringBuilder builder = new();
        builder.Append(targetTransformation.FullID)
            .Append('|')
            .Append(selectedCostumeId)
            .Append('|');

        if (entries != null) {
            for (int i = 0; i < entries.Count; i++) {
                CostumeListEntry entry = entries[i];
                builder.Append(entry.CostumeId)
                    .Append('=')
                    .Append(entry.DisplayName)
                    .Append('|');
            }
        }

        return builder.ToString();
    }

    private void RebuildCostumeButtons() {
        costumeList.Clear();
        OmnitrixPlayer omp = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
        Transformation targetTransformation = omp.GetPaletteTargetTransformation();
        string selectedCostumeId = targetTransformation == null
            ? string.Empty
            : omp.GetSelectedTransformationCostumeId(targetTransformation.FullID);

        for (int i = 0; i < _availableCostumeEntries.Count; i++) {
            CostumeListEntry entry = _availableCostumeEntries[i];
            UITextPanel<string> button = new(BuildCostumeButtonLabel(entry), 0.88f, large: false);
            button.Width.Set(0f, 1f);
            button.Height.Set(44f, 0f);
            bool isSelected = string.Equals(entry.CostumeId, selectedCostumeId, StringComparison.OrdinalIgnoreCase) ||
                (entry.IsDefault && string.IsNullOrWhiteSpace(selectedCostumeId));
            button.BackgroundColor = isSelected ? new Color(32, 56, 74, 235) : new Color(18, 24, 30, 215);
            button.BorderColor = isSelected ? new Color(140, 220, 170) : new Color(85, 100, 115);
            string selectedEntryId = entry.CostumeId;
            button.OnLeftClick += (_, _) => SelectCostume(selectedEntryId);
            costumeList.Add(button);
        }
    }

    private static string BuildCostumeButtonLabel(CostumeListEntry entry) {
        if (entry == null)
            return "Unknown Costume";

        return entry.IsDefault
            ? "Default Look"
            : $"{entry.DisplayName} [{entry.SourceLabel}]";
    }

    private CostumeListEntry GetSelectedCostumeEntry(Transformation targetTransformation, OmnitrixPlayer omp) {
        if (targetTransformation == null)
            return null;

        string selectedCostumeId = omp?.GetSelectedTransformationCostumeId(targetTransformation.FullID) ?? string.Empty;
        for (int i = 0; i < _availableCostumeEntries.Count; i++) {
            CostumeListEntry entry = _availableCostumeEntries[i];
            if (entry.IsDefault && string.IsNullOrWhiteSpace(selectedCostumeId))
                return entry;

            if (string.Equals(entry.CostumeId, selectedCostumeId, StringComparison.OrdinalIgnoreCase))
                return entry;
        }

        return _availableCostumeEntries.Count > 0 ? _availableCostumeEntries[0] : null;
    }

    private void SelectCostume(string costumeId) {
        Player localPlayer = Main.LocalPlayer;
        if (localPlayer == null || !localPlayer.active)
            return;

        OmnitrixPlayer omp = localPlayer.GetModPlayer<OmnitrixPlayer>();
        Transformation targetTransformation = omp.GetPaletteTargetTransformation();
        if (targetTransformation == null)
            return;

        CommitPendingColors();
        bool changed = omp.SetSelectedTransformationCostume(targetTransformation.FullID, costumeId, sync: false);
        if (changed)
            omp.SyncTransformationPaletteStateToServerOrClients();

        RefreshPaletteContext(force: true);
        RefreshCostumeContext(force: true);
    }

    private void RefreshCustomNameContext(bool force) {
        Player localPlayer = Main.LocalPlayer;
        if (localPlayer == null || Main.gameMenu || Main.myPlayer < 0 || Main.myPlayer >= Main.maxPlayers ||
            !localPlayer.active)
            return;

        OmnitrixPlayer omp = localPlayer.GetModPlayer<OmnitrixPlayer>();
        List<string> availableTransformationIds = BuildCustomNameTransformationIds(omp);
        string customNameSignature = BuildCustomNameSignature(availableTransformationIds, omp);
        bool customNameContentChanged = force || customNameSignature != _currentCustomNameSignature;
        if (!customNameContentChanged && _activeTab != CustomizationTab.CustomNames)
            return;

        if (customNameContentChanged) {
            _currentCustomNameSignature = customNameSignature;
            _availableCustomNameTransformationIds.Clear();
            _availableCustomNameTransformationIds.AddRange(availableTransformationIds);

            if (_availableCustomNameTransformationIds.Count == 0) {
                CommitSelectedCustomName();
                _selectedCustomNameTransformationId = string.Empty;
            }
            else if (!_availableCustomNameTransformationIds.Contains(_selectedCustomNameTransformationId, StringComparer.OrdinalIgnoreCase)) {
                CommitSelectedCustomName();
                _selectedCustomNameTransformationId = ResolvePreferredCustomNameSelection(omp, _availableCustomNameTransformationIds);
            }

            RebuildCustomNameButtons();
            LoadSelectedCustomNameIntoInput();
        }

        if (_activeTab == CustomizationTab.CustomNames)
            UpdateCustomNameHeaderState(omp);
    }

    private void UpdateCustomNameHeaderState(OmnitrixPlayer omp) {
        Transformation targetTransformation = TransformationLoader.Resolve(_selectedCustomNameTransformationId);
        if (targetTransformation == null) {
            targetText.SetText("No transformation selected");
            statusText.SetText("Unlock or select a transformation to customize its display name.");
            selectedNameText.SetText("No transformation selected");
            originalNameText.SetText("Original Name: --");
            customNamePreviewText.SetText("No name selected");
            customNameHintText.SetText("Custom names save with your player data. Leave the field blank to use the original name.");
            SetCustomNameControlsInteractive(false);
            return;
        }

        string pendingName = GetPendingCustomNamePreview(targetTransformation);
        targetText.SetText($"Custom Name: {pendingName}");
        statusText.SetText(_hasPendingCustomNameChanges
            ? "Press Apply Name, Enter, switch tabs, or close the screen to save the pending alias."
            : "Custom names save with your player data and show across the roster, HUD, and transformation feedback.");
        selectedNameText.SetText(pendingName);
        originalNameText.SetText($"Original Name: {targetTransformation.TransformationName}");
        customNamePreviewText.SetText(pendingName);
        customNameHintText.SetText($"Leave the field blank to use the original name. Max {OmnitrixPlayer.MaxCustomTransformationNameLength} characters.");
        SetCustomNameControlsInteractive(true);
    }

    private void SetControlsInteractive(bool interactive) {
        redSlider.IsInteractive = interactive;
        greenSlider.IsInteractive = interactive;
        blueSlider.IsInteractive = interactive;
        hueSlider.IsInteractive = interactive;
        saturationSlider.IsInteractive = interactive;
        paletteToggleButton.BackgroundColor = interactive
            ? (_selectedChannelPaletteEnabled ? new Color(76, 118, 83) : new Color(122, 88, 60))
            : new Color(40, 44, 54);
        paletteToggleButton.BorderColor = interactive
            ? (_selectedChannelPaletteEnabled ? new Color(140, 220, 170) : new Color(228, 188, 120))
            : new Color(62, 68, 80);
        paletteToggleButton.SetText(_selectedChannelPaletteEnabled ? "Use Original" : "Use Palette");
        applyButton.BackgroundColor = interactive ? new Color(63, 82, 151) : new Color(40, 44, 54);
        resetChannelButton.BackgroundColor = interactive ? new Color(63, 82, 151) : new Color(40, 44, 54);
        resetAllButton.BackgroundColor = interactive ? new Color(63, 82, 151) : new Color(40, 44, 54);
    }

    private void SetCustomNameControlsInteractive(bool interactive) {
        if (customNameInput != null) {
            customNameInput.IsInteractive = interactive;
            if (!interactive)
                customNameInput.SetFocused(false);
        }

        if (applyNameButton != null) {
            applyNameButton.BackgroundColor = interactive
                ? (_hasPendingCustomNameChanges ? new Color(63, 82, 151) : new Color(54, 64, 88))
                : new Color(40, 44, 54);
            applyNameButton.BorderColor = interactive
                ? (_hasPendingCustomNameChanges ? new Color(130, 165, 255) : new Color(78, 90, 120))
                : new Color(62, 68, 80);
        }

        if (resetNameButton != null) {
            resetNameButton.BackgroundColor = interactive ? new Color(84, 70, 44) : new Color(40, 44, 54);
            resetNameButton.BorderColor = interactive ? new Color(224, 190, 118) : new Color(62, 68, 80);
        }
    }

    private List<string> BuildCustomNameTransformationIds(OmnitrixPlayer omp) {
        List<string> transformationIds = new();
        HashSet<string> seenTransformationIds = new(StringComparer.OrdinalIgnoreCase);

        if (omp?.unlockedTransformations != null) {
            for (int i = 0; i < omp.unlockedTransformations.Count; i++) {
                Transformation transformation = TransformationLoader.Resolve(omp.unlockedTransformations[i]);
                if (transformation != null && seenTransformationIds.Add(transformation.FullID))
                    transformationIds.Add(transformation.FullID);
            }
        }

        Transformation targetTransformation = omp?.GetPaletteTargetTransformation();
        if (targetTransformation != null && seenTransformationIds.Add(targetTransformation.FullID))
            transformationIds.Add(targetTransformation.FullID);

        return transformationIds;
    }

    private static string ResolvePreferredCustomNameSelection(OmnitrixPlayer omp, IReadOnlyList<string> availableTransformationIds) {
        if (availableTransformationIds == null || availableTransformationIds.Count == 0)
            return string.Empty;

        string preferredTransformationId = omp?.GetPaletteTargetTransformationId() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(preferredTransformationId)) {
            for (int i = 0; i < availableTransformationIds.Count; i++) {
                if (string.Equals(availableTransformationIds[i], preferredTransformationId, StringComparison.OrdinalIgnoreCase))
                    return availableTransformationIds[i];
            }
        }

        return availableTransformationIds[0];
    }

    private static string BuildCustomNameSignature(IReadOnlyList<string> transformationIds, OmnitrixPlayer omp) {
        if (transformationIds == null || transformationIds.Count == 0 || omp == null)
            return string.Empty;

        StringBuilder builder = new();
        for (int i = 0; i < transformationIds.Count; i++) {
            string transformationId = transformationIds[i];
            builder.Append(transformationId)
                .Append('=')
                .Append(omp.GetCustomTransformationName(transformationId))
                .Append('|');
        }

        return builder.ToString();
    }

    private void RebuildCustomNameButtons() {
        customNameList.Clear();
        OmnitrixPlayer omp = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();

        for (int i = 0; i < _availableCustomNameTransformationIds.Count; i++) {
            string transformationId = _availableCustomNameTransformationIds[i];
            Transformation transformation = TransformationLoader.Resolve(transformationId);
            if (transformation == null)
                continue;

            string buttonLabel = BuildCustomNameButtonLabel(transformation, omp);
            UITextPanel<string> button = new(buttonLabel, 0.88f, large: false);
            button.Width.Set(0f, 1f);
            button.Height.Set(44f, 0f);
            bool isSelected = string.Equals(_selectedCustomNameTransformationId, transformation.FullID, StringComparison.OrdinalIgnoreCase);
            button.BackgroundColor = isSelected ? new Color(28, 44, 62, 235) : new Color(18, 24, 30, 215);
            button.BorderColor = isSelected ? new Color(120, 220, 170) : new Color(85, 100, 115);
            string selectedTransformationId = transformation.FullID;
            button.OnLeftClick += (_, _) => SelectCustomNameTransformation(selectedTransformationId);
            customNameList.Add(button);
        }
    }

    private static string BuildCustomNameButtonLabel(Transformation transformation, OmnitrixPlayer omp) {
        string customName = omp?.GetCustomTransformationName(transformation) ?? string.Empty;
        return string.IsNullOrWhiteSpace(customName)
            ? transformation.TransformationName
            : $"{customName} ({transformation.TransformationName})";
    }

    private void SelectCustomNameTransformation(string transformationId) {
        if (string.IsNullOrWhiteSpace(transformationId))
            return;

        CommitSelectedCustomName();
        _selectedCustomNameTransformationId = transformationId;
        RebuildCustomNameButtons();
        LoadSelectedCustomNameIntoInput();

        if (_activeTab == CustomizationTab.CustomNames)
            UpdateCustomNameHeaderState(Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>());
    }

    private void LoadSelectedCustomNameIntoInput() {
        OmnitrixPlayer omp = Main.LocalPlayer?.GetModPlayer<OmnitrixPlayer>();
        if (omp == null || customNameInput == null)
            return;

        _suppressCustomNameCallbacks = true;
        _loadedCustomNameValue = string.IsNullOrWhiteSpace(_selectedCustomNameTransformationId)
            ? string.Empty
            : omp.GetCustomTransformationName(_selectedCustomNameTransformationId);
        customNameInput.SetText(_loadedCustomNameValue, invoke: false);
        _hasPendingCustomNameChanges = false;
        _suppressCustomNameCallbacks = false;
        SetCustomNameControlsInteractive(!string.IsNullOrWhiteSpace(_selectedCustomNameTransformationId));
    }

    private void OnCustomNameInputChanged() {
        if (_suppressCustomNameCallbacks || customNameInput == null)
            return;

        _hasPendingCustomNameChanges = !string.Equals(customNameInput.Text, _loadedCustomNameValue, StringComparison.Ordinal);
        if (_activeTab == CustomizationTab.CustomNames)
            UpdateCustomNameHeaderState(Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>());
        else
            SetCustomNameControlsInteractive(!string.IsNullOrWhiteSpace(_selectedCustomNameTransformationId));
    }

    private void CommitSelectedCustomName() {
        if (string.IsNullOrWhiteSpace(_selectedCustomNameTransformationId) || customNameInput == null)
            return;

        OmnitrixPlayer omp = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
        if (_hasPendingCustomNameChanges)
            omp.SetCustomTransformationName(_selectedCustomNameTransformationId, customNameInput.Text);

        _loadedCustomNameValue = omp.GetCustomTransformationName(_selectedCustomNameTransformationId);
        _suppressCustomNameCallbacks = true;
        customNameInput.SetText(_loadedCustomNameValue, invoke: false);
        _hasPendingCustomNameChanges = false;
        _suppressCustomNameCallbacks = false;

        _currentCustomNameSignature = BuildCustomNameSignature(_availableCustomNameTransformationIds, omp);
        RebuildCustomNameButtons();
        if (_activeTab == CustomizationTab.CustomNames)
            UpdateCustomNameHeaderState(omp);
        else
            SetCustomNameControlsInteractive(!string.IsNullOrWhiteSpace(_selectedCustomNameTransformationId));
    }

    private void ResetSelectedCustomName() {
        if (customNameInput == null)
            return;

        customNameInput.SetText(string.Empty);
        CommitSelectedCustomName();
    }

    private string GetPendingCustomNamePreview(Transformation transformation) {
        if (transformation == null)
            return "No transformation selected";

        if (string.Equals(_selectedCustomNameTransformationId, transformation.FullID, StringComparison.OrdinalIgnoreCase) &&
            customNameInput != null) {
            string pendingName = customNameInput.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(pendingName))
                return pendingName;
        }

        string savedCustomName = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().GetCustomTransformationName(transformation);
        return string.IsNullOrWhiteSpace(savedCustomName) ? transformation.TransformationName : savedCustomName;
    }

    private void RebuildChannelButtons() {
        channelList.Clear();

        for (int i = 0; i < _activeChannels.Count; i++) {
            TransformationPaletteChannel channel = _activeChannels[i];
            string displayName = IsPaletteChannelEnabled(channel.Id)
                ? channel.DisplayName
                : $"{channel.DisplayName} (Original)";
            PaletteChannelButton button = new(displayName, () => GetPendingColor(channel.Id),
                () => string.Equals(_selectedChannelId, channel.Id, StringComparison.OrdinalIgnoreCase));
            button.Width.Set(0f, 1f);
            button.Height.Set(40f, 0f);
            string channelId = channel.Id;
            button.OnLeftClick += (_, _) => SelectChannel(channelId);
            channelList.Add(button);
        }
    }

    private void SelectChannel(string channelId) {
        if (string.IsNullOrWhiteSpace(channelId))
            return;

        _selectedChannelId = channelId;
        _selectedChannelPaletteEnabled = IsPaletteChannelEnabled(_selectedChannelId);
        selectedChannelText.SetText(GetSelectedChannelDisplayName());
        UpdatePaletteHeaderState(Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>(),
            Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().GetPaletteTargetTransformation());
        LoadSelectedChannelIntoSliders();
        RebuildChannelButtons();
    }

    private void LoadSelectedChannelIntoSliders() {
        _suppressSliderCallbacks = true;
        TransformationPaletteChannelSettings settings = GetSelectedPendingSettings();
        redSlider.SetValue(settings.Color.R, invoke: false);
        greenSlider.SetValue(settings.Color.G, invoke: false);
        blueSlider.SetValue(settings.Color.B, invoke: false);
        hueSlider.SetValue(settings.Hue, invoke: false);
        saturationSlider.SetValue(settings.Saturation, invoke: false);
        _suppressSliderCallbacks = false;
    }

    private void UpdatePendingColorFromSliders() {
        if (_suppressSliderCallbacks || string.IsNullOrWhiteSpace(_selectedChannelId))
            return;

        OmnitrixPlayer omp = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
        Transformation transformation = omp.GetPaletteTargetTransformation();
        TransformationPaletteChannel channel = transformation?.GetPaletteChannel(_selectedChannelId, omp);
        if (channel == null)
            return;

        Color pendingColor = new(redSlider.Value, greenSlider.Value, blueSlider.Value);
        byte pendingHue = (byte)hueSlider.Value;
        byte pendingSaturation = (byte)saturationSlider.Value;

        _pendingColors[_selectedChannelId] = pendingColor;
        _pendingHueValues[channel.Id] = pendingHue;
        _pendingSaturationValues[channel.Id] = pendingSaturation;
        omp.SetPaletteColor(_currentTransformationId, channel.Id, pendingColor, sync: false);
        omp.SetPaletteHue(_currentTransformationId, channel.Id, pendingHue, sync: false);
        omp.SetPaletteSaturation(_currentTransformationId, channel.Id, pendingSaturation, sync: false);
        _hasPendingPaletteChanges = true;
    }

    private void ApplyPendingColors() {
        CommitPendingColors(forceSync: true);
    }

    private void CommitPendingColors(bool forceSync = false) {
        if (string.IsNullOrWhiteSpace(_currentTransformationId) || _activeChannels.Count == 0)
            return;

        if (!_hasPendingPaletteChanges && !forceSync)
            return;

        OmnitrixPlayer omp = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
        bool changed = false;
        for (int i = 0; i < _activeChannels.Count; i++) {
            TransformationPaletteChannel channel = _activeChannels[i];
            Color pendingColor = GetPendingColor(channel.Id);
            changed |= omp.SetPaletteColor(_currentTransformationId, channel.Id, pendingColor, sync: false);
            changed |= omp.SetPaletteHue(_currentTransformationId, channel.Id, GetPendingHue(channel.Id), sync: false);
            changed |= omp.SetPaletteSaturation(_currentTransformationId, channel.Id, GetPendingSaturation(channel.Id), sync: false);
        }

        if (changed || forceSync)
            omp.SyncTransformationPaletteStateToServerOrClients();

        _hasPendingPaletteChanges = false;
    }

    private void TogglePaletteEnabled() {
        if (string.IsNullOrWhiteSpace(_currentTransformationId) || string.IsNullOrWhiteSpace(_selectedChannelId))
            return;

        OmnitrixPlayer omp = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
        bool newPaletteEnabled = !_selectedChannelPaletteEnabled;
        bool changed = omp.SetPaletteChannelEnabled(_currentTransformationId, _selectedChannelId, newPaletteEnabled,
            sync: false);
        _selectedChannelPaletteEnabled = omp.IsPaletteChannelEnabled(_currentTransformationId, _selectedChannelId);
        _currentChannelEnabledSignature = BuildChannelEnabledSignature(omp.GetPaletteTargetTransformation(), _activeChannels, omp);
        RebuildChannelButtons();
        UpdatePaletteHeaderState(omp, omp.GetPaletteTargetTransformation());
        if (changed)
            omp.SyncTransformationPaletteStateToServerOrClients();
    }

    private void ResetSelectedPendingColor() {
        if (string.IsNullOrWhiteSpace(_selectedChannelId))
            return;

        TransformationPaletteChannel channel = _activeChannels.FirstOrDefault(entry =>
            string.Equals(entry.Id, _selectedChannelId, StringComparison.OrdinalIgnoreCase));
        if (channel == null)
            return;

        _pendingColors[channel.Id] = channel.DefaultColor;
        _pendingHueValues[channel.Id] = TransformationPaletteColorEntry.NeutralHue;
        _pendingSaturationValues[channel.Id] = TransformationPaletteColorEntry.NeutralSaturation;
        OmnitrixPlayer omp = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
        omp.SetPaletteColor(_currentTransformationId, channel.Id, channel.DefaultColor, sync: false);
        omp.SetPaletteHue(_currentTransformationId, channel.Id, TransformationPaletteColorEntry.NeutralHue, sync: false);
        omp.SetPaletteSaturation(_currentTransformationId, channel.Id,
            TransformationPaletteColorEntry.NeutralSaturation, sync: false);
        _hasPendingPaletteChanges = true;
        LoadSelectedChannelIntoSliders();
    }

    private void ResetAllPendingColors() {
        OmnitrixPlayer omp = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
        for (int i = 0; i < _activeChannels.Count; i++) {
            TransformationPaletteChannel channel = _activeChannels[i];
            _pendingColors[channel.Id] = channel.DefaultColor;
            _pendingHueValues[channel.Id] = TransformationPaletteColorEntry.NeutralHue;
            _pendingSaturationValues[channel.Id] = TransformationPaletteColorEntry.NeutralSaturation;
            omp.SetPaletteColor(_currentTransformationId, channel.Id, channel.DefaultColor, sync: false);
            omp.SetPaletteHue(_currentTransformationId, channel.Id, TransformationPaletteColorEntry.NeutralHue,
                sync: false);
            omp.SetPaletteSaturation(_currentTransformationId, channel.Id,
                TransformationPaletteColorEntry.NeutralSaturation, sync: false);
        }

        _hasPendingPaletteChanges = true;
        LoadSelectedChannelIntoSliders();
    }

    private void CopyCurrentPalette() {
        if (string.IsNullOrWhiteSpace(_currentTransformationId) || _activeChannels.Count == 0)
            return;

        OmnitrixPlayer omp = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
        Transformation targetTransformation = omp.GetPaletteTargetTransformation();
        if (targetTransformation == null)
            return;

        PaletteClipboardState clipboard = new() {
            SourceTransformationId = targetTransformation.FullID,
            SourceDisplayName = targetTransformation.GetDisplayName(omp)
        };

        for (int i = 0; i < _activeChannels.Count; i++) {
            TransformationPaletteChannel channel = _activeChannels[i];
            clipboard.Entries.Add(new PaletteClipboardEntry {
                ChannelId = channel.Id,
                ChannelIndex = i,
                Color = GetPendingColor(channel.Id),
                Hue = GetPendingHue(channel.Id),
                Saturation = GetPendingSaturation(channel.Id),
                Enabled = IsPaletteChannelEnabled(channel.Id)
            });
        }

        s_paletteClipboard = clipboard;
        statusText.SetText($"Copied palette from {clipboard.SourceDisplayName}.");
        UpdatePaletteClipboardButtons(omp, targetTransformation, interactive: true);
    }

    private void PastePalette() {
        if (s_paletteClipboard == null || s_paletteClipboard.Entries.Count == 0 ||
            string.IsNullOrWhiteSpace(_currentTransformationId) || _activeChannels.Count == 0)
            return;

        OmnitrixPlayer omp = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
        Transformation targetTransformation = omp.GetPaletteTargetTransformation();
        if (targetTransformation == null)
            return;

        bool changed = false;
        for (int i = 0; i < _activeChannels.Count; i++) {
            TransformationPaletteChannel channel = _activeChannels[i];
            PaletteClipboardEntry clipboardEntry = s_paletteClipboard.Entries.FirstOrDefault(entry =>
                string.Equals(entry.ChannelId, channel.Id, StringComparison.OrdinalIgnoreCase));
            clipboardEntry ??= i < s_paletteClipboard.Entries.Count ? s_paletteClipboard.Entries[i] : null;
            if (clipboardEntry == null)
                continue;

            _pendingColors[channel.Id] = clipboardEntry.Color;
            _pendingHueValues[channel.Id] = clipboardEntry.Hue;
            _pendingSaturationValues[channel.Id] = clipboardEntry.Saturation;
            changed |= omp.SetPaletteColor(_currentTransformationId, channel.Id, clipboardEntry.Color, sync: false);
            changed |= omp.SetPaletteHue(_currentTransformationId, channel.Id, clipboardEntry.Hue, sync: false);
            changed |= omp.SetPaletteSaturation(_currentTransformationId, channel.Id, clipboardEntry.Saturation,
                sync: false);
            changed |= omp.SetPaletteChannelEnabled(_currentTransformationId, channel.Id, clipboardEntry.Enabled,
                sync: false);
        }

        _hasPendingPaletteChanges = false;
        if (changed)
            omp.SyncTransformationPaletteStateToServerOrClients();

        RefreshPaletteContext(force: true);
        statusText.SetText($"Pasted palette from {s_paletteClipboard.SourceDisplayName}.");
    }

    private void LoadPalettePreset(int presetIndex) {
        if (string.IsNullOrWhiteSpace(_currentTransformationId))
            return;

        OmnitrixPlayer omp = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
        if (!omp.ApplyPalettePreset(_currentTransformationId, presetIndex))
            return;

        _hasPendingPaletteChanges = false;
        RefreshPaletteContext(force: true);
    }

    private void SavePalettePreset(int presetIndex) {
        if (string.IsNullOrWhiteSpace(_currentTransformationId))
            return;

        OmnitrixPlayer omp = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
        if (!omp.SavePalettePreset(_currentTransformationId, presetIndex))
            return;

        UpdatePalettePresetButtons(omp, omp.GetPaletteTargetTransformation(), interactive: _activeChannels.Count > 0);
    }

    private void UpdatePalettePresetButtons(OmnitrixPlayer omp, Transformation targetTransformation, bool interactive) {
        string transformationId = targetTransformation?.FullID ?? string.Empty;
        bool hasPresetTarget = interactive && !string.IsNullOrWhiteSpace(transformationId) && omp != null;

        if (palettePresetHintText != null) {
            palettePresetHintText.SetText(hasPresetTarget
                ? "Left click loads a preset. Right click saves the current colours and mask toggles."
                : "Palette presets become available when the selected transformation has custom mask parts.");
        }

        for (int presetIndex = 0; presetIndex < palettePresetButtons.Count; presetIndex++) {
            UITextPanel<string> presetButton = palettePresetButtons[presetIndex];
            if (presetButton == null)
                continue;

            bool hasPreset = hasPresetTarget && omp.HasPalettePreset(transformationId, presetIndex);
            presetButton.SetText(hasPresetTarget
                ? omp.GetPalettePresetLabel(transformationId, presetIndex)
                : $"Preset {presetIndex + 1}");
            presetButton.BackgroundColor = hasPresetTarget
                ? (hasPreset ? new Color(56, 78, 118) : new Color(50, 58, 78))
                : new Color(40, 44, 54);
            presetButton.BorderColor = hasPresetTarget
                ? (hasPreset ? new Color(136, 190, 255) : new Color(82, 98, 128))
                : new Color(62, 68, 80);
        }
    }

    private void UpdatePaletteClipboardButtons(OmnitrixPlayer omp, Transformation targetTransformation, bool interactive) {
        bool canCopy = interactive && targetTransformation != null && _activeChannels.Count > 0;
        bool canPaste = canCopy && s_paletteClipboard != null && s_paletteClipboard.Entries.Count > 0;

        if (copyPaletteButton != null) {
            copyPaletteButton.BackgroundColor = canCopy ? new Color(54, 64, 88) : new Color(40, 44, 54);
            copyPaletteButton.BorderColor = canCopy ? new Color(122, 156, 224) : new Color(62, 68, 80);
        }

        if (pastePaletteButton != null) {
            pastePaletteButton.BackgroundColor = canPaste ? new Color(58, 82, 72) : new Color(40, 44, 54);
            pastePaletteButton.BorderColor = canPaste ? new Color(132, 214, 170) : new Color(62, 68, 80);
        }
    }

    private Color GetPendingColor(string channelId) {
        if (string.IsNullOrWhiteSpace(channelId))
            return Color.Transparent;

        if (_pendingColors.TryGetValue(channelId, out Color color))
            return color;

        TransformationPaletteChannel channel = _activeChannels.FirstOrDefault(entry =>
            string.Equals(entry.Id, channelId, StringComparison.OrdinalIgnoreCase));
        return channel?.DefaultColor ?? Color.Transparent;
    }

    private TransformationPaletteChannelSettings GetSelectedPendingSettings() {
        if (string.IsNullOrWhiteSpace(_selectedChannelId) || string.IsNullOrWhiteSpace(_currentTransformationId))
            return new TransformationPaletteChannelSettings(Color.White);

        return GetPendingSettings(_selectedChannelId);
    }

    private TransformationPaletteChannelSettings GetPendingSettings(string channelId) {
        if (string.IsNullOrWhiteSpace(channelId) || string.IsNullOrWhiteSpace(_currentTransformationId))
            return new TransformationPaletteChannelSettings(Color.White);

        return new TransformationPaletteChannelSettings(
            GetPendingColor(channelId),
            GetPendingHue(channelId),
            GetPendingSaturation(channelId)
        );
    }

    private byte GetPendingHue(string channelId) {
        if (string.IsNullOrWhiteSpace(channelId))
            return TransformationPaletteColorEntry.NeutralHue;

        return _pendingHueValues.TryGetValue(channelId, out byte hue)
            ? hue
            : TransformationPaletteColorEntry.NeutralHue;
    }

    private byte GetPendingSaturation(string channelId) {
        if (string.IsNullOrWhiteSpace(channelId))
            return TransformationPaletteColorEntry.NeutralSaturation;

        return _pendingSaturationValues.TryGetValue(channelId, out byte saturation)
            ? saturation
            : TransformationPaletteColorEntry.NeutralSaturation;
    }

    private string GetSelectedChannelDisplayName() {
        TransformationPaletteChannel channel = _activeChannels.FirstOrDefault(entry =>
            string.Equals(entry.Id, _selectedChannelId, StringComparison.OrdinalIgnoreCase));
        return channel?.DisplayName ?? "No part selected";
    }

    private bool IsPaletteChannelEnabled(string channelId) {
        if (string.IsNullOrWhiteSpace(_currentTransformationId) || string.IsNullOrWhiteSpace(channelId))
            return false;

        Player localPlayer = Main.LocalPlayer;
        if (localPlayer == null || !localPlayer.active)
            return true;

        return localPlayer.GetModPlayer<OmnitrixPlayer>().IsPaletteChannelEnabled(_currentTransformationId, channelId);
    }

    private static string BuildChannelSignature(IReadOnlyList<TransformationPaletteChannel> channels) {
        if (channels == null || channels.Count == 0)
            return string.Empty;

        StringBuilder builder = new();
        for (int i = 0; i < channels.Count; i++)
            builder.Append(channels[i].Id).Append('|');
        return builder.ToString();
    }

    private static string BuildChannelEnabledSignature(Transformation transformation,
        IReadOnlyList<TransformationPaletteChannel> channels, OmnitrixPlayer omp) {
        if (transformation == null || channels == null || channels.Count == 0 || omp == null)
            return string.Empty;

        StringBuilder builder = new();
        for (int i = 0; i < channels.Count; i++) {
            TransformationPaletteChannel channel = channels[i];
            if (channel == null || !channel.IsValid)
                continue;

            builder.Append(channel.Id)
                .Append('=')
                .Append(omp.IsPaletteChannelEnabled(transformation, channel.Id) ? '1' : '0')
                .Append('|');
        }

        return builder.ToString();
    }

    private static string FormatHueValue(int value) {
        float hue = TransformationPaletteMath.GetHueShiftDegrees((byte)value);
        return $"{Math.Round(hue):+0;-0;0} deg";
    }

    private static string FormatSaturationValue(int value) {
        float saturation = TransformationPaletteMath.GetSaturationMultiplier((byte)value);
        return $"{Math.Round(saturation * 100f)}%";
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        base.DrawSelf(spriteBatch);
        if (mainPanel == null)
            return;

        if (ContainsPoint(Main.MouseScreen))
            Main.LocalPlayer.mouseInterface = true;
    }
}
