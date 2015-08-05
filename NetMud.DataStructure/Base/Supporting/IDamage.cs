namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Helper class for inflicting damage to entities
    /// </summary>
    public interface IDamage
    {
    }

    public enum DamageType : short
    {
        None = 0,
        Blunt = 1,
        Sharp = 2,
        Pierce = 3,
        Shred = 4,
        Chop = 5,
        Acidic = 6,
        Base = 7,
        Heat = 8,
        Cold = 9,
        Electric = 10,
        Positronic = 11,
        Endergonic = 12,
        Exergonic = 13,
        Hypermagnetic = 14
    }
}
