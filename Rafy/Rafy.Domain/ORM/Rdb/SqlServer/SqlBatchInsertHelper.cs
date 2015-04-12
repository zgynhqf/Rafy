/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130416
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130416 15:35
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Rafy.Domain.Caching;

namespace Rafy.Domain.ORM.SqlServer
{
    /// <summary>
    /// 如果使用 SqlServer 大量插入数据，可以使用本类来实现 Insert 方法。
    /// </summary>
    public static class SqlBatchInsertHelper
    {
        /// <summary>
        /// 批量插入 Batch SQL
        /// </summary>
        /// <param name="entity"></param>
        public static void BatchInsert(Entity entity)
        {
            //根对象使用批插入
            var reader = new EntityChldrenBatchReader(entity);
            var dic = reader.Read();

            foreach (var kv in dic)
            {
                var repository = RepositoryFactoryHost.Factory.FindByEntity(kv.Key) as IRepositoryInternal;
                AddBatch(repository, kv.Value);

                if (VersionSyncMgr.IsEnabled)
                {
                    VersionSyncMgr.Repository.UpdateVersion(kv.Key);
                }
            }
        }

        /// <summary>
        /// 批量插入大量实体
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="entityList">The entity list.</param>
        /// <exception cref="System.ArgumentOutOfRangeException"></exception>
        /// <exception cref="System.ArgumentNullException">只支持 SqlServer</exception>
        private static void AddBatch(IRepositoryInternal repository, IList<Entity> entityList)
        {
            if (entityList.Count < 1) throw new ArgumentOutOfRangeException();

            var dp = RdbDataProvider.Get(repository);
            var connection = dp.CreateDbAccesser().Connection;
            var tableInfo = dp.DbTable;

            var sqlCon = connection as SqlConnection;
            if (sqlCon == null) throw new ArgumentNullException("只支持 SqlServer");
            new BatchInsert(entityList, sqlCon, tableInfo).Execute();
        }
    }
}
