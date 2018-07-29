using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TARC.Compiler
{
    public class Archive
    {
        public static string ArchiverMagicHeader { get; } = "TARC";
        public static ushort ArchiverVersion { get; } = 1;


        public ArchiveEncoding OriginalEncoding { get; set; } = ArchiveEncoding.UTF8;
        public ArchiveEncoding TranslatedEncoding { get; set; } = ArchiveEncoding.UTF16;

        public ArchiveCompression Compression { get; set; } = ArchiveCompression.Uncompressed;


        public List<Section> Sections { get; protected set; } = new List<Section>();


        public static Archive Read(BinaryReader reader)
        {
            Archive archive = new Archive();

            string magic = Encoding.ASCII.GetString(reader.ReadBytes(4));

            if (magic != ArchiverMagicHeader)
                throw new InvalidDataException("This is not a translation archive.");

            ushort version = reader.ReadUInt16();

            if (version != ArchiverVersion)
                throw new InvalidDataException("This archive was made with an incompatible version of archiver.");
            
            archive.OriginalEncoding = (ArchiveEncoding)reader.ReadByte();
            archive.TranslatedEncoding = (ArchiveEncoding)reader.ReadByte();
            
            archive.Compression = (ArchiveCompression)reader.ReadByte();

            int sectionCount = reader.ReadInt32();
            archive.Sections = new List<Section>(sectionCount);

            for (int i = 0; i < sectionCount; i++)
            {
                archive.Sections.Add(Section.Read(reader, archive.OriginalEncoding.GetEncoding(), archive.TranslatedEncoding.GetEncoding()));
            }

            return archive;
        }

        public void Write(BinaryWriter writer)
        {
            //using (BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII, true))
            //{
            writer.Write(Encoding.ASCII.GetBytes(ArchiverMagicHeader));
            writer.Write(ArchiverVersion);
                
            writer.Write((byte)OriginalEncoding);
            writer.Write((byte)TranslatedEncoding);
            writer.Write((byte)Compression);

            writer.Write(Sections.Count);

            foreach (var section in Sections)
                section.Write(writer, OriginalEncoding.GetEncoding(), TranslatedEncoding.GetEncoding());
            //}
        }
    }
}
