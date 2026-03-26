using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageZone : MonoBehaviour
{
    [SerializeField] private int damageAmount = 10;



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            Monster monster = other.GetComponent<Monster>();
            if (monster != null)
            {
                monster.TakeDamage(damageAmount);
                //// 2. Đẩy quái vật ra ngoài (Xử lý trực tiếp Rigidbody)
                //Rigidbody rb = other.GetComponent<Rigidbody>();
                //if (rb != null)
                //{
                //    // Hướng đẩy = (Vị trí quái - Tâm vùng sát thương)
                //    Vector3 pushDirection = (other.transform.position - transform.position).normalized;
                //    pushDirection.y = 0; // Giữ quái trên mặt đất

                //    // Ghi đè vận tốc tức thì để thắng được code di chuyển trong Update của quái
                //    rb.velocity = pushDirection * 0;

                //    // Nếu muốn đẩy mạnh kiểu "văng" đi, dùng Impulse:
                //    rb.AddForce(pushDirection * 50, ForceMode.Impulse);
                //}
            }

        }

    }
}
