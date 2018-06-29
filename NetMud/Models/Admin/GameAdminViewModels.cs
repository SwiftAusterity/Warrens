using NetMud.Authentication;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.PlayerConfiguration;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NetMud.Models.Admin
{
    public class DashboardViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        public DashboardViewModel()
        {
            Inanimates = Enumerable.Empty<IInanimateData>();
            Rooms = Enumerable.Empty<IRoomData>();
            NPCs = Enumerable.Empty<INonPlayerCharacter>();
            Zones = Enumerable.Empty<IZoneData>();
            Locales = Enumerable.Empty<ILocaleData>();

            DimensionalModels = Enumerable.Empty<IDimensionalModelData>();
            HelpFiles = Enumerable.Empty<IHelp>();
            Materials = Enumerable.Empty<IMaterial>();
            Races = Enumerable.Empty<IRace>();
            Constants = Enumerable.Empty<IConstants>();
            Flora = Enumerable.Empty<IFlora>();
            Fauna = Enumerable.Empty<IFauna>();
            Minerals = Enumerable.Empty<IMineral>();
            UIModules = Enumerable.Empty<IUIModule>();

            DictionaryWords = Enumerable.Empty<IDictata>();

            WebsocketServers = Enumerable.Empty<Websock.Server>();

            LiveZones = 0;
            LiveLocales = 0;
            LiveRooms = 0;
            LiveInanimates = 0;
            LiveNPCs = 0;

            LivePlayers = 0;
        }

        //Backing Data
        public IEnumerable<IRoomData> Rooms { get; set; }
        public IEnumerable<IInanimateData> Inanimates { get; set; }
        public IEnumerable<INonPlayerCharacter> NPCs { get; set; }
        public IEnumerable<IZoneData> Zones { get; set; }
        public IEnumerable<ILocaleData> Locales { get; set; }

        //Lookup Data
        public IEnumerable<IDimensionalModelData> DimensionalModels { get; set; }
        public IEnumerable<IHelp> HelpFiles { get; set; }
        public IEnumerable<IMaterial> Materials { get; set; }
        public IEnumerable<IRace> Races { get; set; }
        public IEnumerable<IConstants> Constants { get; set; }
        public IEnumerable<IFlora> Flora { get; set; }
        public IEnumerable<IFauna> Fauna { get; set; }
        public IEnumerable<IMineral> Minerals { get; set; }
        public IEnumerable<IUIModule> UIModules { get; set; }

        //Config Data
        public IEnumerable<IDictata> DictionaryWords { get; set; }

        //Running Data
        public Dictionary<string, CancellationTokenSource> LiveTaskTokens { get; set; }

        public IEnumerable<Websock.Server> WebsocketServers { get; set; }

        public int LiveZones { get; set; }
        public int LiveLocales { get; set; }
        public int LiveRooms { get; set; }
        public int LiveInanimates { get; set; }
        public int LiveNPCs { get; set; }
        public int LivePlayers { get; set; }
    }
}