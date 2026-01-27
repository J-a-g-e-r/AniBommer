using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Size")]
    public int width = 13;   // X
    public int height = 19;  // Z

    [Header("Tile")]
    public GameObject tilePrefab;
    private Vector3 tileSize;
    void Start()
    {
        CalculateTileSize();
        GenerateGrid();

    }


    void CalculateTileSize()
    {
        Renderer r = tilePrefab.GetComponentInChildren<Renderer>();
        tileSize = r.bounds.size;
    }

    void GenerateGrid()
    {
        Vector3 offset = new Vector3(
            -(width - 1) * tileSize.x / 2f,
            0,
            -(height - 1) * tileSize.z / 2f
        );

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                float spacing = 0.1f; // Optional spacing between tiles
                Vector3 pos = new Vector3(
                    x * (tileSize.x -spacing),
                    0,
                    z * (tileSize.z -spacing)
                ) + offset;

                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                tile.name = $"Tile_{x}_{z}";
            }
        }
    }
}


