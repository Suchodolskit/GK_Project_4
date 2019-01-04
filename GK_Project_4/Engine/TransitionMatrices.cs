using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SoftEngine
{
    public class TransitionMatrices
    {
        public static Matrix LookAt(Camera c)
        {
            Matrix m = new Matrix();
            Vector3 tmp = (c.Position - c.Target);
            Vector3 cZ = Vector3.Divide(tmp,tmp.Length());
            tmp = Vector3.Cross(c.Up, cZ);
            Vector3 cX = Vector3.Divide(tmp, tmp.Length());
            tmp= Vector3.Cross(cZ,cX);
            Vector3 cY = Vector3.Divide(tmp, tmp.Length());

            m.M11 = cX.X;
            m.M12 = cX.Y;
            m.M13 = cX.Z;
            m.M14 = -Vector3.Dot(cX,c.Position);
            m.M21 = cY.X;
            m.M22 = cY.Y;
            m.M23 = cY.Z;
            m.M24 = -Vector3.Dot(cY, c.Position);
            m.M31 = cZ.X;
            m.M32 = cZ.Y;
            m.M33 = cZ.Z;
            m.M34 = -Vector3.Dot(cZ, c.Position);
            m.M41 = 0;
            m.M42 = 0;
            m.M43 = 0;
            m.M44 = 1;
            return m;
        }
        public static Matrix RotationX(double alpha)
        {
            Matrix m = new Matrix();
            m.M11 = 1;
            m.M12 = 0;
            m.M13 = 0;
            m.M14 = 0;
            m.M21 = 0;
            m.M22 = (float)Math.Cos(alpha);
            m.M23 = (float)-Math.Sin(alpha);
            m.M24 = 0;
            m.M31 = 0;
            m.M32 = (float)Math.Sin(alpha);
            m.M33 = (float)Math.Cos(alpha);
            m.M34 = 0;
            m.M41 = 0;
            m.M42 = 0;
            m.M43 = 0;
            m.M44 = 1;
            return m;
        }
        public static Matrix RotationY(double alpha)
        {
            Matrix m = new Matrix();
            m.M11 = (float)Math.Cos(alpha);
            m.M12 = 0;
            m.M13 = (float)Math.Sin(alpha);
            m.M14 = 0;
            m.M21 = 0;
            m.M22 = 1;
            m.M23 = 0;
            m.M24 = 0;
            m.M31 = (float)-Math.Sin(alpha);
            m.M32 = 0;
            m.M33 = (float)Math.Cos(alpha);
            m.M34 = 0;
            m.M41 = 0;
            m.M42 = 0;
            m.M43 = 0;
            m.M44 = 1;
            return m;
        }
        public static Matrix RotationZ(double alpha)
        {
            Matrix m = new Matrix();
            m.M11 = (float)Math.Cos(alpha);
            m.M12 = (float)-Math.Sin(alpha);
            m.M13 = 0;
            m.M14 = 0;
            m.M21 = (float)Math.Sin(alpha);
            m.M22 = (float)Math.Cos(alpha);
            m.M23 = 0;
            m.M24 = 0;
            m.M31 = 0;
            m.M32 = 0;
            m.M33 = 1;
            m.M34 = 0;
            m.M41 = 0;
            m.M42 = 0;
            m.M43 = 0;
            m.M44 = 1;
            return m;
        }
        public static Matrix Translation(Vector3 t)
        {
            Matrix m = new Matrix();
            m.M11 = 1;
            m.M12 = 0;
            m.M13 = 0;
            m.M14 = t.X;
            m.M21 = 0;
            m.M22 = 1;
            m.M23 = 0;
            m.M24 = t.Y;
            m.M31 = 0;
            m.M32 = 0;
            m.M33 = 1;
            m.M34 = t.Z;
            m.M41 = 0;
            m.M42 = 0;
            m.M43 = 0;
            m.M44 = 1;
            return m;
        }
        //n-bliższa płaszczyzna ograniczająca z-towa składowa
        //f-dalsza płaszczyzna ograniczająca z-towa składowa
        //a-stosunek wysokości do szerokości ekranu
        //fov(field of view) - kąt widzenia 
        public static Matrix Prespective(float fov, float a, float n, float f)
        {
            Matrix m = Matrix.Zero;
            m.M22 = (float)(1.0f / Math.Tan(fov * 0.5f));
            m.M11 = (float)(m.M22 / a);
            m.M33 = (float)(-f / (f - n));
            m.M34 = (float)((-f * n) / (f - n));
            m.M43 = -1;

            return m;
        }

        //Macierz przekształcenia współrzędnych gdzie argumenty to wektory układu źródłowego wyrażone we współrzędnych układu docelowego
        public static Matrix TranslationCoordinationSrc(Vector3 x, Vector3 y, Vector3 z, Vector3 Zero)
        {
            Matrix m = Matrix.Identity;
            m.Row1 = new Vector4(x.X, y.X, z.X, Zero.X);
            m.Row2 = new Vector4(x.Y, y.Y, z.Y, Zero.Y);
            m.Row3 = new Vector4(x.Z, y.Z, z.Z, Zero.Z);
            return m;
        }
        //Macierz przekształcenia współrzędnych gdzie argumenty to wektory układu docelowego wyrażone we współrzędnych układu źródłowego
        public static Matrix TranslationCoordinationDst(Vector3 x, Vector3 y, Vector3 z, Vector3 Zero)
        {
            Matrix m = Matrix.Identity;
            m.Row1 = new Vector4(x.X, x.Y, x.Z, Vector3.Dot(x, Zero));
            m.Row2 = new Vector4(y.X, y.Y, y.Z, Vector3.Dot(y, Zero));
            m.Row3 = new Vector4(z.X, z.Y, z.Z, Vector3.Dot(z, Zero));
            return m;
        }

        public static Vector4 TransformCoordinateWithMatrix(Vector4 coordinate, Matrix transform)
        {
            var vec=transform.Multiply(coordinate);
            if (vec.W == 0) return vec;
            return Vector4.Divide(vec, vec.W);
        }
    }
}
