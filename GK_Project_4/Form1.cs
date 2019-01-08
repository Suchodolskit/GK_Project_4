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
        Mesh[][]meshes;
        Camera camera = new Camera();
        Timer Rendring;
        double t = 0.0;

        void CompositionTarget_Rendering(object sender, object e)
        {
            device.Clear(255, 0, 0, 0);
            t = t + 0.01;
           // camera.Up = new Vector3((float)(Math.Cos(t)), camera.Position.Y, (float)(Math.Sin(t)));

            device.MyRender(camera, meshes);

        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.Width = 500; pictureBox1.Height = 500;
            DirectBitmap bmp = new DirectBitmap(500,500);
            device = new Device(bmp,pictureBox1);

            meshes = new Mesh[16][];

            meshes[0] = await device.LoadJSONFileAsync(@"Meshes/Plate.babylon",false,System.Drawing.Color.DarkGreen,10);
            meshes[0][0].Position = new Vector3(0, 0, 0);

            meshes[1] = await device.LoadJSONFileAsync(@"Meshes/Ball.babylon", false, System.Drawing.Color.Coral, 0.5f);
            meshes[1][0].Position = new Vector3(0, 0, 0);

            var m = MakeBillardTriangle(meshes[1][0], new Vector3(0.0f,0.5f,-10.0f), 0.5f);
            for (int i = 1; i <= m.Length; i++)
            {
                meshes[i] = m[i - 1];
            }

            camera.Position = new Vector3(0, 50, 0);
            camera.Target = new Vector3(0,0,0);
            camera.Up = new Vector3(0, 0, 1);

            Rendring = new Timer();
            Rendring.Tick += CompositionTarget_Rendering;
            Rendring.Interval = 50;
            Rendring.Start();

        }

        private Mesh[][] MakeBillardTriangle(Mesh ball, Vector3 Position, float radius)
        {
            Mesh[][] Balls = new Mesh[15][];
            for (int i = 0; i < Balls.Length; i++)
            {
                Balls[i] = new Mesh[1];
                Balls[i][0] = ball.Clone();
                Balls[i][0].Position = Position;
            }
            float pos = -5.0f * radius;
            for (int i = 0; i < 5; i++)
            {
                Balls[i][0].Position = new Vector3(Balls[i][0].Position.X + pos, Balls[i][0].Position.Y, Balls[i][0].Position.Z);
                pos += 2 * radius;
            }
            pos = -4.0f * radius;
            for (int i = 5; i < 9; i++)
            {
                Balls[i][0].Position = new Vector3(Balls[i][0].Position.X + pos, Balls[i][0].Position.Y, Balls[i][0].Position.Z + (float)Math.Sqrt(3) * radius);
                pos += 2 * radius;
            }
            pos = -3.0f * radius;
            for (int i = 9; i < 12; i++)
            {
                Balls[i][0].Position = new Vector3(Balls[i][0].Position.X + pos, Balls[i][0].Position.Y, Balls[i][0].Position.Z + (float)2 * (float)Math.Sqrt(3) * radius);
                pos += 2 * radius;
            }
            pos = -2.0f * radius;
            for (int i = 12; i < 14; i++)
            {
                Balls[i][0].Position = new Vector3(Balls[i][0].Position.X + pos, Balls[i][0].Position.Y, Balls[i][0].Position.Z + (float)3 * (float)Math.Sqrt(3) * radius);
                pos += 2 * radius;
            }
            pos = -1.0f * radius;
            Balls[14][0].Position = new Vector3(Balls[14][0].Position.X + pos, Balls[14][0].Position.Y, Balls[14][0].Position.Z + (float)4 * (float)Math.Sqrt(3) * radius);

            return Balls;
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
