using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BombExplode : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Bomb Settings")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private LayerMask levelMask;
    [SerializeField] private float bombCoundownTime = 3f;
    [SerializeField] private float explosionRange ;

    private bool isExploded = false;
    private Vector2Int grid;

    private SpriteRenderer spriteRenderer;
    public GameObject ExplosionEffectPrefab => explosionEffect;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void OnEnable()
    {
        isExploded = false;
        GetComponent<Collider>().isTrigger = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        CancelInvoke(nameof(Explode));
        Invoke(nameof(Explode), bombCoundownTime);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(Explode));
        StopAllCoroutines();
    }
    public void Init(int range)
    {
        explosionRange = range;
    }

    public void InitPos(Vector2Int gridPos)
    {
        grid = gridPos;
    }

    private async UniTask Explode()
    {
        SpawnExplosion(transform.position + new Vector3(0, 0.5f, 0));
        //Instantiate(explosionEffect, transform.position + new Vector3(0, 0.5f, 0), explosionEffect.transform.rotation); //1
        AudioManager.Instance.PlaySound(explosionSound);

        StartCoroutine(CreateExplosions(Vector3.forward));
        StartCoroutine(CreateExplosions(Vector3.right));
        StartCoroutine(CreateExplosions(Vector3.back));
        StartCoroutine(CreateExplosions(Vector3.left));

        if(spriteRenderer != null) spriteRenderer.enabled = false; //2
        isExploded = true;
        GridMapSpawner.Instance.RemoveBomb(grid);
        GameEvents.OnBombExploded?.Invoke();

        // desspawn sau 0/3f
        await UniTask.Delay(300);
        ObjectPoolingManager.Instance.Despawn(gameObject);
        //Destroy(gameObject, .3f);
    }

    private void SpawnExplosion(Vector3 pos)
    {
        if (explosionEffect == null) return;

        var vfx = ObjectPoolingManager.Instance.Spawn(
            explosionEffect,
            pos,
            explosionEffect.transform.rotation
        );

        // Tự trả VFX về pool sau thời gian particle chạy xong (nếu có ParticleSystem)
        var ps = vfx != null ? vfx.GetComponentInChildren<ParticleSystem>() : null;
        float despawnDelay = 0.1f;

        if (ps != null)
        {
            var main = ps.main;
            // cố gắng lấy thời gian chạy thực tế
            despawnDelay = main.duration + main.startLifetime.constantMax + 0.05f;
            ps.Play(true);
        }

        StartCoroutine(DespawnAfterDelay(vfx, despawnDelay));
    }

    private IEnumerator DespawnAfterDelay(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        ObjectPoolingManager.Instance.Despawn(go);
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
                hitPos.y = transform.position.y + 0.5f;

                SpawnExplosion(hitPos);
                //Instantiate(explosionEffect, hitPos, explosionEffect.transform.rotation);

                // ❌ Gặp block → dừng lan nổ
                break;
            }
            else
            {
                // 💥 Spawn explosion ở ô trống
                Vector3 spawnPos = transform.position + direction * distance;
                spawnPos.y = transform.position.y + 0.5f;

                SpawnExplosion(spawnPos);
                //Instantiate(explosionEffect, spawnPos, explosionEffect.transform.rotation);
            }

            yield return new WaitForSeconds(0.02f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isExploded && other.CompareTag("Explosion"))
        {
            CancelInvoke(nameof(Explode));
            Explode().Forget();
        }
    }
}
