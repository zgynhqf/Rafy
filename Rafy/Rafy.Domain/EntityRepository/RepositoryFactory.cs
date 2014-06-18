/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130927
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130927 16:07
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain
{
    /// <summary>
    /// 仓库工厂
    /// </summary>
    public static class RepositoryFactory
    {
        /// <summary>
        /// 用于查找指定实体的仓库。
        /// </summary>
        /// <returns></returns>
        public static EntityRepository Find(Type entityType)
        {
            return RF.Find(entityType);
        }

        /// <summary>
        /// 用于查找指定实体的仓库。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static EntityRepository Find<TEntity>()
            where TEntity : Entity
        {
            return RF.Find<TEntity>();
        }

        /// <summary>
        /// 用于查找指定类型的仓库。
        /// </summary>
        /// <typeparam name="TRepository"></typeparam>
        /// <returns></returns>
        public static TRepository Concrete<TRepository>()
            where TRepository : EntityRepository
        {
            return RF.Concrete<TRepository>();
        }
    }
}