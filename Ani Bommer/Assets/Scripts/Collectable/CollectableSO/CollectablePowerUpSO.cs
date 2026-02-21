using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerUpType
{
    MaxBomb,
    BombRange,
    MoveSpeed
}

[CreateAssetMenu(menuName = "Collectable/PowerUp", fileName = "New PowerUp Collectable")]
public class CollectablePowerUpSO : CollectableSOBase
{
    [Header("PowerUp Settings")]
    public PowerUpType powerUpType = PowerUpType.MaxBomb;
    
    [Header("Increase Values")]
    public int IntValue = 1;  // Dùng cho MaxBomb và BombRange
    public float FloatValue = 2f;  // Dùng cho MoveSpeed

    public override void Collect(GameObject objectThatCollected)
    {
        var playerStats = FinderHelper.GetComponentOnObject<PlayerStats>(objectThatCollected);
        if (playerStats != null)
        {
            switch (powerUpType)
            {
                case PowerUpType.MaxBomb:
                    playerStats.IncreaseMaxBomb(IntValue);
                    break;
                case PowerUpType.BombRange:
                    playerStats.IncreaseBombRange(IntValue);
                    break;
                case PowerUpType.MoveSpeed:
                    playerStats.IncreaseMoveSpeed(FloatValue);
                    break;
            }
        }

        if (_playerEffects == null)
        {
            GetReference(objectThatCollected);
        }
        _playerEffects.PlayCollectionEffect(CollectClip);
    }
}
