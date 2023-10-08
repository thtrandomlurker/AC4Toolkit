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
        }
    }
}