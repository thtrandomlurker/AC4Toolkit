using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using ACLib.IO;
using System.Runtime.InteropServices;

namespace ACLib.Models
{
    public class UnkSection
    {
        // are these even bounding boxes?
        public Vector3 BoundingBoxMin { get; set; }
        public Vector3 BoundingBoxMax { get; set; }

        public short Unk01 { get; set; }
        public short Unk02 { get; set; }
        public int Unk03 { get; set; }
        public short Unk04 { get; set; }
        public short Unk05 { get; set; }
        public short Unk06 { get; set; }
        public short Unk07 { get; set; }
        public short Unk08 { get; set; }
        public short Unk09 { get; set; }
        public short Unk10 { get; set; }
        public short Unk11 { get; set; }

        public void Read(BinaryReader reader)
        {
            BoundingBoxMin = reader.ReadVector3BE();
            BoundingBoxMax = reader.ReadVector3BE();
            Unk01 = reader.ReadInt16BE();
            Unk02 = reader.ReadInt16BE();
            Unk03 = reader.ReadInt32BE();
            Unk04 = reader.ReadInt16BE();
            Unk05 = reader.ReadInt16BE();
            Unk06 = reader.ReadInt16BE();
            Unk07 = reader.ReadInt16BE();
            Unk08 = reader.ReadInt16BE();
            Unk09 = reader.ReadInt16BE();
            Unk10 = reader.ReadInt16BE();
            Unk11 = reader.ReadInt16BE();
        }
    }

    public class Bone
    {
        public string? Name { get; set; }
        public Vector3 Translation { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }
        // two vectors i don't understand
        public Vector3 VecUnk4 { get; set; }
        public Vector3 VecUnk5 { get; set; }

        public short ParentID { get; set; }
        public short ChildID { get; set; }
        public short SiblingID { get; set; }
        public short Unk01 { get; set; }
        public short Unk02 { get; set; }
        public short Unk03 { get; set; }
        public short Unk04 { get; set; }
        public short Unk05 { get; set; }
        public short Unk06 { get; set; }
        public short Unk07 { get; set; }
        public byte[]? UnkBytes { get; set; }

        public void Read(BinaryReader reader)
        {
            Name = reader.ReadString(StringFormat.FixedLength, 0x20);
            Translation = reader.ReadVector3BE();
            Rotation = reader.ReadVector3BE();
            Scale = reader.ReadVector3BE();
            VecUnk4 = reader.ReadVector3BE();
            VecUnk5 = reader.ReadVector3BE();
            ParentID = reader.ReadInt16BE();
            ChildID = reader.ReadInt16BE();
            SiblingID = reader.ReadInt16BE();
            Unk01 = reader.ReadInt16BE();
            Unk02 = reader.ReadInt16BE();
            Unk03 = reader.ReadInt16BE();
            Unk04 = reader.ReadInt16BE();
            Unk05 = reader.ReadInt16BE();
            Unk06 = reader.ReadInt16BE();
            Unk07 = reader.ReadInt16BE();
            UnkBytes = reader.ReadBytes(32);
        }
    }

    public struct Vertex
    {
        public float vX;
        public float vY;
        public float vZ;
        public float nX;
        public float nY;
        public float nZ;
        public byte vUnk0;
        public byte vUnk1;
        public byte vUnk2;
        public byte vUnk3;
        public byte vUnk4;
        public byte vUnk5;
        public byte vUnk6;
        public byte vUnk7;
        public byte cR;
        public byte cG;
        public byte cB;
        public byte cA;
        public short tc0u;
        public short tc0v;
        public short tc1u;
        public short tc1v;
        public short tc2u;
        public short tc2v;
        public short tc3u;
        public short tc3v;

        public void Read(BinaryReader reader)
        {
            vX = reader.ReadSingleBE();
            vY = reader.ReadSingleBE();
            vZ = reader.ReadSingleBE();
            nX = reader.ReadByte();
            nY = reader.ReadByte();
            nZ = reader.ReadByte();
            vUnk0 = reader.ReadByte();
            vUnk1 = reader.ReadByte();
            vUnk2 = reader.ReadByte();
            vUnk3 = reader.ReadByte();
            vUnk4 = reader.ReadByte();
            vUnk5 = reader.ReadByte();
            vUnk6 = reader.ReadByte();
            vUnk7 = reader.ReadByte();
            tc0u = reader.ReadInt16BE();
            tc0v = reader.ReadInt16BE();
            tc1u = reader.ReadInt16BE();
            tc1v = reader.ReadInt16BE();
            tc2u = reader.ReadInt16BE();
            tc2v = reader.ReadInt16BE();
            tc3u = reader.ReadInt16BE();
            tc3v = reader.ReadInt16BE();
        }

        public void FromBytes(byte[] bytes)
        {
            if (bytes.Length < 28)
            {
                throw new ArgumentException("Given array is too small.");
            }
            vX = BitConverter.ToSingle(bytes.Take(4).Reverse().ToArray());
            vY = BitConverter.ToSingle(bytes.Skip(4).Take(4).Reverse().ToArray());
            vZ = BitConverter.ToSingle(bytes.Skip(8).Take(4).Reverse().ToArray());
        }
    }
    public class MDL
    {
        public char[]? Magic { get; set; }  // MDL4, Version 4 of the model format?
        public short VersionMajor { get; set; }  // always 4?
        public short VersionMinor { get; set; }  // Seems to always be 2. MDL version 4 revision 2.

        public Vector3 BoundingBoxMin { get; set; }
        public Vector3 BoundingBoxMax { get; set; }

        public List<UnkSection> UnkSections { get; set; }
        public List<Material> Materials { get; set; }
        public List<Bone> Skeleton { get; set; }

        public List<Mesh> Meshes { get; set; }

        public byte[]? BufferData { get; set; }

        public void Load(Stream stream)
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Magic = reader.ReadChars(4);
                VersionMajor = reader.ReadInt16BE();
                VersionMinor = reader.ReadInt16BE();

                int modelBufferOffset = reader.ReadInt32BE();
                int modelBufferSize = reader.ReadInt32BE();

                // get buffer
                long cur = reader.BaseStream.Position;

                reader.BaseStream.Seek(modelBufferOffset, SeekOrigin.Begin);
                BufferData = reader.ReadBytes(modelBufferSize);

                reader.BaseStream.Seek(cur, SeekOrigin.Begin);


                int unkSect0Count = reader.ReadInt32BE();
                UnkSections.Capacity = unkSect0Count;
                int materialCount = reader.ReadInt32BE();
                Materials.Capacity = materialCount;
                int skelBoneCount = reader.ReadInt32BE();
                Skeleton.Capacity = skelBoneCount;
                int meshCount = reader.ReadInt32BE();
                Meshes.Capacity = meshCount;
                int meshCountDuplicate = reader.ReadInt32BE();  // Not sure what purpose this serves. it seems to always be the same as the mesh count
                // but there's no data between the mesh info blocks and the actual buffer.
                BoundingBoxMin = reader.ReadVector3BE();
                BoundingBoxMax = reader.ReadVector3BE();

                int vertexCount = reader.ReadInt32BE();
                int tristripPointCount = reader.ReadInt32BE() + 2;  // always -2?

                reader.BaseStream.Seek(0x80, SeekOrigin.Begin);

                for (int i = 0; i < unkSect0Count; i++)
                {
                    UnkSection unk = new UnkSection();
                    unk.Read(reader);
                    UnkSections.Add(unk);
                }

                for (int i = 0; i < materialCount; i++)
                {
                    Material mat = new Material();
                    mat.Read(reader);
                    Console.WriteLine($"Read material {mat.Name}");
                    Materials.Add(mat);
                }

                for (int i = 0; i < skelBoneCount; i++)
                {
                    Bone bone = new Bone();
                    bone.Read(reader);
                    Console.WriteLine($"Read bone {bone.Name}");
                    Skeleton.Add(bone);
                }

                for (int i = 0; i < meshCount; i++)
                {
                    Mesh mesh = new Mesh();
                    mesh.Read(reader);
                    Meshes.Add(mesh);
                }
            }
        }

        public void Load(string filePath)
        {
            Load(File.OpenRead(filePath));
        }

        public MDL()
        {
            UnkSections = new List<UnkSection>();
            Materials = new List<Material>();
            Skeleton = new List<Bone>();
            Meshes = new List<Mesh>();
        }
    }
}
