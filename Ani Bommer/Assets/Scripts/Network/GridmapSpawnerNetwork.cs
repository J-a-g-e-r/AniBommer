using System.Collections.Generic;
using UnityEngine;

public class GridMapSpawnerNetwork : MonoBehaviour
{
    public static GridMapSpawnerNetwork Instance;
    private float tileSize;

    public GameObject floorPrefabA;
    public GameObject floorPrefabB;
    public GameObject[] indestructiblePrefabs;
    // Bỏ destructiblePrefabs khỏi đây — host sẽ spawn qua Network
    //public GameObject playerPrefab; // Bỏ — không dùng nữa
    [SerializeField] private int mapId = 0;

    private TileType[,] mapData;
    private float offsetX;
    private float offsetZ;

    public int MapId => mapId;

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
    }

    void InitMapData()
    {
        mapData = MapLibrary.GetMap(mapId);
    }

    void SpawnMap()
    {
        int width = mapData.GetLength(0);
        int height = mapData.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 pos = new Vector3(
                    x * tileSize + offsetX,
                    0,
                    z * tileSize + offsetZ
                );

                // Spawn floor xen kẽ
                GameObject floorToSpawn = ((x + z) % 2 == 0) ? floorPrefabA : floorPrefabB;
                Instantiate(floorToSpawn, pos, Quaternion.identity, transform);

                // Chỉ spawn Indestructible ở client (visual tĩnh, không cần network)
                // Destructible và PlayerSpawn sẽ do GameplayManager (host) xử lý
                if (mapData[x, z] == TileType.Indestructible)
                {
                    GameObject obj = indestructiblePrefabs[Random.Range(0, indestructiblePrefabs.Length)];
                    Instantiate(obj, pos + Vector3.up * 0.5f, obj.transform.rotation, transform);
                }
            }
        }
    }

    // ─── Grid Utilities (dùng chung cho bomb, movement, v.v.) ───

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

        TileType tile = mapData[grid.x, grid.y];
        return tile == TileType.Empty || tile == TileType.PlayerSpawn;
    }

    public void PlaceBomb(Vector2Int grid) => mapData[grid.x, grid.y] = TileType.Bomb;
    public void RemoveBomb(Vector2Int grid) => mapData[grid.x, grid.y] = TileType.Empty;

    public void RemoveDestructible(Vector2Int grid)
    {
        if (mapData[grid.x, grid.y] == TileType.Destructible)
            mapData[grid.x, grid.y] = TileType.Empty;
    }

    public List<Vector2Int> GetEmptyCells()
    {
        var result = new List<Vector2Int>();
        for (int x = 0; x < mapData.GetLength(0); x++)
            for (int y = 0; y < mapData.GetLength(1); y++)
                if (mapData[x, y] == TileType.Empty)
                    result.Add(new Vector2Int(x, y));
        return result;
    }

    public Vector2Int GetRandomEmptyCell()
    {
        List<Vector2Int> emptyCells = GetEmptyCells();
        if (emptyCells.Count == 0) return Vector2Int.zero;

        var validCells = emptyCells.FindAll(cell => !IsInForbiddenZone(GridToWorld(cell)));
        var pool = validCells.Count > 0 ? validCells : emptyCells;
        return pool[Random.Range(0, pool.Count)];
    }

    // Lấy tất cả vị trí world của một loại tile (dùng cho GameplayManager)
    public List<Vector3> GetWorldPositionsOfType(TileType type)
    {
        var result = new List<Vector3>();
        int width = mapData.GetLength(0);
        int height = mapData.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                if (mapData[x, z] == type)
                {
                    result.Add(GridToWorld(new Vector2Int(x, z)));
                    // Đánh dấu PlayerSpawn thành Empty sau khi lấy
                    if (type == TileType.PlayerSpawn)
                        mapData[x, z] = TileType.Empty;
                }
            }
        }
        return result;
    }

    bool IsInForbiddenZone(Vector3 pos)
    {
        return pos.x >= -2f && pos.x <= 2f && pos.z >= -2f && pos.z <= 2f;
    }
}