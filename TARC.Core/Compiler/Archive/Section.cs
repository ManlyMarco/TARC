using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TARC.Compiler
{
    public class Section
    {
        public string Exe { get; set; }

        public List<CompiledLine> Lines { get; set; } = new List<CompiledLine>();

        public void Write(BinaryWriter writer, Encoding originalEncoding, Encoding translatedEncoding)
        {
            var buffer = Encoding.UTF8.GetBytes(Exe);
            writer.Write(buffer.Length);
            writer.Write(buffer);

            writer.Write(Lines.Count);

            foreach (var line in Lines)
                line.Write(writer, originalEncoding, translatedEncoding);
        }

        public static Section Read(BinaryReader reader, Encoding originalEncoding, Encoding translatedEncoding)
        {
            Section section = new Section();

            int length = reader.ReadInt32();
            section.Exe = Encoding.UTF8.GetString(reader.ReadBytes(length));

            length = reader.ReadInt32();
            for (int i = 0; i < length; i++)
                section.Lines.Add(CompiledLine.Read(reader, originalEncoding, translatedEncoding));

            return section;
        }
    }
}
