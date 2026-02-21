using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CollectableSOBase :ScriptableObject
{
    [Header("Collectable Effects")]
    public AudioClip CollectClip;

    protected PlayerEffects _playerEffects;
    public abstract void Collect(GameObject objectThatCollected);

    public void GetReference(GameObject objectThatCollected)
    {
        _playerEffects = FinderHelper.GetComponentOnObject<PlayerEffects>(objectThatCollected);
    }
}
