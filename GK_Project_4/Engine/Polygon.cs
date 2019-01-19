using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftEngine
{
    class Polygon
    {
        List<Vertex> Vertices;
        public bool NotDrawedPolygon = false;
        public System.Drawing.Color color;
        public Polygon(System.Drawing.Color c, params Vertex[] tab)
        {
            color = c;
            Vertices = new List<Vertex>();
            for (int i = 0; i < tab.Length; i++)
            {
                Vertices.Add(tab[i]);
            }
        }
        public void MakeTemporaryVertexStructureList(Matrix transformMatrix, Matrix cameraMatrix, Matrix PerspectiveMatrix)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                var tmp = Vertices[i].Clone();
                tmp.pw = transformMatrix.Multiply(tmp.Coordinates);
                tmp.nw = transformMatrix.Multiply(tmp.Normal);
                tmp.pbis = cameraMatrix.Multiply(tmp.pw);
                tmp.pbis = PerspectiveMatrix.Multiply(tmp.pbis);
                Vertices[i] = tmp;
            }
        }
        public void ClipByCuttingPlanes()
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                if (Vertices[i].pbis.Z < 0 || Vertices[i].pbis.Z > Vertices[i].pbis.W)
                {
                    NotDrawedPolygon = true;
                    return;
                }
            }
        }
        public void Computepprim()
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertex tmp = Vertices[i].Clone();
                tmp.pprim = Vector4.Divide(Vertices[i].pbis, Vertices[i].pbis.W);
                Vertices[i] = tmp;
            }
        }
        public void ClipByWindowSize()
        {
            int tmp = 0;
            float middlex = 0.0f;
            float middley = 0.0f;
            for (int i = 0; i < Vertices.Count; i++)
            {
                middlex += Vertices[i].pprim.X;
                middley += Vertices[i].pprim.Y;

                if (Math.Abs(Vertices[i].pprim.X) > 1 || Math.Abs(Vertices[i].pprim.Y) > 1)
                {
                    tmp++;
                }
            }
            middlex /= Vertices.Count;
            middley /= Vertices.Count;
            if (tmp == Vertices.Count && Math.Abs(middlex) > 1 && Math.Abs(middley) > 1) NotDrawedPolygon = true;
        }
        public List<Edge> PrepareEdgesToScanLineAlgorithm(int width, int height)
        {
            if (NotDrawedPolygon) return null;
            List<Edge> Scanlinelist = new List<Edge>();

            for (int i = 0; i < Vertices.Count; i++)
            {
                int j = (i + 1) % Vertices.Count;
                Vertex t1 = new Vertex();
                t1.nw = Vertices[i].nw;
                t1.pw = Vertices[i].pw;
                t1.pbis = Vertices[i].pbis;
                t1.pprim = TransformToBitmapCoordinates(Vertices[i].pprim, width, height);

                Vertex t2 = new Vertex();
                t2.nw = Vertices[j].nw;
                t2.pw = Vertices[j].pw;
                t2.pbis = Vertices[j].pbis;
                t2.pprim = TransformToBitmapCoordinates(Vertices[j].pprim, width, height);

                Edge e = new Edge(t1, t2);
                Scanlinelist.Add(e);

            }
            return Scanlinelist;
        }
        public Vector4 TransformToBitmapCoordinates(Vector4 v, int width, int height)
        {
            int h = height - 1;
            int w = width - 1;
            Vector4 tmp = new Vector4();
            tmp.W = 1;
            tmp.X = (v.X + 1) * w / 2;
            tmp.Y = (-v.Y + 1) * h / 2;
            tmp.Z = v.Z;
            return tmp;
        }

    }
}
