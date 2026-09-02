using System.Collections.Generic;
using UnityEngine;

namespace TERRAGRAV.Arena
{
    /// <summary>
    /// Deterministically spawns stylized low-poly environmental decorations (trees, bushes, flowers, rocks)
    /// around the arena perimeter and within non-obstructive buffer zones.
    /// Mobile optimized: keeps batch count and dynamic allocations minimal.
    /// </summary>
    public class ArenaDecorationSpawner : MonoBehaviour
    {
        [Header("Decoration Prefabs (Low-Poly Stylized)")]
        [SerializeField] private GameObject _pineTreePrefab;
        [SerializeField] private GameObject _bushPrefab;
        [SerializeField] private GameObject _flowerClusterPrefab;
        [SerializeField] private GameObject _stoneBlockPrefab;

        [Header("Placement Settings")]
        [SerializeField] private int _seed = 42;
        [SerializeField] private int _perimeterObjectCount = 36;
        [SerializeField] private float _perimeterRadius = 90f;
        [SerializeField] private int _scatteredRockCount = 14;

        private void Start()
        {
            SpawnAllDecorations();
        }

        public void SpawnAllDecorations()
        {
            Random.InitState(_seed);

            // 1. Spawn Perimeter Trees, Bushes & Flowers
            for (int i = 0; i < _perimeterObjectCount; i++)
            {
                float angle = (i / (float)_perimeterObjectCount) * Mathf.PI * 2f + Random.Range(-0.05f, 0.05f);
                float dist = _perimeterRadius - Random.Range(2f, 8f);

                Vector3 pos = new Vector3(
                    Mathf.Cos(angle) * dist,
                    0.25f,
                    Mathf.Sin(angle) * dist
                );

                GameObject selectedPrefab = GetRandomPerimeterPrefab();
                SpawnDecoration(selectedPrefab, pos, Random.Range(0.8f, 1.25f), "PerimeterDeco");
            }

            // 2. Spawn Subtle Scattered Rocks
            for (int i = 0; i < _scatteredRockCount; i++)
            {
                float rx = Random.Range(-65f, 65f);
                float rz = Random.Range(-65f, 65f);
                Vector3 pos = new Vector3(rx, 0.2f, rz);

                SpawnDecoration(_stoneBlockPrefab, pos, Random.Range(0.6f, 1.1f), "ScatteredRock");
            }
        }

        private GameObject GetRandomPerimeterPrefab()
        {
            float roll = Random.value;
            if (roll < 0.45f) return _pineTreePrefab;
            if (roll < 0.80f) return _bushPrefab;
            return _flowerClusterPrefab;
        }

        private void SpawnDecoration(GameObject prefab, Vector3 position, float scale, string defaultName)
        {
            if (prefab != null)
            {
                GameObject obj = Instantiate(prefab, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), transform);
                obj.transform.localScale = Vector3.one * scale;
            }
            else
            {
                // Procedural primitive fallback
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                obj.name = defaultName;
                obj.transform.parent = transform;
                obj.transform.position = position;
                obj.transform.localScale = new Vector3(scale * 0.8f, scale * 1.4f, scale * 0.8f);

                // Disable physics collider so it doesn't block player locomotion
                Collider col = obj.GetComponent<Collider>();
                if (col != null) Destroy(col);
            }
        }
    }
}
