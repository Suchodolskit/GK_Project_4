using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;


namespace SoftEngine
{
    class PointLight
    {
        public Vector3 Position;
        public System.Drawing.Color color;

        public PointLight(Vector3 position, System.Drawing.Color color)
        {
            Position = position;
            this.color = color;
        }
    }
}
