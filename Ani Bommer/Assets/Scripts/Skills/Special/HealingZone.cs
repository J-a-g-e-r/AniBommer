using UnityEngine;

public class HealingZone : MonoBehaviour
{
    [SerializeField] private int healAmount = 20;
    [SerializeField] private float healInterval = 0.5f;

    private PlayerStats playerInZone;
    private float timer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInZone = other.GetComponent<PlayerStats>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<PlayerStats>() == playerInZone)
            {
                playerInZone = null;
                timer = 0f;
            }
        }
    }

    private void Update()
    {
        if (playerInZone == null) return;

        timer += Time.deltaTime;
        if (timer >= healInterval)
        {
            timer -= healInterval;

            // Gọi hàm hồi máu – sửa cho đúng với PlayerStats của bạn
            playerInZone.Heal(healAmount);
            // Nếu không có Heal(int), có thể dùng: playerInZone.TakeDamage(-healAmount);
        }
    }
}