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

using Experimental;

[RequireComponent(typeof(Rigidbody))]
public class SpaceshipController : MonoBehaviour
{
    public Vessel vessel;

    public SpaceshipThruster[] fowrwardThrusters;
    public SpaceshipThruster[] backwardThrusters;

    public float rollRate = 100.0f;
    public float yawRate = 30.0f;
    public float pitchRate = 100.0f;

    public float forceModifier = 1.0f;

    private Rigidbody _cacheRigidbody;

    private void Start()
    {
        if (_cacheRigidbody == null)
            _cacheRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (vessel != null) { vessel.rails = false; vessel.UpdateRails(); }

            foreach (SpaceshipThruster _thruster in fowrwardThrusters)
            {
                _thruster.StartThruster();
                _thruster.thurstCoeff = forceModifier;
            }
        }

        if (Input.GetButtonUp("Fire1"))
        {
            if (vessel != null) { vessel.rails = true; vessel.UpdateRails(); }

            foreach (SpaceshipThruster _thruster in fowrwardThrusters)
            {
                _thruster.StopThruster();
                _thruster.thurstCoeff = forceModifier;
            }
        }

        if (Input.GetButtonDown("Fire2"))
        {
            if (vessel != null) { vessel.rails = false; vessel.UpdateRails(); }

            foreach (SpaceshipThruster _thruster in backwardThrusters)
            {
                _thruster.StartThruster();
                _thruster.thurstCoeff = forceModifier;
            }
        }

        if (Input.GetButtonUp("Fire2"))
        {
            if (vessel != null) { vessel.rails = true; vessel.UpdateRails(); }

            foreach (SpaceshipThruster _thruster in backwardThrusters)
            {
                _thruster.StopThruster();
                _thruster.thurstCoeff = forceModifier;
            }
        }
    }

    private void FixedUpdate()
    {
        _cacheRigidbody.AddRelativeTorque(new Vector3(0, 0, -Input.GetAxis("Horizontal") * rollRate * _cacheRigidbody.mass));

        _cacheRigidbody.AddRelativeTorque(new Vector3(0, Input.GetAxis("Horizontal") * yawRate * _cacheRigidbody.mass, 0));

        _cacheRigidbody.AddRelativeTorque(new Vector3(Input.GetAxis("Vertical") * pitchRate * _cacheRigidbody.mass, 0, 0));
    }
}