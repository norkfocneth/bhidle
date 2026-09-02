using UnityEngine;

namespace TERRAGRAV.Arena
{
    /// <summary>
    /// Procedurally constructs the 2.5D physical arena environment:
    /// Square floor surface and 4 elevated boundary walls.
    /// </summary>
    public class ArenaBuilder : MonoBehaviour
    {
        [Header("Arena Dimensions")]
        [Tooltip("Size of the square arena floor (e.g. 200 for a 200x200 arena).")]
        [SerializeField] private float _arenaSize = 200f;

        [Tooltip("Height of the boundary walls.")]
        [SerializeField] private float _wallHeight = 3.0f;

        [Tooltip("Thickness of the boundary walls.")]
        [SerializeField] private float _wallThickness = 2.0f;

        [Header("Materials")]
        [SerializeField] private Material _floorMaterial;
        [SerializeField] private Material _wallMaterial;

        [Header("Build Settings")]
        [SerializeField] private bool _buildOnStart = true;

        public float ArenaSize => _arenaSize;

        private void Start()
        {
            if (_buildOnStart)
            {
                BuildArena();
            }
        }

        [ContextMenu("Build Arena")]
        public void BuildArena()
        {
            // 1. Clean existing children if rebuilding
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            // 2. Create Arena Floor Quad
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Quad);
            floor.name = "Arena_Floor";
            floor.transform.SetParent(transform);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            floor.transform.localScale = new Vector3(_arenaSize, _arenaSize, 1f);

            if (_floorMaterial != null)
            {
                floor.GetComponent<MeshRenderer>().sharedMaterial = _floorMaterial;
            }

            // 3. Create 4 Boundary Walls
            float halfSize = _arenaSize * 0.5f;
            CreateWall("Wall_North", new Vector3(0f, _wallHeight * 0.5f, halfSize + (_wallThickness * 0.5f)), new Vector3(_arenaSize + (_wallThickness * 2f), _wallHeight, _wallThickness));
            CreateWall("Wall_South", new Vector3(0f, _wallHeight * 0.5f, -halfSize - (_wallThickness * 0.5f)), new Vector3(_arenaSize + (_wallThickness * 2f), _wallHeight, _wallThickness));
            CreateWall("Wall_East", new Vector3(halfSize + (_wallThickness * 0.5f), _wallHeight * 0.5f, 0f), new Vector3(_wallThickness, _wallHeight, _arenaSize));
            CreateWall("Wall_West", new Vector3(-halfSize - (_wallThickness * 0.5f), _wallHeight * 0.5f, 0f), new Vector3(_wallThickness, _wallHeight, _arenaSize));
        }

        private void CreateWall(string wallName, Vector3 position, Vector3 size)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = wallName;
            wall.transform.SetParent(transform);
            wall.transform.localPosition = position;
            wall.transform.localScale = size;

            if (_wallMaterial != null)
            {
                wall.GetComponent<MeshRenderer>().sharedMaterial = _wallMaterial;
            }
        }
    }
}
