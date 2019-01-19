using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;

namespace SoftEngine
{
    public class Scanline
    {
        public List<Edge> Edges;
        public Device device;
        public Camera camera;
        public int shading; // 0 - flat, 1 - Gourand, 2 - Phong
        public Vector3 BackGroundLight;

        public Scanline(Device device, List<Edge> l,Camera c, Vector3 BackGroundLight, int shading=0)
        {
            this.device = device;
            Edges = l;
            camera = c;
            this.shading = shading;
            this.BackGroundLight = BackGroundLight;
        }
        private class AETListElement
        {
            public int Ymax;
            public double X;
            public double ReverseM;
            public Edge edge;
            public AETListElement(int ymax, double x, double reverseM, Edge e)
            {
                Ymax = ymax;
                X = x;
                ReverseM = reverseM;
                edge = e;
            }
            public override string ToString()
            {
                return Ymax.ToString() + ", " + X.ToString() + ", " + ReverseM.ToString();
            }
        }
        private List<AETListElement>[] BucketSort(out int minY, out int maxY)
        {
            minY = int.MaxValue;
            maxY = int.MinValue;
            List<AETListElement>[] tab = new List<AETListElement>[device.bmp.Height+1];
            foreach (var e in Edges)
            {
                if (e.vertex1.pprim.Y < 0)
                {
                    if (e.vertex2.pprim.X - e.vertex1.pprim.X != 0)
                    {
                        double a = (e.vertex2.pprim.Y - e.vertex1.pprim.Y) / (e.vertex2.pprim.X - e.vertex1.pprim.X);
                        double b = e.vertex1.pprim.Y - (a * e.vertex1.pprim.X);
                        e.vertex1.pprim.X = (float)(-b / a);
                    }
                    e.vertex1.pprim.Y = 0;
                }
                if (e.vertex2.pprim.Y < 0)
                {
                    if (e.vertex2.pprim.X - e.vertex1.pprim.X != 0)
                    {
                        double a = (e.vertex2.pprim.Y - e.vertex1.pprim.Y) / (e.vertex2.pprim.X - e.vertex1.pprim.X);
                        double b = e.vertex1.pprim.Y - (a * e.vertex1.pprim.X);
                        e.vertex2.pprim.X = (float)(-b / a);
                    }
                    e.vertex2.pprim.Y = 0;
                }
                if (e.vertex1.pprim.Y >= tab.Length)
                {
                    if (e.vertex2.pprim.X - e.vertex1.pprim.X != 0)
                    {
                        double a = (e.vertex2.pprim.Y - e.vertex1.pprim.Y) / (e.vertex2.pprim.X - e.vertex1.pprim.X);
                        double b = e.vertex1.pprim.Y - (a * e.vertex1.pprim.X);
                        e.vertex1.pprim.X = (float)((tab.Length - 1 - b) / a);
                    }
                    e.vertex1.pprim.Y = tab.Length - 1;
                }
                if (e.vertex2.pprim.Y >= tab.Length)
                {
                    if (e.vertex2.pprim.X - e.vertex1.pprim.X != 0)
                    {
                        double a = (e.vertex2.pprim.Y - e.vertex1.pprim.Y) / (e.vertex2.pprim.X - e.vertex1.pprim.X);
                        double b = e.vertex1.pprim.Y - (a * e.vertex1.pprim.X);
                        e.vertex2.pprim.X = (float)((tab.Length - 1 - b) / a);
                    }
                    e.vertex2.pprim.Y = tab.Length - 1;
                }
                if (e.LowerPoint().Y < minY) minY = (int)e.LowerPoint().Y;
                if (e.HigherPoint().Y > maxY) maxY = (int)e.HigherPoint().Y;
                if (tab[(int)e.LowerPoint().Y] == null) { tab[(int)e.LowerPoint().Y] = new List<AETListElement>(); }
                double ReverseM = ((double)e.HigherPoint().X - (double)e.LowerPoint().X) / ((double)e.HigherPoint().Y - (double)e.LowerPoint().Y);
                tab[(int)e.LowerPoint().Y].Add(new AETListElement((int)e.HigherPoint().Y, (int)e.LowerPoint().X, ReverseM,e));
            }
            return tab;
        }
        public void Fill(System.Drawing.Color c, List<Light> lights)
        {

            Vector3 ka = new Vector3((float)c.R / 255, (float)c.G / 255, (float)c.B / 255);
            Vector3 kd = new Vector3((float)c.R / 255, (float)c.G / 255, (float)c.B / 255);
            Vector3 ks = new Vector3((float)c.R / 255, (float)c.G / 255, (float)c.B / 255);
            System.Drawing.Color col=c;

            if (shading == 0)
            {
                Vector4 DrawingPoint = (Edges[0].vertex1.pw + Edges[1].vertex1.pw + Edges[2].vertex1.pw) / 3;
                var NormalVector = (Edges[0].vertex1.nw + Edges[1].vertex1.nw + Edges[2].vertex1.nw) / 3;
                col = PhongIllumination.Compute(ka, ks, ks, camera.Position, DrawingPoint, NormalVector, BackGroundLight, lights, 1);
            }

            int minY, maxY;
            var Array = BucketSort(out minY, out maxY);
            List<AETListElement> AET = new List<AETListElement>();
            for (int i = minY; i <= maxY;)
            {
                if (Array[i] != null)
                {
                    List<AETListElement> tmplist = new List<AETListElement>(AET.Count + Array[i].Count);
                    tmplist.AddRange(Array[i]);
                    tmplist.AddRange(AET);
                    AET = tmplist;
                }

                int it = 0;
                while (it < AET.Count())
                {
                    if (AET[it].Ymax == i) AET.RemoveAt(it);
                    else it++;
                }
                AET.Sort((el1, el2) => el1.X.CompareTo(el2.X));
                for (int j = 0; j < AET.Count - 1; j += 2)
                {

                    Edge e1 = AET[j].edge;
                    Edge e2 = AET[j + 1].edge;
                    GouardShading s=new GouardShading();
                    PhongShading ps=new PhongShading();

                    if (shading == 1)
                    {
                        var col11 = PhongIllumination.ComputeVector(ka, ks, ks, camera.Position, e1.vertex1.pw, e1.vertex1.nw, BackGroundLight, lights, 1);
                        var col12 = PhongIllumination.ComputeVector(ka, ks, ks, camera.Position, e1.vertex2.pw, e1.vertex2.nw, BackGroundLight, lights, 1);
                        var col21 = PhongIllumination.ComputeVector(ka, ks, ks, camera.Position, e2.vertex1.pw, e2.vertex1.nw, BackGroundLight, lights, 1);
                        var col22 = PhongIllumination.ComputeVector(ka, ks, ks, camera.Position, e2.vertex2.pw, e2.vertex2.nw, BackGroundLight, lights, 1);
                        s = new GouardShading(e1, col11, col12, e2, col21, col22);
                    }
                    if (shading == 2)
                    {
                        ps = new PhongShading(e1, e2);
                    }

                    var gradient1 = e1.HigherPoint().Y != e1.LowerPoint().Y ? (i - e1.HigherPoint().Y) / (e1.LowerPoint().Y - e1.HigherPoint().Y) : 1;
                    var gradient2 = e2.HigherPoint().Y != e2.LowerPoint().Y ? (i - e2.HigherPoint().Y) / (e2.LowerPoint().Y - e2.HigherPoint().Y) : 1;

                    int sx = (int)Interpolate(e1.HigherPoint().X, e1.LowerPoint().X, gradient1);
                    int ex = (int)Interpolate(e2.HigherPoint().X, e2.LowerPoint().X, gradient2);

                    float z1 = Interpolate(e1.HigherPoint().Z, e1.LowerPoint().Z, gradient1);
                    float z2 = Interpolate(e2.HigherPoint().Z, e2.LowerPoint().Z, gradient2);

                    sx = sx < 0 ? 0 : sx;
                    ex = ex > device.renderWidth ? device.renderWidth : ex;

                    for (var x = sx; x < ex; x++)
                    {
                        float gradient = (x - sx) / (float)(ex - sx);

                        var z = Interpolate(z1, z2, gradient);
                        if (x >= 0)
                        {
                            if (shading == 1)
                            {
                                var tmp=s.ComputeColor(x, i);
                                col = System.Drawing.Color.FromArgb((int)(tmp.X * 255), (int)(tmp.Y * 255), (int)(tmp.Z * 255));
                            }
                            if (shading == 2)
                            {
                                var n = ps.ComputeNormal(x, i);
                                var tmp = PhongIllumination.ComputeVector(ka, ks, ks, camera.Position, e1.vertex1.pw, n, BackGroundLight, lights, 1);
                                col = System.Drawing.Color.FromArgb((int)(tmp.X * 255), (int)(tmp.Y * 255), (int)(tmp.Z * 255));
                            }
                            device.PutPixel(x, i,z, col);
                        }
                    }
                }

                i++;
                for (int j = 0; j < AET.Count; j++)
                {
                    AET[j].X += AET[j].ReverseM;
                }
            }
        }
        float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }
        float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

    }
}
