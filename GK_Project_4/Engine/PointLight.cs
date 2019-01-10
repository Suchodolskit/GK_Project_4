using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;


namespace SoftEngine
{
    public class PointLight
    {
        public Vector3 Position;
        public Vector3 color;

        public PointLight(Vector3 position, Vector3 color)
        {
            Position = position;
            this.color = color;
        }
    }
}
