using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableTriggerHandler : MonoBehaviour
{
    [SerializeField] private LayerMask _whoCanCollect = LayerMaskHelper.CreateLayerMask(8); // Default to Player layer
    private Collectable _collectable;

    private void Awake()
    {
        _collectable = GetComponent<Collectable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(LayerMaskHelper.ObjIsInLayerMask(other.gameObject, _whoCanCollect))
        {
            _collectable.Collect(other.gameObject);
            Destroy(gameObject,0.3f);
        } 

        else if (other.CompareTag("Explosion"))
        {
            Destroy(gameObject);
        }


    }

}
