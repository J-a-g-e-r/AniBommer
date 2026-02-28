using UnityEngine;
using System;

public class MonsterTargetSensor : MonoBehaviour
{
    public event Action<Transform> OnTargetEnter;
    public event Action OnTargetExit;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnTargetEnter?.Invoke(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnTargetExit?.Invoke();
        }
    }
}