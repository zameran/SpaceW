using UnityEngine;

namespace SpaceEngine.Startfield
{
    public struct StarfieldStar
    {
        public Vector3 Position;

        public Color Color;

        public StarfieldStar(Vector3 Position, Color Color)
        {
            this.Position = Position;

            this.Color = Color;
        }

        public StarfieldStar(StarfieldStarJson starJson)
        {
            this.Position = new Vector3(starJson.X, starJson.Y, starJson.Z);

            this.Color = new Color(starJson.R, starJson.G, starJson.B, starJson.A);
        }
    }
}