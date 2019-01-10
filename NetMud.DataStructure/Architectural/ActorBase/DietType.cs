namespace NetMud.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// What an entity that IHungers can eat. "Omnivore" is just herb and carn
    /// </summary>
    public enum DietType : short
    {
        Herbivore = 0,
        Carnivore = 1,
        Necrovore = 2,
        Metalvore = 3,
        Magiviore = 4,
        Spirivore = 5,
        Solarvore = 6
    }
}
