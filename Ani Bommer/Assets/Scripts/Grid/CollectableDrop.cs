using UnityEngine;

[System.Serializable]
public class CollectableDrop
{
    public GameObject collectablePrefab;
    [Range(0f, 100f)]
    public float spawnChance = 50f; // Percentage chance to spawn
}
