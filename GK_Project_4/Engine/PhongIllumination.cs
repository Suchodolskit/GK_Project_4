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
        public static System.Drawing.Color Compute(Vector3 ka, Vector3 kd, Vector3 ks,Vector3 CamPos,Vector4 ScenePoint,Vector4 SceneNormal, Vector3 Ia, PointLight light,int m)
        {
            Vector3 li = Vector3.Divide((light.Position - (Vector3)ScenePoint), (light.Position - (Vector3)ScenePoint).Length());
            float r, g, b;
            var d1 = Vector3.Dot((Vector3)SceneNormal,li);
            r = Ia.X * ka.X + light.color.X*( kd.X * Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0) + ks.X *(float)Math.Pow(Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0),m));
            g = Ia.Y * ka.Y + light.color.Y*( kd.Y * Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0) + ks.Y *(float)Math.Pow(Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0),m));
            b = Ia.Z * ka.Z + light.color.Z*( kd.Z * Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0) + ks.Z *(float)Math.Pow(Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0),m));

            int R =(int) (r * 255);
            int G =(int) (g * 255);
            int B =(int) (b * 255);
            if (R > 255) R = 255;
            if (G > 255) G = 255;
            if (B > 255) B = 255;
            //if (R < 0) R = 0;
            //if (G < 0) G = 0;
            //if (B < 0) B = 0;
                return System.Drawing.Color.FromArgb(R,G,B);
        }
    }
}
