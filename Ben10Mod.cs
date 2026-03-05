using System.IO;
using Ben10Mod.Content.Items.Accessories;
using Ben10Mod.Enums;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace Ben10Mod {
	public class Ben10Mod : Mod {
		public override void Load() {
			if (Main.netMode != NetmodeID.Server) {
				// Asset<Effect> dyeShader = this.Assets.Request<Effect>("Effects/BasicTint");
				//
				// GameShaders.Armor.BindShader(ModContent.ItemType<PrototypeOmnitrix>(),
				// 	new ArmorShaderData(dyeShader, "ArmorBasic")).UseColor(1f, 0, 0);
			}
		}

		// Add this enum anywhere in the class (or in a separate file)
		public enum MessageType : byte {
			UnlockTransformation
		}

		public override void HandlePacket(BinaryReader reader, int whoAmI) {
			MessageType msgType = (MessageType)reader.ReadByte();

			switch (msgType) {
				case MessageType.UnlockTransformation:
					int                playerIndex    = reader.ReadByte();
					TransformationEnum transformation = (TransformationEnum)reader.ReadInt32();

					if (playerIndex >= 0 && playerIndex < Main.maxPlayers) {
						var modPlayer = Main.player[playerIndex].GetModPlayer<OmnitrixPlayer>();
						modPlayer.addTransformation(transformation); // client will apply it locally
					}

					break;
			}
		}
	}
}