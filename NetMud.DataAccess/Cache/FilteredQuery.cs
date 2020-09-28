using NetMud.DataStructure.Architectural;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace NetMud.DataAccess.Cache
{
    public class FilteredQuery<T>
    {
        /// <summary>
        /// The type of cache we're hitting
        /// </summary>
        public CacheType SourceType { get; }

        /// <summary>
        /// Current page number requested
        /// </summary>
        public int CurrentPageNumber { get; set; }

        /// <summary>
        /// How many items you want per page
        /// </summary>
        public int ItemsPerPage { get; set; }

        /// <summary>
        /// Function defining the filtering criteria
        /// </summary>
        public Func<T, bool> Filter { get; set; }

        /// <summary>
        /// Function defining the ordering
        /// </summary>
        public Func<T, object> OrderPrimary { get; set; }

        /// <summary>
        /// Function defining the secondary ordering
        /// </summary>
        public Func<T, object> OrderSecondary { get; set; }

        /// <summary>
        /// The stuff
        /// </summary>
        public IQueryable<T> Items { get; }

        public FilteredQuery(CacheType sourceType)
        {
            SourceType = sourceType;
            CurrentPageNumber = 1;
            ItemsPerPage = 25;
            Filter = null;
            OrderPrimary = null;
            OrderSecondary = null;

            switch (SourceType)
            {
                case CacheType.ConfigData:
                    Items = ConfigDataCache.GetAll<T>();
                    break;
                case CacheType.LookupData:
                case CacheType.Template:
                    Items = TemplateCache.GetAll<T>();
                    break;
                case CacheType.PlayerData:
                case CacheType.Live:
                default:
                    Items = Enumerable.Empty<T>().AsQueryable();
                    break;
            }
        }

        public int RawCount()
        {
            return Items.Count();
        }

        public int FilteredCount()
        {
            ParallelQuery<T> parallelQuery = Items.AsParallel();
            if (Filter != null)
            {
                parallelQuery = parallelQuery.Where(value => Filter(value));
            }

            return parallelQuery.Count();
        }

        public IEnumerable<T> ExecuteQuery()
        {
            try
            {
                ParallelQuery<T> parallelQuery = Items.AsParallel();
                if (Filter != null)
                {
                    parallelQuery = parallelQuery.Where(value => Filter(value));
                }

                if (OrderPrimary != null)
                {
                    if (OrderSecondary != null)
                    {

                        parallelQuery = parallelQuery
                                .OrderBy(OrderPrimary)
                                .ThenBy(OrderSecondary);
                    }

                    parallelQuery = parallelQuery
                            .OrderBy(OrderPrimary);
                }

                int skip = (CurrentPageNumber - 1) * ItemsPerPage;
                int take = Math.Abs(Items.Count() - skip) >= ItemsPerPage ? ItemsPerPage : Math.Abs(Items.Count() - skip);

                return parallelQuery.Skip(skip).Take(take);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return Enumerable.Empty<T>();
        }
    }
}
