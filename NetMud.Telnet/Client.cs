using System;
using System.Collections.Generic;
using System.Net;
using NetMud.Authentication;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;

namespace NetMud.Telnet
{
    public class Client : Channel, IDescriptor
    {
        public IPEndPoint remoteEndPoint { get; private set; }
        public DateTime connectedAt { get; private set; }
        public EClientState clientState { get; set; }
        public string commandIssued { get; set; }

        /// <summary>
        /// Unique string for this live entity
        /// </summary>
        public string BirthMark { get; set; }

        /// <summary>
        /// When this entity was born to the world
        /// </summary>
        public DateTime Birthdate { get; set; }

        public Client(IPEndPoint _remoteEndPoint, DateTime _connectedAt, EClientState _clientState)
        {
            remoteEndPoint = _remoteEndPoint;
            connectedAt = _connectedAt;
            clientState = _clientState;
            commandIssued = string.Empty;

            BirthMark = LiveCache.GetUniqueIdentifier(string.Format(cacheKeyFormat, remoteEndPoint.Port));
            Birthdate = DateTime.Now;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Open()
        {
            throw new NotImplementedException();
        }

        public bool SendWrapper(IEnumerable<string> strings)
        {
            throw new NotImplementedException();
        }

        public bool SendWrapper(string str)
        {
            throw new NotImplementedException();
        }

        public void Disconnect(string finalMessage)
        {
            throw new NotImplementedException();
        }

        #region Caching
        /// <summary>
        /// What type of cache is this using
        /// </summary>
        public virtual CacheType CachingType => CacheType.Live;

        /// <summary>
        /// Put it in the cache
        /// </summary>
        /// <returns>success status</returns>
        public virtual bool PersistToCache()
        {
            try
            {
                LiveCache.Add<IDescriptor>(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SystemWarnings);
                return false;
            }

            return true;
        }
        #endregion

        #region Equality Functions
        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(ILiveData other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                        return -1;

                    if (other.BirthMark.Equals(BirthMark))
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

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(ILiveData other)
        {
            if (other != default(ILiveData))
            {
                try
                {
                    return other.GetType() == GetType() && other.BirthMark.Equals(BirthMark);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }

        /// <summary>
        /// Compares an object to another one to see if they are the same object
        /// </summary>
        /// <param name="x">the object to compare to</param>
        /// <param name="y">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(ILiveData x, ILiveData y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <param name="obj">the thing to get the hashcode for</param>
        /// <returns>the hash code</returns>
        public int GetHashCode(ILiveData obj)
        {
            return obj.GetType().GetHashCode() + obj.BirthMark.GetHashCode();
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <returns>the hash code</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode() + BirthMark.GetHashCode();
        }
        #endregion
    }
}
