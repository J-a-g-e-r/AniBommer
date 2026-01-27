using UnityEngine;

public class GridMapSpawner : MonoBehaviour
{
    private float tileSize;

    public GameObject floorPrefab;
    public GameObject[] indestructiblePrefabs;
    public GameObject[] destructiblePrefabs;
    public GameObject playerPrefab;

    private TileType[,] mapData;

    private void Awake()
    {
        tileSize = floorPrefab.GetComponent<Renderer>().bounds.size.x;
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

        mapData = new TileType[13, 19]
        {
            { E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E },
            { E,P,E,D,E,D,E,D,E,D,E,D,E,D,E,D,E,P,E },
            { E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E },
            { E,D,E,D,E,D,E,D,E,D,E,D,E,D,E,D,E,D,E },
            { E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E },
            { E,D,E,D,E,D,E,D,E,D,E,D,E,D,E,D,E,D,E },
            { E,E,E,E,E,E,E,E,E,I,E,E,E,E,E,E,E,E,E },
            { E,D,E,D,E,D,E,D,E,D,E,D,E,D,E,D,E,D,E },
            { E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E },
            { E,D,E,D,E,D,E,D,E,D,E,D,E,D,E,D,E,D,E },
            { E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E },
            { E,P,E,D,E,D,E,D,E,D,E,D,E,D,E,D,E,P,E },
            { E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E,E },
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

                Instantiate(floorPrefab, pos, Quaternion.identity, transform);
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
                Instantiate(playerPrefab, pos + Vector3.up, Quaternion.identity);
                return;
        }

        if (obj != null)
            Instantiate(obj, pos + Vector3.up * 0.5f, Quaternion.identity, transform);
    }
}


