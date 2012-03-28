using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hxy
{
    public static class IEnumerableExtension
    {
        public static IEnumerable<T> JumpToPage<T>(this IEnumerable<T> models, PagerInfo pagerInfo)
        {
            //假如这个是空的,就不需要分页
            if (pagerInfo == null)
            {
                return models;
            }

            //需要记数
            if (pagerInfo.IsNeedCount)
            {
                pagerInfo.TotalCount = models.Count();
            }

            if (pagerInfo.PageIndex == 1)
            {
                return models.Take(pagerInfo.PageSize);
            }

            if (pagerInfo.PageIndex < 1)
            {
                throw new ArgumentOutOfRangeException("PageIndex should be a positive integer.");
            }

            return models
                .Skip((pagerInfo.PageIndex - 1) * pagerInfo.PageSize)
                .Take(pagerInfo.PageSize);
        }

        public static IQueryable<T> JumpToPage<T>(this IQueryable<T> models, PagerInfo pagerInfo)
        {
            //假如这个是空的,就不需要分页
            if (pagerInfo == null)
            {
                return models;
            }

            //需要记数
            if (pagerInfo.IsNeedCount)
            {
                pagerInfo.TotalCount = models.Count();
            }

            if (pagerInfo.PageIndex == 1)
            {
                return models.Take(pagerInfo.PageSize);
            }

            if (pagerInfo.PageIndex < 1)
            {
                throw new ArgumentOutOfRangeException("PageIndex should be a positive integer.");
            }

            return models
                .Skip((pagerInfo.PageIndex - 1) * pagerInfo.PageSize)
                .Take(pagerInfo.PageSize);
        }

        public static IList<T> AsReadOnly<T>(this IList<T> orignalCollections)
        {
            if (orignalCollections == null) throw new ArgumentNullException("orignalCollections");

            return new System.Collections.ObjectModel.ReadOnlyCollection<T>(orignalCollections);
        }
    }
}
