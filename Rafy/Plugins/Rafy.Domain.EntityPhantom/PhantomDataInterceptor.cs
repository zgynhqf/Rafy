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
        private static bool _added = false;

        internal static void Intercept()
        {
            if (!_added)
            {
                RepositoryDataProvider.Querying += RepositoryDataProvider_Querying;

                _added = true;
            }

            DataSaver.SubmitInterceptors.Add(typeof(PhantomSubmitInterceptor));
        }

        private static void RepositoryDataProvider_Querying(object sender, QueryingEventArgs e)
        {
            var dp = sender as RepositoryDataProvider;
            if (!PhantomContext.NeedPhantomData.Value)
            {
                //为查询中的 Where 条件添加 "IsPhantom = 'false'" 条件
                var appender = new PhantomWhereAppender();
                appender.AddConditionToLast = true;
                appender.Append(e.Args.Query);
            }
        }

        #region class PhantomWhereAppender

        /// <summary>
        /// 为查询中的 Where 条件添加 "IsPhantom = 'false'" 条件的类。
        /// </summary>
        class PhantomWhereAppender : EveryTableWhereAppender
        {
            protected override IConstraint GetCondition(ITableSource tableSource, IQuery query)
            {
                if (tableSource.EntityRepository.EntityMeta.IsPhantomEnabled)
                {
                    var isPhantomColumn = tableSource.FindColumn(EntityPhantomExtension.IsPhantomProperty);
                    return isPhantomColumn.Equal(BooleanBoxes.False);
                }
                return null;
            }
        }

        #endregion
    }
}
