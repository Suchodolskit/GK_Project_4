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
        public Vector4 NormalVector;

        public Scanline(Device device, List<Edge> l,Camera c,Vector4 n)
        {
            this.device = device;
            Edges = l;
            camera = c;
            NormalVector = n;
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
                if(e.vertex1.Y<0)
                {
                    e.vertex1.Y = 0;
                }
                if (e.vertex2.Y < 0)
                {
                    e.vertex2.Y = 0;
                }
                if (e.vertex1.Y >= tab.Length)
                {
                    e.vertex1.Y = tab.Length-1;
                }
                if (e.vertex2.Y >= tab.Length)
                {
                    e.vertex2.Y = tab.Length-1;
                }

                if (e.LowerPoint().Y < minY) minY = (int)e.LowerPoint().Y;
                if (e.HigherPoint().Y > maxY) maxY = (int)e.HigherPoint().Y;
                if (tab[(int)e.LowerPoint().Y] == null) { tab[(int)e.LowerPoint().Y] = new List<AETListElement>(); }
                double ReverseM = ((double)e.HigherPoint().X - (double)e.LowerPoint().X) / ((double)e.HigherPoint().Y - (double)e.LowerPoint().Y);
                tab[(int)e.LowerPoint().Y].Add(new AETListElement((int)e.HigherPoint().Y, (int)e.LowerPoint().X, ReverseM,e));
            }
            return tab;
        }
        public void Fill(System.Drawing.Color c)
        {
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

                    var gradient1 = e1.HigherPoint().Y != e1.LowerPoint().Y ? (i - e1.HigherPoint().Y) / (e1.LowerPoint().Y - e1.HigherPoint().Y) : 1;
                    var gradient2 = e2.HigherPoint().Y != e2.LowerPoint().Y ? (i - e2.HigherPoint().Y) / (e2.LowerPoint().Y - e2.HigherPoint().Y) : 1;

                    int sx = (int)Interpolate(e1.HigherPoint().X, e1.LowerPoint().X, gradient1);
                    int ex = (int)Interpolate(e2.HigherPoint().X, e2.LowerPoint().X, gradient2);

                    // starting Z & ending Z
                    float z1 = Interpolate(e1.HigherPoint().Z, e1.LowerPoint().Z, gradient1);
                    float z2 = Interpolate(e2.HigherPoint().Z, e2.LowerPoint().Z, gradient2);

                    // drawing a line from left (sx) to right (ex) 
                    for (var x = sx; x < ex; x++)
                    {
                        float gradient = (x - sx) / (float)(ex - sx);

                        var z = Interpolate(z1, z2, gradient);
                        if (x >= 0)
                        {
                            Vector3 ka = new Vector3(0.5f, 0.0f, 0.0f);
                            Vector3 kd = new Vector3(0.5f, 0.0f, 0.0f);
                            Vector3 ks = new Vector3(0.5f, 0.0f, 0.0f);
                            Vector4 DrawingPoint = new Vector4(x, i, -z, 1);
                            PointLight p = new PointLight(new Vector3(10, 10, 10), System.Drawing.Color.White);

                            System.Drawing.Color col = PhongIllumination.Compute(ka, ks, ks, camera.Position, DrawingPoint, NormalVector, System.Drawing.Color.White, p, 10);



                            device.DrawPoint(new Vector3(x, i, -z), col);
                        }
                    }
                    //for (int k = (int)AET[j].X; k <= (int)AET[j + 1].X; k++)
                    //{
                    //    device.DrawPoint(new Vector3(k, i, 1), c);
                    //}
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
