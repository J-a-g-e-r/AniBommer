using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MonsterChaseMovement : MonoBehaviour
{
    [SerializeField] private float speed = 1f;
    [SerializeField] private float rotateSpeed = 5f;
    private Transform player;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // Tự động tìm player khi monster spawn (nếu player đã tồn tại)
        SetTarget();
    }

    public void OnEnable()
    {
        GameEvents.OnPlayerSpawned += SetTarget;
    }

    private void OnDisable()
    {
        GameEvents.OnPlayerSpawned -= SetTarget;
    }

    private void Update()
    {
        // Nếu mất target, thử tìm lại
        if (player == null)
        {
            SetTarget();
            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0f; 

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );
        }

        // 2️⃣ Di chuyển về phía Player
        //transform.position += direction.normalized * speed * Time.deltaTime;
        //rb.MovePosition(rb.position + direction * speed * Time.deltaTime);
        rb.velocity = direction.normalized * speed;
    }

    public void SetTarget()
    {
        player = FindAnyObjectByType<PlayerController>()?.transform;
    }
}