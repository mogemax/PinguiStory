namespace PinguiStory.Combat
{
    /// <summary>
    /// Cualquier objeto que pueda recibir daño de un proyectil (o de otra fuente futura).
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(int amount);
    }
}
