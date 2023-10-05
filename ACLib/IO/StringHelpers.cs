using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ACLib.IO
{
    public static class StringHelpers
    {
        public static string ReadString(this BinaryReader reader, StringFormat format, int fixedLength = 0, SeekOrigin origin = SeekOrigin.Begin)
        {
            switch (format)
            {
                case StringFormat.FixedLength:
                    {
                        string oStr = new string(reader.ReadChars(fixedLength));
                        return oStr.Substring(0, oStr.IndexOf('\0'));
                    }
                case StringFormat.NullTerminated:
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        while (true)
                        {
                            char c = reader.ReadChar();

                            if (c == '\0')
                            {
                                return stringBuilder.ToString();
                            }
                            else
                            {
                                stringBuilder.Append(c);
                            }
                        }
                    }
                default:
                    throw new InvalidOperationException(string.Format("Unknown StringFormat {0}", format));
            }
        }
        public static string ReadStringOffset(this BinaryReader reader, StringFormat format, int fixedLength = 0, SeekOrigin origin = SeekOrigin.Begin)
        {
            int offset = reader.ReadInt32BE();
            long cur = reader.BaseStream.Position;
            reader.BaseStream.Seek(offset, origin);
            string ret = ReadString(reader, format, fixedLength, origin);
            reader.BaseStream.Seek(cur, origin);
            return ret;
        }
    }
}
