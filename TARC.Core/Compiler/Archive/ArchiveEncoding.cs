using System;
using System.Text;

namespace TARC.Compiler
{
    public enum ArchiveEncoding : byte
    {
        UTF8 = 1,
        UTF16 = 2
    }

    public static class ArchiveEncodingExtensions
    {
        public static Encoding GetEncoding(this ArchiveEncoding encodingEnum)
        {
            switch (encodingEnum)
            {
                case ArchiveEncoding.UTF8:
                    return Encoding.UTF8;
                case ArchiveEncoding.UTF16:
                    return Encoding.Unicode;
                default:
                    throw new ArgumentException(nameof(encodingEnum));
            }
        }
    }
}
