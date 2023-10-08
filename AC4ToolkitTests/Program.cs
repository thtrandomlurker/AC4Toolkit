using ACLib.Models;
using System.Text;
using System.Numerics;
using Assimp;
using ACLib.Helpers;
using Matrix4x4 = Assimp.Matrix4x4;

namespace AC4ToolkitTests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MDL model = new MDL();
            model.Load(args[0]);

            model.Export(Path.GetFileNameWithoutExtension(args[0]) + ".fbx");

            /*AssimpContext aiContext = new AssimpContext();
            Scene aiScene = new Scene();
            aiScene.RootNode = new Node("MDL4");

            // build skeleton


            foreach (var bone in model.Skeleton)
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
                    aiScene.RootNode.FindNode(model.Skeleton[bone.ParentID].Name).Children.Add(boneToNode);
                }
                else
                {
                    aiScene.RootNode.Children.Add(boneToNode);
                }

            }

            foreach (var mat in model.Materials)
            {
                Assimp.Material aiMat = new Assimp.Material();
                aiMat.Name = mat.Name;
                aiScene.Materials.Add(aiMat);
            }

            for (int i = 0; i < model.Meshes.Count; i++)
            {
                Node meshNode = new Node($"mesh{i:4}");

                Assimp.Mesh mesh = new Assimp.Mesh();
                mesh.PrimitiveType = PrimitiveType.Triangle;

                foreach (var bone in model.Skeleton)
                {
                    Matrix4x4 offsetMatrix = AssimpHelpers.CalculateNodeMatrixWS(aiScene.RootNode.FindNode(bone.Name));
                    offsetMatrix.Inverse();

                    Assimp.Bone aiBone = new Assimp.Bone();
                    aiBone.Name = bone.Name;
                    aiBone.OffsetMatrix = offsetMatrix;
                    mesh.Bones.Add(aiBone);
                }

                mesh.MaterialIndex = model.Meshes[i].MaterialIndex;

                byte[] vertexBuffer = model.BufferData.Skip(model.Meshes[i].VertexBufferOffset).Take(model.Meshes[i].VertexBufferSize).ToArray();
                byte[] indexBuffer = model.BufferData.Skip(model.Meshes[i].IndexBufferOffset).Take(model.Meshes[i].IndexBufferSize).ToArray();

                // read vertices into the mesh

                for (int v = 0; v < model.Meshes[i].VertexBufferSize; v += 0x28)
                {
                    float posX = BitConverter.ToSingle(vertexBuffer.Skip(v).Take(4).Reverse().ToArray());
                    float posY = BitConverter.ToSingle(vertexBuffer.Skip(v+4).Take(4).Reverse().ToArray());
                    float posZ = BitConverter.ToSingle(vertexBuffer.Skip(v+8).Take(4).Reverse().ToArray());
                    byte boneIndex = vertexBuffer[v + 12];

                    Vector3D vert = new Vector3D(posX, posY, posZ);

                    Matrix4x4 vert_mat = Matrix4x4.FromTranslation(vert);

                    vert_mat *= AssimpHelpers.CalculateNodeMatrixWS(aiScene.RootNode.FindNode(model.Skeleton[model.Meshes[i].BoneMap[boneIndex]].Name));

                    vert_mat.Decompose(out Vector3D scl, out Assimp.Quaternion rotation, out Vector3D translation);

                    mesh.Vertices.Add(translation);
                    mesh.Bones[model.Meshes[i].BoneMap[boneIndex]].VertexWeights.Add(new VertexWeight(v / 0x28, 1.0f));
                }

                // read tris into the mesh
                int indexBufferPos = 0;
                while (indexBufferPos < model.Meshes[i].IndexBufferSize)
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
                        if (indexBufferPos >= model.Meshes[i].IndexBufferSize)
                            break;
                        f1 = f2;
                        f2 = f3;
                        f3 = BitConverter.ToInt16(indexBuffer.Skip(indexBufferPos).Take(2).Reverse().ToArray());
                        Console.WriteLine($"F3: {f3}");
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
                .First(x => x.FileExtension.Equals(Path.GetExtension("test.fbx").Split('.')[1], StringComparison.OrdinalIgnoreCase)).FormatId;

            aiContext.ExportFile(aiScene, "test.fbx", formatId, Assimp.PostProcessSteps.None);*/

            /*foreach (var mesh in model.Meshes)
            {
                //StreamWriter output = File.CreateText($"test_{mesh.MaterialIndex}.obj");
                //StreamWriter weights = File.CreateText($"test_{mesh.MaterialIndex}.grp");
                //StreamWriter log = File.CreateText("log.txt");
                //output.Write(Encoding.UTF8.GetBytes("o obj\n"));
                Console.WriteLine($"Mesh has {mesh.VertexBufferSize / 0x28} vertices.");
                // get the vertex buffer

                byte[] vertexBuffer = model.BufferData.Skip(mesh.VertexBufferOffset).Take(mesh.VertexBufferSize).ToArray();
                byte[] indexBuffer = model.BufferData.Skip(mesh.IndexBufferOffset).Take(mesh.IndexBufferSize).ToArray();

                // how many bones this mesh can access

                int boneCount = 0;

                for (int i = 0; i < 28; i++)
                {
                    if (mesh.BoneMap[i] == -1)
                    {
                        boneCount = i - 1;
                        break;
                    }
                    else
                    {
                        //weights.WriteLine($"bone {mesh.BoneMap[i]} {model.Skeleton[mesh.BoneMap[i]].Name}");
                    }
                }

                if (boneCount == 0)
                {
                    boneCount = 28;
                }

                for (int i = 0; i < mesh.VertexBufferSize; i+= 0x28)
                {
                    //log.WriteLine($"{i}\t{mesh.VertexBufferSize}");
                    float vX = BitConverter.ToSingle(vertexBuffer.Skip(i).Take(4).Reverse().ToArray());
                    float vY = BitConverter.ToSingle(vertexBuffer.Skip(i + 4).Take(4).Reverse().ToArray());
                    float vZ = BitConverter.ToSingle(vertexBuffer.Skip(i + 8).Take(4).Reverse().ToArray());
                    //float nX = (float)BitConverter.ToUInt16(vertexBuffer.Skip(i + 12).Take(2).Reverse().ToArray()) / 32767;
                    //float nY = (float)BitConverter.ToUInt16(vertexBuffer.Skip(i + 14).Take(2).Reverse().ToArray()) / 32767;
                    //float nZ = (float)BitConverter.ToUInt16(vertexBuffer.Skip(i + 16).Take(2).Reverse().ToArray()) / 32767;
                    //float tU = (float)BitConverter.ToUInt16(vertexBuffer.Skip(i + 24).Take(2).Reverse().ToArray()) / 2048;
                    //float tV = (float)BitConverter.ToUInt16(vertexBuffer.Skip(i + 26).Take(2).Reverse().ToArray()) / 2048;

                    //log.WriteLine($"Mesh has {boneCount} bones. Vertex accesses bone {vertexBuffer[i+12]}.");
                    //log.WriteLine($"Mesh has {boneCount} bones. Vertex accesses bone {vertexBuffer[i+19]}.");
                    if (vertexBuffer[i+12] > boneCount)
                    {
                        //log.WriteLine("Bone access out of bounds.");
                        throw new InvalidDataException();
                    }
                    //output.WriteLine($"v {vX} {vY} {vZ}");
                    //output.Flush();
                    //weights.WriteLine($"bi {vertexBuffer[i + 12]} 0 0 0");
                    //weights.WriteLine($"bw 1 0 0 0");
                    //weights.Flush();
                    //output.Write(Encoding.ASCII.GetBytes($"vn {nX} {nY} {nZ}\n"));
                    //output.Write(Encoding.ASCII.GetBytes($"vt {tU} {tV}\n"));
                }

                int indexBufferPos = 0;
                while (indexBufferPos < mesh.IndexBufferSize)
                {
                    // generate the initial face

                    short f1 = BitConverter.ToInt16(indexBuffer.Skip(indexBufferPos).Take(2).Reverse().ToArray());

                    short f2 = BitConverter.ToInt16(indexBuffer.Skip(indexBufferPos+2).Take(2).Reverse().ToArray());

                    short f3 = BitConverter.ToInt16(indexBuffer.Skip(indexBufferPos+4).Take(2).Reverse().ToArray());

                    // commit face to file

                    //output.Write(Encoding.ASCII.GetBytes($"f {f1 + 1}/{f1 + 1}/{f1 + 1} {f2 + 1}/{f2 + 1}/{f2 + 1} {f3 + 1}/{f3 + 1}/{f3 + 1}\n"));

                    //output.WriteLine($"f {f1} {f2} {f3}");


                    indexBufferPos += 6;

                    bool flip = true;

                    while (true)
                    {
                        if (indexBufferPos >= mesh.IndexBufferSize)
                            break;
                        f1 = f2;
                        f2 = f3;
                        f3 = BitConverter.ToInt16(indexBuffer.Skip(indexBufferPos).Take(2).Reverse().ToArray());
                        Console.WriteLine($"F3: {f3}");
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
                            }
                            else
                            {
                                //output.Write(Encoding.ASCII.GetBytes($"f {f1 + 1}/{f1 + 1}/{f1 + 1} {f2 + 1}/{f2 + 1}/{f2 + 1} {f3 + 1}/{f3 + 1}/{f3 + 1}\n"));

                                //output.WriteLine($"f {f1} {f2} {f3}");
                            }
                            flip = !flip;
                        }
                    }
                }
            }*/
        }
    }
}