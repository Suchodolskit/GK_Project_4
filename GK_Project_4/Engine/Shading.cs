using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace SoftEngine
{
    class GouardShading
    {
        private Edge e1;
        private Edge e2;
        private Vector3 c11;
        private Vector3 c12;
        private Vector3 c21;
        private Vector3 c22;

        public GouardShading(Edge edge1, Vector3 Edge1Vertex1Color, Vector3 Edge1Vertex2Color, Edge edge2, Vector3 Edge2Vertex1Color, Vector3 Edge2Vertex2Color)
        {
            e1 = edge1; e2 = edge2; c11 = Edge1Vertex1Color; c12 = Edge1Vertex2Color; c21 = Edge2Vertex1Color; c22 = Edge2Vertex2Color;
        }

        public GouardShading() { }
        public Vector3 ComputeColor(int x, int y)
        {

            var gradient1 = e1.HigherPoint().Y != e1.LowerPoint().Y ? (y - e1.HigherPoint().Y) / (e1.LowerPoint().Y - e1.HigherPoint().Y) : 1;
            var gradient2 = e2.HigherPoint().Y != e2.LowerPoint().Y ? (y - e2.HigherPoint().Y) / (e2.LowerPoint().Y - e2.HigherPoint().Y) : 1;

            if (e1.vertex1.pprim.Y < e1.vertex2.pprim.Y) gradient1 = 1-gradient1;
            if (e2.vertex1.pprim.Y < e2.vertex2.pprim.Y) gradient2 = 1-gradient2;

            float x1 = Interpolate(e1.vertex1.pprim.X, e1.vertex2.pprim.X, gradient1);
            float x2 = Interpolate(e2.vertex1.pprim.X, e2.vertex2.pprim.X, gradient2);

            float r1 = Interpolate(c11.X, c12.X, gradient1); 
            float g1 = Interpolate(c11.Y, c12.Y, gradient1); 
            float b1 = Interpolate(c11.Z, c12.Z, gradient1);

            float r2 = Interpolate(c21.X, c22.X, gradient2);
            float g2 = Interpolate(c21.Y, c22.Y, gradient2);
            float b2 = Interpolate(c21.Z, c22.Z, gradient2);

            float left = x1 < x2 ? x1 : x2;
            float right = x1 > x2 ? x1 : x2;

            var gradientx = x1 != x2 ? (x - left) / (right - left) : 1;

            float r = Interpolate(r1, r2, gradientx);
            float g = Interpolate(g1, g2, gradientx);
            float b = Interpolate(b1, b2, gradientx);

            return new Vector3(r,g,b); 
        }

        float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }
    }
    class PhongShading
    {
        private Edge e1;
        private Edge e2;

        public PhongShading(Edge edge1, Edge edge2)
        {
            e1 = edge1; e2 = edge2;
        }

        public PhongShading() { }
        public Vector4 ComputeNormal(int x, int y)
        {
            var gradient1 = e1.HigherPoint().Y != e1.LowerPoint().Y ? (y - e1.HigherPoint().Y) / (e1.LowerPoint().Y - e1.HigherPoint().Y) : 1;
            var gradient2 = e2.HigherPoint().Y != e2.LowerPoint().Y ? (y - e2.HigherPoint().Y) / (e2.LowerPoint().Y - e2.HigherPoint().Y) : 1;

            if (e1.vertex1.pprim.Y < e1.vertex2.pprim.Y) gradient1 = 1 - gradient1;
            if (e2.vertex1.pprim.Y < e2.vertex2.pprim.Y) gradient2 = 1 - gradient2;

            float x1 = Interpolate(e1.vertex1.pprim.X, e1.vertex2.pprim.X, gradient1);
            float x2 = Interpolate(e2.vertex1.pprim.X, e2.vertex2.pprim.X, gradient2);

            float nx1 = Interpolate(e1.vertex1.nw.X, e1.vertex2.nw.X, gradient1);
            float ny1 = Interpolate(e1.vertex1.nw.Y, e1.vertex2.nw.Y, gradient1);
            float nz1 = Interpolate(e1.vertex1.nw.Z, e1.vertex2.nw.Z, gradient1);
        
            float nx2 = Interpolate(e2.vertex1.nw.X, e2.vertex2.nw.X, gradient2);
            float ny2 = Interpolate(e2.vertex1.nw.Y, e2.vertex2.nw.Y, gradient2);
            float nz2 = Interpolate(e2.vertex1.nw.Z, e2.vertex2.nw.Z, gradient2);

            float left = x1 < x2 ? x1 : x2;
            float right = x1 > x2 ? x1 : x2;

            var gradientx = x1 != x2 ? (x - left) / (right - left) : 1;

            float nx = Interpolate(nx1, nx2, gradientx);
            float ny = Interpolate(ny1, ny2, gradientx);
            float nz = Interpolate(nz1, nz2, gradientx);

            return new Vector4(nx, ny, nz, 0.0f);
        }

        float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }
    }
}
