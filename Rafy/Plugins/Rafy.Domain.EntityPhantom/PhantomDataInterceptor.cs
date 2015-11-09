/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151022
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151022 16:09
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Domain.ORM.Query;

namespace Rafy.Domain.EntityPhantom
{
    /// <summary>
    /// 数据的删除、查询的拦截器。
    /// </summary>
    internal static class PhantomDataInterceptor
    {
        internal static void Intercept()
        {
            RepositoryDataProvider.Deleting += RepositoryDataProvider_Deleting;
            RepositoryDataProvider.Querying += RepositoryDataProvider_Querying;
        }

        private static void RepositoryDataProvider_Deleting(object sender, EntityCUDEventArgs e)
        {
            var dp = sender as RepositoryDataProvider;
            if (dp.Repository.EntityMeta.IsPhantomEnabled)
            {
                var entity = e.Entity;

                //删除数据的主逻辑变为修改 IsPhantom
                EntityPhantomExtension.SetIsPhantom(entity, true);

                //然后调用 DataSaver 来保存数据。（不直接使用 Sql 语句，因为这里并不一定是使用 RDb 作为持久层。）
                dp.DataSaver.UpdateToPersistence(entity);

                //由于删除后的实体的状态会变为‘New’，所以需要把这个字段的值重置。
                entity.ResetProperty(EntityPhantomExtension.IsPhantomProperty);

                //不再使用默认的删除逻辑。
                e.Cancel = true;
            }
        }

        private static void RepositoryDataProvider_Querying(object sender, QueryingEventArgs e)
        {
            var dp = sender as RepositoryDataProvider;
            if (dp.Repository.EntityMeta.IsPhantomEnabled && !PhantomQueryContext.NeedPhantomData)
            {
                //为查询中的 Where 条件添加 "IsPhantom = 'true'" 条件
                var appender = new PhantomWhereAppender();
                appender.AddConditionToLast = true;
                appender.Append(e.Args.Query);
            }
        }

        #region class PhantomWhereAppender

        /// <summary>
        /// 为查询中的 Where 条件添加 "IsPhantom = 'true'" 条件的类。
        /// </summary>
        class PhantomWhereAppender : MainTableWhereAppender
        {
            protected override IConstraint GetCondition(ITableSource mainTable, IQuery query)
            {
                var isPhantomColumn = mainTable.FindColumn(EntityPhantomExtension.IsPhantomProperty);
                return isPhantomColumn.Equal(BooleanBoxes.False);
            }
        }

        #endregion
    }
}
