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
    public struct Vertex
    {
        public Vector4 Normal;
        public Vector4 Coordinates;
        public Vector4 WorldCoordinates;

        public override bool Equals(object obj)
        {
            if (!(obj is Vertex))
            {
                return false;
            }

            var vertex = (Vertex)obj;
            return Normal.Equals(vertex.Normal) &&
                   Coordinates.Equals(vertex.Coordinates) &&
                   WorldCoordinates.Equals(vertex.WorldCoordinates);
        }

        public override int GetHashCode()
        {
            var hashCode = -1148142964;
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector4>.Default.GetHashCode(Normal);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector4>.Default.GetHashCode(Coordinates);
            hashCode = hashCode * -1521134295 + EqualityComparer<Vector4>.Default.GetHashCode(WorldCoordinates);
            return hashCode;
        }

        public static bool operator ==(Vertex v1, Vertex v2)
        {
            return v1.WorldCoordinates == v2.WorldCoordinates;
        }
        public static bool operator !=(Vertex v1, Vertex v2)
        {
            return !(v1 == v2);
        }

        public Vertex Clone()
        {
            var v1 = Normal;
            var v2 = Coordinates;
            var v3 = WorldCoordinates; 
            Vertex ret = new Vertex();
            ret.Normal = v1;
            ret.Coordinates = v2;
            ret.WorldCoordinates = v3;
            return ret;
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
