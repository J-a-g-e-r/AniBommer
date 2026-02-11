using UnityEngine;

public class GridMapSpawner : MonoBehaviour
{
    private float tileSize;

    public GameObject floorPrefabA;
    public GameObject floorPrefabB;
    public GameObject[] indestructiblePrefabs;
    public GameObject[] destructiblePrefabs;
    public GameObject playerPrefab;
    [SerializeField] private CinemachineTargetSetter camSetter;

    private TileType[,] mapData;

    private void Awake()
    {
        tileSize = floorPrefabA.GetComponent<Renderer>().bounds.size.x;
        InitMapData();
    }
    void Start()
    {
        SpawnMap();

    }

    void InitMapData()
    {
        TileType I = TileType.Indestructible;
        TileType D = TileType.Destructible;
        TileType E = TileType.Empty;
        TileType P = TileType.PlayerSpawn;

        mapData = new TileType[19, 13]
        {
            { E,E,E,D,E,I,I,I,E,D,E,E,E },
            { E,E,E,D,E,E,E,E,E,D,E,P,E },
            { E,E,E,D,E,E,E,E,E,D,E,E,E },
            { D,D,D,D,E,D,E,D,E,D,D,D,D },
            { E,E,E,E,E,E,E,E,E,E,E,E,E },
            { E,D,D,D,D,D,D,D,D,D,D,D,E },
            { E,D,D,I,E,E,E,E,E,I,D,D,E },
            { E,D,D,D,E,E,E,E,E,D,D,D,E },
            { E,D,D,D,E,E,E,E,E,D,D,D,E },
            { E,D,D,D,E,E,E,E,E,D,D,D,E },
            { E,D,D,D,E,E,E,E,E,D,D,D,E },
            { E,D,D,D,E,E,E,E,E,D,D,D,E },
            { E,D,D,I,E,E,E,E,E,I,D,D,E },
            { E,D,D,D,D,D,D,D,D,D,D,D,E },
            { E,E,E,E,E,E,E,E,E,E,E,E,E },
            { D,D,D,D,E,D,E,D,E,D,D,D,D },
            { E,E,E,D,E,E,E,E,E,D,E,E,E },
            { E,E,E,D,E,E,E,E,E,D,E,E,E },
            { E,E,E,D,E,I,I,I,E,D,E,E,E },

        };
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
                GameObject player = Instantiate(playerPrefab, pos + new Vector3(0,0.5f,0), playerPrefab.transform.rotation);
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
            Instantiate(obj, pos + Vector3.up * 0.5f, obj.transform.rotation, transform);
    }

}


