using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ModelController : MonoBehaviour
{

    private PlayerInputAction playerInput;

    private bool canSwipe = false;

    // Tốc độ xoay
    [SerializeField] private float rotationSpeed = 0.5f;

    void Awake()
    {
        // 1. Khởi tạo đối tượng Input
        playerInput = new @PlayerInputAction();
    }

    void OnEnable()
    {
        // 2. Kích hoạt Action Map
        playerInput.Enable();
    }

    void OnDisable()
    {
        // 3. Hủy kích hoạt để tránh leak bộ nhớ
        playerInput.Disable();
    }

    void Update()
    {
        // Kiểm tra nếu không có màn hình cảm ứng (hoặc chuột) thì bỏ qua
        if (Touchscreen.current == null) return;

        var touch = Touchscreen.current.primaryTouch;

        // Khi bắt đầu chạm: Kiểm tra xem có chạm trúng nhân vật không
        if (touch.press.wasPressedThisFrame)
        {
            CheckTouchOnPlayer(touch.position.ReadValue());
        }

        // Khi đang giữ và vuốt
        if (canSwipe && touch.press.isPressed)
        {
            // Đọc giá trị Swipe (Vector2 delta) từ Player Action Map
            Vector2 swipeDelta = playerInput.PlayerController.Swipe.ReadValue<Vector2>();

            if (swipeDelta.magnitude > 0.1f)
            {
                RotatePlayer(swipeDelta);
            }
        }

        // Khi thả tay: Ngừng cho phép xoay
        if (touch.press.wasReleasedThisFrame)
        {
            canSwipe = false;
        }
    }

    void CheckTouchOnPlayer(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Kiểm tra nếu raycast trúng collider của chính Object này
            if (hit.transform == transform)
            {
                canSwipe = true;
            }
        }
    }

    void RotatePlayer(Vector2 delta)
    {
        // Cách xoay phổ biến cho nhân vật (Xoay quanh trục Y dựa trên độ lệch ngang X của cú vuốt)
        // Nếu bạn muốn xoay tự do theo mọi hướng, hãy dùng cả delta.y
        float rotationAmount = delta.x * rotationSpeed;
        transform.Rotate(Vector3.up, -rotationAmount, Space.World);
    }

}
