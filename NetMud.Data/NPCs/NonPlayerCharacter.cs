using NetMud.CentralControl;
using NetMud.Data.Architectural;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Inanimates;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.NPC.IntelligenceControl;
using NetMud.DataStructure.System;
using NetMud.Intelligence;
using NetMud.Interp;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace NetMud.Data.NPCs
{
    /// <summary>
    /// NPCs
    /// </summary>
    [Serializable]
    public class NonPlayerCharacter : EntityPartial, INonPlayerCharacter
    {
        #region Template and Framework Values
        public bool IsPlayer()
        {
            return false;
        }

        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override string TemplateName
        {
            get
            {
                return Template<INonPlayerCharacterTemplate>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T Template<T>()
        {
            return (T)TemplateCache.Get(new TemplateCacheKey(typeof(INonPlayerCharacterTemplate), TemplateId));
        }

        /// <summary>
        /// Max stamina for this
        /// </summary>
        public int TotalStamina { get; set; }

        /// <summary>
        /// Max health for this
        /// </summary>
        public int TotalHealth { get; set; }

        /// <summary>
        /// Gender data string for NPCs
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// "family name" for NPCs
        /// </summary>
        public string SurName { get; set; }

        /// <summary>
        /// NPC's race data
        /// </summary>
        public IRace Race { get; set; }

        /// <summary>
        /// The matrix of preferences and AI details
        /// </summary>
        public IPersonality Personality { get; set; }

        /// <summary>
        /// What this merchant is willing to purchase
        /// </summary>
        public HashSet<IMerchandise> WillPurchase { get; set; }

        /// <summary>
        /// What this merchant is willing to sell
        /// </summary>
        public HashSet<IMerchandise> WillSell { get; set; }

        /// <summary>
        /// Inventory this merchant will generate on a timer Item, Quantity
        /// </summary>
        public HashSet<MerchandiseStock> InventoryRestock { get; set; }

        /// <summary>
        /// Abilities this teacher can teach
        /// </summary>
        public HashSet<IUse> TeachableAbilities { get; set; }

        /// <summary>
        /// Qualities this teacher can impart, the quality value is the max level it can be taught to (1 at a time)
        /// </summary>
        public HashSet<IQuality> TeachableProficencies { get; set; }
        #endregion

        /// <summary>
        /// Abilities this can use freely
        /// </summary>
        public HashSet<IUse> UsableAbilities { get; set; }

        /// <summary>
        /// Current stamina for this
        /// </summary>
        public int CurrentStamina { get; set; }

        /// <summary>
        /// Current health for this
        /// </summary>
        public int CurrentHealth { get; set; }

        /// <summary>
        /// News up an empty entity
        /// </summary>
        public NonPlayerCharacter()
        {
            //IDatas need parameterless constructors
            Inventory = new EntityContainer<IInanimate>();
            Qualities = new HashSet<IQuality>();
            Interactions = new HashSet<IInteraction>();
            DecayEvents = new HashSet<IDecayEvent>();
            WillPurchase = new HashSet<IMerchandise>();
            WillSell = new HashSet<IMerchandise>();
            InventoryRestock = new HashSet<MerchandiseStock>();
            TeachableAbilities = new HashSet<IUse>();
            TeachableProficencies = new HashSet<IQuality>();
            UsableAbilities = new HashSet<IUse>();

            Hypothalamus = new Brain();
            Race = new Race();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        public NonPlayerCharacter(INonPlayerCharacterTemplate backingStore)
        {
            Inventory = new EntityContainer<IInanimate>();
            Qualities = new HashSet<IQuality>();
            Interactions = new HashSet<IInteraction>();
            DecayEvents = new HashSet<IDecayEvent>();
            WillPurchase = new HashSet<IMerchandise>();
            WillSell = new HashSet<IMerchandise>();
            InventoryRestock = new HashSet<MerchandiseStock>();
            TemplateId = backingStore.Id;
            TeachableAbilities = new HashSet<IUse>();
            TeachableProficencies = new HashSet<IQuality>();
            UsableAbilities = new HashSet<IUse>();

            Hypothalamus = new Brain();
            Race = new Race();

            SpawnNewInWorld();
        }

        /// <summary>
        /// News up an entity with its backing data and where to spawn it into
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        /// <param name="spawnTo">where to spawn this into</param>
        public NonPlayerCharacter(INonPlayerCharacterTemplate backingStore, IGlobalPosition spawnTo)
        {
            Inventory = new EntityContainer<IInanimate>();
            Qualities = new HashSet<IQuality>();
            WillPurchase = new HashSet<IMerchandise>();
            WillSell = new HashSet<IMerchandise>();
            InventoryRestock = new HashSet<MerchandiseStock>();
            TemplateId = backingStore.Id;
            TeachableAbilities = new HashSet<IUse>();
            TeachableProficencies = new HashSet<IQuality>();
            UsableAbilities = new HashSet<IUse>();

            Hypothalamus = new Brain();
            Race = new Race();

            SpawnNewInWorld(spawnTo);
        }

        public int Exhaust(int exhaustionAmount)
        {
            int stam = Sleep(-1 * exhaustionAmount);

            //TODO: Check for total exhaustion

            return stam;
        }

        public int Harm(int damage)
        {
            int health = Recover(-1 * damage);

            //TODO: Check for DEATH

            return health;
        }

        public int Recover(int recovery)
        {
            CurrentHealth = Math.Max(0, Math.Min(TotalHealth, TotalHealth + recovery));

            return CurrentHealth;
        }

        public int Sleep(int hours)
        {
            CurrentStamina = Math.Max(0, Math.Min(TotalStamina, TotalStamina + hours * 10));

            return CurrentStamina;
        }

        /// <summary>
        /// Gets the actual vision modifier taking into account blindness and other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override ValueRange<float> GetVisualRange()
        {
            INonPlayerCharacterTemplate dT = Template<INonPlayerCharacterTemplate>();
            int returnTop = 1;
            int returnBottom = 100;

            //TODO: Check for blindess/magical type affects

            return new ValueRange<float>(returnTop, returnBottom);
        }


        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public override float GetCurrentLuminosity()
        {
            float lumins = 0;

            foreach (IInanimate dude in Inventory.EntitiesContained())
            {
                lumins += dude.GetCurrentLuminosity();
            }

            //TODO: Magical light, equipment, make inventory less bright depending on where it is

            return lumins;
        }

        #region Merchant
        /// <summary>
        /// Indicates whether this is actually a merchant or not who sells things
        /// </summary>
        /// <returns>if they sell things (but not if they have stock)</returns>
        public bool DoISellThings()
        {
            return WillSell != null && WillSell.Count() > 0;
        }

        /// <summary>
        /// Indicates whether this is actually a merchant who buys things
        /// </summary>
        /// <returns>if they buy things (but not if they have the money to do so)</returns>
        public bool DoIBuyThings()
        {
            return WillPurchase != null && WillPurchase.Count() > 0;
        }

        /// <summary>
        /// Check the price this will be sold for
        /// </summary>
        /// <param name="item">The item in question</param>
        /// <returns>the price, -1 indicates it wont be sold or isn't in stock</returns>
        public int PriceCheck(IInanimate item, bool mustBeInStock)
        {
            decimal value = -1;

            //If we need it in stock but don't have it it's sell price is invalid
            if (item != null && (!mustBeInStock || Inventory.Contains(item)))
            {
                if (WillSell.Any(merch => merch.Item.Id == item.TemplateId))
                {
                    DataStructure.Gaia.IEconomy theEconomy = CurrentLocation.CurrentZone.GetWorld().Macroeconomy;

                    if (theEconomy != null)
                    {
                        DataStructure.Gaia.IEconomicBasis priceBasis = theEconomy.Bases.FirstOrDefault(basis => basis.ItemType.Id == item.TemplateId);

                        if (priceBasis != null)
                        {
                            value = priceBasis.Basis * priceBasis.Adjustment;
                        }
                        else
                        {
                            value = theEconomy.MakeValuation(item.Template<IInanimateTemplate>());
                        }

                        foreach (IQuality quality in item.Qualities)
                        {
                            DataStructure.Gaia.IEconomicTrend trend = theEconomy.Trends.FirstOrDefault(trnd => trnd.Quality.Equals(quality.Name, StringComparison.InvariantCultureIgnoreCase));

                            if (trend != null)
                            {
                                value += trend.Basis * trend.Adjustment;
                            }
                            else
                            {
                                value += theEconomy.MakeValuation(item.Template<IInanimateTemplate>());
                            }
                        }

                        decimal myAdjustments = 1;

                        foreach (IMerchandise adjustment in WillSell.Where(merch => merch.Item.Id == item.TemplateId))
                        {
                            if (string.IsNullOrWhiteSpace(adjustment.Quality) || item.Qualities.Any(quality => quality.Name.Equals(adjustment.Quality)
                                 && quality.Value.IsBetweenOrEqual(adjustment.QualityRange.Low, adjustment.QualityRange.High)))
                            {
                                myAdjustments *= adjustment.MarkRate;
                            }
                        }

                        value *= myAdjustments;
                    }
                }
            }

            return (int)Math.Truncate(value);
        }

        /// <summary>
        /// What will the merchant buy this item for
        /// </summary>
        /// <param name="item">the item in question</param>
        /// <returns>the price, -1 indicates they wont buy it</returns>
        public int HaggleCheck(IInanimate item)
        {
            decimal value = -1;

            if (item != null)
            {
                var template = item.Template<IInanimateTemplate>();
                if (WillPurchase.Any(merch => merch.Item.Id == item.TemplateId))
                {
                    DataStructure.Gaia.IEconomy theEconomy = CurrentLocation.CurrentZone.GetWorld().Macroeconomy;

                    if (theEconomy != null)
                    {
                        DataStructure.Gaia.IEconomicBasis priceBasis = theEconomy.Bases.FirstOrDefault(basis => basis.ItemType.Id == item.TemplateId);

                        if (priceBasis != null)
                        {
                            value = priceBasis.Basis * priceBasis.Adjustment;
                        }
                        else
                        {
                            value = theEconomy.MakeValuation(template);
                        }

                        foreach (IQuality quality in item.Qualities)
                        {
                            DataStructure.Gaia.IEconomicTrend trend = theEconomy.Trends.FirstOrDefault(trnd => trnd.Quality.Equals(quality.Name, StringComparison.InvariantCultureIgnoreCase));

                            if (trend != null)
                            {
                                value += trend.Basis * trend.Adjustment;
                            }
                            else
                            {
                                value += theEconomy.MakeValuation(template);
                            }
                        }

                        decimal myAdjustments = 1;

                        foreach (IMerchandise adjustment in WillPurchase.Where(merch => merch.Item.Id == item.TemplateId))
                        {
                            if (string.IsNullOrWhiteSpace(adjustment.Quality) || item.Qualities.Any(quality => quality.Name.Equals(adjustment.Quality)
                                 && quality.Value.IsBetweenOrEqual(adjustment.QualityRange.Low, adjustment.QualityRange.High)))
                            {
                                myAdjustments += adjustment.MarkRate;
                            }
                        }

                        value += value * myAdjustments;
                    }
                }
            }

            return (int)Math.Truncate(value);
        }

        /// <summary>
        /// Render the inventory to someone
        /// </summary>
        /// <param name="customer">The person asking</param>
        /// <returns>the inventory display</returns>
        public string RenderInventory(IEntity customer)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Current Stock:");
            foreach (IInanimate item in Inventory.EntitiesContained())
            {
                int price = PriceCheck(item, true);

                if (price < 0)
                {
                    continue;
                }

                sb.AppendFormattedLine("{0} : {1}blz", item.GetDescribableName(customer), price);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Render the buy list to someone
        /// </summary>
        /// <param name="customer">The person asking</param>
        /// <returns>the biuy list display</returns>
        public string RenderPurchaseSheet(IEntity customer)
        {
            StringBuilder sb = new StringBuilder();
            DataStructure.Gaia.IEconomy theEconomy = CurrentLocation.CurrentZone.GetWorld().Macroeconomy;

            sb.AppendLine("Current Stock:");
            foreach (IMerchandise merchandise in WillPurchase)
            {
                var valuation = theEconomy.MakeValuation(merchandise.Item);
                int price = (int)Math.Truncate(valuation + valuation * merchandise.MarkRate);

                if (price < 0)
                {
                    continue;
                }

                string qualityString = string.IsNullOrWhiteSpace(merchandise.Quality)
                    ? string.Empty
                    : string.Format("({0}:{1}-{2}) ", merchandise.Quality, merchandise.QualityRange.Low, merchandise.QualityRange.High);

                if (merchandise.Item != null)
                {
                    sb.AppendFormattedLine("{2}{0} : {1}blz", merchandise.Item.Name, price, qualityString);
                }
                else
                {
                    sb.AppendFormattedLine("{0}: {1}blz", qualityString, price);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Execute a sale
        /// </summary>
        /// <param name="item">the item in question</param>
        /// <param name="price">the sale price</param>
        /// <returns>error or blank</returns>
        public string MakeSale(IMobile customer, IInanimate item, int price)
        {
            string returnString = string.Empty;

            if (customer == null)
            {
                return "Invalid customer";
            }

            if (item == null || !Inventory.Contains(item))
            {
                return "I don't have that item in stock.";
            }

            int customerWallet = customer.GetQuality("Bells");

            if (customerWallet < price)
            {
                return "Insufficient Funds.";
            }

            customer.SetQuality(-1 * price, "Bells", true);
            item.TryMoveTo(customer.GetContainerAsLocation());

            return returnString;
        }

        /// <summary>
        /// Execute a purchase
        /// </summary>
        /// <param name="item">the item in question</param>
        /// <param name="price">the sale price</param>
        /// <returns>error or blank</returns>
        public string MakePurchase(IMobile customer, IInanimate item, int price)
        {
            string returnString = string.Empty;

            if (customer == null)
            {
                return "Invalid customer";
            }

            if (item == null)
            {
                return "Invalid item.";
            }

            customer.SetQuality(price, "Bells", true);
            item.TryMoveTo(GetContainerAsLocation());

            return returnString;
        }
        #endregion

        #region Teacher
        /// <summary>
        /// Indicates whether this is actually a merchant or not who sells things
        /// </summary>
        /// <returns>if they sell things (but not if they have stock)</returns>
        public bool DoITeachThings()
        {
            return (TeachableProficencies != null && TeachableProficencies.Count() > 0)
                || (TeachableAbilities != null && TeachableAbilities.Count() > 0);
        }

        /// <summary>
        /// Check the price this will be taught for
        /// </summary>
        /// <param name="useName">The ability in question</param>
        /// <returns>the price, -1 indicates it wont be taught</returns>
        public int InstructionPriceCheck(string useName)
        {
            decimal value = -1;

            //If we need it in stock but don't have it it's sell price is invalid
            if (!string.IsNullOrWhiteSpace(useName) && TeachableAbilities.Any(use => use.Name.Equals(useName, StringComparison.InvariantCultureIgnoreCase)))
            {
                value = 50; //base price, needs to be a config TODO

                var ability = TeachableAbilities.FirstOrDefault(use => use.Name.Equals(useName, StringComparison.InvariantCultureIgnoreCase));

                value *= ability.AffectPattern.Count();
                value += ability.Results.Count() * 10;
                value += ability.Criteria.Count() * 5;

                value -= ability.HealthCost * 2;
                value -= ability.StaminaCost * 3;
            }

            return (int)Math.Truncate(value);
        }

        /// <summary>
        /// Price check on teaching qualities
        /// </summary>
        /// <param name="name">The name of the quality</param>
        /// <param name="level">The level to teach to</param>
        /// <returns>the price, -1 indicates it wont be taught</returns>
        public int InstructionPriceCheck(string qualityName, int level)
        {
            decimal value = -1;
            var quality = TeachableProficencies.FirstOrDefault(qual => qual.Name.Equals(qualityName, StringComparison.InvariantCultureIgnoreCase) && qual.Value > level);

            //If we need it in stock but don't have it it's sell price is invalid
            if (quality != null)
            {
                value = 5; //base price, needs to be a config TODO

                value *= level;
            }

            return (int)Math.Truncate(value);
        }

        /// <summary>
        /// Render the list of skills to teach to someone
        /// </summary>
        /// <param name="customer">The person asking</param>
        /// <returns>the inventory display</returns>
        public string RenderInstructionList(IEntity customer)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Abilities:");
            foreach (IUse ability in TeachableAbilities)
            {
                int price = InstructionPriceCheck(ability.Name);

                if (price < 0)
                {
                    continue;
                }

                sb.AppendFormattedLine("{0}: {1}blz", ability.Name, price);
            }

            sb.AppendLine("Proficencies:");
            foreach (IQuality proficency in TeachableProficencies)
            {
                int price = InstructionPriceCheck(proficency.Name, 1);

                if (price < 0)
                {
                    continue;
                }

                sb.AppendFormattedLine("{0} up to {1} for {1}blz/rank", proficency.Name, proficency.Value, price);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Execute a sale
        /// </summary>
        /// <param name="item">the item in question</param>
        /// <param name="price">the sale price</param>
        /// <returns>error or blank</returns>
        public string Instruct(IMobile customer, string useName, int price)
        {
            string returnString = string.Empty;

            if (customer == null)
            {
                return "Invalid customer";
            }

            var ability = TeachableAbilities.FirstOrDefault(use => use.Name.Equals(useName, StringComparison.InvariantCultureIgnoreCase));
            if (ability == null)
            {
                return "I can't teach that ability.";
            }

            if (customer.UsableAbilities.Contains(ability))
            {
                return "You already have that ability.";
            }

            int customerWallet = customer.GetQuality("Bells");

            if (customerWallet < price)
            {
                return "Insufficient Funds.";
            }

            customer.SetQuality(-1 * price, "Bells", true);
            customer.UsableAbilities.Add(ability);
            customer.Save();

            return returnString;
        }

        /// <summary>
        /// Execute a sale
        /// </summary>
        /// <param name="item">the item in question</param>
        /// <param name="price">the sale price</param>
        /// <returns>error or blank</returns>
        public string Instruct(IMobile customer, string qualityName, int level, int price)
        {
            string returnString = string.Empty;

            if (customer == null)
            {
                return "Invalid customer";
            }

            var quality = TeachableProficencies.FirstOrDefault(qual => qual.Name.Equals(qualityName, StringComparison.InvariantCultureIgnoreCase) && qual.Value > level);
            if (quality == null)
            {
                return "I can't teach that proficency to you.";
            }

            int customerWallet = customer.GetQuality("Bells");

            if (customerWallet < price)
            {
                return "Insufficient Funds.";
            }

            customer.SetQuality(-1 * price, "Bells", true);
            customer.SetQuality(1, quality.Name, true);

            return returnString;
        }

        #endregion

        #region Behavioral
        public IBrain Hypothalamus { get; set; }

        public void DoTheThing(Motivator motivator)
        {
            Accomplisher action = Hypothalamus.HowToDo(motivator);
            bool wander = false;

            switch (action)
            {
                case Accomplisher.Drink:
                    IInanimate waterBottle = Inventory.EntitiesContained().FirstOrDefault(ent => ent.GetQuality("Water") > 0);

                    if (waterBottle != null)
                    {
                        waterBottle.SetQuality(-1, "Water", true);
                        Hypothalamus.ApplyPressure(Motivator.Thirst, -10);
                        Exhaust(-10);
                    }
                    else
                    {
                        wander = true;
                    }
                    break;
                case Accomplisher.Eat:
                    IInanimate food = Inventory.EntitiesContained().FirstOrDefault(ent => ent.GetQuality("Food") > 0);

                    if (food != null)
                    {
                        //TODO: turn this into an eat command
                        int foodValue = food.GetQuality("Food");
                        food.Remove();
                        Hypothalamus.ApplyPressure(Motivator.Hunger, -1 * foodValue);
                        Harm(-1 * foodValue);
                    }
                    else
                    {
                        wander = true;
                    }
                    break;
                case Accomplisher.Sleep:
                    //Can't sleep yet
                    break;
                case Accomplisher.Speak:
                    if (CurrentLocation.CurrentZone != null)
                    {
                        CurrentLocation.CurrentZone.BroadcastEvent("$A$ moos.", this);
                    }
                    break;
                case Accomplisher.Wander:
                    wander = true;
                    break;
            }

            if (wander)
            {
                Random rand = new Random();
                MovementDirectionType direction = (MovementDirectionType)rand.Next(0, 7);

                //Run the command like anyone else
                Interpret.Render(direction.ToString(), this);
            }
        }
        #endregion

        #region Rendering
        #endregion

        #region Container
        /// <summary>
        /// Inanimates contained in this
        /// </summary>
        public IEntityContainer<IInanimate> Inventory { get; set; }
        public int Capacity => 50;

        /// <summary>
        /// Get all of the entities matching a type inside this
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <returns>the contained entities</returns>
        public IEnumerable<T> GetContents<T>()
        {
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            List<T> contents = new List<T>();

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                contents.AddRange(Inventory.EntitiesContained().Select(ent => (T)ent));
            }

            return contents;
        }

        /// <summary>
        /// Move an entity into this
        /// </summary>
        /// <typeparam name="T">the type of the entity to add</typeparam>
        /// <param name="thing">the entity to add</param>
        /// <returns>errors</returns>
        public string MoveInto<T>(T thing)
        {
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                IInanimate obj = (IInanimate)thing;

                if (Inventory.Contains(obj))
                {
                    return "That is already in the container";
                }

                IEnumerable<IInanimate> contents = Inventory.EntitiesContained();

                //Should we even check? we have 100 slots make sure there's not one just open anyways
                if (contents.Count() == 100)
                {
                    //Check for stacking
                    int sameType = contents.Where(entity => entity.TemplateId.Equals(obj.TemplateId)).Count();
                    int numStacks = Math.DivRem(sameType, obj.Template<IInanimateTemplate>().AccumulationCap, out int extraSpace);

                    //We need an extra slot for this to go in, we're operating on 100 slots of inventory and they're all full and no room in an existing stack so error
                    if (extraSpace == 0)
                    {
                        return "Your inventory is full and no stacks inside it have room.";
                    }
                }

                Inventory.Add(obj);
                UpsertToLiveWorldCache();
                return string.Empty;
            }

            return "Invalid type to move to container.";
        }

        /// <summary>
        /// Move an entity out of this
        /// </summary>
        /// <typeparam name="T">the type of entity to remove</typeparam>
        /// <param name="thing">the entity</param>
        /// <returns>errors</returns>
        public string MoveFrom<T>(T thing)
        {
            IEnumerable<Type> implimentedTypes = DataUtility.GetAllImplimentingedTypes(typeof(T));

            if (implimentedTypes.Contains(typeof(IInanimate)))
            {
                IInanimate obj = (IInanimate)thing;

                if (!Inventory.Contains(obj))
                {
                    return "That is not in the container";
                }

                Inventory.Remove(obj);
                obj.TryMoveTo(new GlobalPosition(default(IContains)));
                UpsertToLiveWorldCache();
                return string.Empty;
            }

            return "Invalid type to move from container.";
        }

        /// <summary>
        /// Returns this entity as a container position
        /// </summary>
        /// <returns></returns>
        public IGlobalPosition GetContainerAsLocation()
        {
            return new GlobalPosition(this);
        }

        /// <summary>
        /// Show the stacks in this container, only for inanimates
        /// </summary>
        /// <returns>A list of the item stacks</returns>
        public HashSet<IItemStack> ShowStacks(IEntity observer)
        {
            HashSet<IItemStack> stacks = new HashSet<IItemStack>();

            foreach (IGrouping<long, IInanimate> itemGroup in Inventory.EntitiesContained().GroupBy(item => item.TemplateId))
            {
                stacks.Add(new ItemStack(itemGroup));
            }

            return stacks;
        }
        #endregion

        #region SpawnBehavior
        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            if (CurrentLocation == null)
            {
                throw new NotImplementedException("NPCs can't spawn to nothing");
            }

            SpawnNewInWorld(CurrentLocation);
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IGlobalPosition position)
        {
            INonPlayerCharacterTemplate bS = Template<INonPlayerCharacterTemplate>() ?? throw new InvalidOperationException("Missing backing data store on NPC spawn event.");

            Keywords = bS.Keywords;

            if (string.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            CurrentHealth = bS.TotalHealth;
            CurrentStamina = bS.TotalStamina;

            Qualities = bS.Qualities;
            DecayEvents = bS.DecayEvents;
            Interactions = bS.Interactions;
            UsableAbilities = bS.UsableAbilities;
            TotalHealth = bS.TotalHealth;
            TotalStamina = bS.TotalStamina;
            Race = bS.Race;
            Personality = bS.Personality;
            SurName = bS.SurName;
            Gender = bS.Gender;
            WillPurchase = bS.WillPurchase;
            WillSell = bS.WillSell;
            InventoryRestock = bS.InventoryRestock;
            TeachableAbilities = bS.TeachableAbilities;
            TeachableProficencies = bS.TeachableProficencies;

            TryMoveTo(position);

            if (CurrentLocation == null)
            {
                throw new NotImplementedException("Objects can't spawn to nothing");
            }

            UpsertToLiveWorldCache(true);

            KickoffProcesses();
        }

        public override string TryMoveTo(IGlobalPosition newPosition)
        {
            string error = string.Empty;

            if (CurrentLocation?.CurrentContainer != null)
            {
                error = CurrentLocation.CurrentContainer.MoveFrom(this);
            }

            //validate position
            if (newPosition != null && string.IsNullOrEmpty(error))
            {
                if (newPosition.CurrentContainer != null)
                {
                    error = newPosition.CurrentContainer.MoveInto(this);
                }

                if (string.IsNullOrEmpty(error))
                {
                    //Check for intruders
                    newPosition = IntruderSlide(newPosition);

                    CurrentLocation = newPosition;
                    UpsertToLiveWorldCache();
                    error = string.Empty;
                }
            }
            else
            {
                error = "Cannot move to an invalid location";
            }

            return error;
        }


        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new NonPlayerCharacter
            {
                Qualities = Qualities,
                TemplateId = TemplateId,
                CurrentHealth = CurrentHealth,
                CurrentStamina = CurrentStamina,
                Hypothalamus = Hypothalamus
            };
        }
        #endregion

        #region Processes
        internal override void KickoffProcesses()
        {
            //Start decay eventing for this zone
            base.KickoffProcesses();

            Processor.StartSubscriptionLoop("Needs", () => AdvanceNeeds(), 1 * 60, false);
        }

        private bool AdvanceNeeds()
        {
            Random rand = new Random();
            Hypothalamus.ApplyPressure(Motivator.Hunger, 5);
            Hypothalamus.ApplyPressure(Motivator.Thirst, 5);
            Hypothalamus.ApplyPressure(Motivator.Tiredness, 5);
            Hypothalamus.ApplyPressure(Motivator.Lonliness, rand.Next(1, 10));
            Hypothalamus.ApplyPressure(Motivator.Status, rand.Next(1, 10));
            Hypothalamus.ApplyPressure(Motivator.Success, rand.Next(1, 10));

            DoTheThing(Hypothalamus.HighestCurrentNeed);

            return Save();
        }
        #endregion
    }
}
