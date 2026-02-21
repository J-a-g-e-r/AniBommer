using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Player Stats")]
    public float MoveSpeed;
    public int BombRange;
    public int MaxBomb;



    private int currentBomb;
    public void Init(Characters data)
    {
        MoveSpeed = data.moveSpeed;
        BombRange = data.bombRange;
        MaxBomb = data.maxBombs;

        currentBomb = MaxBomb;
    }

    private void OnEnable()
    {
        GameEvents.OnBombPlaced += OnBombPlaced;
        GameEvents.OnBombExploded += OnBombExploded;
    }

    private void OnDisable()
    {
        GameEvents.OnBombPlaced -= OnBombPlaced;
        GameEvents.OnBombExploded -= OnBombExploded;
    }

    private void OnBombPlaced()
    {
        if (currentBomb <= 0) return;

        currentBomb--;
    }

    private void OnBombExploded()
    {
        currentBomb = Mathf.Min(currentBomb + 1, MaxBomb);
    }

    public bool CanPlaceBomb()
    {
        return currentBomb > 0;
    }

    public void IncreaseMaxBomb(int amount)
    {
        MaxBomb += amount;
        // Tăng currentBomb tương ứng để player có thể đặt thêm bom ngay
        currentBomb += amount;
    }

    public void IncreaseBombRange(int amount)
    {
        BombRange += amount;
    }

    public void IncreaseMoveSpeed(float amount)
    {
        MoveSpeed += amount;
    }
}