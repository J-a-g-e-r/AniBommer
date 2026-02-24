public static class MapLibrary
{
    private const TileType E = TileType.Empty;
    private const TileType I = TileType.Indestructible;
    private const TileType D = TileType.Destructible;
    private const TileType P = TileType.PlayerSpawn;

    //Map 0
    private static readonly TileType[,] Map0Template = new TileType[19, 13]
        {
            { E,E,E,D,E,I,I,I,E,D,E,E,E },
            { E,E,E,D,E,E,E,E,E,D,E,P,E },
            { E,E,E,D,E,E,E,E,E,D,E,E,E },
            { D,D,D,D,E,D,E,D,E,E,E,E,E },
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

    public static TileType[,] GetMap(int mapId)
    {
        return mapId switch
        {
            0 => Clone(Map0Template),
            _ => Clone(Map0Template)
        };
    }

    private static TileType[,] Clone(TileType[,] source)
    {
        int rows = source.GetLength(0);
        int cols = source.GetLength(1);

        var clone = new TileType[rows, cols];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                clone[y, x] = source[y, x];
            }
        }

        return clone;
    }
}