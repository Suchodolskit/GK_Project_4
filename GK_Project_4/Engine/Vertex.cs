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
}
