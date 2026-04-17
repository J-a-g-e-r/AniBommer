using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class HealingZone : MonoBehaviour
{
    [SerializeField] private int healAmount = 20;
    [SerializeField] private float healInterval = 0.5f;

    private sealed class HealTarget
    {
        public PlayerStats Offline;
        public PlayerStatsNetwork Net;
    }

    private readonly Dictionary<int, HealTarget> _targets = new Dictionary<int, HealTarget>();

    private float _timer;

    private static bool TryGetHealTarget(Collider other, out int rootId, out HealTarget target)
    {
        target = null;
        rootId = 0;
        if (other == null || !other.CompareTag("Player")) return false;

        var root = other.transform.root.gameObject;
        rootId = root.GetInstanceID();

        var net = root.GetComponentInChildren<PlayerStatsNetwork>();
        if (net != null)
        {
            target = new HealTarget { Net = net };
            return true;
        }

        var offline = root.GetComponentInChildren<PlayerStats>();
        if (offline != null)
        {
            target = new HealTarget { Offline = offline };
            return true;
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!TryGetHealTarget(other, out int rootId, out var t)) return;
        _targets[rootId] = t;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!TryGetHealTarget(other, out int rootId, out _)) return;
        _targets.Remove(rootId);
    }

    private void Update()
    {
        if (_targets.Count == 0) return;

        _timer += Time.deltaTime;
        if (_timer < healInterval) return;
        _timer -= healInterval;

        foreach (var t in _targets.Values)
        {
            if (t.Net != null)
            {
                if (!t.Net.IsSpawned) continue;
                t.Net.Heal(healAmount);
            }
            else if (t.Offline != null)
            {
                t.Offline.Heal(healAmount);
            }
        }
    }
}
