using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Collectable/Currency",fileName = "New Coin Collectable")]
public class CollectableCurrencySO : CollectableSOBase
{
    [Header("Currency Settings")]
    public int CurrencyAmount = 10; 
    public override void Collect(GameObject objectThatCollected)
    {
        MoneyManager.instance.IncreaseMoney(CurrencyAmount);
        if(_playerEffects == null)
        {
            GetReference(objectThatCollected);
        }
        _playerEffects.PlayCollectionEffect(CollectClip);
    }
}
