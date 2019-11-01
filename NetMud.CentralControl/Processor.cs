using NetMud.Communication;
using NetMud.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;

namespace NetMud.CentralControl
{
    /// <summary>
    /// Central handler for all things async and recursive
    /// </summary>
    public static class Processor
    {
        private static readonly ObjectCache globalCache = MemoryCache.Default;
        private static readonly CacheItemPolicy globalPolicy = new CacheItemPolicy();
        private static readonly string cancellationTokenCacheKeyFormat = "AsyncCancellationToken.{0}";
        private static readonly string subscriptionLoopCacheKeyFormat = "SubscriptionLoop.{0}";
        private static readonly int _maxPulseCount = 18000; //half an hour

        /// <summary>
        /// Starts a brand new Loop
        /// </summary>
        /// <param name="designator">what the loop will be called</param>
        /// <param name="subscriberProcess">What delegate process to call for the loop</param>
        /// <param name="pulseCount">on what pulse does this fire (out of 18000)</param>
        /// <param name="fireOnce">Does this fire only once or does it fire every X pulses</param>
        public static void StartSubscriptionLoop(string designator, Func<bool> subscriberProcess, int pulseCount, bool fireOnce)
        {
            IList<Tuple<Func<bool>, int>> currentList = SubscribeToLoop(designator, subscriberProcess, pulseCount);

            //Only one means we need to start the loop
            if (currentList.Count() == 1)
            {
                CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

                StoreCancellationToken(designator, cancelTokenSource);

                async void looperProcess(object args)
                {
                    SubscriptionLoopArgs subArgs = args as SubscriptionLoopArgs;
                    while (1 == 1)
                    {
                        IList<Tuple<Func<bool>, int>> subList = GetAllCurrentSubscribers(subArgs.Designation, subArgs.CurrentPulse, fireOnce);

                        foreach (Tuple<Func<bool>, int> pulsar in subList)
                        {
                            //false return means it wants to be removed
                            if(!pulsar.Item1.Invoke())
                            {
                                RemoveSubscriber(designator, pulsar);
                            }
                        }

                        subArgs.CurrentPulse++;

                        if (subArgs.CurrentPulse == _maxPulseCount)
                        {
                            subArgs.CurrentPulse = 0;
                        }

                        await Task.Delay(10);
                    }
                }

                Task newLoop = new Task(looperProcess, new SubscriptionLoopArgs(designator, 0), cancelTokenSource.Token, TaskCreationOptions.LongRunning);

                newLoop.ContinueWith((previousTask) =>
                {
                    //Just end it
                    RemoveSubscriberList(designator);
                    RemoveCancellationToken(designator);
                }, TaskContinuationOptions.NotOnRanToCompletion);

                newLoop.Start();
            }

        }

        /// <summary>
        /// Starts a brand new Loop
        /// </summary>
        /// <param name="designator">what the loop will be called</param>
        /// <param name="rampupDelay">In # of Seconds - how long to wait before performing the first operation on start</param>
        /// <param name="cooldownDelay">In # of Seconds - how long to wait before either shutting the loop down or recurring it</param>
        /// <param name="maxDuration">In # of Seconds - kill this loop after this many seconds</param>
        /// <param name="workProcess">What delegate process to call for the loop</param>
        public static void StartSingeltonChainedLoop(string designator, int rampupDelay, int cooldownDelay, int maxDuration, Func<bool> workProcess)
        {
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

            if (maxDuration > 0)
            {
                cancelTokenSource.CancelAfter(maxDuration * 1000); //seconds * 1000 for miliseconds
            }

            StoreCancellationToken(designator, cancelTokenSource);

            bool loopedProcess()
            {
                StartLoop(workProcess, rampupDelay);
                return true;
            }

            Task<bool> newLoop = new Task<bool>(loopedProcess, cancelTokenSource.Token, TaskCreationOptions.LongRunning);

            newLoop.ContinueWith(async (previousTask) =>
            {
                await Task.Delay(cooldownDelay * 1000);

                //Just end it
                RemoveCancellationToken(designator);

                if (!previousTask.IsFaulted && !previousTask.IsCanceled && previousTask.IsCompleted)
                {
                    StartSingeltonChainedLoop(designator, rampupDelay, cooldownDelay, maxDuration, workProcess);
                }
            });

            newLoop.Start();
        }

        /// <summary>
        /// Starts a brand new Loop
        /// </summary>
        /// <param name="designator">what the loop will be called</param>
        /// <param name="rampupDelay">In # of Seconds - how long to wait before performing the first operation on start</param>
        /// <param name="cooldownDelay">In # of Seconds - how long to wait before either shutting the loop down or recurring it</param>
        /// <param name="maxDuration">In # of Seconds - kill this loop after this many seconds</param>
        /// <param name="workProcess">What delegate process to call for the loop</param>
        /// <param name="successTailProcess">What delegate process to call when this loop terminates in success</param>
        /// <param name="failedTailProcess">What delegate process to call when this loop terminates in failure</param>
        public static void StartSingeltonLoop(string designator, int rampupDelay, int cooldownDelay, int maxDuration
            , Func<bool> workProcess, Func<bool> successTailProcess = null, Func<bool> failedTailProcess = null)
        {
            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

            if (maxDuration > 0)
            {
                cancelTokenSource.CancelAfter(maxDuration * 1000); //seconds * 1000 for miliseconds
            }

            StoreCancellationToken(designator, cancelTokenSource);

            bool loopedProcess()
            {
                StartLoop(workProcess, rampupDelay);
                return true;
            }

            Task<bool> newLoop = new Task<bool>(loopedProcess, cancelTokenSource.Token, TaskCreationOptions.LongRunning);

            newLoop.ContinueWith(async (previousTask) =>
            {
                await Task.Delay(cooldownDelay * 1000);

                if (previousTask.IsFaulted && failedTailProcess != null)
                {
                    failedTailProcess.Invoke();
                }
                else if (successTailProcess != null)
                {
                    successTailProcess.Invoke();
                }

                RemoveCancellationToken(designator);
            });

            newLoop.Start();
        }

        /// <summary>
        /// Starts an endless loop
        /// </summary>
        /// <param name="worker">The action to perform every cycle</param>
        /// <param name="startDelay">The amount of seconds to delay before running the worker</param>
        private static async void StartLoop(Func<bool> worker, int startDelay)
        {
            await Task.Delay(5000);
            worker.Invoke();
        }

        /// <summary>
        /// Tries to shutdown an existing loop
        /// </summary>
        /// <param name="designator">what the loop is called</param>
        /// <param name="shutdownDelay">In # of Seconds - how long to wait before cancelling it</param>
        /// <param name="shutdownAnnouncementFrequency">In # of Seconds - how often to announce the shutdown, <= 0 is only one announcement</param>
        /// <param name="shutdownAnnouncement">What to announce (string.format format) before shutting down, empty string = no announcements, ex "World shutting down in {0} seconds."</param>
        public static void ShutdownLoop(string designator, int shutdownDelay, string shutdownAnnouncement, int shutdownAnnouncementFrequency = -1)
        {
            CancellationTokenSource cancelToken = GetCancellationToken(designator);

            if (cancelToken != null && cancelToken.Token.CanBeCanceled)
            {
                cancelToken.CancelAfter(shutdownDelay * 1000);
            }

            if (!string.IsNullOrWhiteSpace(shutdownAnnouncement))
            {
                SystemCommunicationsUtility.BroadcastToAll(string.Format(shutdownAnnouncement, shutdownDelay));
            }

            if (shutdownAnnouncementFrequency > 0)
            {
                Task.Run(() => RunAnnouncements(shutdownDelay, shutdownAnnouncement, shutdownAnnouncementFrequency));
            }
        }


        /// <summary>
        /// Get all the living task cancellation tokens
        /// </summary>
        /// <returns>All the tokens in the live cache</returns>
        public static Dictionary<string, CancellationTokenSource> GetAllLiveTaskStatusTokens()
        {
            Dictionary<string, CancellationTokenSource> returnDict = new Dictionary<string, CancellationTokenSource>();
            foreach (KeyValuePair<string, object> kvp in globalCache.Where(kvp => kvp.Value.GetType() == typeof(CancellationTokenSource)))
            {
                returnDict.Add(kvp.Key.Replace("AsyncCancellationToken.", string.Empty), (CancellationTokenSource)kvp.Value);
            }

            return returnDict;
        }

        /// <summary>
        /// Cancel ALL the current loops
        /// </summary>
        /// <param name="shutdownDelay">In # of Seconds - how long to wait before cancelling it</param>
        /// <param name="shutdownAnnouncementFrequency">In # of Seconds - how often to announce the shutdown, < = 0 is only one announcement</param>
        /// <param name="shutdownAnnouncement">What to announce (string.format format) before shutting down, empty string = no announcements</param>
        public static void ShutdownAll(int shutdownDelay, string shutdownAnnouncement, int shutdownAnnouncementFrequency = -1)
        {
            IEnumerable<CancellationTokenSource> tokens
                = globalCache.Where(kvp => kvp.Value.GetType() == typeof(CancellationTokenSource)).Select(kvp => (CancellationTokenSource)kvp.Value);

            foreach (CancellationTokenSource token in tokens.Where(tk => !tk.IsCancellationRequested && tk.Token.CanBeCanceled))
            {
                if (token != null)
                {
                    token.CancelAfter(shutdownDelay * 1000);
                }
            }

            if (!string.IsNullOrWhiteSpace(shutdownAnnouncement))
            {
                SystemCommunicationsUtility.BroadcastToAll(string.Format(shutdownAnnouncement, shutdownDelay));
            }

            if (shutdownAnnouncementFrequency > 0)
            {
                Task.Run(() => RunAnnouncements(shutdownDelay, shutdownAnnouncement, shutdownAnnouncementFrequency));
            }
        }

        /// <summary>
        /// Runs looped formatted messages until the timer runs out
        /// </summary>
        /// <param name="shutdownDelay">Total amount of message time</param>
        /// <param name="shutdownAnnouncement">What to announce</param>
        /// <param name="shutdownAnnouncementFrequency">How often to announce it</param>
        private static async void RunAnnouncements(int shutdownDelay, string shutdownAnnouncement, int shutdownAnnouncementFrequency)
        {
            int secondsLeftBeforeShutdown = shutdownDelay;
            while (secondsLeftBeforeShutdown > 0)
            {
                SystemCommunicationsUtility.BroadcastToAll(string.Format(shutdownAnnouncement, secondsLeftBeforeShutdown));
                await Task.Delay(shutdownAnnouncementFrequency * 1000);
                secondsLeftBeforeShutdown -= shutdownAnnouncementFrequency;
            }
        }

        /// <summary>
        /// Store the cancel token in the cache
        /// </summary>
        /// <param name="designator">what the loop is called</param>
        /// <param name="token">the token to store</param>
        private static void StoreCancellationToken(string designator, CancellationTokenSource token)
        {
            globalCache.AddOrGetExisting(string.Format(cancellationTokenCacheKeyFormat, designator), token, globalPolicy);
        }

        /// <summary>
        /// Gets the token from the cache
        /// </summary>
        /// <param name="designator">what the loop is called</param>
        /// <returns>the token</returns>
        private static CancellationTokenSource GetCancellationToken(string designator)
        {
            try
            {
                return (CancellationTokenSource)globalCache[string.Format(cancellationTokenCacheKeyFormat, designator)];
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, false);
            }

            return null;
        }

        /// <summary>
        /// Removes a token from the cache
        /// </summary>
        /// <param name="designator">The token's designator</param>
        private static void RemoveCancellationToken(string designator)
        {
            globalCache.Remove(string.Format(cancellationTokenCacheKeyFormat, designator));
        }

        /// <summary>
        /// Store the cancel token in the cache
        /// </summary>
        /// <param name="designator">what the loop is called</param>
        /// <param name="token">the token to store</param>
        private static IList<Tuple<Func<bool>, int>> SubscribeToLoop(string designator, Func<bool> subscriber, int pulseCount)
        {
            string taskKey = string.Format(subscriptionLoopCacheKeyFormat, designator);

            if (!(globalCache.Get(taskKey) is IList<Tuple<Func<bool>, int>> taskList))
            {
                taskList = new List<Tuple<Func<bool>, int>>();
            }

            taskList.Add(new Tuple<Func<bool>, int>(subscriber, pulseCount));

            globalCache.Remove(taskKey);

            globalCache.Add(taskKey, taskList, globalPolicy);

            return taskList;
        }

        private static IList<Tuple<Func<bool>, int>> GetAllCurrentSubscribers(string designator, int pulseCount, bool fireOnce)
        {
            if (pulseCount == 0)
            {
                return new List<Tuple<Func<bool>, int>>();
            }

            return GetLoopSubscribers(designator).Where(ls => (fireOnce && ls.Item2.Equals(pulseCount)) || pulseCount % ls.Item2 == 0).ToList();
        }

        /// <summary>
        /// Gets the token from the cache
        /// </summary>
        /// <param name="designator">what the loop is called</param>
        /// <returns>the token</returns>
        private static IList<Tuple<Func<bool>, int>> GetLoopSubscribers(string designator)
        {
            string taskKey = string.Format(subscriptionLoopCacheKeyFormat, designator);

            try
            {
                if (!(globalCache.Get(taskKey) is IList<Tuple<Func<bool>, int>> subscriberList))
                {
                    subscriberList = new List<Tuple<Func<bool>, int>>();
                }

                return subscriberList;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, false);
            }

            return null;
        }

        private static void RemoveSubscriber(string designator, Tuple<Func<bool>, int> memberPulser)
        {
            string taskKey = string.Format(subscriptionLoopCacheKeyFormat, designator);

            try
            {
                if (!(globalCache.Get(taskKey) is IList<Tuple<Func<bool>, int>> subscriberList))
                {
                    subscriberList = new List<Tuple<Func<bool>, int>>();
                }

                subscriberList.Remove(memberPulser);

                globalCache.Remove(taskKey);

                globalCache.Add(taskKey, subscriberList, globalPolicy);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, false);
            }
        }

        /// <summary>
        /// Removes a token from the cache
        /// </summary>
        /// <param name="designator">The token's designator</param>
        private static void RemoveSubscriberList(string designator)
        {
            string taskKey = string.Format(subscriptionLoopCacheKeyFormat, designator);

            globalCache.Remove(taskKey);
        }
    }
}
