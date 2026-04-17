using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class CollectableTriggerHandlerNetwork : NetworkBehaviour
{
    [SerializeField] private LayerMask whoCanCollect = LayerMaskHelper.CreateLayerMask(8);
    private Collectable collectable;
    private bool picked;

    private void Awake()
    {
        collectable = GetComponent<Collectable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (picked) return;
        if (!LayerMaskHelper.ObjIsInLayerMask(other.gameObject, whoCanCollect)) return;

        var playerNetObj = other.GetComponentInParent<NetworkObject>();
        if (playerNetObj == null) return;

        picked = true;

        // Client cũng có thể va chạm -> gửi lên server xử lý
        RequestPickupServerRpc(playerNetObj.NetworkObjectId);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(ulong playerNetworkObjectId)
    {
        if (collectable == null) return;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out var playerNetObj))
            return;

        // Chạy Collect ở phía server (để tăng stat network)
        collectable.Collect(playerNetObj.gameObject);

        // Despawn collectable trên server => tự sync xoá tới mọi client
        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn(true);
        else
            Destroy(gameObject);
    }
}