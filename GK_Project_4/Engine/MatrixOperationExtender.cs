using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SoftEngine
{
    public static class MatrixOperationExtender
    {
        public static Vector4 Multiply(this Matrix m, Vector4 v)
        {
            Vector4 output = new Vector4();
            for (int i = 0; i < 4; i++)
            {
                output[i] = m[i, 0] * v[0] + m[i, 1] * v[1] + m[i, 2] * v[2] + m[i, 3] * v[3];
            }
            return output;
        }
        public static Vector4 ConvertToPoint(this Vector3 v)
        {
            return new Vector4(v, 1);
        }
        public static Vector4 ConvertToVector(this Vector3 v)
        {
            return new Vector4(v, 0);
        }
        public static Vector3 ConvertFromPointOrVector(this Vector4 v)
        {
            return new Vector3(v[0],v[1],v[2]);
        }
        public static System.Drawing.Color Multiply(this System.Drawing.Color value,float scale)
        {
            return System.Drawing.Color.FromArgb((int)(value.R * scale),(int) (value.G * scale), (int)(value.B * scale), (int)(value.A * scale));
        }
    }
}
