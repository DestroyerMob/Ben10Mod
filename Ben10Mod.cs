using System.IO;
using System;
using Ben10Mod.Common.Absorption;
using Ben10Mod.Content.DamageClasses;
using Ben10Mod.Content.Items.Armour;
using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.Transformations;
using Ben10Mod.Content.Transformations.BuzzShock;
using Ben10Mod.Content.Items.Vanity.ShaderDyes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod {
	public class Ben10Mod : Mod {
		public override object Call(params object[] args) {
			if (args.Length == 0 || args[0] is not string command)
				throw new ArgumentException("Ben10Mod.Call requires a command string as the first argument.");

			return command switch {
				"RegisterAbsorbableMaterial" => CallRegisterAbsorbableMaterial(args),
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

			if (ModLoader.TryGetMod("ColoredDamageTypes", out Mod coloreddamagetypes)) {
				//Color version
				coloreddamagetypes.Call("AddDamageType", ModContent.GetInstance<HeroDamage>(), new Color(0, 200, 00),
					new Color(0, 200, 0), new Color(0, 255, 0));
			}

			if (Main.netMode != NetmodeID.Server) {
				Asset<Effect> dyeShader = this.Assets.Request<Effect>("Effects/MyDyes");
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
			TransformationBranchRegistry.Clear();
		}

		public enum MessageType : byte {
			UnlockTransformation,
			RemoveTransformation,
			RequestUnlockTransformation,
			RequestRemoveTransformation,
			RequestSyncTransformationState,
			RequestAbsorbMaterial,
			AbsorbMaterialFeedback,
			SyncAbsorbedMaterial,
			RelayDodgeVisual,
			ExecuteBuzzShockTeleport,
			RequestGhostFreakPossession,
			SyncGhostFreakPossessionState
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			MessageType msgType = (MessageType)reader.ReadByte();

			switch (msgType) {
				case MessageType.UnlockTransformation: {
					int playerIndex = reader.ReadByte();
					string transformationId = reader.ReadString();

					if (Main.netMode != NetmodeID.MultiplayerClient)
						return;

					if (playerIndex < 0 || playerIndex >= Main.maxPlayers)
						return;

					Player player = Main.player[playerIndex];
					if (!player.active)
						return;

					player.GetModPlayer<OmnitrixPlayer>().UnlockTransformation(transformationId, sync: false,
						showEffects: playerIndex == Main.myPlayer);
					break;
				}
				case MessageType.RemoveTransformation: {
					int playerIndex = reader.ReadByte();
					string transformationId = reader.ReadString();

					if (Main.netMode != NetmodeID.MultiplayerClient)
						return;

					if (playerIndex < 0 || playerIndex >= Main.maxPlayers)
						return;

					Player player = Main.player[playerIndex];
					if (!player.active)
						return;

					player.GetModPlayer<OmnitrixPlayer>().RemoveTransformation(transformationId, sync: false,
						showEffects: playerIndex == Main.myPlayer);
					break;
				}
				case MessageType.RequestUnlockTransformation: {
					if (Main.netMode != NetmodeID.Server)
						return;

					if (whoAmI < 0 || whoAmI >= Main.maxPlayers)
						return;

					Player player = Main.player[whoAmI];
					if (!player.active)
						return;

					string transformationId = reader.ReadString();
					Transformation transformation = TransformationLoader.Resolve(transformationId);
					if (transformation == null)
						return;

					player.GetModPlayer<OmnitrixPlayer>()
						.UnlockTransformation(transformation.FullID, sync: true, showEffects: false);
					break;
				}
					case MessageType.RequestRemoveTransformation: {
						if (Main.netMode != NetmodeID.Server)
							return;

					if (whoAmI < 0 || whoAmI >= Main.maxPlayers)
						return;

					Player player = Main.player[whoAmI];
					if (!player.active)
						return;

					string transformationId = reader.ReadString();
					Transformation transformation = TransformationLoader.Resolve(transformationId);
					if (transformation == null)
						return;

						player.GetModPlayer<OmnitrixPlayer>()
							.RemoveTransformation(transformation.FullID, sync: true, showEffects: false);
						break;
					}
					case MessageType.RequestSyncTransformationState: {
						if (Main.netMode != NetmodeID.Server)
							return;

						if (whoAmI < 0 || whoAmI >= Main.maxPlayers)
							return;

						Player player = Main.player[whoAmI];
						if (!player.active)
							return;

						int slotCount = reader.ReadByte();
						string[] slots = new string[slotCount];
						for (int i = 0; i < slotCount; i++)
							slots[i] = reader.ReadString();

						int unlockedCount = reader.ReadUInt16();
						string[] unlocked = new string[unlockedCount];
						for (int i = 0; i < unlockedCount; i++)
							unlocked[i] = reader.ReadString();

						player.GetModPlayer<OmnitrixPlayer>().ApplyTransformationStateSync(slots, unlocked);
						break;
					}
					case MessageType.RequestAbsorbMaterial: {
						if (Main.netMode != NetmodeID.Server)
							return;

					if (whoAmI < 0 || whoAmI >= Main.maxPlayers)
						return;

					Player player = Main.player[whoAmI];
					if (!player.active)
						return;

					player.GetModPlayer<OmnitrixPlayer>().HandleAbsorbMaterialRequest();
					break;
				}
				case MessageType.AbsorbMaterialFeedback: {
					if (Main.netMode != NetmodeID.MultiplayerClient)
						return;

					string message = reader.ReadString();
					byte r = reader.ReadByte();
					byte g = reader.ReadByte();
					byte b = reader.ReadByte();

					Main.NewText(message, new Color(r, g, b));
					break;
				}
				case MessageType.SyncAbsorbedMaterial: {
					int playerIndex = reader.ReadByte();
					int itemType = reader.ReadInt32();
					int timeLeft = reader.ReadInt32();
					bool showEffects = reader.ReadBoolean();

					if (Main.netMode != NetmodeID.MultiplayerClient)
						return;

					if (playerIndex < 0 || playerIndex >= Main.maxPlayers)
						return;

					Player player = Main.player[playerIndex];
					if (!player.active)
						return;

					player.GetModPlayer<OmnitrixPlayer>().ApplyAbsorbedMaterialSync(itemType, timeLeft, showEffects);
					break;
				}
					case MessageType.RelayDodgeVisual: {
						int playerIndex = reader.ReadByte();

					if (playerIndex < 0 || playerIndex >= Main.maxPlayers)
						return;

					Player player = Main.player[playerIndex];
					if (!player.active)
						return;

					if (Main.netMode == NetmodeID.Server) {
						ModPacket packet = GetPacket();
						packet.Write((byte)MessageType.RelayDodgeVisual);
						packet.Write((byte)playerIndex);
						packet.Send(-1, whoAmI);
						return;
					}

						player.GetModPlayer<HeroPlumberArmorPlayer>().PlayRelayDodgeVisual(sync: false);
						break;
					}
					case MessageType.ExecuteBuzzShockTeleport: {
						if (Main.netMode != NetmodeID.Server)
							return;

						if (whoAmI < 0 || whoAmI >= Main.maxPlayers)
							return;

						Vector2 destination = new(reader.ReadSingle(), reader.ReadSingle());
						Player player = Main.player[whoAmI];
						if (!player.active || player.dead)
							return;

						OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
						if (omp.currentTransformationId != "Ben10Mod:BuzzShock" || !omp.IsPrimaryAbilityActive)
							return;

						BuzzShockTransformation.ExecutePrimaryAbilityTeleport(player, destination);
						break;
					}
					case MessageType.RequestGhostFreakPossession: {
						if (Main.netMode != NetmodeID.Server)
							return;

						if (whoAmI < 0 || whoAmI >= Main.maxPlayers)
							return;

						int targetIndex = reader.ReadInt32();
						if (targetIndex < 0 || targetIndex >= Main.maxNPCs)
							return;

						Player player = Main.player[whoAmI];
						if (!player.active || player.dead)
							return;

						OmnitrixPlayer omp = player.GetModPlayer<OmnitrixPlayer>();
						if (omp.currentTransformationId != "Ben10Mod:GhostFreak" || omp.inPossessionMode)
							return;

						NPC target = Main.npc[targetIndex];
						if (!target.active || !target.CanBeChasedBy())
							return;

						omp.BeginPossession(targetIndex, player.position);
						break;
					}
					case MessageType.SyncGhostFreakPossessionState: {
						if (Main.netMode != NetmodeID.MultiplayerClient)
							return;

						int playerIndex = reader.ReadByte();
						bool active = reader.ReadBoolean();
						int targetIndex = reader.ReadInt32();
						Vector2 returnPosition = new(reader.ReadSingle(), reader.ReadSingle());
						int timer = reader.ReadInt32();

						if (playerIndex < 0 || playerIndex >= Main.maxPlayers)
							return;

						Player player = Main.player[playerIndex];
						if (!player.active)
							return;

						player.GetModPlayer<OmnitrixPlayer>()
							.ApplyPossessionStateSync(active, targetIndex, returnPosition, timer);
						break;
					}
				}
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
	}
}
