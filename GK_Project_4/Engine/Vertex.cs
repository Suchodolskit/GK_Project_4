using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SoftEngine
{
    public struct Vertex
    {
        public Vector4 Normal;
        public Vector4 Coordinates;
        public Vector4 pw;
        public Vector4 nw;
        public Vector4 pprim;
        public Vector4 pbis;

        public Vertex Clone()
        {
            Vertex ret = new Vertex();
            ret.Normal = Normal;
            ret.Coordinates = Coordinates;
            ret.nw = nw;
            ret.pw = pw;
            ret.pprim = pprim;
            ret.pbis = pbis;
            return ret;
        }
    }
}
