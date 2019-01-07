using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
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
        private readonly float[] ZBuffer;

        //długość i szerokość obszaru renderowania
        public readonly int renderWidth;
        public readonly int renderHeight;

        private PictureBox picturebox;

        public Device(DirectBitmap bmp, PictureBox p)
        {
            this.bmp = bmp;
            renderWidth = bmp.Width;
            renderHeight = bmp.Height;
            picturebox = p;

            //potrzeba * 4 bo jeden pixel reprezetowany jako ARGB na 4 bajtach 
            backBuffer = new byte[bmp.Width * bmp.Height * 4];

            ZBuffer = new float[bmp.Width * bmp.Height];
        }

        // Metoda czyści bufory na zadany kolor
        public void Clear(byte a, byte r, byte g, byte b)
        {
            // czyszczenie back buffora
            for (var index = 0; index < backBuffer.Length; index += 4)
            {
                backBuffer[index] = r;
                backBuffer[index + 1] = g;
                backBuffer[index + 2] = b;
                backBuffer[index + 3] = a;
            }

            // czyszczenie depth buffora
            for (var index = 0; index < ZBuffer.Length; index++)
            {
                ZBuffer[index] = float.MaxValue;
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

            if (ZBuffer[index] < z)
            {
                return; // odrzucenie jeśli już na tym pixelu "jest coś bliżej"
            }

            //wypełnienie bufforów
            ZBuffer[index] = z;

            backBuffer[index4] = (byte)(color.B);
            backBuffer[index4 + 1] = (byte)(color.G);
            backBuffer[index4 + 2] = (byte)(color.R);
            backBuffer[index4 + 3] = (byte)(color.A);
        }

        public Vertex Project(Vertex vertex, Matrix transMat, Matrix world)
        {
            // transforming the coordinates into 2D space
            //var point2d = Vector3.TransformCoordinate(vertex.Coordinates, transMat);

            var p1 = vertex.Coordinates;
            var p2 = vertex.Normal;
            transMat.Transpose();
            var point2d = TransitionMatrices.TransformCoordinateWithMatrix(p1, transMat);
            transMat.Transpose();


            // transforming the coordinates & the normal to the vertex in the 3D world


            //var point3dWorld = Vector3.TransformCoordinate(vertex.Coordinates, world);
            //var normal3dWorld = Vector3.TransformCoordinate(vertex.Normal, world);
            world.Transpose();
            var point3dWorld = TransitionMatrices.TransformCoordinateWithMatrix(p1, world);
            var normal3dWorld = TransitionMatrices.TransformCoordinateWithMatrix(p2, world);
            world.Transpose();


            // The transformed coordinates will be based on coordinate system
            // starting on the center of the screen. But drawing on screen normally starts
            // from top left. We then need to transform them again to have x:0, y:0 on top left.
            var x = point2d.X * renderWidth + renderWidth / 2.0f;
            var y = -point2d.Y * renderHeight + renderHeight / 2.0f;

            return new Vertex
            {
                Coordinates = new Vector4(x, y, point2d.Z,1),
                Normal =normal3dWorld,
                WorldCoordinates =point3dWorld
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

        public void MyRender(Camera camera, Mesh[][] meshes)
        {
            var viewMatrix = TransitionMatrices.LookAt(camera);
            var ProjectionMatrix= TransitionMatrices.Prespective(0.8f, (float)500/500, -0.1f, 0.1f);


            Parallel.For(0, meshes.Length, fa =>
                 {
                     List<Polygon> l = new List<Polygon>();

                     var WorldMatrix = TransitionMatrices.RotationX(meshes[fa][0].Rotation.Y) * TransitionMatrices.RotationX(meshes[fa][0].Rotation.X) * TransitionMatrices.RotationX(meshes[fa][0].Rotation.Z) * TransitionMatrices.Translation(meshes[fa][0].Position);

                     foreach (var face in meshes[fa][0].Faces)
                     {
                         var vertexA = meshes[fa][0].Vertices[face.A];
                         var vertexB = meshes[fa][0].Vertices[face.B];
                         var vertexC = meshes[fa][0].Vertices[face.C];
                         Polygon p = new Polygon(face.Color, vertexA, vertexB, vertexC);



                         p.MakeTemporaryVertexStructureList(WorldMatrix, viewMatrix, ProjectionMatrix);
                         p.ClipByCuttingPlanes();
                         p.Computepprim();
                         p.ClipByWindowSize();
                         l.Add(p);
                     }

                     List<Polygon> l2 = new List<Polygon>();


                     foreach (var tmp in l)
                     {
                         if (!tmp.NotDrawedPolygon)
                         {
                             l2.Add(tmp);
                             var lis = tmp.PrepareEdgesToScanLineAlgorithm(renderWidth, renderHeight);
                             Scanline s = new Scanline(this, lis,camera,tmp.StructureList[0].nw);
                             s.Fill(tmp.color);
                         }
                     }
                 });
            Present();
        }

        //wczytanie pliku JSONowego ze strkturą
        public async Task<Mesh[]> LoadJSONFileAsync(string fileName, bool IfRandomColores, System.Drawing.Color col,float scale = 1.0f)
        {
            var meshes = new List<Mesh>();
            // var file = await System.Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(fileName);
            //var data = await Windows.Storage.FileIO.ReadTextAsync(file);
            //string path = @"D:ProgramerenEngine ProjectSoftEngineSoftEngine" + fileName;

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fileName);

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
                    var x = (float)verticesArray[index * verticesStep].Value*scale;
                    var y = (float)verticesArray[index * verticesStep + 1].Value*scale;
                    var z = (float)verticesArray[index * verticesStep + 2].Value*scale;
                    // Loading the vertex normal exported by Blender
                    var nx = (float)verticesArray[index * verticesStep + 3].Value;
                    var ny = (float)verticesArray[index * verticesStep + 4].Value;
                    var nz = (float)verticesArray[index * verticesStep + 5].Value;
                    mesh.Vertices[index] = new Vertex { Coordinates = new Vector4(x, y, z,1), Normal = new Vector4(nx, ny, nz,0) };
                }

                // Then filling the Faces array

                Random r = new Random();

                for (var index = 0; index < facesCount; index++)
                {
                    var a = (int)indicesArray[index * 3].Value;
                    var b = (int)indicesArray[index * 3 + 1].Value;
                    var c = (int)indicesArray[index * 3 + 2].Value;
                    if (IfRandomColores)
                        mesh.Faces[index] = new Face { A = a, B = b, C = c, Color = System.Drawing.Color.FromArgb(255, r.Next() % 255, r.Next() % 255, r.Next() % 255) };
                    else
                    {
                        mesh.Faces[index] = new Face { A = a, B = b, C = c, Color = col };
                    }
                }

                // Getting the position you've set in Blender
                var position = jsonObject.meshes[meshIndex].position;
                mesh.Position = new Vector3((float)position[0].Value, (float)position[1].Value, (float)position[2].Value);
                meshes.Add(mesh);
            }
            return meshes.ToArray();
        }

    }
}
