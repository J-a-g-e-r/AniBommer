using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class GridMapSpawner : MonoBehaviour
{
    public static GridMapSpawner Instance;
    private float tileSize;

    public GameObject floorPrefabA;
    public GameObject floorPrefabB;
    public GameObject[] indestructiblePrefabs;
    public GameObject[] destructiblePrefabs;
    public GameObject playerPrefab;
    [SerializeField] private GameDatabase gameDatabase;
    [SerializeField] private CinemachineTargetSetter camSetter;
    [SerializeField] private int mapId = 0;

    private TileType[,] mapData;


    private float offsetX;
    private float offsetZ;


    [SerializeField] private NavMeshSurface navMeshSurface;

    private void Awake()
    {
        Instance = this;
        tileSize = floorPrefabA.GetComponent<Renderer>().bounds.size.x;
        InitMapData();

        int width = mapData.GetLength(0);
        int height = mapData.GetLength(1);

        offsetX = -(width * tileSize) / 2f + tileSize / 2f;
        offsetZ = -(height * tileSize) / 2f + tileSize / 2f;
    }
    void Start()
    {

        SpawnMap();
        if (!navMeshSurface) return;

        navMeshSurface.BuildNavMesh();
        GameEvents.OnMapReady?.Invoke();
    }

    void InitMapData()
    {
        mapData = MapLibrary.GetMap(mapId);
        //TileType I = TileType.Indestructible;
        //TileType D = TileType.Destructible;
        //TileType E = TileType.Empty;
        //TileType P = TileType.PlayerSpawn;

        //mapData = new TileType[19, 13]
        //{
        //    { E,E,E,D,E,I,I,I,E,D,E,E,E },
        //    { E,E,E,D,E,E,E,E,E,D,E,P,E },
        //    { E,E,E,D,E,E,E,E,E,D,E,E,E },
        //    { D,D,D,D,E,D,E,D,E,E,E,E,E },
        //    { E,E,E,E,E,E,E,E,E,E,E,E,E },
        //    { E,D,D,D,D,D,D,D,D,D,D,D,E },
        //    { E,D,D,I,E,E,E,E,E,I,D,D,E },
        //    { E,D,D,D,E,E,E,E,E,D,D,D,E },
        //    { E,D,D,D,E,E,E,E,E,D,D,D,E },
        //    { E,D,D,D,E,E,E,E,E,D,D,D,E },
        //    { E,D,D,D,E,E,E,E,E,D,D,D,E },
        //    { E,D,D,D,E,E,E,E,E,D,D,D,E },
        //    { E,D,D,I,E,E,E,E,E,I,D,D,E },
        //    { E,D,D,D,D,D,D,D,D,D,D,D,E },
        //    { E,E,E,E,E,E,E,E,E,E,E,E,E },
        //    { D,D,D,D,E,D,E,D,E,D,D,D,D },
        //    { E,E,E,D,E,E,E,E,E,D,E,E,E },
        //    { E,E,E,D,E,E,E,E,E,D,E,E,E },
        //    { E,E,E,D,E,I,I,I,E,D,E,E,E },

        //};
    }

    void SpawnMap()
    {
        int width = mapData.GetLength(0);
        int height = mapData.GetLength(1);

        float offsetX = -(width * tileSize) / 2f + tileSize / 2f;
        float offsetZ = -(height * tileSize) / 2f + tileSize / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 pos = new Vector3(
                    x * tileSize + offsetX,
                    0,
                    z * tileSize + offsetZ
                );

                // 🔥 Chọn tile xen kẽ
                GameObject floorToSpawn =
                    ((x + z) % 2 == 0) ? floorPrefabA : floorPrefabB;

                Instantiate(floorToSpawn, pos, Quaternion.identity, transform);

                SpawnTile(mapData[x, z], pos);
            }
        }
    }

    void SpawnTile(TileType type, Vector3 pos)
    {
        GameObject obj = null;

        switch (type)
        {
            case TileType.Indestructible:
                obj = indestructiblePrefabs[Random.Range(0, indestructiblePrefabs.Length)];
                break;

            case TileType.Destructible:
                obj = destructiblePrefabs[Random.Range(0, destructiblePrefabs.Length)];
                break;

            case TileType.PlayerSpawn:
                GameObject playerPrefabToSpawn = GetPlayerPrefabFromData();

                GameObject player = Instantiate(playerPrefabToSpawn, pos + new Vector3(0, 0.5f, 0), playerPrefabToSpawn.transform.rotation);
                mapData[WorldToGrid(pos).x, WorldToGrid(pos).y] = TileType.Empty;
                camSetter.SetTarget(player.transform);
                var gm = FindObjectOfType<GameManager>();
                if (gm == null)
                {
                    Debug.LogError("❌ KHÔNG TÌM THẤY GameManager");
                }
                else
                {
                    gm.RegisterLocalPlayer(player);
                }
                return;
        }

        if (obj != null)
        {
            GameObject block = Instantiate(obj, pos + Vector3.up * 0.5f, obj.transform.rotation, transform);

            BreakableBlock bb = block.GetComponent<BreakableBlock>();
            if(bb != null)
            {
                Vector2Int gridPos = WorldToGrid(pos);
                bb.SetGridPosition(gridPos);
            }
        }
    }


    //
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - offsetX) / tileSize);
        int z = Mathf.RoundToInt((worldPos.z - offsetZ) / tileSize);
        return new Vector2Int(x, z);
    }

    public Vector3 GridToWorld(Vector2Int grid)
    {
        return new Vector3(
            grid.x * tileSize + offsetX,
            0,
            grid.y * tileSize + offsetZ
        );
    }

    public bool CanPlaceBomb(Vector2Int grid)
    {
        if (grid.x < 0 || grid.y < 0 ||
            grid.x >= mapData.GetLength(0) ||
            grid.y >= mapData.GetLength(1))
            return false;

        return mapData[grid.x, grid.y] == TileType.Empty;
    }

    public void PlaceBomb(Vector2Int grid)
    {
        mapData[grid.x, grid.y] = TileType.Bomb;
    }

    public void RemoveBomb(Vector2Int grid)
    {
        mapData[grid.x, grid.y] = TileType.Empty;
    }

    public void RemoveDestructible(Vector2Int grid)
    {
        if (mapData[grid.x, grid.y] == TileType.Destructible)
        {
            mapData[grid.x, grid.y] = TileType.Empty;
        }
    }


    public List<Vector2Int> GetEmptyCells()
    {
        List<Vector2Int> result = new List<Vector2Int>();

        for (int x = 0; x < mapData.GetLength(0); x++)
        {
            for (int y = 0; y < mapData.GetLength(1); y++)
            {
                if (mapData[x, y] == TileType.Empty)
                {
                    result.Add(new Vector2Int(x, y));
                }
            }
        }

        return result;
    }

    public Vector2Int GetRandomEmptyCell()
    {
        List<Vector2Int> emptyCells = GetEmptyCells();
        if (emptyCells == null || emptyCells.Count == 0)
        {
            Debug.LogWarning("GridMapSpawner: No empty cells available for spawning!");
            return Vector2Int.zero;
        }

        // Lọc những ô không nằm trong vùng cấm quanh tâm map: x(-2,2), z(-2,2)
        List<Vector2Int> validCells = new List<Vector2Int>();
        foreach (var cell in emptyCells)
        {
            Vector3 worldPos = GridToWorld(cell);
            if (!IsInForbiddenZone(worldPos))
            {
                validCells.Add(cell);
            }
        }

        // Nếu không còn ô hợp lệ ngoài vùng cấm, fallback dùng toàn bộ emptyCells
        if (validCells.Count == 0)
        {
            Debug.LogWarning("GridMapSpawner: No cells outside forbidden zone, using all empty cells.");
            return emptyCells[Random.Range(0, emptyCells.Count)];
        }

        return validCells[Random.Range(0, validCells.Count)];
    }

    bool IsInForbiddenZone(Vector3 pos)
    {
        return pos.x >= -2f && pos.x <= 2f &&
               pos.z >= -2f && pos.z <= 2f;
    }

    private GameObject GetPlayerPrefabFromData()
    {
        // Kiểm tra DataManager và PlayerData
        if (DataManager.Instance == null || DataManager.Instance.PlayerData == null)
        {
            Debug.LogWarning("⚠️ DataManager hoặc PlayerData chưa được khởi tạo, sử dụng prefab mặc định");
            return playerPrefab;
        }

        string equippedCharacterId = DataManager.Instance.PlayerData.equippedCharacterId;

        if (string.IsNullOrEmpty(equippedCharacterId))
        {
            Debug.LogWarning("⚠️ equippedCharacterId rỗng, sử dụng prefab mặc định");
            return playerPrefab;
        }

        // Kiểm tra GameDatabase
        if (gameDatabase == null)
        {
            Debug.LogError("❌ GameDatabase chưa được gán trong Inspector!");
            return playerPrefab;
        }

        // Lấy CharacterConfig từ GameDatabase
        CharacterConfig characterConfig = gameDatabase.GetCharacter(equippedCharacterId);

        if (characterConfig == null)
        {
            Debug.LogError($"❌ Không tìm thấy CharacterConfig với id: {equippedCharacterId}");
            return playerPrefab;
        }

        if (characterConfig.prefab == null)
        {
            Debug.LogError($"❌ CharacterConfig {equippedCharacterId} không có prefab!");
            return playerPrefab;
        }

        return characterConfig.prefab;
    }
}


