using System;
using System.Collections.Generic;
using System.IO;
using Ben10Mod.Content.Transformations;
using Microsoft.Xna.Framework;

namespace Ben10Mod.Common.Networking;

public static class OmnitrixPacketRouter {
    public static void WriteTransformationPaletteEntries(BinaryWriter writer,
        IReadOnlyList<TransformationPaletteColorEntry> entries) {
        ushort count = (ushort)Math.Min(entries?.Count ?? 0, ushort.MaxValue);
        writer.Write(count);
        if (entries == null)
            return;

        for (int i = 0; i < count; i++) {
            TransformationPaletteColorEntry entry = entries[i];
            writer.Write(entry.TransformationId ?? "");
            writer.Write(entry.ChannelId ?? "");
            writer.Write(entry.Color.R);
            writer.Write(entry.Color.G);
            writer.Write(entry.Color.B);
            writer.Write(entry.Hue);
            writer.Write(entry.Saturation);
            writer.Write(entry.Brightness);
        }
    }

    public static void WritePaletteChannelKeys(BinaryWriter writer, IReadOnlyList<string> enabledChannelKeys) {
        ushort count = (ushort)Math.Min(enabledChannelKeys?.Count ?? 0, ushort.MaxValue);
        writer.Write(count);
        if (enabledChannelKeys == null)
            return;

        for (int i = 0; i < count; i++)
            writer.Write(enabledChannelKeys[i] ?? "");
    }

    public static void WriteOmnitrixVisualPaletteEntries(BinaryWriter writer,
        IReadOnlyList<OmnitrixVisualPaletteColorEntry> entries) {
        ushort count = (ushort)Math.Min(entries?.Count ?? 0, ushort.MaxValue);
        writer.Write(count);
        if (entries == null)
            return;

        for (int i = 0; i < count; i++) {
            OmnitrixVisualPaletteColorEntry entry = entries[i];
            writer.Write(entry.ChannelId ?? "");
            writer.Write(entry.Color.R);
            writer.Write(entry.Color.G);
            writer.Write(entry.Color.B);
            writer.Write(entry.Hue);
            writer.Write(entry.Saturation);
            writer.Write(entry.Brightness);
        }
    }

    public static void WriteSelectedTransformationCostumes(BinaryWriter writer,
        IReadOnlyList<KeyValuePair<string, string>> entries) {
        ushort count = (ushort)Math.Min(entries?.Count ?? 0, ushort.MaxValue);
        writer.Write(count);
        if (entries == null)
            return;

        for (int i = 0; i < count; i++) {
            writer.Write(entries[i].Key ?? "");
            writer.Write(entries[i].Value ?? "");
        }
    }

    public static TransformationPaletteColorEntry[] ReadTransformationPaletteEntries(BinaryReader reader) {
        int count = reader.ReadUInt16();
        TransformationPaletteColorEntry[] entries = new TransformationPaletteColorEntry[count];
        for (int i = 0; i < count; i++) {
            string transformationId = reader.ReadString();
            string channelId = reader.ReadString();
            byte r = reader.ReadByte();
            byte g = reader.ReadByte();
            byte b = reader.ReadByte();
            byte hue = reader.ReadByte();
            byte saturation = reader.ReadByte();
            byte brightness = reader.ReadByte();
            entries[i] = new TransformationPaletteColorEntry(transformationId, channelId, new Color(r, g, b),
                hue, saturation, brightness);
        }

        return entries;
    }

    public static string[] ReadPaletteChannelKeys(BinaryReader reader) {
        int count = reader.ReadUInt16();
        string[] enabledChannelKeys = new string[count];
        for (int i = 0; i < count; i++)
            enabledChannelKeys[i] = reader.ReadString();

        return enabledChannelKeys;
    }

    public static OmnitrixVisualPaletteColorEntry[] ReadOmnitrixVisualPaletteEntries(BinaryReader reader) {
        int count = reader.ReadUInt16();
        OmnitrixVisualPaletteColorEntry[] entries = new OmnitrixVisualPaletteColorEntry[count];
        for (int i = 0; i < count; i++) {
            string channelId = reader.ReadString();
            byte r = reader.ReadByte();
            byte g = reader.ReadByte();
            byte b = reader.ReadByte();
            byte hue = reader.ReadByte();
            byte saturation = reader.ReadByte();
            byte brightness = reader.ReadByte();
            entries[i] = new OmnitrixVisualPaletteColorEntry(channelId, new Color(r, g, b), hue, saturation,
                brightness);
        }

        return entries;
    }

    public static KeyValuePair<string, string>[] ReadSelectedTransformationCostumes(BinaryReader reader) {
        int count = reader.ReadUInt16();
        KeyValuePair<string, string>[] entries = new KeyValuePair<string, string>[count];
        for (int i = 0; i < count; i++)
            entries[i] = new KeyValuePair<string, string>(reader.ReadString(), reader.ReadString());

        return entries;
    }
}
