using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftEngine
{
    public class Edge
    {
        public TemporaryVertexStructure vertex1;
        public TemporaryVertexStructure vertex2;

        public Edge(TemporaryVertexStructure vertex1, TemporaryVertexStructure vertex2)
        {
            this.vertex1 = vertex1;
            this.vertex2 = vertex2;
        }

        public override string ToString()
        {
            return "(" + vertex1 + "," + vertex2 + ")";
        }

        public Vector4 LowerPoint()
        {
            return vertex1.pprim.Y <= vertex2.pprim.Y ? vertex1.pprim : vertex2.pprim;
        }
        public Vector4 HigherPoint()
        {
            if (LowerPoint() == vertex1.pprim) return vertex2.pprim;
            return vertex1.pprim;
        }
    }
}
