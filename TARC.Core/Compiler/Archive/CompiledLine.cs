using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TARC.Compiler
{
    public class CompiledLine
    {
        public string OriginalLine { get; set; }
        public string TranslatedLine { get; set; }

        public byte[] Levels { get; set; }

        public LineFlags Flags { get; set; } = new LineFlags();

        public byte? FontSize { get; set; }

        public void Write(BinaryWriter writer, Encoding originalEncoding, Encoding translatedEncoding)
        {
            byte[] buffer = originalEncoding.GetBytes(OriginalLine);
            writer.Write(buffer.Length);
            writer.Write(buffer);

            buffer = translatedEncoding.GetBytes(TranslatedLine);
            writer.Write(buffer.Length);
            writer.Write(buffer);

            writer.Write((byte)Levels.Length);
            foreach (byte level in Levels)
                writer.Write(level);

            buffer = Flags.ExportAsBytes();
            writer.Write((byte)buffer.Length);
            writer.Write(buffer);

            writer.Write(FontSize ?? 0);
        }

        public static CompiledLine Read(BinaryReader reader, Encoding originalEncoding, Encoding translatedEncoding)
        {
            CompiledLine line = new CompiledLine();

            int length = reader.ReadInt32();
            line.OriginalLine = originalEncoding.GetString(reader.ReadBytes(length));

            length = reader.ReadInt32();
            line.TranslatedLine = translatedEncoding.GetString(reader.ReadBytes(length));

            List<byte> levels = new List<byte>();

            length = reader.ReadByte();
            for (int i = 0; i < length; i++)
                levels.Add(reader.ReadByte());

            line.Levels = levels.ToArray();

            length = reader.ReadByte();
            line.Flags = LineFlags.Create(reader.ReadBytes(length));

            byte fontSize = reader.ReadByte();
            line.FontSize = fontSize != 0 ? fontSize : (byte?)null;

            return line;
        }
    }
}
