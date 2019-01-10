using NetMud.DataStructure.Architectural.EntityBase;

namespace NetMud.DataStructure.System
{ 
    /// <summary>
    /// Helper class for inflicting damage to entities
    /// </summary>
    public interface IDamage
    {
        DamageType Type { get; set; }
    }
}
