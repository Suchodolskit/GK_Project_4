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
        }

        private Device device;
        Mesh[][] meshes;
        Camera mera = new Camera();
        Timer Rendring;

        // Rendering loop handler
        void CompositionTarget_Rendering(object sender, object e)
        {
            device.Clear(0, 0, 0, 255);

            for (int i = 0; i < 1; i++)
            {
                // rotating slightly the cube during each frame rendered
                foreach (var mesh in meshes[i])
                {
                    // rotating slightly the meshes during each frame rendered
                    mesh.Rotation = new Vector3(mesh.Rotation.X + 0.01f*i+0.01f, mesh.Rotation.Y + 0.01f*i+0.01f, mesh.Rotation.Z);
                }
                // Doing the various matrix operations
                device.Render(mera, meshes[i]);
            }
                
                //device.Render(mera, meshes2);

            // Flushing the back buffer into the front buffer
            device.Present();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // Choose the back buffer resolution here
            DirectBitmap bmp = new DirectBitmap(520, 320);

            device = new Device(bmp,pictureBox1);

            pictureBox1.Image = bmp.Bitmap;

            meshes = new Mesh[1][];

            for (int i = 0; i < 1; i++)
            {
                meshes[i] = await device.LoadJSONFileAsync("dd");
            }

            //meshes1 = await device.LoadJSONFileAsync("monkey.babylon");
            //meshes2 =await device.LoadJSONFileAsync("monkey.babylon");
            //foreach (var mesh in meshes2)
            //{
            //    var p = mesh.Position;
            //    mesh.Position = new Vector3(p.X + 50, p.Y + 50, p.Z + 50);
            //}

            mera.Position = new Vector3(0, 0, 5.0f);
            mera.Target = new Vector3(0,0,0);
            mera.Up = new Vector3(0, 1, 0);

            Rendring = new Timer();
            Rendring.Tick += CompositionTarget_Rendering;
            Rendring.Interval = 1;
            Rendring.Start();

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
