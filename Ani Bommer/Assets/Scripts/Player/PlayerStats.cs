using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Player Stats")]
    public float PlayerHealth;
    public float MoveSpeed;
    public int BombRange;
    public int MaxBomb;

    private float limitedMoveSpeed = 15f;
    private int limitedBombRange = 6;
    private int limitedBomb = 8;



    private int currentBomb;
    public void Init(Characters data)
    {
        PlayerHealth = data.playerHealth;
        MoveSpeed = data.moveSpeed;
        BombRange = data.bombRange;
        MaxBomb = data.maxBombs;

        currentBomb = MaxBomb;
        HUDManager.instance.InitPlayerStats(PlayerHealth, MaxBomb, currentBomb, MoveSpeed);
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
        HUDManager.instance.UpdateMaxBombText(currentBomb);
    }

    private void OnBombExploded()
    {
        currentBomb = Mathf.Min(currentBomb + 1, MaxBomb);
        HUDManager.instance.UpdateMaxBombText(currentBomb);
    }

    public bool CanPlaceBomb()
    {
        return currentBomb > 0;
    }

    public void IncreaseMaxBomb(int amount)
    {
        if(MaxBomb >= limitedBomb) return;
        MaxBomb += amount;
        // Tăng currentBomb tương ứng để player có thể đặt thêm bom ngay
        currentBomb += amount;
        HUDManager.instance.UpdateMaxBombText(currentBomb);
    }

    public void IncreaseBombRange(int amount)
    {
        if (BombRange >= limitedBombRange) return;
        BombRange += amount;
        HUDManager.instance.UpdateBombRangeText(BombRange);
    }

    public void IncreaseMoveSpeed(float amount)
    {
        if (MoveSpeed >= limitedMoveSpeed) return;
        MoveSpeed += amount;
        HUDManager.instance.UpdateSpeedText(MoveSpeed);
    }
}