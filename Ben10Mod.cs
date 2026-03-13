using System.IO;
using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Content.Items.Vanity.ShaderDyes;
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
		public override void Load() {
			if (Main.netMode != NetmodeID.Server) {
				Asset<Effect> dyeShader = this.Assets.Request<Effect>("Effects/MyDyes");
				Asset<Effect> filterShader = this.Assets.Request<Effect>("Effects/MyFilters");


				GameShaders.Armor.BindShader(ModContent.ItemType<DiscoDye>(),
					new ArmorShaderData(dyeShader, "BasicTint"));

				
				Filters.Scene["Ben10Mod:Grayscale"] = new Filter(new ScreenShaderData(filterShader, "Grayscale"), EffectPriority.Medium);
				Filters.Scene["Ben10Mod:Bluescale"] = new Filter(new ScreenShaderData(filterShader, "Bluescale"), EffectPriority.Medium);
			}
		}

		// Add this enum anywhere in the class (or in a separate file)
		public enum MessageType : byte {
			UnlockTransformation
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

					player.GetModPlayer<OmnitrixPlayer>().UnlockTransformation(transformationId, sync: false, showEffects: playerIndex == Main.myPlayer);
					break;
				}
			}
		}
	}
}
