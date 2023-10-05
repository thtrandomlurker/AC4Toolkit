using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ACLib.IO
{
    public static class EndianHelpers
    {
        public static int ReadInt32BE(this BinaryReader reader)
        {
            byte[] intBuf = reader.ReadBytes(4);

            return BitConverter.ToInt32(intBuf.Reverse().ToArray(), 0);
        }
        public static short ReadInt16BE(this BinaryReader reader)
        {
            byte[] intBuf = reader.ReadBytes(2);

            return BitConverter.ToInt16(intBuf.Reverse().ToArray(), 0);
        }
    }
}
