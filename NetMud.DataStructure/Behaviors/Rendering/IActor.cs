using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Automation;

namespace NetMud.DataStructure.Behaviors.Rendering
{
    /// <summary>
    /// Midpoint interface for entities that have affects and can be declared as Actor for commands and events
    /// </summary>
    public interface IActor : IEntity, ICanBeAffected
    {
    }
}
