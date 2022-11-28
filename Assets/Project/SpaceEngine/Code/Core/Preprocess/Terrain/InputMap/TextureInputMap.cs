using System;

using UnityEngine;

namespace SpaceEngine.Core.Preprocess.Terrain
{
    public class TextureInputMap : InputMap
    {
        public Texture2D Texture;

        [SerializeField]
        private bool FlipX = false;

        [SerializeField]
        private bool FlipY = false;

        public override int Width => Texture.width;

        public override int Height => Texture.height;

        public override int Channels => 4;

        /// <inheritdoc />
        protected override void Awake()
        {
            if (Texture == null) { throw new NullReferenceException("Please, provide an input map! Current input is null!"); }

            base.Awake();
        }

        public override Vector4 GetValue(int x, int y)
        {
            if (FlipX) x = Width - x - 1;
            if (FlipY) y = Height - y - 1;

            return Texture.GetPixel(x, y);
        }
    }
}