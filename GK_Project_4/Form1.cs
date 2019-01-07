using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SoftEngine;
using SharpDX;
using System.IO;
using System.Reflection;

namespace GK_Project_4
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        private Device device;
        Mesh[][] meshes;
        Camera mera = new Camera();
        Timer Rendring;
        double t = 0.0;

        // Rendering loop handler
        void CompositionTarget_Rendering(object sender, object e)
        {
            //device.Clear(255, 0, 0, 0);

            //t += 0.001;
            //for (int i = 0; i < 1; i++)
            //{
            //    // rotating slightly the cube during each frame rendered
            //    foreach (var mesh in meshes[i])
            //    {
            //        // rotating slightly the meshes during each frame rendered
            //        //mesh.Rotation = new Vector3(mesh.Rotation.X + 0.05f, mesh.Rotation.Y, mesh.Rotation.Z);
            //        //mesh.Position = new Vector3(mesh.Position.X, mesh.Position.Y, mesh.Position.Z - 0.1f);
            //        mera.Position = new Vector3((float)(10 * Math.Cos(t)), (float)(10 * Math.Sin(t)),mera.Position.Z );
            //    }
            //    // Doing the various matrix operations
            //    device.Render(mera, meshes[i]);
            //}

            //// Flushing the back buffer into the front buffer
            //device.Present();

            device.Clear(255, 0, 0, 0);
            t = t + 0.01;
            mera.Position = new Vector3(mera.Position.X, mera.Position.Y, mera.Position.Z);

            for (int fa = 0; fa < 4; fa++)
            {
                List<Polygon> l = new List<Polygon>();

                foreach (var face in meshes[fa][0].Faces)
                {
                    var vertexA = meshes[fa][0].Vertices[face.A];
                    var vertexB = meshes[fa][0].Vertices[face.B];
                    var vertexC = meshes[fa][0].Vertices[face.C];
                    Polygon p = new Polygon(face.Color, vertexA, vertexB, vertexC);

                    var viewMatrix = TransitionMatrices.LookAt(mera);
                    var rrr = TransitionMatrices.Prespective(0.8f, (float)480 / 640, -1.0f, 1.0f);
                    Matrix m = TransitionMatrices.Translation(meshes[fa][0].Position);

                    p.MakeTemporaryVertexStructureList(m, viewMatrix, rrr);
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
                        var lis = tmp.PrepareEdgesToScanLineAlgorithm(device.renderWidth, device.renderHeight);
                        Scanline s = new Scanline(device, lis);

                        s.Fill(tmp.color);
                    }
                }
            }
            device.Present();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Choose the back buffer resolution here
            DirectBitmap bmp = new DirectBitmap(640,480);
            device = new Device(bmp,pictureBox1);
            pictureBox1.Image = bmp.Bitmap;
            meshes = new Mesh[4][];
            for (int i = 0; i < 4; i++)
            {
                meshes[i] = await device.LoadJSONFileAsync("dd");
                meshes[i][0].Position = new Vector3(0, i, 0);
            }
            //meshes[0][0] = ProduceCube();



            mera.Position = new Vector3(0, 0, 10);
            mera.Target = new Vector3(0,0,0);
            mera.Up = new Vector3(0, 1, 0);

            Rendring = new Timer();
            Rendring.Tick += CompositionTarget_Rendering;
            Rendring.Interval = 1;
            Rendring.Start();

        }

        private Mesh ProduceCube()
        {
            var mesh = new SoftEngine.Mesh("Cube", 8, 12);
            mesh.Vertices[0] = new Vertex(); mesh.Vertices[0].Coordinates = new Vector4(-1, 1, 1, 1);
            mesh.Vertices[1] = new Vertex(); mesh.Vertices[1].Coordinates = new Vector4(1, 1, 1, 1);
            mesh.Vertices[2] = new Vertex(); mesh.Vertices[2].Coordinates = new Vector4(-1, -1, 1, 1);
            mesh.Vertices[3] = new Vertex(); mesh.Vertices[3].Coordinates = new Vector4(1, -1, 1, 1);
            mesh.Vertices[4] = new Vertex(); mesh.Vertices[4].Coordinates = new Vector4(-1, 1, -1, 1);
            mesh.Vertices[5] = new Vertex(); mesh.Vertices[5].Coordinates = new Vector4(1, 1, -1, 1);
            mesh.Vertices[6] = new Vertex(); mesh.Vertices[6].Coordinates = new Vector4(1, -1, -1, 1);
            mesh.Vertices[7] = new Vertex(); mesh.Vertices[7].Coordinates = new Vector4(-1, -1, -1, 1);

            mesh.Faces[0] = new Face { A = 0, B = 1, C = 2, Color = System.Drawing.Color.Green };
            mesh.Faces[1] = new Face { A = 1, B = 2, C = 3, Color = System.Drawing.Color.Gold };
            mesh.Faces[2] = new Face { A = 1, B = 3, C = 6, Color = System.Drawing.Color.Gray };
            mesh.Faces[3] = new Face { A = 1, B = 5, C = 6, Color = System.Drawing.Color.HotPink };
            mesh.Faces[4] = new Face { A = 0, B = 1, C = 4, Color = System.Drawing.Color.Red };
            mesh.Faces[5] = new Face { A = 1, B = 4, C = 5, Color = System.Drawing.Color.White };

            mesh.Faces[6] = new Face { A = 2, B = 3, C = 7, Color = System.Drawing.Color.Violet };
            mesh.Faces[7] = new Face { A = 3, B = 6, C = 7, Color = System.Drawing.Color.Yellow };
            mesh.Faces[8] = new Face { A = 0, B = 2, C = 7, Color = System.Drawing.Color.Orange};
            mesh.Faces[9] = new Face { A = 0, B = 4, C = 7, Color = System.Drawing.Color.Olive };
            mesh.Faces[10] = new Face { A = 4, B = 5, C = 6, Color = System.Drawing.Color.LightSalmon };
            mesh.Faces[11] = new Face { A = 4, B = 6, C = 7, Color = System.Drawing.Color.LightCyan };
            return mesh;
        }

        private void button1_Click(object sender, EventArgs e) { 
        }
    }
}
