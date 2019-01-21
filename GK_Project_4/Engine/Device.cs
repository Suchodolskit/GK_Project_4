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
    public class Device
    {
        private byte[] backBuffer;

        public DirectBitmap bmp;

        private readonly double[] ZBuffer;

        public readonly int renderWidth;
        public readonly int renderHeight;
        public int shading = 0;
        public Vector3 BackgroundLight;
        public int m;


        private PictureBox picturebox;

        public Device(DirectBitmap bmp, PictureBox p, Vector3 BackGroundLight, int m)
        {
            this.bmp = bmp;
            renderWidth = bmp.Width;
            renderHeight = bmp.Height;
            picturebox = p;

            backBuffer = new byte[bmp.Width * bmp.Height * 4];

            ZBuffer = new double[bmp.Width * bmp.Height];
            this.BackgroundLight = BackGroundLight;
            this.m = m;
        }

        public void Clear(byte a, byte r, byte g, byte b)
        {
            for (var index = 0; index < backBuffer.Length; index += 4)
            {
                backBuffer[index] = r;
                backBuffer[index + 1] = g;
                backBuffer[index + 2] = b;
                backBuffer[index + 3] = a;
            }

            for (var index = 0; index < ZBuffer.Length; index++)
            {
                ZBuffer[index] = float.MaxValue;
            }
        }

        public void Present()
        {
            using (var stream = new MemoryStream(bmp.Bits))
            {
                stream.Write(backBuffer, 0, backBuffer.Length);
            }
            picturebox.Image = bmp.Bitmap;
        }

        public void PutPixel(int x, int y, float z, System.Drawing.Color color)
        {
            var index = (x + y * renderWidth);
            var index4 = index * 4;

            if (ZBuffer[index] < z)
            {
                return;
            }

            ZBuffer[index] = z;

            backBuffer[index4] = (byte)(color.B);
            backBuffer[index4 + 1] = (byte)(color.G);
            backBuffer[index4 + 2] = (byte)(color.R);
            backBuffer[index4 + 3] = (byte)(color.A);
        }

        public void MyRender(Camera camera, Mesh[][] meshes, List<Light> lights)
        {
            var viewMatrix = TransitionMatrices.LookAt(camera);
            var ProjectionMatrix= TransitionMatrices.Prespective(0.8f, (float)renderHeight/renderWidth, 0.01f, 1.0f);

            for(int fa=0;fa<meshes.Length;fa++)
            //Parallel.For(0, meshes.Length, fa =>
                 {
                     List<Polygon> l = new List<Polygon>();

                     var WorldMatrix = TransitionMatrices.Translation(meshes[fa][0].Position) * TransitionMatrices.RotationX(meshes[fa][0].Rotation.Y) * TransitionMatrices.RotationX(meshes[fa][0].Rotation.X) * TransitionMatrices.RotationX(meshes[fa][0].Rotation.Z);

                     foreach (var face in meshes[fa][0].Faces)
                     {
                         var vertexA = meshes[fa][0].Vertices[face.A];
                         var vertexB = meshes[fa][0].Vertices[face.B];
                         var vertexC = meshes[fa][0].Vertices[face.C];
                         Polygon p = new Polygon(face.Color, vertexA, vertexB, vertexC);

                         p.MakeTemporaryVertexStructureList(WorldMatrix, viewMatrix, ProjectionMatrix);
                         p.Computepprim();
                         l.Add(p);
                     }

                     List<Polygon> l2 = new List<Polygon>();


                     foreach (var tmp in l)
                     {
                         if (!tmp.NotDrawedPolygon)
                         {
                             l2.Add(tmp);
                             var lis = tmp.PrepareEdgesToScanLineAlgorithm(renderWidth, renderHeight);
                             Scanline s = new Scanline(this, lis, camera, BackgroundLight,m, shading);
                             s.Fill(tmp.color, lights);
                         }
                     }
                 }//);
        }

        public async Task<Mesh[]> LoadJSONFileAsync(string fileName, bool IfRandomColores, System.Drawing.Color col,float scale = 1.0f)
        {
            var meshes = new List<Mesh>();

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
                var indicesArray = jsonObject.meshes[meshIndex].indices;

                var uvCount = jsonObject.meshes[meshIndex].uvCount.Value;
                var verticesStep = 1;

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

                var verticesCount = verticesArray.Count / verticesStep;
                var facesCount = indicesArray.Count / 3;
                var mesh = new Mesh(jsonObject.meshes[meshIndex].name.Value, verticesCount, facesCount);

                for (var index = 0; index < verticesCount; index++)
                {
                    var x = (float)verticesArray[index * verticesStep].Value*scale;
                    var y = (float)verticesArray[index * verticesStep + 1].Value*scale;
                    var z = (float)verticesArray[index * verticesStep + 2].Value*scale;

                    var nx = (float)verticesArray[index * verticesStep + 3].Value;
                    var ny = (float)verticesArray[index * verticesStep + 4].Value;
                    var nz = (float)verticesArray[index * verticesStep + 5].Value;
                    mesh.Vertices[index] = new Vertex { Coordinates = new Vector4(x, y, z,1), Normal = new Vector4(nx, ny, nz,0) };
                }

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

                var position = jsonObject.meshes[meshIndex].position;
                mesh.Position = new Vector3((float)position[0].Value, (float)position[1].Value, (float)position[2].Value);
                meshes.Add(mesh);
            }
            return meshes.ToArray();
        }
    }
}
