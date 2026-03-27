using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

public class ObjectPoolingManager : MonoBehaviour
{
    public static ObjectPoolingManager Instance { get; private set; }

    [System.Serializable]
    public class Prewarm
    {
        public GameObject prefab;
        public int count = 10;
        public Transform parent;
    }

    [Header("Optional prewarm (tuỳ chọn)")]
    [SerializeField] private List<Prewarm> prewarm = new();

    private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
    private readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();
    private readonly HashSet<GameObject> _warmed = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // prewarm theo cấu hình inspector (tuỳ chọn)
        foreach (var p in prewarm)
        {
            if (p.prefab == null || p.count <= 0) continue;
            Warm(p.prefab, p.count, p.parent);
        }
    }

    public void Warm(GameObject prefab, int count, Transform parent = null)
    {
        if (prefab == null || count <= 0) return;

        if (!_pools.TryGetValue(prefab, out var q))
        {
            q = new Queue<GameObject>(count);
            _pools[prefab] = q;
        }

        for (int i = 0; i < count; i++)
        {
            var go = CreateNew(prefab, parent);
            go.SetActive(false);
            q.Enqueue(go);
        }

        _warmed.Add(prefab);
    }

    public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent = null, int lazyWarmCount = 0)
    {
        if (prefab == null) return null;

        if (!_pools.TryGetValue(prefab, out var q) || q.Count == 0)
        {
            // Lazy warm nếu bạn muốn (tránh “prewarm nhầm”)
            if (lazyWarmCount > 0 && !_warmed.Contains(prefab))
            {
                Warm(prefab, lazyWarmCount, parent);
                q = _pools[prefab];
            }

            if (q == null) q = new Queue<GameObject>();
            else { _pools[prefab] = q; }
        }

        GameObject go = (q != null && q.Count > 0) ? q.Dequeue() : CreateNew(prefab, parent);

        if (parent != null) go.transform.SetParent(parent, false);
        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);
        return go;
    }

    public void Despawn(GameObject instance)
    {
        if (instance == null) return;

        if (!_instanceToPrefab.TryGetValue(instance, out var prefab) || prefab == null)
        {
            Destroy(instance); // fallback
            return;
        }

        instance.SetActive(false);

        if (!_pools.TryGetValue(prefab, out var q))
        {
            q = new Queue<GameObject>();
            _pools[prefab] = q;
        }

        q.Enqueue(instance);
    }

    private GameObject CreateNew(GameObject prefab, Transform parent)
    {
        var go = Instantiate(prefab, parent);
        _instanceToPrefab[go] = prefab;

        return go;
    }
}
