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
        public static System.Drawing.Color Compute(Vector3 ka, Vector3 kd, Vector3 ks,Vector3 CamPos,Vector4 ScenePoint,Vector4 SceneNormal, System.Drawing.Color Ia, PointLight light,int m)
        {
            Vector3 li = Vector3.Divide((light.Position - (Vector3)ScenePoint), (light.Position - (Vector3)ScenePoint).Length());
            float r, g, b;
            float IaR = (float)(Ia.R / 255);
            float IaG = (float)(Ia.G / 255);
            float IaB = (float)(Ia.B / 255);
            float IiR = (float)(light.color.R / 255);
            float IiG = (float)(light.color.G / 255);
            float IiB = (float)(light.color.B / 255);
            r = IaR * ka.X + IiR*( kd.X * Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0) + ks.X *(float)Math.Pow(Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0),m));
            g = IaG * ka.X + IiG*( kd.X * Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0) + ks.X *(float)Math.Pow(Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0),m));
            b = IaB * ka.X + IiB*( kd.X * Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0) + ks.X *(float)Math.Pow(Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0),m));

            int R =(int) (r * 255);
            int G =(int) (r * 255);
            int B =(int) (r * 255);
            if (R > 255) R = 255;
            if (G > 255) G = 255;
            if (B > 255) B = 255;

            return System.Drawing.Color.FromArgb(R,G,B);
        }
    }
}
