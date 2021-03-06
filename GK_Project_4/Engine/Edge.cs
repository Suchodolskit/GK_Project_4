﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftEngine
{
    public class Edge
    {
        public Vertex vertex1;
        public Vertex vertex2;

        public Edge(Vertex vertex1, Vertex vertex2)
        {
            this.vertex1 = vertex1;
            this.vertex2 = vertex2;
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
