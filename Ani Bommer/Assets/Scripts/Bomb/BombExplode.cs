using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombExplode : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Bomb Settings")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private LayerMask levelMask;
    [SerializeField] private float bombCoundownTime = 3f;
    [SerializeField] private float explosionRange = 1f;

    private bool isExploded = false;
    private Vector2Int grid;
    void Start()
    {
        Invoke("Explode", bombCoundownTime);
    }

    public void Init(int range)
    {
        explosionRange = range;
    }

    public void InitPos(Vector2Int gridPos)
    {
        grid = gridPos;
    }

    private void Explode()
    {
        Instantiate(explosionEffect, transform.position + new Vector3(0, 1, 0), explosionEffect.transform.rotation); //1

        StartCoroutine(CreateExplosions(Vector3.forward));
        StartCoroutine(CreateExplosions(Vector3.right));
        StartCoroutine(CreateExplosions(Vector3.back));
        StartCoroutine(CreateExplosions(Vector3.left));

        GetComponent<MeshRenderer>().enabled = false; //2
        isExploded = true;
        GridMapSpawner.Instance.RemoveBomb(grid);
        GameEvents.OnBombExploded?.Invoke();
        Destroy(gameObject, .3f);
    }

    private IEnumerator CreateExplosions(Vector3 direction)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        for (int i = 1; i <= explosionRange; i++)
        {
            RaycastHit hit;
            float distance = i * 2f;

            bool hasHit = Physics.Raycast(
                origin,
                direction,
                out hit,
                distance,
                levelMask
            );

            if (hasHit)
            {
                // 💥 Spawn explosion NGAY TẠI collider bị trúng
                Vector3 hitPos = hit.point;
                hitPos.y = transform.position.y + 1f;

                Instantiate(explosionEffect, hitPos, explosionEffect.transform.rotation);

                // ❌ Gặp block → dừng lan nổ
                break;
            }
            else
            {
                // 💥 Spawn explosion ở ô trống
                Vector3 spawnPos = transform.position + direction * distance;
                spawnPos.y = transform.position.y + 1f;

                Instantiate(explosionEffect, spawnPos, explosionEffect.transform.rotation);
            }

            yield return new WaitForSeconds(0.02f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isExploded && other.CompareTag("Explosion"))
        {
            CancelInvoke("Explode");
            Explode();
        }
    }
}
