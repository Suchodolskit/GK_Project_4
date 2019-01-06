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
            //device.Clear(0, 0, 0, 255);

            //for (int i = 0; i < 1; i++)
            //{
            //    // rotating slightly the cube during each frame rendered
            //    foreach (var mesh in meshes[i])
            //    {
            //        // rotating slightly the meshes during each frame rendered
            //        mesh.Rotation = new Vector3(mesh.Rotation.X + 0.05f, mesh.Rotation.Y, mesh.Rotation.Z);
            //        mesh.Position = new Vector3(mesh.Position.X, mesh.Position.Y, mesh.Position.Z - 0.1f);
            //    }
            //    // Doing the various matrix operations
            //    device.Render(mera, meshes[i]);
            //}

            //    //device.Render(mera, meshes2);

            //// Flushing the back buffer into the front buffer
            //device.Present();

            t = t + 0.1;
            mera.Position = new Vector3((float)(10*Math.Cos(t)), (float)(10*Math.Sin(t)), mera.Position.Z);

            List<Polygon> l = new List<Polygon>();

            foreach (var face in meshes[0][0].Faces)
            {
                var vertexA = meshes[0][0].Vertices[face.A];
                var vertexB = meshes[0][0].Vertices[face.B];
                var vertexC = meshes[0][0].Vertices[face.C];
                Polygon p = new Polygon(vertexA, vertexB, vertexC);
                var viewMatrix = TransitionMatrices.LookAt(mera);
                var pm = TransitionMatrices.Prespective(0.5f, (float)640 / 480, 0.0f,1.0f);
                Matrix m = Matrix.Identity;
                var m2 = pm * viewMatrix;

                p.MakeTemporaryVertexStructureList(m2, m);
                p.ClipByCuttingPlanes();
                p.Computepprim();
                p.ClipByWindowSize();
                l.Add(p);
            }

            List<Polygon> l2 = new List<Polygon>();

            Random r = new Random();
            device.Clear(0, 0, 0, 255);

            foreach (var tmp in l)
            {
                if (!tmp.NotDrawedPolygon)
                {
                    l2.Add(tmp);
                    var lis = tmp.PrepareEdgesToScanLineAlgorithm(device.renderWidth, device.renderHeight);
                    Scanline s = new Scanline(device, lis);

                    s.Fill(System.Drawing.Color.FromArgb(r.Next() % 255, r.Next() % 255, r.Next() % 255, r.Next() % 255));
                }
            }
            device.Present();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Choose the back buffer resolution here
            DirectBitmap bmp = new DirectBitmap(640, 480);

            device = new Device(bmp,pictureBox1);

            pictureBox1.Image = bmp.Bitmap;

            meshes = new Mesh[1][];

            for (int i = 0; i < 1; i++)
            {
                meshes[i] = await device.LoadJSONFileAsync("dd");
            }

            mera.Position = new Vector3(10, 0, 0);
            mera.Target = new Vector3(0,0,0);
            mera.Up = new Vector3(0, 0, 1);

            Rendring = new Timer();
            Rendring.Tick += CompositionTarget_Rendering;
            Rendring.Interval = 1;
            Rendring.Start();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<Polygon> l = new List<Polygon>();

            //for (int i = 0; i < meshes[0][0].Faces.Length; i++)
            //{
            //    Polygon p=new Polygon()
            //}
            foreach (var face in meshes[0][0].Faces)
            {
                var vertexA = meshes[0][0].Vertices[face.A];
                var vertexB = meshes[0][0].Vertices[face.B];
                var vertexC = meshes[0][0].Vertices[face.C];
                Polygon p = new Polygon(vertexA,vertexB,vertexC);
                var viewMatrix = TransitionMatrices.LookAt(mera);
                var pm = TransitionMatrices.Prespective(0.78f, (float)320 / 240, 0.0f, 100.0f);
                var m = TransitionMatrices.Translation(meshes[0][0].Position) * TransitionMatrices.RotationZ(meshes[0][0].Rotation.Z) * TransitionMatrices.RotationY(meshes[0][0].Rotation.Y) * TransitionMatrices.RotationX(meshes[0][0].Rotation.X);
                m.Transpose();
                var m2 = pm * viewMatrix;

                p.MakeTemporaryVertexStructureList(m, m2);
                p.ClipByCuttingPlanes();
                p.Computepprim();
                p.ClipByWindowSize();
                l.Add(p);
            }

            List<Polygon> l2 = new List<Polygon>();

            Random r = new Random();
            device.Clear(0, 0, 0, 255);

            foreach (var tmp in l)
            {
                if (!tmp.NotDrawedPolygon)
                {
                    l2.Add(tmp);
                    var lis = tmp.PrepareEdgesToScanLineAlgorithm(device.renderWidth, device.renderHeight);
                    Scanline s = new Scanline(device, lis);

                    s.Fill(System.Drawing.Color.Blue);
                }
            }
            device.Present();
        }
    }
}
