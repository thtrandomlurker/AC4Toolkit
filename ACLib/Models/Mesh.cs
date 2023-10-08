using ACLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACLib.Models
{
    public class Mesh
    {
        public short MaterialIndex { get; set; }
        public byte Unk01 { get; set; }
        public byte Unk02 { get; set; }

        public short Unk03 { get; set; }
        public short Unk04 { get; set; }

        // 29 bone ids
        public short[] BoneMap { get; set; }

        public int IndexBufferSize { get; set; }
        public int IndexBufferOffset { get; set; }

        public int VertexBufferSize { get; set; }
        public int VertexBufferOffset { get; set; }

        public void Read(BinaryReader reader)
        {
            MaterialIndex = reader.ReadInt16BE();
            Unk01 = reader.ReadByte();
            Unk02 = reader.ReadByte();
            Unk03 = reader.ReadInt16BE();
            Unk04 = reader.ReadInt16BE();
            for (int i = 0; i < 28; i++)
            {
                BoneMap[i] = reader.ReadInt16BE();
            }
            IndexBufferSize = reader.ReadInt32BE();
            IndexBufferOffset = reader.ReadInt32BE();
            VertexBufferSize = reader.ReadInt32BE();
            VertexBufferOffset = reader.ReadInt32BE();
        }

        public Mesh()
        {
            BoneMap = new short[28];
        }
    }
}
