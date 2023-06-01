#region License
// Procedural planet generator.
//  
// Copyright (C) 2015-2023 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: 2017.02.20
// Creation Time: 10:45 PM
// Creator: zameran
// 
#endregion

using SpaceEngine.Helpers;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace SpaceEngine.Core.Bodies
{
    public class CelestialBody : Body, ICelestialBody
    {              
        #region ICelestialBody

        public float Radius
        {
            get => Size;
            set => Size = value;
        }

        public override List<string> GetKeywords()
        {
            var keywords = new List<string>();

            var lightCount = Suns.Count(sun => sun != null && sun.gameObject.activeInHierarchy);
            var leastOneLight = lightCount != 0;

            if (leastOneLight)
            {
                keywords.Add($"LIGHT_{lightCount}");

                var shadowsCount = ShadowCasters.Count(shadow => shadow != null && Helper.Enabled(shadow));

                if (shadowsCount > 0 && GodManager.Instance.Planetshadows)
                {
                    keywords.Add($"SHADOW_{shadowsCount}");
                }
                else
                {
                    keywords.Add("SHADOW_0");
                }
            }
            else
            {
                keywords.Add("LIGHT_0");
                keywords.Add("SHADOW_0");
            }

            if (EclipseCasters.Count == 0)
            {
                keywords.Add("ECLIPSES_OFF");
            }
            else
            {
                keywords.Add(GodManager.Instance.Eclipses && leastOneLight ? "ECLIPSES_ON" : "ECLIPSES_OFF");
            }

            if (ShineCasters.Count == 0)
            {
                keywords.Add("SHINE_OFF");
            }
            else
            {
                keywords.Add(GodManager.Instance.Planetshine && leastOneLight ? "SHINE_ON" : "SHINE_OFF");
            }

            if (Ring != null)
            {
                if (RingEnabled)
                {
                    keywords.Add("RING_ON");
                    keywords.Add("SCATTERING");
                }
                else
                {
                    keywords.Add("RING_OFF");
                }
            }
            else
            {
                keywords.Add("RING_OFF");
            }

            if (Atmosphere != null)
            {
                keywords.Add(AtmosphereEnabled ? "ATMOSPHERE_ON" : "ATMOSPHERE_OFF");

                if (Ocean != null)
                {
                    if (OceanEnabled && AtmosphereEnabled)
                    {
                        keywords.Add("OCEAN_ON");
                    }
                    else
                    {
                        keywords.Add("OCEAN_OFF");
                    }
                }
                else
                {
                    keywords.Add("OCEAN_OFF");
                    keywords.Add("OCEAN_DEPTH_OFF");
                }
            }
            else
            {
                keywords.Add("ATMOSPHERE_OFF");
                keywords.Add("OCEAN_OFF");
                keywords.Add("OCEAN_DEPTH_OFF");
            }

            return keywords;
        }

        #endregion

        #region IUniformed<MaterialPropertyBlock>

        public override void InitUniforms(MaterialPropertyBlock target)
        {
            base.InitUniforms(target);
        }

        public override void SetUniforms(MaterialPropertyBlock target)
        {
            base.SetUniforms(target);
        }

        public override void InitSetUniforms()
        {
            base.InitSetUniforms();
        }

        #endregion

        #region IReanimateable

        public override void Reanimate()
        {
            base.Reanimate();
        }

        #endregion

        #region Node

        protected override void InitNode()
        {
            Offset = new Vector3(0.0f, 0.0f, Radius);

            base.InitNode();
        }

        protected override void UpdateNode()
        {
            base.UpdateNode();
        }

        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        #endregion

        #region IRenderable

        public override void Render(int layer = 8)
        {
            base.Render(layer);
        }

        #endregion

        protected override void OnApplicationFocus(bool focusStatus)
        {
            base.OnApplicationFocus(focusStatus);
        }
    }
}