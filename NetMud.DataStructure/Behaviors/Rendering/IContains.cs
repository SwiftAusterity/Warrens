using NetMud.DataStructure.Base.System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Behaviors.Rendering
{
    public interface IContains : IEntity
    {
        string MoveInto<T>(T thing);
        string MoveInto<T>(T thing, string containerName);
        string MoveFrom<T>(T thing);
        string MoveFrom<T>(T thing, string containerName);

        IEnumerable<T> GetContents<T>();
        IEnumerable<T> GetContents<T>(string containerName);
    }
}
