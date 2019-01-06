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

        public Scanline(Device device, List<Edge> l)
        {
            this.device = device;
            Edges = l;
        }

        private class AETListElement
        {
            public int Ymax;
            public double X;
            public double ReverseM;
            public AETListElement(int ymax, double x, double reverseM)
            {
                Ymax = ymax;
                X = x;
                ReverseM = reverseM;
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
                if (e.LowerPoint().Y < minY) minY = (int)e.LowerPoint().Y;
                if (e.HigherPoint().Y > maxY) maxY = (int)e.HigherPoint().Y;
                if (tab[(int)e.LowerPoint().Y] == null) { tab[(int)e.LowerPoint().Y] = new List<AETListElement>(); }
                double ReverseM = ((double)e.HigherPoint().X - (double)e.LowerPoint().X) / ((double)e.HigherPoint().Y - (double)e.LowerPoint().Y);
                tab[(int)e.LowerPoint().Y].Add(new AETListElement((int)e.HigherPoint().Y, (int)e.LowerPoint().X, ReverseM));
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
                    for (int k = (int)AET[j].X; k <= (int)AET[j + 1].X; k++)
                    {
                        //b.SetPixel(k, i, c);
                        device.DrawPoint(new Vector3(k, i, 1), c);
                    }
                }

                i++;
                for (int j = 0; j < AET.Count; j++)
                {
                    AET[j].X += AET[j].ReverseM;
                }
            }
        }
    }
}
