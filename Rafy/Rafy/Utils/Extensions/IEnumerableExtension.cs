/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：2008
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2008
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy
{
    public static class IEnumerableExtension
    {
        /// <summary>
        /// 进行指定的分页操作。
        /// 
        /// 如果分页信息指定了要统计所有的行数，则立刻执行 Count 方法获取所有行数。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="models">The models.</param>
        /// <param name="pagingInfo">The paging information.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">索引不能是负数。</exception>
        public static IQueryable<T> JumpToPage<T>(this IQueryable<T> models, PagingInfo pagingInfo)
        {
            //假如分页信息为空，直接返回，不需要分页。
            if (PagingInfo.IsNullOrEmpty(pagingInfo)) { return models; }
            if (pagingInfo.PageNumber < 1) { throw new ArgumentOutOfRangeException("索引不能是负数。"); }

            //需要记数
            if (pagingInfo.IsNeedCount)
            {
                pagingInfo.TotalCount = models.LongCount();
            }

            if (pagingInfo.PageNumber == 1)
            {
                return models.Take(pagingInfo.PageSize);
            }

            return models
                .Skip((int)(pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                .Take(pagingInfo.PageSize);
        }

        /// <summary>
        /// 进行指定的分页操作。
        /// 
        /// 如果分页信息指定了要统计所有的行数，则立刻执行 Count 方法获取所有行数。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="models">The models.</param>
        /// <param name="pagingInfo">The paging information.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">索引不能是负数。</exception>
        public static IEnumerable<T> JumpToPage<T>(this IEnumerable<T> models, PagingInfo pagingInfo)
        {
            //假如这个是空的,就不需要分页
            if (PagingInfo.IsNullOrEmpty(pagingInfo)) { return models; }
            if (pagingInfo.PageNumber < 1) { throw new ArgumentOutOfRangeException("索引不能是负数。"); }

            //需要记数
            if (pagingInfo.IsNeedCount)
            {
                pagingInfo.TotalCount = models.LongCount();
            }

            if (pagingInfo.PageNumber == 1)
            {
                return models.Take(pagingInfo.PageSize);
            }

            return models
                .Skip((int)(pagingInfo.PageNumber - 1) * pagingInfo.PageSize)
                .Take(pagingInfo.PageSize);
        }

        /// <summary>
        /// 转换为一个只读的集合。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="orignalCollections"></param>
        /// <returns></returns>
        public static IList<T> AsReadOnly<T>(this IList<T> orignalCollections)
        {
            if (orignalCollections == null) throw new ArgumentNullException("orignalCollections");

            return new System.Collections.ObjectModel.ReadOnlyCollection<T>(orignalCollections);
        }
    }
}
