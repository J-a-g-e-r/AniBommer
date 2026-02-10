using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float MoveSpeed;
    public int BombRange;
    public int MaxBomb; 

    public void Init(Characters data)
    {
        MoveSpeed = data.moveSpeed;
        BombRange = data.bombRange;
        MaxBomb = data.maxBombs;
    }
}