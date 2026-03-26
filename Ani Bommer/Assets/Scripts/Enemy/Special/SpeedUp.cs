using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedUp : MonoBehaviour
{
    [SerializeField] private MonsterTargetSensor sensor;
    private MonsterChaseMovement monsterChaseMovement; 
    private MonsterController monsterController;

    private void Awake()
    {
        monsterChaseMovement = FinderHelper.GetComponentOnObject<MonsterChaseMovement>(gameObject);
        monsterController = FinderHelper.GetComponentOnObject<MonsterController>(gameObject);
    }

    private void OnEnable()
    {
        // Đăng ký nhận sự kiện từ Sensor
        if (sensor != null)
        {
            sensor.OnTargetEnter += HandleTargetEnter;
            sensor.OnTargetExit += HandleTargetExit;
        }
    }

    private void OnDisable()
    {
        // Hủy đăng ký khi object bị ẩn/hủy để tránh lỗi bộ nhớ
        if (sensor != null)
        {
            sensor.OnTargetEnter -= HandleTargetEnter;
            sensor.OnTargetExit -= HandleTargetExit;
        }
    }

    private void HandleTargetEnter(Transform targetTransform)
    {
        monsterChaseMovement.speed *= 2f;
        monsterController.CheckSpeed(monsterChaseMovement.speed);

    }

    private void HandleTargetExit()
    {
        monsterChaseMovement.speed /= 2f;
        monsterController.CheckSpeed(monsterChaseMovement.speed);
    }
}
