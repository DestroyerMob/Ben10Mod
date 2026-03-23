using System;
using System.Collections.Generic;
using System.Linq;
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
        Utils.DrawBorderString(spriteBatch, _value.ToString(), new Vector2(dims.X + dims.Width - 10f, dims.Y + 6f),
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

public sealed class PalettePreviewSwatch : UIElement {
    public Func<Color> ResolveColor { get; set; }
    public Func<string> ResolveLabel { get; set; }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        CalculatedStyle dims = GetDimensions();
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Rectangle outer = dims.ToRectangle();
        Color color = ResolveColor?.Invoke() ?? Color.Transparent;
        string label = ResolveLabel?.Invoke() ?? "Preview";

        spriteBatch.Draw(pixel, outer, new Color(18, 22, 28, 220));
        spriteBatch.Draw(pixel, new Rectangle(outer.X, outer.Y, outer.Width, 2), new Color(110, 140, 160));
        spriteBatch.Draw(pixel, new Rectangle(outer.X, outer.Bottom - 2, outer.Width, 2), new Color(110, 140, 160));
        spriteBatch.Draw(pixel, new Rectangle(outer.X, outer.Y, 2, outer.Height), new Color(110, 140, 160));
        spriteBatch.Draw(pixel, new Rectangle(outer.Right - 2, outer.Y, 2, outer.Height), new Color(110, 140, 160));

        Rectangle swatch = new Rectangle(outer.X + 14, outer.Y + 14, outer.Width - 28, outer.Height - 48);
        spriteBatch.Draw(pixel, swatch, color);

        Utils.DrawBorderString(spriteBatch, label, new Vector2(dims.X + 12f, dims.Y + dims.Height - 24f),
            Color.White, 0.82f);
    }
}

public class TransformationPaletteScreen : UIState {
    private UIPanel mainPanel;
    private UIText titleText;
    private UIText targetText;
    private UIText statusText;
    private UIList channelList;
    private UIScrollbar channelScrollbar;
    private UIText selectedChannelText;
    private PalettePreviewSwatch previewSwatch;
    private PaletteByteSlider redSlider;
    private PaletteByteSlider greenSlider;
    private PaletteByteSlider blueSlider;
    private UITextPanel<string> applyButton;
    private UITextPanel<string> resetChannelButton;
    private UITextPanel<string> resetAllButton;

    private string _currentTransformationId = string.Empty;
    private string _currentChannelSignature = string.Empty;
    private string _selectedChannelId = string.Empty;
    private bool _suppressSliderCallbacks;
    private readonly List<TransformationPaletteChannel> _activeChannels = new();
    private readonly Dictionary<string, Color> _pendingColors = new(StringComparer.OrdinalIgnoreCase);

    public override void OnInitialize() {
        mainPanel = new UIPanel();
        mainPanel.Width.Set(980f, 0f);
        mainPanel.Height.Set(620f, 0f);
        mainPanel.HAlign = 0.5f;
        mainPanel.VAlign = 0.5f;
        Append(mainPanel);

        titleText = new UIText("Transformation Palette", 1.35f);
        titleText.Left.Set(24f, 0f);
        titleText.Top.Set(20f, 0f);
        mainPanel.Append(titleText);

        targetText = new UIText("No transformation selected", 1f);
        targetText.Left.Set(24f, 0f);
        targetText.Top.Set(58f, 0f);
        mainPanel.Append(targetText);

        statusText = new UIText("Pick a transformation with palette masks to begin.", 0.9f);
        statusText.Left.Set(24f, 0f);
        statusText.Top.Set(88f, 0f);
        mainPanel.Append(statusText);

        UIPanel channelsPanel = new UIPanel();
        channelsPanel.Width.Set(320f, 0f);
        channelsPanel.Height.Set(454f, 0f);
        channelsPanel.Left.Set(24f, 0f);
        channelsPanel.Top.Set(126f, 0f);
        mainPanel.Append(channelsPanel);

        UIText channelsHeader = new UIText("Custom Parts", 1.05f);
        channelsHeader.Left.Set(16f, 0f);
        channelsHeader.Top.Set(12f, 0f);
        channelsPanel.Append(channelsHeader);

        channelList = new UIList();
        channelList.Width.Set(-30f, 1f);
        channelList.Height.Set(-52f, 1f);
        channelList.Left.Set(10f, 0f);
        channelList.Top.Set(40f, 0f);
        channelList.ListPadding = 8f;
        channelsPanel.Append(channelList);

        channelScrollbar = new UIScrollbar();
        channelScrollbar.Height.Set(-52f, 1f);
        channelScrollbar.Left.Set(-20f, 1f);
        channelScrollbar.Top.Set(40f, 0f);
        channelsPanel.Append(channelScrollbar);
        channelList.SetScrollbar(channelScrollbar);

        UIPanel controlsPanel = new UIPanel();
        controlsPanel.Width.Set(590f, 0f);
        controlsPanel.Height.Set(454f, 0f);
        controlsPanel.Left.Set(366f, 0f);
        controlsPanel.Top.Set(126f, 0f);
        mainPanel.Append(controlsPanel);

        selectedChannelText = new UIText("No part selected", 1.05f);
        selectedChannelText.Left.Set(18f, 0f);
        selectedChannelText.Top.Set(16f, 0f);
        controlsPanel.Append(selectedChannelText);

        previewSwatch = new PalettePreviewSwatch {
            ResolveColor = GetSelectedPendingColor,
            ResolveLabel = GetSelectedChannelPreviewLabel
        };
        previewSwatch.Left.Set(18f, 0f);
        previewSwatch.Top.Set(50f, 0f);
        previewSwatch.Width.Set(180f, 0f);
        previewSwatch.Height.Set(180f, 0f);
        controlsPanel.Append(previewSwatch);

        redSlider = CreateColorSlider("Red", new Color(225, 80, 80), 220f);
        greenSlider = CreateColorSlider("Green", new Color(90, 220, 120), 288f);
        blueSlider = CreateColorSlider("Blue", new Color(90, 155, 245), 356f);
        controlsPanel.Append(redSlider);
        controlsPanel.Append(greenSlider);
        controlsPanel.Append(blueSlider);

        applyButton = CreateActionButton("Apply Changes", 18f, 410f, (_, _) => ApplyPendingColors());
        resetChannelButton = CreateActionButton("Reset Part", 208f, 410f, (_, _) => ResetSelectedPendingColor());
        resetAllButton = CreateActionButton("Reset All", 398f, 410f, (_, _) => ResetAllPendingColors());
        controlsPanel.Append(applyButton);
        controlsPanel.Append(resetChannelButton);
        controlsPanel.Append(resetAllButton);

        UITextPanel<string> closeButton = CreateActionButton("Close", 760f, 580f, (_, _) => {
            ModContent.GetInstance<UISystem>().HideMyUI();
            Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().showingUI = false;
        }, width: 180f);
        mainPanel.Append(closeButton);
    }

    public override void OnActivate() {
        base.OnActivate();
        RefreshPaletteContext(force: true);
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);
        RefreshPaletteContext(force: false);

        if (mainPanel.ContainsPoint(Main.MouseScreen))
            Main.LocalPlayer.mouseInterface = true;
    }

    private PaletteByteSlider CreateColorSlider(string label, Color accentColor, float top) {
        PaletteByteSlider slider = new() {
            Label = label,
            AccentColor = accentColor
        };
        slider.Left.Set(220f, 0f);
        slider.Top.Set(top, 0f);
        slider.Width.Set(340f, 0f);
        slider.Height.Set(50f, 0f);
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
        OmnitrixPlayer omp = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
        Transformation targetTransformation = omp.GetPaletteTargetTransformation();
        string targetTransformationId = targetTransformation?.FullID ?? string.Empty;
        IReadOnlyList<TransformationPaletteChannel> channels = targetTransformation?.GetPaletteChannels(omp)
            ?.Where(channel => channel != null && channel.IsValid)
            .ToArray() ?? Array.Empty<TransformationPaletteChannel>();
        string channelSignature = BuildChannelSignature(channels);

        if (!force && targetTransformationId == _currentTransformationId && channelSignature == _currentChannelSignature)
            return;

        _currentTransformationId = targetTransformationId;
        _currentChannelSignature = channelSignature;
        _activeChannels.Clear();
        _activeChannels.AddRange(channels);
        _pendingColors.Clear();

        if (targetTransformation != null) {
            for (int i = 0; i < _activeChannels.Count; i++) {
                TransformationPaletteChannel channel = _activeChannels[i];
                _pendingColors[channel.Id] = omp.GetPaletteColor(targetTransformation, channel.Id);
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

        RebuildChannelButtons();
        UpdateHeaderState(omp, targetTransformation);
        LoadSelectedChannelIntoSliders();
    }

    private void UpdateHeaderState(OmnitrixPlayer omp, Transformation targetTransformation) {
        if (targetTransformation == null) {
            targetText.SetText("No active transformation context");
            statusText.SetText("Transform, or select an Omnitrix slot first, to customize palette parts.");
            selectedChannelText.SetText("No part selected");
            SetControlsInteractive(false);
            return;
        }

        string contextLabel = omp.IsTransformed ? "Current Form" : "Selected Form";
        targetText.SetText($"{contextLabel}: {targetTransformation.GetDisplayName(omp)}");

        if (_activeChannels.Count == 0) {
            statusText.SetText("This transformation has no custom mask parts configured.");
            selectedChannelText.SetText("No part selected");
            SetControlsInteractive(false);
            return;
        }

        statusText.SetText("Select a custom part, adjust the sliders, then apply your changes.");
        selectedChannelText.SetText(GetSelectedChannelDisplayName());
        SetControlsInteractive(true);
    }

    private void SetControlsInteractive(bool interactive) {
        redSlider.IsInteractive = interactive;
        greenSlider.IsInteractive = interactive;
        blueSlider.IsInteractive = interactive;
        applyButton.BackgroundColor = interactive ? new Color(63, 82, 151) : new Color(40, 44, 54);
        resetChannelButton.BackgroundColor = interactive ? new Color(63, 82, 151) : new Color(40, 44, 54);
        resetAllButton.BackgroundColor = interactive ? new Color(63, 82, 151) : new Color(40, 44, 54);
    }

    private void RebuildChannelButtons() {
        channelList.Clear();

        for (int i = 0; i < _activeChannels.Count; i++) {
            TransformationPaletteChannel channel = _activeChannels[i];
            PaletteChannelButton button = new(channel.DisplayName, () => GetPendingColor(channel.Id),
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
        selectedChannelText.SetText(GetSelectedChannelDisplayName());
        LoadSelectedChannelIntoSliders();
    }

    private void LoadSelectedChannelIntoSliders() {
        _suppressSliderCallbacks = true;
        Color color = GetSelectedPendingColor();
        redSlider.SetValue(color.R, invoke: false);
        greenSlider.SetValue(color.G, invoke: false);
        blueSlider.SetValue(color.B, invoke: false);
        _suppressSliderCallbacks = false;
    }

    private void UpdatePendingColorFromSliders() {
        if (_suppressSliderCallbacks || string.IsNullOrWhiteSpace(_selectedChannelId))
            return;

        _pendingColors[_selectedChannelId] = new Color(redSlider.Value, greenSlider.Value, blueSlider.Value);
    }

    private void ApplyPendingColors() {
        if (string.IsNullOrWhiteSpace(_currentTransformationId) || _activeChannels.Count == 0)
            return;

        OmnitrixPlayer omp = Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>();
        bool changed = false;
        for (int i = 0; i < _activeChannels.Count; i++) {
            TransformationPaletteChannel channel = _activeChannels[i];
            Color pendingColor = GetPendingColor(channel.Id);
            changed |= omp.SetPaletteColor(_currentTransformationId, channel.Id, pendingColor, sync: false);
        }

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
        LoadSelectedChannelIntoSliders();
    }

    private void ResetAllPendingColors() {
        for (int i = 0; i < _activeChannels.Count; i++) {
            TransformationPaletteChannel channel = _activeChannels[i];
            _pendingColors[channel.Id] = channel.DefaultColor;
        }

        LoadSelectedChannelIntoSliders();
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

    private Color GetSelectedPendingColor() {
        return GetPendingColor(_selectedChannelId);
    }

    private string GetSelectedChannelDisplayName() {
        TransformationPaletteChannel channel = _activeChannels.FirstOrDefault(entry =>
            string.Equals(entry.Id, _selectedChannelId, StringComparison.OrdinalIgnoreCase));
        return channel?.DisplayName ?? "No part selected";
    }

    private string GetSelectedChannelPreviewLabel() {
        return string.IsNullOrWhiteSpace(_selectedChannelId)
            ? "Preview"
            : $"{GetSelectedChannelDisplayName()} Preview";
    }

    private static string BuildChannelSignature(IReadOnlyList<TransformationPaletteChannel> channels) {
        if (channels == null || channels.Count == 0)
            return string.Empty;

        StringBuilder builder = new();
        for (int i = 0; i < channels.Count; i++)
            builder.Append(channels[i].Id).Append('|');
        return builder.ToString();
    }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        base.DrawSelf(spriteBatch);

        if (ContainsPoint(Main.MouseScreen))
            Main.LocalPlayer.mouseInterface = true;
    }
}
