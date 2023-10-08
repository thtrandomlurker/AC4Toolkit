using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Assimp;

namespace ACLib.Helpers
{
    public static class AssimpHelpers
    {
        public static Assimp.Matrix4x4 CalculateNodeMatrixWS(Node node)
        {
            // get the node transform matrix
            Assimp.Matrix4x4 val = node.Transform;
            if (node.Parent != null)
            {
                // if the node has a parent run this block of code until there is no parent
                val *= CalculateNodeMatrixWS(node.Parent);
            }
            return val;
        }

        public static System.Numerics.Matrix4x4 ToNumerics(this Assimp.Matrix4x4 matrix)
        {
            return new System.Numerics.Matrix4x4(matrix.A1, matrix.A2, matrix.A3, matrix.A4, matrix.B1, matrix.B2, matrix.B3, matrix.B4, matrix.C1, matrix.C2, matrix.C3, matrix.C4, matrix.D1, matrix.D2, matrix.D3, matrix.D4);

        }

        public static Assimp.Matrix4x4 ToAssimp(this System.Numerics.Matrix4x4 matrix)
        {
            return new Assimp.Matrix4x4(matrix.M11, matrix.M12, matrix.M13, matrix.M14, matrix.M21, matrix.M22, matrix.M23, matrix.M24, matrix.M31, matrix.M32, matrix.M33, matrix.M34, matrix.M41, matrix.M42, matrix.M43, matrix.M44);

        }
    }
}
