using SharpDX;
using System.Collections.Generic;

namespace SoftEngine
{
    public struct Face
    {
        public int A;
        public int B;
        public int C;
        public System.Drawing.Color Color;
        public Face Clone()
        {
            Face f = new Face();
            f.A = A;f.B = B; f.C = C;f.Color = Color;
            return f;
        }
    }
    public class Mesh
    {
        public string Name { get; set; }
        public Vertex[] Vertices { get; private set; }
        public Face[] Faces { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public Mesh(string name, int verticesCount, int facesCount)
        {
            Vertices = new Vertex[verticesCount];
            Faces = new Face[facesCount];
            Name = name;
        }

        public Mesh Clone()
        {
            var m = new Mesh(Name, Vertices.Length, Faces.Length);
            for (int i = 0; i < Vertices.Length; i++)
            {
                m.Vertices[i] = Vertices[i].Clone();
            }
            for (int i = 0; i < Faces.Length; i++)
            {
                m.Faces[i] = Faces[i].Clone();
            }
            m.Position = Position;
            m.Rotation = Rotation;
            return m;
        }
    }
}
