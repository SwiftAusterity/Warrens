using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.Behaviors.Automation
{
    interface IEat<DietType>
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
