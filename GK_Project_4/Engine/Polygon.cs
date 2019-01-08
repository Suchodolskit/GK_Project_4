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
        public List<TemporaryVertexStructure> StructureList;
        public bool NotDrawedPolygon=false;
        public System.Drawing.Color color;
        public Polygon(System.Drawing.Color c,params Vertex[] tab)
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
            StructureList = new List<TemporaryVertexStructure>();
            for (int i = 0; i < Vertices.Count; i++)
            {
                StructureList.Add(new TemporaryVertexStructure(Vertices[i], transformMatrix, cameraMatrix, PerspectiveMatrix));
            }
        }
        public void ClipByCuttingPlanes()
        {

            //bliższa płaszczyzna

            List<bool> CuttedVertextab = new List<bool>() ;
            int CuttedVertexCounter = 0;
            for (int i = 0; i < StructureList.Count; i++)
            {
                if (StructureList[i].pbis.Z < 0) { CuttedVertextab.Add(true); CuttedVertexCounter++; }
                else CuttedVertextab.Add(false);
            }
            //nie ma wielokąta do narysowania
            if (StructureList.Count - CuttedVertexCounter < 1)
            {
                NotDrawedPolygon = true;
                return;
            }
            if (CuttedVertexCounter == 1)
            {
                for (int i = 0; i < StructureList.Count; i++)
                {
                    //obcinamy pojedyńczy wierzchołek - powstaje nowa krawędź
                    if (CuttedVertextab[i])
                    {
                        int j = i - 1 < 0 ? StructureList.Count - 1 : i - 1;
                        int k = (i + 1) % StructureList.Count;
                        TemporaryVertexStructure tmp1 = ComputeNewPositionPlane1(StructureList[j], StructureList[i]);
                        TemporaryVertexStructure tmp2 = ComputeNewPositionPlane1(StructureList[k], StructureList[i]);
                        StructureList[i] = tmp1;
                        StructureList.Insert(i + 1, tmp2);
                        break;
                    }
                }
            }
            else if (CuttedVertexCounter>0)
            {
                var l = new List<Vertex>();
                var ls = new List<TemporaryVertexStructure>();

                int i = 0;
                while (i < StructureList.Count)
                {
                    int j = i - 1 < 0 ? StructureList.Count - 1 : i - 1;
                    int k = (i + 1) % StructureList.Count;
                    //można usunąć i-ty wierzchołek;
                    if (CuttedVertextab[i] && CuttedVertextab[j] && CuttedVertextab[k])
                    {
                        Vertices.RemoveAt(i);
                        StructureList.RemoveAt(i);
                        CuttedVertextab.RemoveAt(i);
                    }
                    else i++;
                }
                i = 0;
                int c = 0;
                while (c < 2)
                {
                    int j = i - 1 < 0 ? StructureList.Count - 1 : i - 1;
                    int k = (i + 1) % StructureList.Count;
                    //zmieniamy i-ty
                    if (CuttedVertextab[i] && !CuttedVertextab[j])
                    {
                        StructureList[i] = ComputeNewPositionPlane1(StructureList[j], StructureList[i]);
                        c++;
                    }
                    if (CuttedVertextab[i] && !CuttedVertextab[k])
                    {
                        StructureList[i] = ComputeNewPositionPlane1(StructureList[k], StructureList[i]);
                        c++;
                    }
                    i = (i + 1) % StructureList.Count;
                }
            }


            //dalsza płaszczyzna 

            CuttedVertextab = new List<bool>();
            CuttedVertexCounter = 0;
            for (int i = 0; i < StructureList.Count; i++)
            {
                if (StructureList[i].pbis.Z > StructureList[i].pbis.W) { CuttedVertextab.Add(true); CuttedVertexCounter++; }
                else CuttedVertextab.Add(false);
            }
            //nie ma wielokąta do narysowania
            if (StructureList.Count - CuttedVertexCounter < 1)
            {
                NotDrawedPolygon = true;
                return;
            }
            if (CuttedVertexCounter == 1)
            {
                for (int i = 0; i < StructureList.Count; i++)
                {
                    //obcinamy pojedyńczy wierzchołek - powstaje nowa krawędź
                    if (CuttedVertextab[i])
                    {
                        int j = i - 1 < 0 ? StructureList.Count - 1 : i - 1;
                        int k = (i + 1) % StructureList.Count;
                        TemporaryVertexStructure tmp1 = ComputeNewPositionPlane2(StructureList[i], StructureList[j]);
                        TemporaryVertexStructure tmp2 = ComputeNewPositionPlane2(StructureList[i], StructureList[k]);
                        StructureList[i] = tmp1;
                        StructureList.Insert(i + 1, tmp2);
                        break;
                    }
                }
            }
            else if (CuttedVertexCounter > 0)
            {
                var l = new List<Vertex>();
                var ls = new List<TemporaryVertexStructure>();

                int i = 0;
                while (i < StructureList.Count)
                {
                    int j = i - 1 < 0 ? StructureList.Count - 1 : i - 1;
                    int k = (i + 1) % StructureList.Count;
                    //można usunąć i-ty wierzchołek;
                    if (CuttedVertextab[i] && CuttedVertextab[j] && CuttedVertextab[k])
                    {
                        Vertices.RemoveAt(i);
                        StructureList.RemoveAt(i);
                        CuttedVertextab.RemoveAt(i);
                    }
                    else i++;
                }
                i = 0;
                int c = 0;
                while (c < 2)
                {
                    int j = i - 1 < 0 ? StructureList.Count - 1 : i - 1;
                    int k = (i + 1) % StructureList.Count;
                    //zmieniamy i-ty
                    if (CuttedVertextab[i] && !CuttedVertextab[j])
                    {
                        StructureList[i] = ComputeNewPositionPlane2(StructureList[i], StructureList[j]);
                        c++;
                    }
                    if (CuttedVertextab[i] && !CuttedVertextab[k])
                    {
                        StructureList[i] = ComputeNewPositionPlane2(StructureList[i], StructureList[k]);
                        c++;
                    }
                    i = (i + 1) % StructureList.Count;
                }
            }
        }

        private TemporaryVertexStructure ComputeNewPositionPlane1(TemporaryVertexStructure appropriate, TemporaryVertexStructure inappropriate)
        {
            float t = appropriate.pbis.Z / (appropriate.pbis.Z - inappropriate.pbis.Z);
            Vector4 pwt = Vector4.Add(Vector4.Multiply(appropriate.pw, (float)(1 - t)), Vector4.Multiply(inappropriate.pw, t));
            Vector4 nwt = Vector4.Add(Vector4.Multiply(appropriate.nw, (float)(1 - t)), Vector4.Multiply(inappropriate.nw, t));
            Vector4 pbist = Vector4.Add(Vector4.Multiply(appropriate.pbis, (float)(1 - t)), Vector4.Multiply(inappropriate.pbis, t));
            TemporaryVertexStructure tmp1 = new TemporaryVertexStructure();
            tmp1.nw = nwt;
            tmp1.pbis = pbist;
            tmp1.pw = pwt;
            return tmp1;
        }
        private TemporaryVertexStructure ComputeNewPositionPlane2(TemporaryVertexStructure inappropriate, TemporaryVertexStructure appropriate)
        {
            float t = (inappropriate.pbis.Z - inappropriate.pbis.W) / (inappropriate.pbis.Z - inappropriate.pbis.W + appropriate.pbis.Z - appropriate.pbis.W); 
            Vector4 pwt = Vector4.Add(Vector4.Multiply(inappropriate.pw, (float)(1 - t)), Vector4.Multiply(appropriate.pw, t));
            Vector4 nwt = Vector4.Add(Vector4.Multiply(inappropriate.nw, (float)(1 - t)), Vector4.Multiply(appropriate.nw, t));
            Vector4 pbist = Vector4.Add(Vector4.Multiply(inappropriate.pbis, (float)(1 - t)), Vector4.Multiply(appropriate.pbis, t));
            TemporaryVertexStructure tmp1 = new TemporaryVertexStructure();
            tmp1.nw = nwt;
            tmp1.pbis = pbist;
            tmp1.pw = pwt;
            return tmp1;
        }

        public void Computepprim()
        {
            for (int i = 0; i < StructureList.Count; i++)
            {
                StructureList[i].pprim = Vector4.Divide(StructureList[i].pbis, StructureList[i].pbis.W);
            }
        }
        public void ClipByWindowSize()
        {
            int tmp = 0;
            float middlex = 0.0f;
            float middley = 0.0f;
            for (int i = 0; i < StructureList.Count; i++)
            {
                middlex += StructureList[i].pprim.X;
                middley += StructureList[i].pprim.Y;

                if (Math.Abs(StructureList[i].pprim.X) > 1 || Math.Abs(StructureList[i].pprim.Y) > 1)
                {
                    tmp++;
                }
            }
            middlex /= StructureList.Count;
            middley /= StructureList.Count;
            if (tmp == StructureList.Count && Math.Abs(middlex) > 1 && Math.Abs(middley) > 1) NotDrawedPolygon = true;
        }
        public List<Edge> PrepareEdgesToScanLineAlgorithm(int width, int height)
        {
            if (NotDrawedPolygon) return null;
            List<Edge> Scanlinelist = new List<Edge>();
            for (int i = 0; i < StructureList.Count; i++)
            {
                int j = (i + 1) % StructureList.Count;
                TemporaryVertexStructure ts1 = new TemporaryVertexStructure();
                ts1.nw = StructureList[i].nw;
                ts1.pbis = StructureList[i].pbis;
                ts1.pw = StructureList[i].pw;
                ts1.pprim = TransformToBitmapCoordinates(StructureList[i].pprim, width, height);

                TemporaryVertexStructure ts2 = new TemporaryVertexStructure();
                ts2.nw = StructureList[j].nw;
                ts2.pbis = StructureList[j].pbis;
                ts2.pw = StructureList[j].pw;
                ts2.pprim = TransformToBitmapCoordinates(StructureList[j].pprim, width, height);

                Edge e = new Edge(ts1, ts2);
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
            tmp.Y= (-v.Y + 1) * h / 2;
            tmp.Z = v.Z;
            return tmp;
        }

    }
    public class TemporaryVertexStructure
    {
        public Vector4 pw;
        public Vector4 nw;
        public Vector4 pprim;
        public Vector4 pbis;

        public TemporaryVertexStructure(Vertex v, Matrix transformMatrix, Matrix CameraMatrix, Matrix Perspective)
        {
            this.pw = transformMatrix.Multiply(v.Coordinates);
            this.nw = transformMatrix.Multiply(v.Normal);
            this.pbis = CameraMatrix.Multiply(pw);
            pbis = Perspective.Multiply(pbis);
        }
        public TemporaryVertexStructure() { }
    }
    public class TemporaryVertexStructureFactory
    {
        public Matrix TransformMatrix;
        public Matrix CameraPerspectiveMatrix;

        public TemporaryVertexStructureFactory(Matrix transformMatrix, Matrix cameraPerspectiveMatrix)
        {
            TransformMatrix = transformMatrix;
            CameraPerspectiveMatrix = cameraPerspectiveMatrix;
        }
    }
}
