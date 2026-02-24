using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(CollectableTriggerHandler))]
public class Collectable : MonoBehaviour
{
    [SerializeField] private CollectableSOBase _collectable;
    private Animator _animator;
    private BoxCollider _boxCollider;
    private void Awake()
    {
        _animator = FinderHelper.GetComponentOnObject<Animator>(gameObject);
        _boxCollider = GetComponent<BoxCollider>();
    }
    private void Reset()
    {
        _boxCollider.isTrigger = true;
    }

    public void Collect(GameObject objectThatCollected)
    {
        if (_animator != null)
        {
            _animator.SetTrigger("IsCollected");
        }
        _collectable.Collect(objectThatCollected);
        _boxCollider.enabled = false; // Disable collider to prevent multiple collections


    }

    private void OnEnable()
    {
        if (_boxCollider != null)
            StartCoroutine(EnableColliderDelayed());
    }

    private IEnumerator EnableColliderDelayed()
    {
        _boxCollider.enabled = false;
        yield return new WaitForSeconds(0.7f);
        _boxCollider.enabled = true;
    }
}
