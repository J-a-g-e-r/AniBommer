using UnityEngine;
public class DaggerProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private int damage = 5;
    [SerializeField] private float lifeTime = 4f;
    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }
    private void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Monster"))
            return;
        var monster = other.GetComponent<Monster>();
        if (monster != null)
            monster.TakeDamage(damage);
        Destroy(gameObject);
    }
}