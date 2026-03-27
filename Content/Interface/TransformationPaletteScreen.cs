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
    private enum CustomizationTab {
        Palette,
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

    private static PaletteClipboardState s_paletteClipboard;

    internal static void ClearSharedState() {
        s_paletteClipboard = null;
    }

    private UIPanel mainPanel;
    private UIText titleText;
    private UIText targetText;
    private UIText statusText;
    private UITextPanel<string> paletteTabButton;
    private UITextPanel<string> customNamesTabButton;
    private UIElement paletteContentRoot;
    private UIElement customNamesContentRoot;
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
    private UITextPanel<string> copyPaletteButton;
    private UITextPanel<string> pastePaletteButton;
    private UITextPanel<string> paletteToggleButton;
    private UITextPanel<string> applyButton;
    private UITextPanel<string> resetChannelButton;
    private UITextPanel<string> resetAllButton;
    private UIText palettePresetHintText;
    private readonly List<UITextPanel<string>> palettePresetButtons = new();
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
    private string _currentPreviewBaseSignature = string.Empty;
    private string _currentChannelEnabledSignature = string.Empty;
    private string _currentCustomNameSignature = string.Empty;
    private string _selectedChannelId = string.Empty;
    private string _selectedCustomNameTransformationId = string.Empty;
    private string _loadedCustomNameValue = string.Empty;
    private bool _selectedChannelPaletteEnabled = true;
    private bool _suppressSliderCallbacks;
    private bool _hasPendingPaletteChanges;
    private bool _suppressCustomNameCallbacks;
    private bool _hasPendingCustomNameChanges;
    private readonly List<TransformationPaletteChannel> _activeChannels = new();
    private readonly List<string> _activePreviewBaseTexturePaths = new();
    private readonly List<string> _availableCustomNameTransformationIds = new();
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

        paletteContentRoot = new UIElement();
        paletteContentRoot.Width.Set(0f, 1f);
        paletteContentRoot.Height.Set(0f, 1f);
        paletteContentRoot.Left.Set(0f, 0f);
        mainPanel.Append(paletteContentRoot);

        customNamesContentRoot = new UIElement();
        customNamesContentRoot.Width.Set(0f, 1f);
        customNamesContentRoot.Height.Set(0f, 1f);
        customNamesContentRoot.Left.Set(-1600f, 0f);
        mainPanel.Append(customNamesContentRoot);

        titleText = new UIText("Alien Customization", 1.35f);
        titleText.Left.Set(24f, 0f);
        titleText.Top.Set(20f, 0f);
        mainPanel.Append(titleText);

        paletteTabButton = CreateActionButton("Palette", 686f, 20f, (_, _) => SetActiveTab(CustomizationTab.Palette), width: 120f);
        customNamesTabButton = CreateActionButton("Custom Names", 816f, 20f,
            (_, _) => SetActiveTab(CustomizationTab.CustomNames), width: 140f);
        mainPanel.Append(paletteTabButton);
        mainPanel.Append(customNamesTabButton);

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
        paletteContentRoot.Append(channelsPanel);

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
        paletteContentRoot.Append(controlsPanel);

        selectedChannelText = new UIText("No part selected", 1.05f);
        selectedChannelText.Left.Set(18f, 0f);
        selectedChannelText.Top.Set(16f, 0f);
        controlsPanel.Append(selectedChannelText);

        copyPaletteButton = CreateActionButton("Copy Palette", 336f, 12f, (_, _) => CopyCurrentPalette(), width: 110f);
        pastePaletteButton = CreateActionButton("Paste Palette", 454f, 12f, (_, _) => PastePalette(), width: 118f);
        controlsPanel.Append(copyPaletteButton);
        controlsPanel.Append(pastePaletteButton);

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
        sliderPanel.Top.Set(222f, 0f);
        sliderPanel.Width.Set(554f, 0f);
        sliderPanel.Height.Set(164f, 0f);
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

        paletteToggleButton = CreateActionButton("Use Original", 18f, 398f, (_, _) => TogglePaletteEnabled(), width: 131f);
        applyButton = CreateActionButton("Apply Changes", 159f, 398f, (_, _) => ApplyPendingColors(), width: 131f);
        resetChannelButton = CreateActionButton("Reset Part", 300f, 398f, (_, _) => ResetSelectedPendingColor(), width: 131f);
        resetAllButton = CreateActionButton("Reset All", 441f, 398f, (_, _) => ResetAllPendingColors(), width: 131f);
        controlsPanel.Append(paletteToggleButton);
        controlsPanel.Append(applyButton);
        controlsPanel.Append(resetChannelButton);
        controlsPanel.Append(resetAllButton);

        palettePresetHintText = new UIText(
            "Left click loads a preset. Right click saves the current colours and mask toggles.",
            0.74f);
        palettePresetHintText.Left.Set(18f, 0f);
        palettePresetHintText.Top.Set(438f, 0f);
        palettePresetHintText.Width.Set(554f, 0f);
        controlsPanel.Append(palettePresetHintText);

        const float presetButtonWidth = 176f;
        const float presetButtonSpacing = 13f;
        for (int presetIndex = 0; presetIndex < OmnitrixPlayer.PalettePresetSlotCount; presetIndex++) {
            int capturedPresetIndex = presetIndex;
            UITextPanel<string> presetButton = CreateActionButton($"Preset {presetIndex + 1}", 18f + presetIndex * (presetButtonWidth + presetButtonSpacing),
                464f, (_, _) => LoadPalettePreset(capturedPresetIndex), width: presetButtonWidth);
            presetButton.OnRightClick += (_, _) => SavePalettePreset(capturedPresetIndex);
            palettePresetButtons.Add(presetButton);
            controlsPanel.Append(presetButton);
        }

        UIPanel customNameListPanel = new UIPanel();
        customNameListPanel.Width.Set(320f, 0f);
        customNameListPanel.Height.Set(512f, 0f);
        customNameListPanel.Left.Set(24f, 0f);
        customNameListPanel.Top.Set(126f, 0f);
        customNamesContentRoot.Append(customNameListPanel);

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

        UIPanel customNameControlsPanel = new UIPanel();
        customNameControlsPanel.Width.Set(590f, 0f);
        customNameControlsPanel.Height.Set(512f, 0f);
        customNameControlsPanel.Left.Set(366f, 0f);
        customNameControlsPanel.Top.Set(126f, 0f);
        customNamesContentRoot.Append(customNameControlsPanel);

        selectedNameText = new UIText("No transformation selected", 1.05f);
        selectedNameText.Left.Set(18f, 0f);
        selectedNameText.Top.Set(16f, 0f);
        customNameControlsPanel.Append(selectedNameText);

        originalNameText = new UIText("Original Name: --", 0.92f);
        originalNameText.Left.Set(18f, 0f);
        originalNameText.Top.Set(48f, 0f);
        customNameControlsPanel.Append(originalNameText);

        UIPanel customNamePreviewPanel = new UIPanel();
        customNamePreviewPanel.Left.Set(18f, 0f);
        customNamePreviewPanel.Top.Set(82f, 0f);
        customNamePreviewPanel.Width.Set(554f, 0f);
        customNamePreviewPanel.Height.Set(118f, 0f);
        customNameControlsPanel.Append(customNamePreviewPanel);

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
        customNameControlsPanel.Append(inputLabel);

        customNameInput = new CustomNameTextInputPanel("Leave blank to use the original alien name") {
            MaxLength = OmnitrixPlayer.MaxCustomTransformationNameLength
        };
        customNameInput.Left.Set(18f, 0f);
        customNameInput.Top.Set(250f, 0f);
        customNameInput.Width.Set(554f, 0f);
        customNameInput.Height.Set(52f, 0f);
        customNameInput.TextChanged += _ => OnCustomNameInputChanged();
        customNameInput.Submitted += _ => CommitSelectedCustomName();
        customNameControlsPanel.Append(customNameInput);

        customNameHintText = new UIText("Custom names save with your player data. Press Enter, Apply Name, or close the screen to save.",
            0.88f);
        customNameHintText.Left.Set(18f, 0f);
        customNameHintText.Top.Set(318f, 0f);
        customNameHintText.Width.Set(540f, 0f);
        customNameHintText.IsWrapped = true;
        customNameControlsPanel.Append(customNameHintText);

        applyNameButton = CreateActionButton("Apply Name", 18f, 442f, (_, _) => CommitSelectedCustomName(), width: 170f);
        resetNameButton = CreateActionButton("Use Original Name", 198f, 442f,
            (_, _) => ResetSelectedCustomName(), width: 190f);
        customNameControlsPanel.Append(applyNameButton);
        customNameControlsPanel.Append(resetNameButton);

        UITextPanel<string> closeButton = CreateActionButton("Close", 786f, 654f, (_, _) => {
            CommitPendingColors();
            CommitSelectedCustomName();
            ModContent.GetInstance<UISystem>().HideMyUI();
            Main.LocalPlayer.GetModPlayer<OmnitrixPlayer>().showingUI = false;
        }, width: 170f);
        mainPanel.Append(closeButton);

        UpdateTabButtonState();
    }

    public override void OnActivate() {
        base.OnActivate();
        if (mainPanel == null)
            return;
        RefreshPaletteContext(force: true);
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

        RefreshPaletteContext(force: false);
        RefreshCustomNameContext(force: false);

        if (mainPanel.ContainsPoint(Main.MouseScreen))
            Main.LocalPlayer.mouseInterface = true;
    }

    private void SetActiveTab(CustomizationTab tab, bool refreshState = true) {
        if (_activeTab == tab && !refreshState)
            return;

        CommitSelectedCustomName();
        customNameInput?.SetFocused(false);
        _activeTab = tab;

        float paletteLeft = tab == CustomizationTab.Palette ? 0f : -1600f;
        float customNamesLeft = tab == CustomizationTab.CustomNames ? 0f : -1600f;
        paletteContentRoot.Left.Set(paletteLeft, 0f);
        customNamesContentRoot.Left.Set(customNamesLeft, 0f);
        paletteContentRoot.Recalculate();
        customNamesContentRoot.Recalculate();

        UpdateTabButtonState();

        if (!refreshState)
            return;

        RefreshPaletteContext(force: true);
        RefreshCustomNameContext(force: true);
    }

    private void UpdateTabButtonState() {
        UpdateTabButtonVisual(paletteTabButton, _activeTab == CustomizationTab.Palette);
        UpdateTabButtonVisual(customNamesTabButton, _activeTab == CustomizationTab.CustomNames);
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
