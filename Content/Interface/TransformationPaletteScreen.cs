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

public sealed class PalettePreviewSwatch : UIElement {
    private readonly struct ResolvedPreviewBase {
        public ResolvedPreviewBase(string texturePath, Texture2D texture) {
            TexturePath = texturePath ?? string.Empty;
            Texture = texture;
        }

        public string TexturePath { get; }
        public Texture2D Texture { get; }
    }

    private readonly struct ResolvedPreviewOverlay {
        public ResolvedPreviewOverlay(string texturePath, Texture2D baseTexture, Texture2D maskTexture,
            TransformationPaletteChannelSettings settings, bool usePaletteColor) {
            TexturePath = texturePath ?? string.Empty;
            BaseTexture = baseTexture;
            MaskTexture = maskTexture;
            Settings = settings;
            UsePaletteColor = usePaletteColor;
        }

        public string TexturePath { get; }
        public Texture2D BaseTexture { get; }
        public Texture2D MaskTexture { get; }
        public TransformationPaletteChannelSettings Settings { get; }
        public bool UsePaletteColor { get; }
    }

    public Func<Color> ResolveColor { get; set; }
    public Func<string> ResolveLabel { get; set; }
    public Func<IReadOnlyList<string>> ResolveBaseTexturePaths { get; set; }
    public Func<IReadOnlyList<TransformationPaletteChannel>> ResolveChannels { get; set; }
    public Func<string, TransformationPaletteChannelSettings> ResolveChannelSettings { get; set; }
    public Func<string, bool> ResolveChannelEnabled { get; set; }

    protected override void DrawSelf(SpriteBatch spriteBatch) {
        CalculatedStyle dims = GetDimensions();
        Texture2D pixel = TextureAssets.MagicPixel.Value;
        Rectangle outer = dims.ToRectangle();
        Color color = ResolveColor?.Invoke() ?? Color.Transparent;
        string label = ResolveLabel?.Invoke() ?? "Preview";
        IReadOnlyList<string> previewBaseTexturePaths = ResolveBaseTexturePaths?.Invoke() ??
            Array.Empty<string>();
        IReadOnlyList<TransformationPaletteChannel> channels = ResolveChannels?.Invoke() ??
            Array.Empty<TransformationPaletteChannel>();

        spriteBatch.Draw(pixel, outer, new Color(18, 22, 28, 220));
        spriteBatch.Draw(pixel, new Rectangle(outer.X, outer.Y, outer.Width, 2), new Color(110, 140, 160));
        spriteBatch.Draw(pixel, new Rectangle(outer.X, outer.Bottom - 2, outer.Width, 2), new Color(110, 140, 160));
        spriteBatch.Draw(pixel, new Rectangle(outer.X, outer.Y, 2, outer.Height), new Color(110, 140, 160));
        spriteBatch.Draw(pixel, new Rectangle(outer.Right - 2, outer.Y, 2, outer.Height), new Color(110, 140, 160));

        Rectangle previewArea = new Rectangle(outer.X + 18, outer.Y + 18, outer.Width - 36, outer.Height - 68);
        spriteBatch.Draw(pixel, previewArea, new Color(11, 14, 18, 235));
        spriteBatch.Draw(pixel, new Rectangle(previewArea.X, previewArea.Bottom - 18, previewArea.Width, 18),
            new Color(20, 28, 34, 245));

        DrawPreviewFigure(spriteBatch, previewArea, previewBaseTexturePaths, channels);

        Rectangle swatch = new Rectangle(outer.Right - 62, outer.Bottom - 38, 30, 18);
        spriteBatch.Draw(pixel, swatch, color);
        spriteBatch.Draw(pixel, new Rectangle(swatch.X, swatch.Y, swatch.Width, 1), Color.Black);
        spriteBatch.Draw(pixel, new Rectangle(swatch.X, swatch.Bottom - 1, swatch.Width, 1), Color.Black);
        spriteBatch.Draw(pixel, new Rectangle(swatch.X, swatch.Y, 1, swatch.Height), Color.Black);
        spriteBatch.Draw(pixel, new Rectangle(swatch.Right - 1, swatch.Y, 1, swatch.Height), Color.Black);

        Utils.DrawBorderString(spriteBatch, label, new Vector2(dims.X + 12f, dims.Y + dims.Height - 28f),
            Color.White, 0.82f);
    }

    private void DrawPreviewFigure(SpriteBatch spriteBatch, Rectangle previewArea,
        IReadOnlyList<string> previewBaseTexturePaths, IReadOnlyList<TransformationPaletteChannel> channels) {
        List<ResolvedPreviewBase> baseLayers = new();
        List<ResolvedPreviewOverlay> overlays = new();
        HashSet<string> seenBasePaths = new(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < previewBaseTexturePaths.Count; i++) {
            string texturePath = previewBaseTexturePaths[i];
            if (!seenBasePaths.Add(texturePath) || !TryGetPreviewTexture(texturePath, out Texture2D baseTexture))
                continue;

            baseLayers.Add(new ResolvedPreviewBase(texturePath, baseTexture));
        }

        for (int i = 0; i < channels.Count; i++) {
            TransformationPaletteChannel channel = channels[i];
            if (channel == null || !channel.IsValid)
                continue;

            bool channelEnabled = ResolveChannelEnabled?.Invoke(channel.Id) ?? true;
            TransformationPaletteChannelSettings settings = ResolveChannelSettings?.Invoke(channel.Id) ??
                new TransformationPaletteChannelSettings(channel.DefaultColor);
            for (int j = 0; j < channel.Overlays.Count; j++) {
                TransformationPaletteOverlay overlay = channel.Overlays[j];
                if (overlay == null || !overlay.TryGetTextures(out Texture2D baseTexture, out Texture2D maskTexture))
                    continue;

                if (seenBasePaths.Add(overlay.BaseTexturePath))
                    baseLayers.Add(new ResolvedPreviewBase(overlay.BaseTexturePath, baseTexture));

                overlays.Add(new ResolvedPreviewOverlay(overlay.BaseTexturePath, baseTexture, maskTexture,
                    settings, channelEnabled));
            }
        }

        if (baseLayers.Count == 0) {
            Utils.DrawBorderString(spriteBatch, "No preview available", new Vector2(previewArea.X + 14f, previewArea.Y + 14f),
                new Color(160, 170, 182), 0.82f);
            return;
        }

        baseLayers.Sort(static (left, right) => ComparePreviewLayerOrder(left.TexturePath, right.TexturePath));
        overlays.Sort(static (left, right) => ComparePreviewLayerOrder(left.TexturePath, right.TexturePath));

        const int previewFrameWidth = 40;
        const int previewFrameHeight = 56;
        float scale = Math.Min((previewArea.Width - 40f) / previewFrameWidth, (previewArea.Height - 26f) / previewFrameHeight);
        scale = MathHelper.Clamp(scale, 1.6f, 4.2f);

        Vector2 drawPosition = new(previewArea.Center.X, previewArea.Center.Y + 6f);
        Vector2 origin = new(previewFrameWidth * 0.5f, previewFrameHeight * 0.5f);

        int shadowWidth = (int)(previewFrameWidth * scale * 0.7f);
        Rectangle shadow = new(previewArea.Center.X - shadowWidth / 2, previewArea.Bottom - 20, shadowWidth, 10);
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, shadow, new Color(0, 0, 0, 105));

        for (int i = 0; i < baseLayers.Count; i++) {
            ResolvedPreviewBase previewBase = baseLayers[i];
            Texture2D texture = previewBase.Texture;
            if (texture == null)
                continue;

            List<Texture2D> masksForBase = new();
            for (int overlayIndex = 0; overlayIndex < overlays.Count; overlayIndex++) {
                ResolvedPreviewOverlay overlay = overlays[overlayIndex];
                if (overlay.BaseTexture == texture)
                    masksForBase.Add(overlay.MaskTexture);
            }

            Texture2D baseToDraw = masksForBase.Count > 0
                ? TransformationPaletteTextureCache.GetMaskedBaseTexture(texture, masksForBase)
                : texture;

            spriteBatch.Draw(baseToDraw, drawPosition, ResolvePreviewFrame(baseToDraw),
                Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
        }

        for (int i = 0; i < overlays.Count; i++) {
            ResolvedPreviewOverlay overlay = overlays[i];
            if (overlay.MaskTexture == null || overlay.BaseTexture == null)
                continue;

            Texture2D processedOverlay = TransformationPaletteTextureCache.GetProcessedOverlayTexture(
                overlay.BaseTexture,
                overlay.MaskTexture,
                overlay.Settings,
                overlay.UsePaletteColor
            );
            if (processedOverlay == null)
                continue;

            spriteBatch.Draw(processedOverlay, drawPosition, ResolvePreviewFrame(processedOverlay),
                Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
        }
    }

    private static bool TryGetPreviewTexture(string texturePath, out Texture2D texture) {
        texture = null;
        if (Main.dedServ || string.IsNullOrWhiteSpace(texturePath))
            return false;

        try {
            texture = ModContent.Request<Texture2D>(texturePath).Value;
            return texture != null;
        }
        catch {
            return false;
        }
    }

    private static Rectangle ResolvePreviewFrame(Texture2D texture) {
        if (texture == null)
            return Rectangle.Empty;

        const int previewFrameWidth = 40;
        const int previewFrameHeight = 56;
        if (texture.Width >= previewFrameWidth && texture.Height >= previewFrameHeight &&
            texture.Width % previewFrameWidth == 0 && texture.Height % previewFrameHeight == 0) {
            return new Rectangle(0, 0, previewFrameWidth, previewFrameHeight);
        }

        return new Rectangle(0, 0, texture.Width, texture.Height);
    }

    private static int ComparePreviewLayerOrder(string leftPath, string rightPath) {
        int leftOrder = GetPreviewLayerOrder(leftPath);
        int rightOrder = GetPreviewLayerOrder(rightPath);
        if (leftOrder != rightOrder)
            return leftOrder.CompareTo(rightOrder);

        return string.Compare(leftPath, rightPath, StringComparison.OrdinalIgnoreCase);
    }

    private static int GetPreviewLayerOrder(string texturePath) {
        if (string.IsNullOrWhiteSpace(texturePath))
            return 99;

        if (texturePath.EndsWith("_Back", StringComparison.OrdinalIgnoreCase))
            return 0;

        if (texturePath.EndsWith("_Waist", StringComparison.OrdinalIgnoreCase))
            return 1;

        if (texturePath.EndsWith("_Legs", StringComparison.OrdinalIgnoreCase))
            return 2;

        if (texturePath.EndsWith("_Body", StringComparison.OrdinalIgnoreCase))
            return 3;

        if (texturePath.EndsWith("_Head", StringComparison.OrdinalIgnoreCase))
            return 4;

        return 5;
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
    private UIList sliderList;
    private UIScrollbar sliderScrollbar;
    private PaletteByteSlider redSlider;
    private PaletteByteSlider greenSlider;
    private PaletteByteSlider blueSlider;
    private PaletteByteSlider hueSlider;
    private PaletteByteSlider saturationSlider;
    private UITextPanel<string> paletteToggleButton;
    private UITextPanel<string> applyButton;
    private UITextPanel<string> resetChannelButton;
    private UITextPanel<string> resetAllButton;

    private string _currentTransformationId = string.Empty;
    private string _currentChannelSignature = string.Empty;
    private string _currentPreviewBaseSignature = string.Empty;
    private string _currentChannelEnabledSignature = string.Empty;
    private string _selectedChannelId = string.Empty;
    private bool _selectedChannelPaletteEnabled = true;
    private bool _suppressSliderCallbacks;
    private bool _hasPendingPaletteChanges;
    private readonly List<TransformationPaletteChannel> _activeChannels = new();
    private readonly List<string> _activePreviewBaseTexturePaths = new();
    private readonly Dictionary<string, Color> _pendingColors = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, byte> _pendingHueValues = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, byte> _pendingSaturationValues = new(StringComparer.OrdinalIgnoreCase);

    public override void OnInitialize() {
        mainPanel = new UIPanel();
        mainPanel.Width.Set(980f, 0f);
        mainPanel.Height.Set(714f, 0f);
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
        channelsPanel.Height.Set(512f, 0f);
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
        controlsPanel.Height.Set(512f, 0f);
        controlsPanel.Left.Set(366f, 0f);
        controlsPanel.Top.Set(126f, 0f);
        mainPanel.Append(controlsPanel);

        selectedChannelText = new UIText("No part selected", 1.05f);
        selectedChannelText.Left.Set(18f, 0f);
        selectedChannelText.Top.Set(16f, 0f);
        controlsPanel.Append(selectedChannelText);

        previewSwatch = new PalettePreviewSwatch {
            ResolveColor = GetSelectedPendingColor,
            ResolveLabel = GetSelectedChannelPreviewLabel,
            ResolveBaseTexturePaths = () => _activePreviewBaseTexturePaths,
            ResolveChannels = () => _activeChannels,
            ResolveChannelSettings = GetPendingSettings,
            ResolveChannelEnabled = IsPaletteChannelEnabled
        };
        previewSwatch.Left.Set(18f, 0f);
        previewSwatch.Top.Set(54f, 0f);
        previewSwatch.Width.Set(554f, 0f);
        previewSwatch.Height.Set(156f, 0f);
        controlsPanel.Append(previewSwatch);

        UIPanel sliderPanel = new UIPanel();
        sliderPanel.Left.Set(18f, 0f);
        sliderPanel.Top.Set(226f, 0f);
        sliderPanel.Width.Set(554f, 0f);
        sliderPanel.Height.Set(188f, 0f);
        sliderPanel.PaddingTop = 8f;
        sliderPanel.PaddingBottom = 8f;
        sliderPanel.PaddingLeft = 8f;
        sliderPanel.PaddingRight = 8f;
        controlsPanel.Append(sliderPanel);

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

        paletteToggleButton = CreateActionButton("Use Original", 18f, 442f, (_, _) => TogglePaletteEnabled(), width: 131f);
        applyButton = CreateActionButton("Apply Changes", 159f, 442f, (_, _) => ApplyPendingColors(), width: 131f);
        resetChannelButton = CreateActionButton("Reset Part", 300f, 442f, (_, _) => ResetSelectedPendingColor(), width: 131f);
        resetAllButton = CreateActionButton("Reset All", 441f, 442f, (_, _) => ResetAllPendingColors(), width: 131f);
        controlsPanel.Append(paletteToggleButton);
        controlsPanel.Append(applyButton);
        controlsPanel.Append(resetChannelButton);
        controlsPanel.Append(resetAllButton);

        UITextPanel<string> closeButton = CreateActionButton("Close", 786f, 654f, (_, _) => {
            CommitPendingColors();
            ModContent.GetInstance<UISystem>().HideMyUI();
            Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().showingUI = false;
        }, width: 170f);
        mainPanel.Append(closeButton);
    }

    public override void OnActivate() {
        base.OnActivate();
        if (mainPanel == null)
            return;
        RefreshPaletteContext(force: true);
    }

    public override void OnDeactivate() {
        CommitPendingColors();
        base.OnDeactivate();
    }

    public override void Update(GameTime gameTime) {
        base.Update(gameTime);
        if (mainPanel == null)
            return;
        RefreshPaletteContext(force: false);

        if (mainPanel.ContainsPoint(Main.MouseScreen))
            Main.LocalPlayer.mouseInterface = true;
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
        IReadOnlyList<TransformationPaletteChannel> channels = targetTransformation?.GetPaletteChannels(omp)
            ?.Where(channel => channel != null && channel.IsValid)
            .ToArray() ?? Array.Empty<TransformationPaletteChannel>();
        IReadOnlyList<string> previewBaseTexturePaths = targetTransformation?.GetPalettePreviewBaseTexturePaths(omp)
            ?.Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? Array.Empty<string>();
        string channelSignature = BuildChannelSignature(channels);
        string previewBaseSignature = BuildPreviewBaseSignature(previewBaseTexturePaths);
        string channelEnabledSignature = BuildChannelEnabledSignature(targetTransformation, channels, omp);
        bool channelContentChanged = force || targetTransformationId != _currentTransformationId ||
            channelSignature != _currentChannelSignature;
        bool previewChanged = channelContentChanged || previewBaseSignature != _currentPreviewBaseSignature;
        bool channelStateChanged = channelContentChanged || channelEnabledSignature != _currentChannelEnabledSignature;

        if (!previewChanged && !channelStateChanged)
            return;

        _currentTransformationId = targetTransformationId;
        _currentChannelSignature = channelSignature;
        _currentPreviewBaseSignature = previewBaseSignature;
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

        if (previewChanged) {
            _activePreviewBaseTexturePaths.Clear();
            _activePreviewBaseTexturePaths.AddRange(previewBaseTexturePaths);
        }

        _selectedChannelPaletteEnabled = IsPaletteChannelEnabled(_selectedChannelId);
        if (channelStateChanged)
            RebuildChannelButtons();

        UpdateHeaderState(omp, targetTransformation);
        if (channelContentChanged)
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

        statusText.SetText(_selectedChannelPaletteEnabled
            ? "Select a custom part, adjust the sliders, then apply your changes."
            : $"{GetSelectedChannelDisplayName()} is using the original texture. Hue and saturation still apply while custom RGB stays stored for later.");
        selectedChannelText.SetText(GetSelectedChannelDisplayName());
        SetControlsInteractive(true);
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
        UpdateHeaderState(Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>(),
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
        UpdateHeaderState(omp, omp.GetPaletteTargetTransformation());
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

    private string GetSelectedChannelPreviewLabel() {
        return string.IsNullOrWhiteSpace(_selectedChannelId)
            ? "Preview"
            : $"{GetSelectedChannelDisplayName()} Preview";
    }

    private bool IsPaletteChannelEnabled(string channelId) {
        if (string.IsNullOrWhiteSpace(_currentTransformationId) || string.IsNullOrWhiteSpace(channelId))
            return false;

        Player localPlayer = Main.LocalPlayer;
        if (localPlayer == null || !localPlayer.active)
            return true;

        return localPlayer.GetModPlayer<OmnitrixPlayer>().IsPaletteChannelEnabled(_currentTransformationId, channelId);
    }

    private static string BuildPreviewBaseSignature(IReadOnlyList<string> previewBaseTexturePaths) {
        if (previewBaseTexturePaths == null || previewBaseTexturePaths.Count == 0)
            return string.Empty;

        StringBuilder builder = new();
        for (int i = 0; i < previewBaseTexturePaths.Count; i++)
            builder.Append(previewBaseTexturePaths[i]).Append('|');
        return builder.ToString();
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
