using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using ACLib.IO;
using System.Runtime.InteropServices;
using ACLib.Helpers;
using Assimp;
using Matrix4x4 = Assimp.Matrix4x4;
using System.Security.Cryptography.X509Certificates;

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
        public byte boneIdx;
        public byte vUnk0;
        public byte vUnk1;
        public byte vUnk2;
        public byte vUnk3;
        public byte vUnk4;
        public byte vUnk5;
        public byte vUnk6;
        public byte vUnk7;
        public byte vUnk8;
        public byte vUnk9;
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
            boneIdx = reader.ReadByte();
            vUnk0 = reader.ReadByte();
            vUnk1 = reader.ReadByte();
            vUnk2 = reader.ReadByte();
            vUnk3 = reader.ReadByte();
            vUnk4 = reader.ReadByte();
            vUnk5 = reader.ReadByte();
            vUnk6 = reader.ReadByte();
            vUnk7 = reader.ReadByte();
            vUnk8 = reader.ReadByte();
            vUnk9 = reader.ReadByte();
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

        public void Export(string filePath)
        {


            AssimpContext aiContext = new AssimpContext();
            Scene aiScene = new Scene();
            aiScene.RootNode = new Node("MDL4");

            // build skeleton


            foreach (var bone in Skeleton)
            {
                Node boneToNode = new Node(bone.Name);

                Matrix4x4 trsMatrix = Matrix4x4.FromTranslation(new Vector3D(bone.Translation.X, bone.Translation.Y, bone.Translation.Z));
                Matrix4x4 rotMatrix = Matrix4x4.FromEulerAnglesXYZ(bone.Rotation.X * (float)Math.PI / 180, bone.Rotation.Y * (float)Math.PI / 180, bone.Rotation.Z * (float)Math.PI / 180);
                Matrix4x4 sclMatrix = Matrix4x4.FromScaling(new Vector3D(bone.Scale.X, bone.Scale.Y, bone.Scale.Z));
                Matrix4x4 outMatrix = Matrix4x4.Identity;
                outMatrix *= trsMatrix;
                outMatrix *= rotMatrix;
                outMatrix *= sclMatrix;
                //outMatrix.Inverse();

                boneToNode.Transform = outMatrix;
                if (bone.ParentID != -1)
                {
                    aiScene.RootNode.FindNode(Skeleton[bone.ParentID].Name).Children.Add(boneToNode);
                }
                else
                {
                    aiScene.RootNode.Children.Add(boneToNode);
                }

            }

            foreach (var mat in Materials)
            {
                Assimp.Material aiMat = new Assimp.Material();
                aiMat.Name = mat.Name;
                aiScene.Materials.Add(aiMat);
            }

            for (int i = 0; i < Meshes.Count; i++)
            {
                Node meshNode = new Node($"mesh{i:d4}");

                Assimp.Mesh mesh = new Assimp.Mesh();
                mesh.PrimitiveType = PrimitiveType.Triangle;


                foreach (var boneIdx in Meshes[i].BoneMap)
                {
                    if (boneIdx == -1)
                        break;
                    Matrix4x4 offsetMatrix = AssimpHelpers.CalculateNodeMatrixWS(aiScene.RootNode.FindNode(Skeleton[boneIdx].Name));
                    offsetMatrix.Inverse();

                    Assimp.Bone aiBone = new Assimp.Bone();
                    aiBone.Name = Skeleton[boneIdx].Name;
                    aiBone.OffsetMatrix = offsetMatrix;
                    mesh.Bones.Add(aiBone);
                }

                mesh.MaterialIndex = Meshes[i].MaterialIndex;

                byte[] vertexBuffer = BufferData.Skip(Meshes[i].VertexBufferOffset).Take(Meshes[i].VertexBufferSize).ToArray();
                byte[] indexBuffer = BufferData.Skip(Meshes[i].IndexBufferOffset).Take(Meshes[i].IndexBufferSize).ToArray();

                // read vertices into the mesh

                for (int v = 0; v < Meshes[i].VertexBufferSize; v += 0x28)
                {
                    float posX = BitConverter.ToSingle(vertexBuffer.Skip(v).Take(4).Reverse().ToArray());
                    float posY = BitConverter.ToSingle(vertexBuffer.Skip(v + 4).Take(4).Reverse().ToArray());
                    float posZ = BitConverter.ToSingle(vertexBuffer.Skip(v + 8).Take(4).Reverse().ToArray());

                    float colR = (float)vertexBuffer[v + 20] / 255;
                    float colG = (float)vertexBuffer[v + 21] / 255;
                    float colB = (float)vertexBuffer[v + 22] / 255;
                    float colA = (float)vertexBuffer[v + 23] / 255;

                    float U0 = (float)BitConverter.ToInt16(vertexBuffer.Skip(v + 24).Take(2).Reverse().ToArray()) / 16384;
                    float V0 = (float)BitConverter.ToInt16(vertexBuffer.Skip(v + 26).Take(2).Reverse().ToArray()) / 16384;
                    float U1 = (float)BitConverter.ToInt16(vertexBuffer.Skip(v + 28).Take(2).Reverse().ToArray()) / 16384;
                    float V1 = (float)BitConverter.ToInt16(vertexBuffer.Skip(v + 30).Take(2).Reverse().ToArray()) / 16384;
                    float U2 = (float)BitConverter.ToInt16(vertexBuffer.Skip(v + 32).Take(2).Reverse().ToArray()) / 16384;
                    float V2 = (float)BitConverter.ToInt16(vertexBuffer.Skip(v + 34).Take(2).Reverse().ToArray()) / 16384;
                    float U3 = (float)BitConverter.ToInt16(vertexBuffer.Skip(v + 36).Take(2).Reverse().ToArray()) / 16384;
                    float V3 = (float)BitConverter.ToInt16(vertexBuffer.Skip(v + 38).Take(2).Reverse().ToArray()) / 16384;
                    byte boneIndex = vertexBuffer[v + 12];

                    Vector3D vert = new Vector3D(posX, posY, posZ);

                    Matrix4x4 vert_mat = Matrix4x4.FromTranslation(vert);

                    vert_mat *= AssimpHelpers.CalculateNodeMatrixWS(aiScene.RootNode.FindNode(Skeleton[Meshes[i].BoneMap[boneIndex]].Name));

                    vert_mat.Decompose(out Vector3D scl, out Assimp.Quaternion rotation, out Vector3D translation);

                    mesh.Vertices.Add(translation);
                    mesh.Bones[boneIndex].VertexWeights.Add(new VertexWeight(v / 0x28, 1.0f));

                    mesh.TextureCoordinateChannels[0].Add(new Vector3D(U0, V0, 1.0f));
                    mesh.TextureCoordinateChannels[1].Add(new Vector3D(U1, V1, 1.0f));
                    mesh.TextureCoordinateChannels[2].Add(new Vector3D(U2, V2, 1.0f));
                    mesh.TextureCoordinateChannels[3].Add(new Vector3D(U3, V3, 1.0f));

                    mesh.VertexColorChannels[0].Add(new Color4D(colR, colG, colB, colA));
                }

                // read tris into the mesh
                int indexBufferPos = 0;
                while (indexBufferPos < Meshes[i].IndexBufferSize)
                {
                    // generate the initial face

                    short f1 = BitConverter.ToInt16(indexBuffer.Skip(indexBufferPos).Take(2).Reverse().ToArray());

                    short f2 = BitConverter.ToInt16(indexBuffer.Skip(indexBufferPos + 2).Take(2).Reverse().ToArray());

                    short f3 = BitConverter.ToInt16(indexBuffer.Skip(indexBufferPos + 4).Take(2).Reverse().ToArray());

                    // commit face to file

                    //output.Write(Encoding.ASCII.GetBytes($"f {f1 + 1}/{f1 + 1}/{f1 + 1} {f2 + 1}/{f2 + 1}/{f2 + 1} {f3 + 1}/{f3 + 1}/{f3 + 1}\n"));

                    //output.WriteLine($"f {f1} {f2} {f3}");

                    mesh.Faces.Add(new Face(new int[] { f1, f2, f3 }));


                    indexBufferPos += 6;

                    bool flip = true;

                    while (true)
                    {
                        if (indexBufferPos >= Meshes[i].IndexBufferSize)
                            break;
                        f1 = f2;
                        f2 = f3;
                        f3 = BitConverter.ToInt16(indexBuffer.Skip(indexBufferPos).Take(2).Reverse().ToArray());
                        indexBufferPos += 2;
                        if (f3 == -1)
                        {
                            break;
                        }
                        else
                        {
                            if (flip)
                            {
                                //output.Write(Encoding.ASCII.GetBytes($"f {f3 + 1}/{f3 + 1}/{f3 + 1} {f2 + 1}/{f2 + 1}/{f2 + 1} {f1 + 1}/{f1 + 1}/{f1 + 1}\n"));

                                //output.WriteLine($"f {f3} {f2} {f1}");

                                mesh.Faces.Add(new Face(new int[] { f3, f2, f1 }));
                            }
                            else
                            {
                                //output.Write(Encoding.ASCII.GetBytes($"f {f1 + 1}/{f1 + 1}/{f1 + 1} {f2 + 1}/{f2 + 1}/{f2 + 1} {f3 + 1}/{f3 + 1}/{f3 + 1}\n"));

                                //output.WriteLine($"f {f1} {f2} {f3}");

                                mesh.Faces.Add(new Face(new int[] { f1, f2, f3 }));
                            }
                            flip = !flip;
                        }
                    }
                }
                aiScene.Meshes.Add(mesh);
                meshNode.MeshIndices.Add(i);
                aiScene.RootNode.Children.Add(meshNode);
            }

            string formatId = new AssimpContext().GetSupportedExportFormats()
                .First(x => x.FileExtension.Equals(Path.GetExtension(filePath).Split('.')[1], StringComparison.OrdinalIgnoreCase)).FormatId;

            aiContext.ExportFile(aiScene, filePath, formatId, Assimp.PostProcessSteps.None);
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
