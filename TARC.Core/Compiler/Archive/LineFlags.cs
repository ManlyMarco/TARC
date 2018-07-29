using System.Collections;

namespace TARC.Compiler
{
    public class LineFlags
    {
        protected BitArray Bits { get; set; } = new BitArray(32);

        #region Direct Bool Properties
        public bool IsOriginalRegex
        {
            get => Bits[0];
            set => Bits[0] = value;
        }

        public bool IsTranslationRegex
        {
            get => Bits[1];
            set => Bits[1] = value;
        }

        public bool ConvertWideNumbers
        {
            get => Bits[2];
            set => Bits[2] = value;
        }
        
        #endregion

        #region Nullable Bool Properties

        public bool? AllowOverflow
        {
            get => Bits[8] ? Bits[9] : (bool?)null;
            set
            {
                Bits[8] = value != null;
                Bits[9] = value.GetValueOrDefault();
            }
        }

        public bool? UseAutosize
        {
            get => Bits[10] ? Bits[11] : (bool?)null;
            set
            {
                Bits[10] = value != null;
                Bits[11] = value.GetValueOrDefault();
            }
        }
        
        #endregion

        public byte[] ExportAsBytes()
        {
            byte[] buffer = new byte[4];

            Bits.CopyTo(buffer, 0);

            return buffer;
        }

        public static LineFlags Create(byte[] data)
        {
            return new LineFlags()
            {
                Bits = new BitArray(data)
            };
        }

        public static explicit operator byte[](LineFlags flags) => flags.ExportAsBytes();

        public static explicit operator LineFlags(byte[] data) => Create(data);
    }
}
