using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
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

        public static float ReadSingleBE(this BinaryReader reader)
        {
            byte[] intBuf = reader.ReadBytes(4);

            return BitConverter.ToSingle(intBuf.Reverse().ToArray(), 0);
        }

        public static System.Numerics.Vector3 ReadVector3(this BinaryReader reader)
        {
            return new System.Numerics.Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
        public static System.Numerics.Vector3 ReadVector3BE(this BinaryReader reader)
        {
            return new System.Numerics.Vector3(reader.ReadSingleBE(), reader.ReadSingleBE(), reader.ReadSingleBE());
        }
    }
}
