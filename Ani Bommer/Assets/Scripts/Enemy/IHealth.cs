public interface IHealth
{
    void TakeDamage(int amount);
    bool IsDead { get; }
}