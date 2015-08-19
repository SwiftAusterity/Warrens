﻿namespace NetMud.DataStructure.Behaviors.Automation
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
        Necrovore = 2,
        Metalvore = 3,
        Magiviore = 4,
        Spirivore = 5
    }
}
