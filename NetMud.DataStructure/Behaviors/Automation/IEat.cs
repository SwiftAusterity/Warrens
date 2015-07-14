using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
