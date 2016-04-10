#region License
/* Procedural planet generator.
*
* Copyright (C) 2015-2016 Denis Ovchinnikov
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions
* are met:
* 1. Redistributions of source code must retain the above copyright
*    notice, this list of conditions and the following disclaimer.
* 2. Redistributions in binary form must reproduce the above copyright
*    notice, this list of conditions and the following disclaimer in the
*    documentation and/or other materials provided with the distribution.
* 3. Neither the name of the copyright holders nor the names of its
*    contributors may be used to endorse or promote products derived from
*    this software without specific prior written permission.

* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
* AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
* IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
* ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
* LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
* CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
* SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
* INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
* CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
* ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
* THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

using UnityEngine;

public class SpaceshipThruster : MonoBehaviour
{
    public float thrusterForce = 10000;

    [HideInInspector]
    public float thurstCoeff = 1.0f;

    public bool addForceAtPosition = false;

    public float soundEffectVolume = 1.0f;

    private bool isActive = false;

    private Transform _cacheTransform;
    private Rigidbody _cacheParentRigidbody;
    private Light _cacheLight;
    private ParticleSystem _cacheParticleSystem;

    public void StartThruster()
    {
        isActive = true;
    }

    public void StopThruster()
    {
        isActive = false;
    }

    void Start()
    {
        _cacheTransform = transform;

        if (transform.parent.GetComponent<Rigidbody>() != null)
        {
            _cacheParentRigidbody = transform.parent.GetComponent<Rigidbody>();
        }
        else
        {
            Debug.LogError("Thruster has no parent with rigidbody that it can apply the force to.");
        }

        _cacheLight = transform.GetComponent<Light>().GetComponent<Light>();

        if (_cacheLight == null)
        {
            Debug.LogError("Thruster prefab has lost its child light. Recreate the thruster using the original prefab.");
        }

        _cacheParticleSystem = GetComponent<ParticleSystem>();

        if (_cacheParticleSystem == null)
        {
            Debug.LogError("Thruster has no particle system. Recreate the thruster using the original prefab.");
        }

        GetComponent<AudioSource>().loop = true;
        GetComponent<AudioSource>().volume = soundEffectVolume;
        GetComponent<AudioSource>().mute = true;
        GetComponent<AudioSource>().Play();
    }

    void Update()
    {
        if (_cacheLight != null)
        {
            _cacheLight.intensity = _cacheParticleSystem.particleCount / 20;
        }

        if (isActive)
        {
            if (GetComponent<AudioSource>().mute)
                GetComponent<AudioSource>().mute = false;

            if (GetComponent<AudioSource>().volume < soundEffectVolume)
                GetComponent<AudioSource>().volume += 5f * Time.deltaTime;

            if (_cacheParticleSystem != null)
            {
                var em = _cacheParticleSystem.emission;
                em.enabled = true;
            }
        }
        else
        {
            if (GetComponent<AudioSource>().volume > 0.01f)
                GetComponent<AudioSource>().volume -= 5f * Time.deltaTime;
            else
                GetComponent<AudioSource>().mute = true;

            if (_cacheParticleSystem != null)
            {
                var em = _cacheParticleSystem.emission;
                em.enabled = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (isActive)
            if (addForceAtPosition)
                _cacheParentRigidbody.AddForceAtPosition(_cacheTransform.up * thrusterForce * thurstCoeff, _cacheTransform.position);
            else
                _cacheParentRigidbody.AddRelativeForce(Vector3.forward * thrusterForce * thurstCoeff);
    }
}