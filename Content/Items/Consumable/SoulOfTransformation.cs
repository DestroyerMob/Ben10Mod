using System.IO;
using Ben10Mod.Content.Transformations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Ben10Mod.Content.Items.Consumable;

public enum SoulOfTransformationSource : byte {
    None,
    Boss,
    Event
}

public class SoulOfTransformation : ModItem {
    private const string TransformationIdTag = "transformationId";
    private const string SourceTag = "source";

    public string TransformationId { get; private set; } = string.Empty;
    public SoulOfTransformationSource UnlockSource { get; private set; } = SoulOfTransformationSource.None;

    public bool HasTransformationUnlock => !string.IsNullOrWhiteSpace(TransformationId);

    public override ModItem Clone(Item item) {
        SoulOfTransformation clone = (SoulOfTransformation)base.Clone(item);
        clone.TransformationId = TransformationId;
        clone.UnlockSource = UnlockSource;
        return clone;
    }

    public override void SetStaticDefaults() {
        Item.ResearchUnlockCount = 1;
        ItemID.Sets.ItemIconPulse[Type] = true;
        ItemID.Sets.ItemNoGravity[Type] = true;
    }

    public override void SetDefaults() {
        Item.width = 20;
        Item.height = 20;
        Item.maxStack = Item.CommonMaxStack;
        Item.rare = ItemRarityID.LightPurple;
        Item.value = Item.buyPrice(gold: 1);
    }

    public void SetTransformationUnlock(string transformationId, SoulOfTransformationSource source) {
        Transformation transformation = TransformationLoader.Resolve(transformationId);
        TransformationId = transformation?.FullID ?? (transformationId ?? string.Empty).Trim();
        UnlockSource = string.IsNullOrWhiteSpace(TransformationId) ? SoulOfTransformationSource.None : source;
    }

    public override void SaveData(TagCompound tag) {
        if (!HasTransformationUnlock)
            return;

        tag[TransformationIdTag] = TransformationId;
        tag[SourceTag] = (byte)UnlockSource;
    }

    public override void LoadData(TagCompound tag) {
        TransformationId = tag.TryGet(TransformationIdTag, out string transformationId)
            ? transformationId
            : string.Empty;
        UnlockSource = tag.TryGet(SourceTag, out byte source)
            ? (SoulOfTransformationSource)source
            : SoulOfTransformationSource.None;
    }

    public override void NetSend(BinaryWriter writer) {
        writer.Write(TransformationId ?? string.Empty);
        writer.Write((byte)UnlockSource);
    }

    public override void NetReceive(BinaryReader reader) {
        TransformationId = reader.ReadString();
        UnlockSource = (SoulOfTransformationSource)reader.ReadByte();
    }

    public override bool ItemSpace(Player player) {
        return HasTransformationUnlock || base.ItemSpace(player);
    }

    public override bool CanPickup(Player player) {
        if (HasTransformationUnlock && Main.netMode == NetmodeID.Server)
            TryGrantTransformation(player, showFeedback: false);

        return base.CanPickup(player);
    }

    public override bool OnPickup(Player player) {
        if (!HasTransformationUnlock)
            return true;

        TryGrantTransformation(player, showFeedback: true);
        return false;
    }

    public override void PostUpdate() {
        Lighting.AddLight(Item.Center, Color.LimeGreen.ToVector3() * 0.45f * Main.essScale);
    }

    public static int Spawn(IEntitySource source, Rectangle area, string transformationId,
        SoulOfTransformationSource unlockSource) {
        if (string.IsNullOrWhiteSpace(transformationId))
            return -1;

        int itemIndex = Item.NewItem(source, area, ModContent.ItemType<SoulOfTransformation>(), noBroadcast: true);
        if (itemIndex < 0 || itemIndex >= Main.maxItems)
            return itemIndex;

        if (Main.item[itemIndex].ModItem is SoulOfTransformation soul)
            soul.SetTransformationUnlock(transformationId, unlockSource);

        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex);

        return itemIndex;
    }

    public static int Spawn(IEntitySource source, Vector2 center, string transformationId,
        SoulOfTransformationSource unlockSource) {
        Rectangle area = new((int)center.X - 10, (int)center.Y - 10, 20, 20);
        return Spawn(source, area, transformationId, unlockSource);
    }

    private void TryGrantTransformation(Player player, bool showFeedback) {
        if (player == null)
            return;

        Transformation transformation = TransformationLoader.Resolve(TransformationId);
        if (transformation == null)
            return;

        OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
        if (omp.IsTransformationUnlocked(transformation)) {
            ShowAlreadyUnlockedMessage(player, showFeedback);
            return;
        }

        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        bool showUnlockEffects = showFeedback && Main.netMode != NetmodeID.Server && player.whoAmI == Main.myPlayer;
        omp.UnlockTransformation(transformation.FullID, sync: Main.netMode == NetmodeID.Server,
            showEffects: showUnlockEffects);
    }

    private void ShowAlreadyUnlockedMessage(Player player, bool showFeedback) {
        if (!showFeedback || Main.netMode == NetmodeID.Server || player.whoAmI != Main.myPlayer)
            return;

        string message = UnlockSource == SoulOfTransformationSource.Event
            ? "You have survived this ordeal already."
            : "You have slain this foe before.";
        Main.NewText(message, new Color(170, 235, 190));
    }
}
