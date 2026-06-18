using System.IO;
using System;
using Ben10Mod.Common.Absorption;
using Ben10Mod.Common.Networking;
using Ben10Mod.Common.Systems;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Vanity.ShaderDyes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod {
	public class Ben10Mod : Mod {
		public static bool IsUnloading { get; private set; }

		public override object Call(params object[] args) {
			if (args.Length == 0 || args[0] is not string command)
				throw new ArgumentException("Ben10Mod.Call requires a command string as the first argument.");

			return command switch {
				"RegisterAbsorbableMaterial" => CallRegisterAbsorbableMaterial(args),
				"RegisterTransformationUnlockCondition" => CallRegisterTransformationUnlockCondition(args),
				"BlacklistTransformation" => CallBlacklistTransformation(args),
				"BlacklistFeature" => CallBlacklistFeature(args),
				"GetTransformationUnlockCondition" => args.Length >= 2 && args[1] is string transformationId
					? TransformationUnlockConditionRegistry.Get(transformationId)
					: string.Empty,
				"IsTransformationBlacklisted" => args.Length >= 2 && args[1] is string blacklistedTransformationId && Ben10FeatureBlacklistRegistry.IsTransformationBlacklisted(blacklistedTransformationId),
				"IsFeatureBlacklisted" => CallIsFeatureBlacklisted(args),
				"IsAbsorbableMaterialRegistered" => args.Length >= 2 && args[1] is int itemType &&
				                                    MaterialAbsorptionRegistry.IsRegistered(itemType),
				"GetAbsorbableMaterialProfile" => args.Length >= 2 && args[1] is int profileItemType &&
				                                  MaterialAbsorptionRegistry.TryGetProfile(profileItemType,
					                                  out MaterialAbsorptionProfile profile)
					? profile
					: null,
				_ => throw new ArgumentException($"Unknown Ben10Mod.Call command '{command}'.")
			};
		}

		public override void Load() {
			IsUnloading = false;
			Ben10FeatureBlacklistRegistry.Clear();
			TransformationUnlockConditionRegistry.Clear();
			TransformationUnlockConditionRegistry.RegisterBaseConditions();

			if (ModLoader.TryGetMod("ColoredDamageTypes", out Mod coloreddamagetypes)) {
				//Color version
				coloreddamagetypes.Call("AddDamageType", ModContent.GetInstance<HeroDamage>(), new Color(0, 200, 00),
					new Color(0, 200, 0), new Color(0, 255, 0));
			}

			if (Main.netMode != NetmodeID.Server) {
				Asset<Effect> dyeShader    = this.Assets.Request<Effect>("Effects/MyDyes");
				Asset<Effect> filterShader = this.Assets.Request<Effect>("Effects/MyFilters");


				GameShaders.Armor.BindShader(ModContent.ItemType<DiscoDye>(),
					new ArmorShaderData(dyeShader, "BasicTint"));


				Filters.Scene["Ben10Mod:Grayscale"] = new Filter(new ScreenShaderData(filterShader, "Grayscale"),
					EffectPriority.Medium);
				Filters.Scene["Ben10Mod:Bluescale"] = new Filter(new ScreenShaderData(filterShader, "Bluescale"),
					EffectPriority.Medium);
			}
		}

		public override void Unload() {
			IsUnloading = true;
		}

		private void TryUnloadStep(string stepName, Action action) {
			try {
				action?.Invoke();
			}
			catch (Exception ex) {
				Logger.Warn($"Ignoring unload error while clearing {stepName}: {ex}");
			}
		}

		private void TryUnloadSceneFilter(string filterKey) {
			try {
				var sceneFilters = Filters.Scene;
				if (sceneFilters == null)
					return;

				Filter filter = sceneFilters[filterKey];
				if (filter == null)
					return;

				if (filter.IsActive())
					sceneFilters.Deactivate(filterKey);
			}
			catch (Exception ex) {
				Logger.Warn($"Ignoring unload error while clearing scene filter '{filterKey}': {ex}");
			}
		}

		public enum MessageType : byte {
			UnlockTransformation,
			RemoveTransformation,
			RequestUnlockTransformation,
			RequestRemoveTransformation,
			RecordEventParticipation,
			SyncOmnitrixEvolution,
			RequestSyncTransformationState,
			SyncTransformationState,
			RequestSyncTransformationPaletteState,
			SyncTransformationPaletteState,
			RequestSyncTransformationSpeedBoostSetting,
			SyncTransformationSpeedBoostSetting,
			RequestAbsorbMaterial,
			AbsorbMaterialFeedback,
			SyncAbsorbedMaterial,
			RelayDodgeVisual,
			ExecuteAmpFibianPhaseShift,
			ExecuteBuzzShockTeleport,
			ExecuteEchoEchoShift,
			ExecuteUltimateEchoEchoRelay,
			RequestGhostFreakPossession,
			SyncGhostFreakPossessionState,
			RequestCompletedOmnitrixRevival,
			SyncCompletedOmnitrixRevival
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			OmnitrixPacketHandler.HandlePacket(this, reader, whoAmI);
		}


		private static object CallRegisterAbsorbableMaterial(object[] args) {
			if (args.Length < 6)
				throw new ArgumentException(
					"RegisterAbsorbableMaterial requires source, sword, helmet, body, and leg item IDs.");

			if (args[1] is not int sourceItemType || args[2] is not int swordItemType ||
			    args[3] is not int helmetItemType ||
			    args[4] is not int bodyItemType || args[5] is not int legItemType)
				throw new ArgumentException("RegisterAbsorbableMaterial item IDs must be ints.");

			var registration = MaterialAbsorptionRegistry.CreateRegistration(sourceItemType, swordItemType,
				helmetItemType, bodyItemType, legItemType);

			if (args.Length >= 7) {
				if (args[6] is Action<MaterialAbsorptionRegistration> configure)
					configure(registration);
				else
					throw new ArgumentException(
						"RegisterAbsorbableMaterial optional 7th argument must be an Action<MaterialAbsorptionRegistration>.");
			}

			MaterialAbsorptionRegistry.Register(registration);
			return null;
		}

		private static object CallRegisterTransformationUnlockCondition(object[] args) {
			if (args.Length < 3)
				throw new ArgumentException(
					"RegisterTransformationUnlockCondition requires a transformation ID and unlock condition text.");

			if (args[1] is not string transformationId || args[2] is not string unlockConditionText)
				throw new ArgumentException(
					"RegisterTransformationUnlockCondition arguments must be a transformation ID string and a condition string.");

			TransformationUnlockConditionRegistry.Register(transformationId, unlockConditionText);
			return null;
		}

		private static object CallBlacklistTransformation(object[] args) {
			if (args.Length < 2)
				throw new ArgumentException(
					"BlacklistTransformation requires one or more transformation IDs or mod IDs.");

			for (int i = 1; i < args.Length; i++) {
				if (args[i] is not string transformationIdOrModId)
					throw new ArgumentException(
						"BlacklistTransformation entries must be transformation ID strings or mod ID strings.");

				Ben10FeatureBlacklistRegistry.BlacklistTransformation(transformationIdOrModId);
			}

			return null;
		}

		private static object CallBlacklistFeature(object[] args) {
			if (args.Length < 3)
				throw new ArgumentException(
					"BlacklistFeature requires a feature type and one or more mod IDs or transformation IDs.");

			if (args[1] is not string featureKey ||
			    !Ben10FeatureBlacklistRegistry.TryParseFeatureType(featureKey, out Ben10FeatureType featureType))
				throw new ArgumentException(
					"BlacklistFeature feature type must be one of: Transformation, Omnitrix, PlumbersBadge, WorldGen.");

			for (int i = 2; i < args.Length; i++) {
				if (args[i] is not string identifier)
					throw new ArgumentException(
						"BlacklistFeature entries must be mod ID strings, or transformation IDs when using the Transformation feature type.");

				if (featureType == Ben10FeatureType.Transformation)
					Ben10FeatureBlacklistRegistry.BlacklistTransformation(identifier);
				else
					Ben10FeatureBlacklistRegistry.BlacklistFeature(featureType, identifier);
			}

			return null;
		}

		private static object CallIsFeatureBlacklisted(object[] args) {
			if (args.Length < 3)
				throw new ArgumentException(
					"IsFeatureBlacklisted requires a feature type and a mod ID or transformation ID.");

			if (args[1] is not string featureKey ||
			    !Ben10FeatureBlacklistRegistry.TryParseFeatureType(featureKey, out Ben10FeatureType featureType))
				throw new ArgumentException(
					"IsFeatureBlacklisted feature type must be one of: Transformation, Omnitrix, PlumbersBadge, WorldGen.");

			if (args[2] is not string identifier)
				throw new ArgumentException(
					"IsFeatureBlacklisted requires a mod ID string, or a transformation ID when checking Transformation.");

			return featureType == Ben10FeatureType.Transformation
				? Ben10FeatureBlacklistRegistry.IsTransformationBlacklisted(identifier)
				: Ben10FeatureBlacklistRegistry.IsFeatureBlacklisted(featureType, identifier);
		}
	}
}
