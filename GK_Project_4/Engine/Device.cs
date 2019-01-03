using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;

namespace SoftEngine
{
    public struct ScanLineData
    {
        public int currentY;
        public float ndotla;
        public float ndotlb;
        public float ndotlc;
        public float ndotld;
    }
    public class Device
    {
        //bufor służący do rysowania
        private byte[] backBuffer;

        //bitmapa do rysowania
        public DirectBitmap bmp;

        //buffor głębokości - z-buffor
        private readonly float[] depthBuffer;

        //długość i szerokość obszaru renderowania
        private readonly int renderWidth;
        private readonly int renderHeight;

        private PictureBox picturebox;

        public Device(DirectBitmap bmp, PictureBox p)
        {
            this.bmp = bmp;
            renderWidth = bmp.Width;
            renderHeight = bmp.Height;
            picturebox = p;

            //potrzeba * 4 bo jeden pixel reprezetowany jako ARGB na 4 bajtach 
            backBuffer = new byte[bmp.Width * bmp.Height * 4];

            depthBuffer = new float[bmp.Width * bmp.Height];
        }

        // Metoda czyści bufory na zadany kolor
        public void Clear(byte a, byte r, byte g, byte b)
        {
            // czyszczenie back buffora
            for (var index = 0; index < backBuffer.Length; index += 4)
            {
                backBuffer[index] = a;
                backBuffer[index + 1] = r;
                backBuffer[index + 2] = g;
                backBuffer[index + 3] = b;
            }

            // czyszczenie depth buffora
            for (var index = 0; index < depthBuffer.Length; index++)
            {
                depthBuffer[index] = float.MaxValue;
            }
        }

        //przepisuje wartość backbuffora do bitmapy a bitmape do picrureboxa 
        public void Present()
        {
            using (var stream = new MemoryStream(bmp.Bits))
            {
                // przepisuje bajty z buffora na bitmapę
                stream.Write(backBuffer, 0, backBuffer.Length);
            }

            //przypisuje bitmape do pictureboxa/przerysowuje
            picturebox.Image = bmp.Bitmap;
        }

        // wrzuca pixel o odpowiednich współrzędnych do backbuffora (jeśli jest takowa potrzeba)
        public void PutPixel(int x, int y, float z, System.Drawing.Color color)
        {
            // indexy w odpowiednich bufforach
            var index = (x + y * renderWidth);
            var index4 = index * 4;

            if (depthBuffer[index] < z)
            {
                return; // odrzucenie jeśli już na tym pixelu "jest coś bliżej"
            }

            //wypełnienie bufforów
            depthBuffer[index] = z;

            backBuffer[index4] = (byte)(color.A);
            backBuffer[index4 + 1] = (byte)(color.R);
            backBuffer[index4 + 2] = (byte)(color.G);
            backBuffer[index4 + 3] = (byte)(color.B);
        }


        // Transformacja współrzędnych z użyciem macierzy transMat
        //public Vector3 Project(Vector3 coord, Matrix transMat)
        //{
        //    //wersja z wykorzystaniem moich extenderów
        //    Vector3 point = transMat.Multiply(coord.ConvertToPoint()).ConvertFromPointOrVector();
        //    float length = (transMat.M14 * coord[0]) + (transMat.M24 * coord[1]) + (transMat.M34 * coord[2]) + transMat.M44;
        //    point = Vector3.Multiply(point, 1.0f / length);

        //    //wersja alternatywna z biblioteki
        //    //var point = Vector3.TransformCoordinate(coord, transMat);

        //    //transformacja współrzędnych ze współrzędnych "zwykłych" (środek bitmapy ma współrzędny 0,0) na wpsółrzędne bitmapy
        //    var x = point.X * bmp.Width + bmp.Width / 2.0f;
        //    var y = -point.Y * bmp.Height + bmp.Height / 2.0f;
        //    return (new Vector3(x, y, point.Z));
        //}

        public Vertex Project(Vertex vertex, Matrix transMat, Matrix world)
        {
            // transforming the coordinates into 2D space
            var point2d = Vector3.TransformCoordinate(vertex.Coordinates, transMat);
            // transforming the coordinates & the normal to the vertex in the 3D world
            var point3dWorld = Vector3.TransformCoordinate(vertex.Coordinates, world);
            var normal3dWorld = Vector3.TransformCoordinate(vertex.Normal, world);

            // The transformed coordinates will be based on coordinate system
            // starting on the center of the screen. But drawing on screen normally starts
            // from top left. We then need to transform them again to have x:0, y:0 on top left.
            var x = point2d.X * renderWidth + renderWidth / 2.0f;
            var y = -point2d.Y * renderHeight + renderHeight / 2.0f;

            return new Vertex
            {
                Coordinates = new Vector3(x, y, point2d.Z),
                Normal = normal3dWorld,
                WorldCoordinates = point3dWorld
            };
        }


        // Rysuje punkty - wywołuje putpixel tylko wtedy gdy widzi, że pixel jest na ekranie
        public void DrawPoint(Vector3 point, System.Drawing.Color color)
        {
            // sprawdza czy na ekranie
            if (point.X >= 0 && point.Y >= 0 && point.X < bmp.Width && point.Y < bmp.Height)
            {
                // rysuje
                PutPixel((int)point.X, (int)point.Y, point.Z, color);
            }
        }

        // renderowanie sceny w każdej klatce
        public void Render(Camera camera, params Mesh[] meshes)
        {
            // Mcierz widoku
            //var viewMatrix = Matrix.LookAtLH(camera.Position,camera.Target, Vector3.UnitY);
            var viewMatrix = TransitionMatrices.LookAt(camera);

            //transponowanie gdyż inna reprezentacja
            viewMatrix.Transpose();


            //macierz projekcji
            var projectionMatrix = Matrix.PerspectiveFovRH(0.78f, (float)bmp.Width / bmp.Height, 0.01f, 1.0f);

            var pm = TransitionMatrices.Prespective(0.78f, (float)bmp.Width / bmp.Height, 0.01f, 1.0f);
            pm.Transpose();
            projectionMatrix = pm;

            foreach (Mesh mesh in meshes) 
            {
                // Macierz świata 
                var worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) * Matrix.Translation(mesh.Position);

                //macierz transormacji współrzędnych
                var transformMatrix = /*worldMatrix */ viewMatrix * projectionMatrix;


                var faceIndex = 0;

                // pętla po wszystkich stronach
                foreach (var face in mesh.Faces)
                {
                    //pobranie pixeli ze strony
                    var vertexA = mesh.Vertices[face.A];
                    var vertexB = mesh.Vertices[face.B];
                    var vertexC = mesh.Vertices[face.C];

                    //przekształcenie ich do narysowania
                    var pixelA = Project(vertexA, transformMatrix, worldMatrix);
                    var pixelB = Project(vertexB, transformMatrix, worldMatrix);
                    var pixelC = Project(vertexC, transformMatrix, worldMatrix);

                    //rysowanie trójkąta
                    DrawTriangle(pixelA, pixelB, pixelC, face.Color);
                    faceIndex++;
                }
            }
        }

        //wczytanie pliku JSONowego ze strkturą
        public async Task<Mesh[]> LoadJSONFileAsync(string fileName)
        {
            var meshes = new List<Mesh>();
            // var file = await System.Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(fileName);
            //var data = await Windows.Storage.FileIO.ReadTextAsync(file);
            //string path = @"D:ProgramerenEngine ProjectSoftEngineSoftEngine" + fileName;

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"torus.babylon");

            string data;

            using (FileStream file = new FileStream(path, FileMode.Open))

            {

                using (StreamReader stream = new StreamReader(file))

                {

                    data = await stream.ReadToEndAsync();

                }

            }




            dynamic jsonObject = Newtonsoft.Json.JsonConvert.DeserializeObject(data);

            for (var meshIndex = 0; meshIndex < jsonObject.meshes.Count; meshIndex++)
            {
                var verticesArray = jsonObject.meshes[meshIndex].vertices;
                // Faces
                var indicesArray = jsonObject.meshes[meshIndex].indices;

                var uvCount = jsonObject.meshes[meshIndex].uvCount.Value;
                var verticesStep = 1;

                // Depending of the number of texture's coordinates per vertex
                // we're jumping in the vertices array  by 6, 8 & 10 windows frame
                switch ((int)uvCount)
                {
                    case 0:
                        verticesStep = 6;
                        break;
                    case 1:
                        verticesStep = 8;
                        break;
                    case 2:
                        verticesStep = 10;
                        break;
                }

                // the number of interesting vertices information for us
                var verticesCount = verticesArray.Count / verticesStep;
                // number of faces is logically the size of the array divided by 3 (A, B, C)
                var facesCount = indicesArray.Count / 3;
                var mesh = new Mesh(jsonObject.meshes[meshIndex].name.Value, verticesCount, facesCount);

                // Filling the Vertices array of our mesh first
                for (var index = 0; index < verticesCount; index++)
                {
                    var x = (float)verticesArray[index * verticesStep].Value;
                    var y = (float)verticesArray[index * verticesStep + 1].Value;
                    var z = (float)verticesArray[index * verticesStep + 2].Value;
                    // Loading the vertex normal exported by Blender
                    var nx = (float)verticesArray[index * verticesStep + 3].Value;
                    var ny = (float)verticesArray[index * verticesStep + 4].Value;
                    var nz = (float)verticesArray[index * verticesStep + 5].Value;
                    mesh.Vertices[index] = new Vertex { Coordinates = new Vector3(x, y, z), Normal = new Vector3(nx, ny, nz) };
                }

                // Then filling the Faces array



                for (var index = 0; index < facesCount; index++)
                {
                    var a = (int)indicesArray[index * 3].Value;
                    var b = (int)indicesArray[index * 3 + 1].Value;
                    var c = (int)indicesArray[index * 3 + 2].Value;
                    mesh.Faces[index] = new Face { A = a, B = b, C = c,Color=System.Drawing.Color.Black};
                }

                // Getting the position you've set in Blender
                var position = jsonObject.meshes[meshIndex].position;
                mesh.Position = new Vector3((float)position[0].Value, (float)position[1].Value, (float)position[2].Value);
                meshes.Add(mesh);
            }
            return meshes.ToArray();
        }

        // przycinanie do odpowiedniej wartości
        float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        // Interpolating the value between 2 vertices 
        // min is the starting point, max the ending point
        // and gradient the % between the 2 points
        float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

        // drawing line between 2 points from left to right
        // papb -> pcpd
        // pa, pb, pc, pd must then be sorted before


        // drawing line between 2 points from left to right
        // papb -> pcpd
        // pa, pb, pc, pd must then be sorted before
        void ProcessScanLine(ScanLineData data, Vertex va, Vertex vb, Vertex vc, Vertex vd, System.Drawing.Color color)
        {
            Vector3 pa = va.Coordinates;
            Vector3 pb = vb.Coordinates;
            Vector3 pc = vc.Coordinates;
            Vector3 pd = vd.Coordinates;

            // Thanks to current Y, we can compute the gradient to compute others values like
            // the starting X (sx) and ending X (ex) to draw between
            // if pa.Y == pb.Y or pc.Y == pd.Y, gradient is forced to 1
            var gradient1 = pa.Y != pb.Y ? (data.currentY - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (data.currentY - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);

            // starting Z & ending Z
            float z1 = Interpolate(pa.Z, pb.Z, gradient1);
            float z2 = Interpolate(pc.Z, pd.Z, gradient2);

            var snl = Interpolate(data.ndotla, data.ndotlb, gradient1);
            var enl = Interpolate(data.ndotlc, data.ndotld, gradient2);

            // drawing a line from left (sx) to right (ex) 
            for (var x = sx; x < ex; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);

                var z = Interpolate(z1, z2, gradient);
                var ndotl = Interpolate(snl, enl, gradient);
                // changing the color value using the cosine of the angle
                // between the light vector and the normal vector
                DrawPoint(new Vector3(x, data.currentY, z), color.Multiply(ndotl));
            }
        }

        public void DrawTriangle(Vertex v1, Vertex v2, Vertex v3, System.Drawing.Color color)
        {
            // Sorting the points in order to always have this order on screen p1, p2 & p3
            // with p1 always up (thus having the Y the lowest possible to be near the top screen)
            // then p2 between p1 & p3
            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            if (v2.Coordinates.Y > v3.Coordinates.Y)
            {
                var temp = v2;
                v2 = v3;
                v3 = temp;
            }

            if (v1.Coordinates.Y > v2.Coordinates.Y)
            {
                var temp = v2;
                v2 = v1;
                v1 = temp;
            }

            Vector3 p1 = v1.Coordinates;
            Vector3 p2 = v2.Coordinates;
            Vector3 p3 = v3.Coordinates;

            // Light position 
            Vector3 lightPos = new Vector3(0, 10, 10);
            // computing the cos of the angle between the light vector and the normal vector
            // it will return a value between 0 and 1 that will be used as the intensity of the color
            float nl1 = ComputeNDotL(v1.WorldCoordinates, v1.Normal, lightPos);
            float nl2 = ComputeNDotL(v2.WorldCoordinates, v2.Normal, lightPos);
            float nl3 = ComputeNDotL(v3.WorldCoordinates, v3.Normal, lightPos);

            var data = new ScanLineData { };

            // computing lines' directions
            float dP1P2, dP1P3;

            // http://en.wikipedia.org/wiki/Slope
            // Computing slopes
            if (p2.Y - p1.Y > 0)
                dP1P2 = (p2.X - p1.X) / (p2.Y - p1.Y);
            else
                dP1P2 = 0;

            if (p3.Y - p1.Y > 0)
                dP1P3 = (p3.X - p1.X) / (p3.Y - p1.Y);
            else
                dP1P3 = 0;

            if (dP1P2 > dP1P3)
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    data.currentY = y;

                    if (y < p2.Y)
                    {
                        data.ndotla = nl1;
                        data.ndotlb = nl3;
                        data.ndotlc = nl1;
                        data.ndotld = nl2;
                        ProcessScanLine(data, v1, v3, v1, v2, color);
                    }
                    else
                    {
                        data.ndotla = nl1;
                        data.ndotlb = nl3;
                        data.ndotlc = nl2;
                        data.ndotld = nl3;
                        ProcessScanLine(data, v1, v3, v2, v3, color);
                    }
                }
            }
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    data.currentY = y;

                    if (y < p2.Y)
                    {
                        data.ndotla = nl1;
                        data.ndotlb = nl2;
                        data.ndotlc = nl1;
                        data.ndotld = nl3;
                        ProcessScanLine(data, v1, v2, v1, v3, color);
                    }
                    else
                    {
                        data.ndotla = nl2;
                        data.ndotlb = nl3;
                        data.ndotlc = nl1;
                        data.ndotld = nl3;
                        ProcessScanLine(data, v2, v3, v1, v3, color);
                    }
                }
            }
        }


        float ComputeNDotL(Vector3 vertex, Vector3 normal, Vector3 lightPosition)
        {
            var lightDirection = lightPosition - vertex;

            normal.Normalize();
            lightDirection.Normalize();

            return Math.Max(0, Vector3.Dot(normal, lightDirection));
        }
    }
}
