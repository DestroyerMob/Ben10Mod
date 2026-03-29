using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod.Content.Transformations {
    public abstract class TransformationCostume : ModType {
        private sealed class CostumeHeadTexture : EquipTexture {
            public override bool IsVanitySet(int head, int body, int legs) => true;
        }

        private readonly Dictionary<EquipType, int> _registeredSlots = new();

        public virtual string CostumeName => Name;
        public virtual string FullID => $"{Mod.Name}:{CostumeName}";
        public abstract string TargetTransformationId { get; }
        public virtual string DisplayName => CostumeName;
        public virtual string Description => "An alternate look for this transformation.";
        public virtual string IconPath => string.Empty;
        public virtual int SortOrder => 0;
        public virtual bool MergeTransformationPaletteChannels => true;
        public virtual IReadOnlyList<TransformationPaletteChannel> PaletteChannels => Array.Empty<TransformationPaletteChannel>();

        protected virtual string HeadTexturePath => string.Empty;
        protected virtual string BodyTexturePath => string.Empty;
        protected virtual string LegsTexturePath => string.Empty;
        protected virtual string BackTexturePath => string.Empty;
        protected virtual string WaistTexturePath => string.Empty;
        protected virtual string WingsTexturePath => string.Empty;
        protected virtual string HandOffTexturePath => string.Empty;
        protected virtual string HandOnTexturePath => string.Empty;
        protected virtual string ShieldTexturePath => string.Empty;
        protected virtual string NeckTexturePath => string.Empty;
        protected virtual string FaceTexturePath => string.Empty;
        protected virtual string FrontTexturePath => string.Empty;

        protected virtual bool DrawHead => false;
        protected virtual bool HideTopSkin => true;
        protected virtual bool HideArms => true;
        protected virtual bool HideBottomSkin => true;
        protected virtual bool DrawBackInTailLayer => false;

        public virtual Asset<Texture2D> GetIcon() {
            if (!string.IsNullOrWhiteSpace(IconPath))
                return ModContent.Request<Texture2D>(IconPath);

            Transformation transformation = TransformationLoader.Resolve(TargetTransformationId);
            return transformation?.GetTransformationIcon();
        }

        public virtual void ApplyVisuals(Player player, OmnitrixPlayer omp, Transformation transformation) {
            ApplyVisualSlot(player, EquipType.Head, slot => player.head = slot);
            ApplyVisualSlot(player, EquipType.Body, slot => player.body = slot);
            ApplyVisualSlot(player, EquipType.Legs, slot => player.legs = slot);
            ApplyVisualSlot(player, EquipType.Back, slot => player.back = slot);
            ApplyVisualSlot(player, EquipType.Waist, slot => player.waist = slot);
            ApplyVisualSlot(player, EquipType.Wings, slot => player.wings = slot);
            ApplyVisualSlot(player, EquipType.HandsOff, slot => player.handoff = slot);
            ApplyVisualSlot(player, EquipType.HandsOn, slot => player.handon = slot);
            ApplyVisualSlot(player, EquipType.Shield, slot => player.shield = slot);
            ApplyVisualSlot(player, EquipType.Neck, slot => player.neck = slot);
            ApplyVisualSlot(player, EquipType.Face, slot => player.face = slot);
            ApplyVisualSlot(player, EquipType.Front, slot => player.front = slot);
        }

        internal IReadOnlyList<TransformationPaletteChannel> GetMergedPaletteChannels(Transformation transformation,
            OmnitrixPlayer omp) {
            IReadOnlyList<TransformationPaletteChannel> costumeChannels = PaletteChannels ?? Array.Empty<TransformationPaletteChannel>();
            IReadOnlyList<TransformationPaletteChannel> baseChannels =
                transformation?.PaletteChannels ?? Array.Empty<TransformationPaletteChannel>();

            if (!MergeTransformationPaletteChannels || baseChannels.Count == 0)
                return costumeChannels;

            if (costumeChannels.Count == 0)
                return baseChannels;

            List<TransformationPaletteChannel> mergedChannels = new(baseChannels.Count + costumeChannels.Count);
            Dictionary<string, int> channelIndexById = new(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < baseChannels.Count; i++) {
                TransformationPaletteChannel channel = baseChannels[i];
                if (channel == null || !channel.IsValid)
                    continue;

                channelIndexById[channel.Id] = mergedChannels.Count;
                mergedChannels.Add(channel);
            }

            for (int i = 0; i < costumeChannels.Count; i++) {
                TransformationPaletteChannel channel = costumeChannels[i];
                if (channel == null || !channel.IsValid)
                    continue;

                if (channelIndexById.TryGetValue(channel.Id, out int existingIndex))
                    mergedChannels[existingIndex] = channel;
                else {
                    channelIndexById[channel.Id] = mergedChannels.Count;
                    mergedChannels.Add(channel);
                }
            }

            return mergedChannels;
        }

        internal TransformationPaletteChannel GetMergedPaletteChannel(Transformation transformation, string channelId,
            OmnitrixPlayer omp) {
            if (string.IsNullOrWhiteSpace(channelId))
                return null;

            IReadOnlyList<TransformationPaletteChannel> channels = GetMergedPaletteChannels(transformation, omp);
            for (int i = 0; i < channels.Count; i++) {
                TransformationPaletteChannel channel = channels[i];
                if (channel != null &&
                    string.Equals(channel.Id, channelId, StringComparison.OrdinalIgnoreCase))
                    return channel;
            }

            return null;
        }

        public sealed override void Load() {
            if (Main.netMode != NetmodeID.Server)
                RegisterConfiguredEquipTextures();
            OnCostumeLoad();
        }

        public sealed override void Unload() {
            _registeredSlots.Clear();
            OnCostumeUnload();
        }

        protected virtual void OnCostumeLoad() {
        }

        protected virtual void OnCostumeUnload() {
        }

        protected sealed override void Register() {
            TransformationCostumeLoader.Register(this);
        }

        public sealed override void SetupContent() {
            SetStaticDefaults();
            ApplyConfiguredDrawSettings();
        }

        protected int GetRegisteredSlot(EquipType equipType) {
            return _registeredSlots.TryGetValue(equipType, out int slot)
                ? slot
                : -1;
        }

        protected virtual EquipTexture CreateEquipTexture(EquipType equipType) {
            return equipType == EquipType.Head ? new CostumeHeadTexture() : null;
        }

        private void RegisterConfiguredEquipTextures() {
            RegisterConfiguredEquipTexture(EquipType.Head, HeadTexturePath);
            RegisterConfiguredEquipTexture(EquipType.Body, BodyTexturePath);
            RegisterConfiguredEquipTexture(EquipType.Legs, LegsTexturePath);
            RegisterConfiguredEquipTexture(EquipType.Back, BackTexturePath);
            RegisterConfiguredEquipTexture(EquipType.Waist, WaistTexturePath);
            RegisterConfiguredEquipTexture(EquipType.Wings, WingsTexturePath);
            RegisterConfiguredEquipTexture(EquipType.HandsOff, HandOffTexturePath);
            RegisterConfiguredEquipTexture(EquipType.HandsOn, HandOnTexturePath);
            RegisterConfiguredEquipTexture(EquipType.Shield, ShieldTexturePath);
            RegisterConfiguredEquipTexture(EquipType.Neck, NeckTexturePath);
            RegisterConfiguredEquipTexture(EquipType.Face, FaceTexturePath);
            RegisterConfiguredEquipTexture(EquipType.Front, FrontTexturePath);
        }

        private void RegisterConfiguredEquipTexture(EquipType equipType, string texturePath) {
            if (string.IsNullOrWhiteSpace(texturePath))
                return;

            int slot = EquipLoader.AddEquipTexture(Mod, texturePath, equipType, item: null,
                name: $"{CostumeName}_{equipType}", equipTexture: CreateEquipTexture(equipType));
            if (slot >= 0)
                _registeredSlots[equipType] = slot;
        }

        private void ApplyConfiguredDrawSettings() {
            if (Main.netMode == NetmodeID.Server)
                return;

            int headSlot = GetRegisteredSlot(EquipType.Head);
            if (headSlot >= 0)
                ArmorIDs.Head.Sets.DrawHead[headSlot] = DrawHead;

            int bodySlot = GetRegisteredSlot(EquipType.Body);
            if (bodySlot >= 0) {
                ArmorIDs.Body.Sets.HidesTopSkin[bodySlot] = HideTopSkin;
                ArmorIDs.Body.Sets.HidesArms[bodySlot] = HideArms;
            }

            int legsSlot = GetRegisteredSlot(EquipType.Legs);
            if (legsSlot >= 0)
                ArmorIDs.Legs.Sets.HidesBottomSkin[legsSlot] = HideBottomSkin;

            int backSlot = GetRegisteredSlot(EquipType.Back);
            if (backSlot >= 0)
                ArmorIDs.Back.Sets.DrawInTailLayer[backSlot] = DrawBackInTailLayer;
        }

        private void ApplyVisualSlot(Player player, EquipType equipType, Action<int> applyAction) {
            int slot = GetRegisteredSlot(equipType);
            if (slot >= 0)
                applyAction(slot);
        }
    }
}
