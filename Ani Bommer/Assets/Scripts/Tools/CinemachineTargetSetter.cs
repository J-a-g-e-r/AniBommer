using UnityEngine;
using Cinemachine;

public class CinemachineTargetSetter : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;

    private void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
    }

    public void SetTarget(Transform target)
    {
        vcam.Follow = target;

    }
}