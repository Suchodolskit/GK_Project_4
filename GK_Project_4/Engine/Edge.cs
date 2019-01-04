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
        private bool vertical;
        private bool horizontal;
        public int length; // <=0 oznacza, że krawędź nie ma ustalonej długości

        public Edge(Vertex vertex1, Vertex vertex2)
        {
            this.vertex1 = vertex1;
            this.vertex2 = vertex2;
            this.vertical = false;
            this.horizontal = false;
            this.length = 0;
        }
        public override string ToString()
        {
            return "(" + vertex1 + "," + vertex2 + ")";
        }

        public bool Vertical { get => vertical; set { if (!horizontal) vertical = value; else if (value == false) vertical = false; } }
        public bool Horizontal { get => horizontal; set { if (!vertical) horizontal = value; else if (value == false) horizontal = false; } }

        public bool FixedLenght()
        {
            return length > 0;
        }
        public void ClearConstrains()
        {
            vertical = false;
            horizontal = false;
            length = 0;
        }
        public void ClearLength()
        {
            length = 0;
        }
        //public double DidtansToPoint(Point p)
        //{
        //    double A = (vertex1.Y - vertex2.Y);
        //    double B = (vertex2.X - vertex1.X);
        //    double C = (vertex1.X * vertex2.Y - vertex2.X * vertex1.Y);
        //    return (Math.Abs(A * p.X + B * p.Y + C)) / Math.Sqrt(A * A + B * B);
        //}
        //public static bool Compare(Edge e1, Edge e2)
        //{
        //    return ((e1.vertex1 == e2.vertex1) && (e1.vertex2 == e2.vertex2)) || ((e1.vertex2 == e2.vertex1) && (e1.vertex1 == e2.vertex2));
        //}
        public Vertex LowerPoint()
        {
            return vertex1.Coordinates.Y <= vertex2.Coordinates.Y ? vertex1 : vertex2;
        }
        public Vertex HigherPoint()
        {
            if (LowerPoint() == vertex1) return vertex2;
            return vertex1;
        }
    }
}
