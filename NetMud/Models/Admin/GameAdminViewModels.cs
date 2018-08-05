using NetMud.Authentication;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.PlayerConfiguration;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Base.World;
using NetMud.DataStructure.Linguistic;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            Worlds = Enumerable.Empty<IGaiaData>();

            DimensionalModels = Enumerable.Empty<IDimensionalModelData>();
            HelpFiles = Enumerable.Empty<IHelp>();
            Materials = Enumerable.Empty<IMaterial>();
            Races = Enumerable.Empty<IRace>();
            Constants = Enumerable.Empty<IConstants>();
            Flora = Enumerable.Empty<IFlora>();
            Fauna = Enumerable.Empty<IFauna>();
            Minerals = Enumerable.Empty<IMineral>();
            UIModules = Enumerable.Empty<IUIModule>();
            Celestials = Enumerable.Empty<ICelestial>();
            Journals = Enumerable.Empty<IJournalEntry>();

            DictionaryWords = Enumerable.Empty<IDictata>();
            Languages = Enumerable.Empty<ILanguage>();

            LiveWorlds = 0;
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
        public IEnumerable<IGaiaData> Worlds { get; set; }

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
        public IEnumerable<ICelestial> Celestials { get; set; }
        public IEnumerable<IJournalEntry> Journals { get; set; }

        //Config Data
        public IEnumerable<IDictata> DictionaryWords { get; set; }
        public IEnumerable<ILanguage> Languages { get; set; }

        //Running Data
        public Dictionary<string, CancellationTokenSource> LiveTaskTokens { get; set; }

        public int LiveWorlds { get; set; }
        public int LiveZones { get; set; }
        public int LiveLocales { get; set; }
        public int LiveRooms { get; set; }
        public int LiveInanimates { get; set; }
        public int LiveNPCs { get; set; }
        public int LivePlayers { get; set; }
    }

    public class GlobalConfigViewModel : BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Display(Name = "Websocket Portal Available", Description = "Are new connections being accepted over websockets?")]
        public bool WebsocketPortalActive { get; set; }

        [Display(Name = "System Language", Description = "The default language used for the system.")]
        [DataType(DataType.Text)]
        public string SystemLanguage { get; set; }

        public IEnumerable<ILanguage> ValidLanguages { get; set; }
        public IGlobalConfig DataObject { get; set; }
    }
}