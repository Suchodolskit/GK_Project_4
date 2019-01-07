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

        void CompositionTarget_Rendering(object sender, object e)
        {
            device.Clear(255, 0, 0, 0);
            t = t + 0.1;
            mera.Position = new Vector3(mera.Position.X, mera.Position.Y-0.01f, mera.Position.Z);
            //meshes[0][0].Rotation = new Vector3(meshes[0][0].Rotation.X, meshes[0][0].Rotation.Y, meshes[0][0].Rotation.Z + 0.1f);

            device.MyRender(mera, meshes);
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Choose the back buffer resolution here
            DirectBitmap bmp = new DirectBitmap(640,480);
            device = new Device(bmp,pictureBox1);
            pictureBox1.Image = bmp.Bitmap;
            meshes = new Mesh[4][];

            meshes[0] = await device.LoadJSONFileAsync(@"Meshes/Plate.babylon");
            meshes[0][0].Position = new Vector3(0, 0, 0);

            meshes[1] = await device.LoadJSONFileAsync(@"Meshes/Ball.babylon",0.2f);
            meshes[1][0].Position = new Vector3(0, 0.2f, 0);

            meshes[2] = await device.LoadJSONFileAsync(@"Meshes/Ball.babylon", 0.2f);
            meshes[2][0].Position = new Vector3(0.4f, 0.2f, 0);

            meshes[3] = await device.LoadJSONFileAsync(@"Meshes/Ball.babylon", 0.2f);
            meshes[3][0].Position = new Vector3(-0.4f, 0.2f, 0);

            //meshes[2] = await device.LoadJSONFileAsync(@"Meshes/Ball.babylon", 0.2f);
            //meshes[2][0].Position = new Vector3(0.4f, 0.2f, 0);

            //meshes[0][0] = ProduceCube();



            mera.Position = new Vector3(0, 2, 5);
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
