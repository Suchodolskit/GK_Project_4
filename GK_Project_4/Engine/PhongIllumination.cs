using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SoftEngine
{
    class PhongIllumination
    {
        public static System.Drawing.Color Compute(Vector3 ka, Vector3 kd, Vector3 ks,Vector3 CamPos,Vector4 ScenePoint,Vector4 SceneNormal, Vector3 Ia, List<Light> lights,int m)
        {
            float r = Ia.X * ka.X;
            float g = Ia.Y * ka.Y;
            float b = Ia.Z * ka.Z;

            foreach (Light l in lights)
            {
                var v = l.ComputePhongIlumination(ka, kd, ks, CamPos, ScenePoint, SceneNormal, m);
                r += v.X;
                g += v.Y;
                b += v.Z;
            }

            int R = (int)(r * 255);
            int G = (int)(g * 255);
            int B = (int)(b * 255);

            if (R > 255) R = 255;
            if (G > 255) G = 255;
            if (B > 255) B = 255;

            if (R < 0) R = 0;
            if (G < 0) G = 0;
            if (B < 0) B = 0;


            return System.Drawing.Color.FromArgb(R,G,B);
        }
        public static Vector3 ComputeVector(Vector3 ka, Vector3 kd, Vector3 ks, Vector3 CamPos, Vector4 ScenePoint, Vector4 SceneNormal, Vector3 Ia, List<Light> lights, int m)
        {
            var tmp = SceneNormal;
            float r = Ia.X * ka.X;
            float g = Ia.Y * ka.Y;
            float b = Ia.Z * ka.Z;

            foreach (Light l in lights)
            {
                var v = l.ComputePhongIlumination(ka, kd, ks, CamPos, ScenePoint, SceneNormal, m);
                r += v.X;
                g += v.Y;
                b += v.Z;
            }

            if (r > 1) r = 1;
            if (g > 1) g = 1;
            if (b > 1) b = 1;

            if (r < 0) r = 0;
            if (g < 0) g = 0;
            if (b < 0) b = 0;

            return new Vector3(r, g, b);
        }
    }
}
