namespace NetMud.DataStructure.Behaviors.Automation
{
    /// <summary>
    /// For IHunger entities, details what they can eat
    /// </summary>
    /// <typeparam name="DietType">the type they can eat</typeparam>
    public interface IEat<DietType>
    {
    }

    /// <summary>
    /// What an entity that IHungers can eat.
    /// </summary>
    public enum DietType : short
    {
        Herbivore = 0,
        Carnivore = 1,
        OmniVore = 2,
        Necrovore = 3,
        Metalvore = 4,
        Magiviore = 5,
        Spirivore = 6
    }
}
