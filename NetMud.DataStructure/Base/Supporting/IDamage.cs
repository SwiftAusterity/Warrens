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
        None = -1,
        Blunt = 0,
        Sharp = 1,
        Pierce = 2,
        Shred = 3,
        Chop = 4,
        Acidic = 5,
        Base = 6,
        Heat = 7,
        Cold = 8,
        Electric = 9,
        Positronic = 10,
        Endergonic = 11,
        Exergonic = 12,
        Hypermagnetic = 13
    }
}
