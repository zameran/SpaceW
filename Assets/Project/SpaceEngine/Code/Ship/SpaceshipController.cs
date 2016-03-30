using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpaceshipController : MonoBehaviour
{
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
            foreach (SpaceshipThruster _thruster in fowrwardThrusters)
            {
                _thruster.StartThruster();
                _thruster.thurstCoeff = forceModifier;
            }
        }

        if (Input.GetButtonUp("Fire1"))
        {
            foreach (SpaceshipThruster _thruster in fowrwardThrusters)
            {
                _thruster.StopThruster();
                _thruster.thurstCoeff = forceModifier;
            }
        }

        if (Input.GetButtonDown("Fire2"))
        {
            foreach (SpaceshipThruster _thruster in backwardThrusters)
            {
                _thruster.StartThruster();
                _thruster.thurstCoeff = forceModifier;
            }
        }

        if (Input.GetButtonUp("Fire2"))
        {
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