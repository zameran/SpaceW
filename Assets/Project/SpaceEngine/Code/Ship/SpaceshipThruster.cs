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