using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    private bool destroyed = false;

    private Vector2Int gridPos;

    public void SetGridPosition(Vector2Int grid)
    {
        gridPos = grid;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (destroyed) return;

        if (other.CompareTag("Explosion"))
        {
            destroyed = true;
            DestroyBlock();
        }
    }

    private void DestroyBlock()
    {
        // TODO: spawn break VFX
        GridMapSpawner.Instance.RemoveDestructible(gridPos);
        Destroy(gameObject);
    }
}
