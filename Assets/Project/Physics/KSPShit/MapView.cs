namespace Experimental
{
    using System;

    using UnityEngine;

    public class MapView : MonoBehaviour
    {
        public static MapView fetch;

        public Material orbitMaterial;
        public Material patchMaterial;

        public Color[] targetPatchColors = new Color[]
        {
            XKCDColors.Red,
            XKCDColors.Green,
            XKCDColors.Blue,
            XKCDColors.Grapefruit,
            XKCDColors.Heather,
            XKCDColors.Iris
        };

        public Color[] patchColors = new Color[]
        {
            XKCDColors.GreenGrey,
            XKCDColors.GreyishBlue,
            XKCDColors.HotGreen,
            XKCDColors.Khaki,
            XKCDColors.Scarlet,
            XKCDColors.Turquoise
        };

        public static Material OrbitMaterial
        {
            get { return fetch.orbitMaterial; }
        }

        public static Material PatchMaterial
        {
            get { return fetch.patchMaterial; }
        }

        public static Color[] TargetPatchColors
        {
            get
            {
                return fetch.targetPatchColors;
            }
        }

        public static Color[] PatchColors
        {
            get
            {
                return fetch.patchColors;
            }
        }

        private void Awake()
        {
            if (fetch != null)
            {
                throw new Exception("Don't try to instantiate the singleton MapView class more than once!");
            }

            fetch = this;
        }
    }
}