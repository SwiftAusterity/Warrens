using NetMud.DataAccess;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using System;
using System.Collections.Generic;

namespace NetMud.Data.Game
{
    public abstract class EntityPartial : IEntity
    {
        #region Data and live tracking properties
        public string BirthMark { get; internal set; }
        public DateTime Birthdate { get; internal set; }
        public IData DataTemplate { get; internal set; }

        private string[] _keywords;
        public string[] Keywords
        {
            get { return _keywords; }
            set
            {
                _keywords = value;
                UpsertToLiveWorldCache();
            }
        }
        #endregion

        private Func<IEnumerable<string>, bool> _writeTo;
        public Func<IEnumerable<string>, bool> WriteTo 
        { 
            get
            {
                if (_writeTo != null)
                {
                    var pred = new Predicate<IEnumerable<string>>(_writeTo);
                    return new Func<IEnumerable<string>, bool>(pred);
                }

                return (input) => TriggerAIAction(input);
            }
            set
            {
                _writeTo = value;
            }
        }

        private string _currentLocationBirthmark;
        public IContains CurrentLocation
        {
            get 
            { 
                if(!String.IsNullOrWhiteSpace(_currentLocationBirthmark))
                    return LiveCache.Get<IContains>(new LiveCacheKey(typeof(IContains), _currentLocationBirthmark));

                return null; 
            }
            set
            {
                if (value == null)
                    return;

                _currentLocationBirthmark = value.BirthMark;
                UpsertToLiveWorldCache();
            }
        }

        public abstract void SpawnNewInWorld();

        public abstract void SpawnNewInWorld(IContains spawnTo);

        public void UpsertToLiveWorldCache()
        {
            LiveCache.Add(this);
        }

        public bool TriggerAIAction(IEnumerable<string> input, AITriggerType trigger = AITriggerType.Seen)
        {
            return true;
        }

        public abstract IEnumerable<string> RenderToLook();

        #region Equality Functions
        public int CompareTo(IEntity other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != this.GetType())
                        return -1;

                    if (other.BirthMark.Equals(this.BirthMark))
                        return 1;

                    return 0;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return -99;
        }

        public bool Equals(IEntity other)
        {
            if (other != default(IEntity))
            {
                try
                {
                    return other.GetType() == this.GetType() && other.BirthMark.Equals(this.BirthMark);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }
        #endregion

        public abstract byte[] Serialize();

        public abstract IEntity DeSerialize(byte[] bytes);
    }
}
