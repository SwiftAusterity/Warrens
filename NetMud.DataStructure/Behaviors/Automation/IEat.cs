namespace NetMud.DataStructure.Behaviors.Automation
{
    public interface IEat<DietType>
    {
    }

    public enum DietType
    {
        Herbivore,
        Carnivore,
        Necrovore,
        Metalvore,
        Magiviore,
        Spirivore
    }
}
