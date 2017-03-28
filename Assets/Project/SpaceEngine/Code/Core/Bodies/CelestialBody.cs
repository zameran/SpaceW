#region License
// Procedural planet generator.
//  
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//     notice, this list of conditions and the following disclaimer.
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

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace SpaceEngine.Core.Bodies
{
    public class CelestialBody : Body, ICelestialBody
    {
        public Ring Ring;

        public List<Shadow> Shadows = new List<Shadow>();

        public bool RingEnabled = true;

        public Texture2D GroundDiffuse;
        public Texture2D GroundNormal;
        public Texture2D DetailedNormal;

        #region ICelestialBody

        public float Radius { get { return Size; } set { Size = value; } }

        public override List<string> GetKeywords()
        {
            var Keywords = new List<string>();

            if (Ring != null)
            {
                if (RingEnabled)
                {
                    Keywords.Add("RING_ON");
                    Keywords.Add("SCATTERING");

                    var shadowsCount = Shadows.Count((shadow) => shadow != null && Helper.Enabled(shadow));

                    if (shadowsCount > 0 && GodManager.Instance.Eclipses)
                    {
                        for (byte i = 0; i < shadowsCount; i++)
                        {
                            Keywords.Add("SHADOW_" + (i + 1));
                        }
                    }
                    else
                    {
                        Keywords.Add("SHADOW_0");
                    }
                }
                else
                {
                    Keywords.Add("RING_OFF");
                }
            }
            else
            {
                Keywords.Add("RING_OFF");
            }

            if (Atmosphere != null)
            {
                if (AtmosphereEnabled)
                {
                    var lightCount = Atmosphere.Suns.Count((sun) => sun != null && sun.gameObject.activeInHierarchy);

                    if (lightCount != 0)
                        Keywords.Add("LIGHT_" + lightCount);

                    if (Atmosphere.EclipseCasters.Count == 0)
                    {
                        Keywords.Add("ECLIPSES_OFF");
                    }
                    else
                    {
                        Keywords.Add(GodManager.Instance.Eclipses ? "ECLIPSES_ON" : "ECLIPSES_OFF");
                    }

                    if (Atmosphere.ShineCasters.Count == 0)
                    {
                        Keywords.Add("SHINE_OFF");
                    }
                    else
                    {
                        Keywords.Add(GodManager.Instance.Planetshine ? "SHINE_ON" : "SHINE_OFF");
                    }

                    Keywords.Add("ATMOSPHERE_ON");
                }
                else
                {
                    Keywords.Add("ATMOSPHERE_OFF");
                }

                if (Ocean != null)
                {
                    if (OceanEnabled && AtmosphereEnabled)
                    {
                        Keywords.Add("OCEAN_ON");
                    }
                    else
                    {
                        Keywords.Add("OCEAN_OFF");
                    }
                }
                else
                {
                    Keywords.Add("OCEAN_OFF");
                }
            }
            else
            {
                Keywords.Add("LIGHT_0");
                Keywords.Add("ATMOSPHERE_OFF");
                Keywords.Add("OCEAN_OFF");
            }

            return Keywords;
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

            if (Ring != null)
            {
                Ring.SetShadows(MPB, Shadows);
            }
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
            if (Ring != null)
            {
                if (Ring.ParentBody == null)
                    Ring.ParentBody = this;
            }

            Offset = new Vector3(0.0f, 0.0f, Radius);

            base.InitNode();
        }

        protected override void UpdateNode()
        {
            if (Ring != null)
            {
                if (RingEnabled)
                {
                    Ring.Render();
                }
            }

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

        protected override void OnApplicationFocus(bool focusStatus)
        {
            base.OnApplicationFocus(focusStatus);
        }

        protected override void ResetMPB()
        {
            base.ResetMPB();
        }
    }
}