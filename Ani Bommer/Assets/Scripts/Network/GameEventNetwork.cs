using System;

public static class GameEventNetwork
{
    public static event Action<ulong> OnBombPlaced;
    public static event Action<ulong> OnBombExploded;
    public static event Action<ulong> OnPlayerSpawned;

    public static void RaiseBombPlaced(ulong ownerClientId)
    {
        OnBombPlaced?.Invoke(ownerClientId);
    }

    public static void RaiseBombExploded(ulong ownerClientId)
    {
        OnBombExploded?.Invoke(ownerClientId);
    }

    public static void RaisePlayerSpawned(ulong ownerClientId)
    {
        OnPlayerSpawned?.Invoke(ownerClientId);
    }

    public static void ClearAll()
    {
        OnBombPlaced = null;
        OnBombExploded = null;
        OnPlayerSpawned = null;
    }
}