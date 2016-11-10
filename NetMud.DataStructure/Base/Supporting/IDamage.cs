using System.ComponentModel;
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
        [Description("0 None")]
        None = 0,
        [Description("# Blunt")]
        Blunt = 1,
        [Description("/ Sharp")]
        Sharp = 2,
        [Description("^ Pierce")]
        Pierce = 3,
        [Description("> Shred")]
        Shred = 4,
        [Description("} Chop")]
        Chop = 5,
        [Description("A Acidic")]
        Acidic = 6,
        [Description("B Base")]
        Base = 7,
        [Description("H Heat")]
        Heat = 8,
        [Description("C Cold")]
        Cold = 9,
        [Description("E Electric")]
        Electric = 10,
        [Description("P Positronic")]
        Positronic = 11,
        [Description("N Endergonic")]
        Endergonic = 12,
        [Description("X Exergonic")]
        Exergonic = 13,
        [Description("M Hypermagnetic")]
        Hypermagnetic = 14
    }
}
