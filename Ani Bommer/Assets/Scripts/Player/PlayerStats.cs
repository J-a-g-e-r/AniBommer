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
        Debug.Log(currentBomb);
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
}