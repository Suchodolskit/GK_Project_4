using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;


namespace SoftEngine
{
    public abstract class Light
    {
        public abstract Vector3 ComputePhongIlumination(Vector3 ka, Vector3 kd, Vector3 ks, Vector3 CamPos, Vector4 ScenePoint, Vector4 SceneNormal, int m = 1);
    }
    public class PointLight : Light
    {
        public Vector3 Position;
        public Vector3 color;
        public PointLight(Vector3 position, Vector3 color)
        {
            this.Position = position;
            this.color = color;
        }
        public override Vector3 ComputePhongIlumination(Vector3 ka, Vector3 kd, Vector3 ks, Vector3 CamPos, Vector4 ScenePoint, Vector4 SceneNormal, int m = 1)
        {
            Vector3 li = Vector3.Divide((Position - (Vector3)ScenePoint), (Position - (Vector3)ScenePoint).Length());
            Vector3 v = Vector3.Divide((CamPos - (Vector3)ScenePoint), (CamPos - (Vector3)ScenePoint).Length());
            Vector3 ri = Vector3.Multiply((Vector3)SceneNormal, 2 * Vector3.Dot((Vector3)SceneNormal, li)) - li;

            float r, g, b;

            r = color.X * (kd.X * Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0) + ks.X * (float)Math.Pow(Math.Max(Vector3.Dot(v,ri), 0), m));
            g = color.Y * (kd.Y * Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0) + ks.Y * (float)Math.Pow(Math.Max(Vector3.Dot(v,ri), 0), m));
            b = color.Z * (kd.Z * Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0) + ks.Z * (float)Math.Pow(Math.Max(Vector3.Dot(v,ri), 0), m));
            return new Vector3(r, g, b);
        }
    }

    public class DirectionalLight : Light
    {
        public Vector3 Direction;
        public Vector3 color;
        public DirectionalLight(Vector3 dir, Vector3 color)
        {
            Direction = dir;
            this.color = color;
        }
        public override Vector3 ComputePhongIlumination(Vector3 ka, Vector3 kd, Vector3 ks, Vector3 CamPos, Vector4 ScenePoint, Vector4 SceneNormal, int m = 1)
        {
            Vector3 li = -Direction;
            Vector3 v = Vector3.Divide((CamPos - (Vector3)ScenePoint), (CamPos - (Vector3)ScenePoint).Length());
            Vector3 ri = Vector3.Multiply((Vector3)SceneNormal, 2 * Vector3.Dot((Vector3)SceneNormal, li)) - li;
            ri.Normalize();

            float r, g, b;

            var tmp1 = Vector3.Dot((Vector3)SceneNormal, li);
            var tmp2 = Math.Max(Vector3.Dot(v, ri), 0);

            r = color.X * (kd.X * Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0) + ks.X * (float)Math.Pow(Math.Max(Vector3.Dot(v, ri), 0), m));
            g = color.Y * (kd.Y * Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0) + ks.Y * (float)Math.Pow(Math.Max(Vector3.Dot(v, ri), 0), m));
            b = color.Z * (kd.Z * Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0) + ks.Z * (float)Math.Pow(Math.Max(Vector3.Dot(v, ri), 0), m));
            return new Vector3(r, g, b);
        }
    }

    public class SpotLight: Light
    {
        public Vector3 Position;
        public Vector3 Direction;
        public Vector3 color;
        public float focus;

        public SpotLight(Vector3 position, Vector3 direction, Vector3 color, float focus)
        {
            Position = position;
            Direction = direction;
            this.color = color;
            this.focus = focus;
        }

        public override Vector3 ComputePhongIlumination(Vector3 ka, Vector3 kd, Vector3 ks, Vector3 CamPos, Vector4 ScenePoint, Vector4 SceneNormal, int m = 1)
        {
            Vector3 li = Vector3.Divide((Position - (Vector3)ScenePoint), (Position - (Vector3)ScenePoint).Length());
            Vector3 v = Vector3.Divide((CamPos - (Vector3)ScenePoint), (CamPos - (Vector3)ScenePoint).Length());
            Vector3 ri = Vector3.Multiply((Vector3)SceneNormal, 2 * Vector3.Dot((Vector3)SceneNormal, li)) - li;

            float r, g, b;

            var tmp = Vector3.Dot(-Direction, li);

            Vector3 Ii = color * Math.Max(Vector3.Dot(-Direction, li), 0);

            r = Ii.X * (kd.X * Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0) + ks.X * (float)Math.Pow(Math.Max(Vector3.Dot(v, ri), 0), m));
            g = Ii.Y * (kd.Y * Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0) + ks.Y * (float)Math.Pow(Math.Max(Vector3.Dot(v, ri), 0), m));
            b = Ii.Z * (kd.Z * Math.Max(Vector3.Dot((Vector3)SceneNormal, li), 0) + ks.Z * (float)Math.Pow(Math.Max(Vector3.Dot(v, ri), 0), m));
            return new Vector3(r, g, b);
        }
    }
}
