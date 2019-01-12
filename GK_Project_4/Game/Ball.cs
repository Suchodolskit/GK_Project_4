using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoftEngine;
using SharpDX;

namespace GK_Project_4.Game
{
    class Ball
    {
        private Mesh Triangles;
        public float Radius;
        public float VelocityValue;
        public Vector3 VelocityDirection;
        public float Mass;

        public Ball(Mesh triangles, float radius, float Mass = 1)
        { 
            Triangles = triangles;
            Radius = radius;
            this.Mass = Mass;
        }

        public Vector3 Position { get => Triangles.Position; set => Triangles.Position = value; }
        public Vector3 Rotation { get => Triangles.Rotation; set => Triangles.Rotation = value; }
    }
}
