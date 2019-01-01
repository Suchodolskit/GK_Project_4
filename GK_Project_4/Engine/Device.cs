//using Windows.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using SharpDX;

namespace SoftEngine
{
    public class Device
    {
        private byte[] backBuffer;
        public DirectBitmap bmp;

        public Device(DirectBitmap bmp)
        {
            this.bmp = bmp;
            // the back buffer size is equal to the number of pixels to draw
            // on screen (width*height) * 4 (R,G,B & Alpha values). 
            backBuffer = new byte[bmp.Width * bmp.Height * 4];
        }

        // This method is called to clear the back buffer with a specific color
        public void Clear(byte a, byte r, byte g, byte b) {
            for (var index = 0; index < backBuffer.Length; index += 4)
            {
                // BGRA is used by Windows instead by RGBA in HTML5
                backBuffer[index] = a;
                backBuffer[index + 1] = r;
                backBuffer[index + 2] = g;
                backBuffer[index + 3] = b;
            }
        }

        // Once everything is ready, we can flush the back buffer
        // into the front buffer. 
        public void Present()
        {
            using (var stream = new MemoryStream(bmp.Bits))
            {
                // writing our byte[] back buffer into our WriteableBitmap stream
                stream.Write(backBuffer, 0, backBuffer.Length);
            }
            // request a redraw of the entire bitmap
            
            //bmp.Invalidate();
        }

        // Called to put a pixel on screen at a specific X,Y coordinates
        public void PutPixel(int x, int y, System.Drawing.Color color)
        {
            // As we have a 1-D Array for our back buffer
            // we need to know the equivalent cell in 1-D based
            // on the 2D coordinates on screen
            var index = (x + y * bmp.Width) * 4;

            backBuffer[index] = (byte)(color.A * 255);
            backBuffer[index + 1] = (byte)(color.R * 255);
            backBuffer[index + 2] = (byte)(color.G * 255);
            backBuffer[index + 3] = (byte)(color.B * 255);
        }


        // Project takes some 3D coordinates and transform them
        // in 2D coordinates using the transformation matrix
        public Vector3 Project(Vector3 coord, Matrix transMat)
        {
            // transforming the coordinates
            var point = Vector3.TransformCoordinate(coord, transMat);
            // The transformed coordinates will be based on coordinate system
            // starting on the center of the screen. But drawing on screen normally starts
            // from top left. We then need to transform them again to have x:0, y:0 on top left.
            var x = point.X * bmp.Width + bmp.Width / 2.0f;
            var y = -point.Y * bmp.Height + bmp.Height / 2.0f;
            return (new Vector3(x, y, point.Z));
        }

        // DrawPoint calls PutPixel but does the clipping operation before
        public void DrawPoint(Vector2 point,System.Drawing.Color c)
        {
            // Clipping what's visible on screen
            if (point.X >= 0 && point.Y >= 0 && point.X < bmp.Width && point.Y < bmp.Height)
            {
                // Drawing a point
                PutPixel((int)point.X, (int)point.Y, c);
            }
        }

        // The main method of the engine that re-compute each vertex projection
        // during each frame
        public void Render(Camera camera, params Mesh[] meshes)
        {
            // To understand this part, please read the prerequisites resources
            var viewMatrix = Matrix.LookAtLH(camera.Position, camera.Target, Vector3.UnitY);
            var projectionMatrix = Matrix.PerspectiveFovRH(0.78f, 
                                                           (float)bmp.Width / bmp.Height, 
                                                           0.01f, 1.0f);

            foreach (Mesh mesh in meshes) 
            {
                // Beware to apply rotation before translation 
                var worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) * 
                                  Matrix.Translation(mesh.Position);

                var transformMatrix = worldMatrix * viewMatrix * projectionMatrix;


                foreach (var face in mesh.Faces)
                {
                    var vertexA = mesh.Vertices[face.A];
                    var vertexB = mesh.Vertices[face.B];
                    var vertexC = mesh.Vertices[face.C];

                    var pixelA = Project(vertexA, transformMatrix);
                    var pixelB = Project(vertexB, transformMatrix);
                    var pixelC = Project(vertexC, transformMatrix);

                    DrawLine(pixelA, pixelB);
                    DrawLine(pixelB, pixelC);
                    DrawLine(pixelC, pixelA);
                }
            }
        }

        public void DrawLine(Vector2 point0, Vector2 point1)
        {
            int x0 = (int)point0.X;
            int y0 = (int)point0.Y;
            int x1 = (int)point1.X;
            int y1 = (int)point1.Y;

            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);
            var sx = (x0 < x1) ? 1 : -1;
            var sy = (y0 < y1) ? 1 : -1;
            var err = dx - dy;

            while (true)
            {
                DrawPoint(new Vector2(x0, y0));

                if ((x0 == x1) && (y0 == y1)) break;
                var e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }

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
                    mesh.Vertices[index] = new Vector3(x, y, z);
                }

                // Then filling the Faces array
                for (var index = 0; index < facesCount; index++)
                {
                    var a = (int)indicesArray[index * 3].Value;
                    var b = (int)indicesArray[index * 3 + 1].Value;
                    var c = (int)indicesArray[index * 3 + 2].Value;
                    mesh.Faces[index] = new Face { A = a, B = b, C = c };
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
