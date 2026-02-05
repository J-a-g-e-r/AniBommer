using UnityEngine;

public class BreakableBlock : MonoBehaviour
{
    private bool destroyed = false;

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
        Destroy(gameObject);
    }
}
