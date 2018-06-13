namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Helper class for inflicting damage to entities
    /// </summary>
    public interface IDamage
    {
        DamageType Type { get; set; }
    }
}
