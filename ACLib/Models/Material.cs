using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ACLib.IO;

namespace ACLib.Models
{
    public enum MaterialParameterType : byte
    {
        Bool = 0,
        Float = 1,
        Vec2 = 2,
        Vec3 = 3,
        Vec4 = 4,
        String = 5
    }

    public class MaterialParameter
    {
        public MaterialParameterType Type { get; set; }
        public string Name { get; set; }
        public byte[] RawData { get; set; }

        public void Read (BinaryReader reader)
        {
            Type = (MaterialParameterType)reader.ReadByte();
            Name = reader.ReadString(StringFormat.FixedLength, 0x1F);

            RawData = reader.ReadBytes(32);
        }

        public MaterialParameter()
        {
            Name = "";
            RawData = new byte[32];
        }
    }
    public class Material
    {
        public string Name { get; set; }

        public string ShaderName { get; set; }
        public short UsedParameterCount { get; set; }

        public MaterialParameter[] Parameters { get; set; }

        public void Read(BinaryReader reader)
        {
            Name = reader.ReadString(StringFormat.FixedLength, 0x1F);
            ShaderName = reader.ReadString(StringFormat.FixedLength, 0x1F);
            UsedParameterCount = reader.ReadInt16BE();

            for (int i = 0; i < 32; i++)
            {
                MaterialParameter param = new MaterialParameter();
                param.Read(reader);
                Parameters[i] = param;
            }
        }

        public Material()
        {
            Name = "";
            ShaderName = "";
            Parameters = new MaterialParameter[32];
        }

    }
}
