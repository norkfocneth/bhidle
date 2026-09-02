using UnityEngine;

namespace TERRAGRAV.VFX
{
    /// <summary>
    /// Procedural 2.5D Paper Confetti Particle Burst VFX.
    /// Emits randomized tumbling paper particles with custom player faction colors upon elimination.
    /// </summary>
    public class ConfettiExplosionVFX : MonoBehaviour
    {
        [Header("Particle Settings")]
        [SerializeField] private int _particleCount = 45;
        [SerializeField] private float _minSpeed = 5f;
        [SerializeField] private float _maxSpeed = 16f;
        [SerializeField] private float _lifetime = 1.2f;
        [SerializeField] private float _gravity = 14f;

        private ParticleSystem _particleSystem;
        private ParticleSystem.Particle[] _particles;

        private void Awake()
        {
            SetupParticleSystem();
        }

        private void SetupParticleSystem()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            if (_particleSystem == null)
            {
                _particleSystem = gameObject.AddComponent<ParticleSystem>();
            }

            var main = _particleSystem.main;
            main.playOnAwake = false;
            main.loop = false;
            main.startLifetime = _lifetime;
            main.startSpeed = new ParticleSystem.MinMaxCurve(_minSpeed, _maxSpeed);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
            main.gravityModifier = _gravity * 0.05f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = _particleSystem.emission;
            emission.enabled = false;

            var shape = _particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            var renderer = GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.renderMode = ParticleSystemRenderMode.Billboard;
            }

            _particles = new ParticleSystem.Particle[_particleCount];
        }

        /// <summary>
        /// Triggers a colorful paper burst at the target world position.
        /// </summary>
        public void PlayBurst(Vector3 position, Color primaryColor)
        {
            transform.position = position;

            Color[] palette = new Color[]
            {
                primaryColor,
                Color.white,
                new Color(1f, 0.84f, 0f), // Gold
                new Color(0f, 0.82f, 1f), // Cyan
                new Color(1f, 0.18f, 0.33f) // Pink
            };

            for (int i = 0; i < _particleCount; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float speed = Random.Range(_minSpeed, _maxSpeed);
                float elevationSpeed = Random.Range(2f, 7f);

                Vector3 velocity = new Vector3(Mathf.Cos(angle) * speed, elevationSpeed, Mathf.Sin(angle) * speed);

                _particles[i].position = position;
                _particles[i].velocity = velocity;
                _particles[i].startSize = Random.Range(0.25f, 0.5f);
                _particles[i].startLifetime = _lifetime;
                _particles[i].remainingLifetime = _lifetime;
                _particles[i].startColor = palette[Random.Range(0, palette.Length)];
                _particles[i].rotation = Random.Range(0f, 360f);
            }

            _particleSystem.SetParticles(_particles, _particleCount);
            _particleSystem.Play();
        }
    }
}
