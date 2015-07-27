using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NetMud.Models.GameAdmin
{
    public class DashboardViewModel : BaseViewModel
    {
        public IEnumerable<IRoomData> Rooms { get; set; }
        public IEnumerable<IInanimateData> Inanimates { get; set; }
        public IEnumerable<INonPlayerCharacter> NPCs { get; set; }

        public int LivePlayers { get; set; }
    }
}