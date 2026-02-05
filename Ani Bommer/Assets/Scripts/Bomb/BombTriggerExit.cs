using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTriggerExit : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Collider>().isTrigger = true;
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(EnableTriggerAfterDelay());

        }
    }

    private IEnumerator EnableTriggerAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);
        GetComponent<Collider>().isTrigger = false;
    }
}
