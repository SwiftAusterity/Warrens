using NetMud.DataStructure.Base.System;
using System.Collections.Generic;
namespace NetMud.DataStructure.Behaviors.Automation
{
    /// <summary>
    /// For IHunger entities, details what they can eat
    /// </summary>
    /// <typeparam name="DietType">the type they can eat</typeparam>
    public interface IEat
    {
        /// <summary>
        /// What can this eat
        /// </summary>
        IEnumerable<DietType> Diets { get; set; }

        /// <summary>
        /// What's in yo belly
        /// </summary>
        IEnumerable<IEntity> StomachContents { get; set; }

        /// <summary>
        /// The act of eating
        /// </summary>
        /// <param name="food">the thing being eaten</param>
        /// <returns>current statiation</returns>
        int Consume(IEntity food);
    }

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
