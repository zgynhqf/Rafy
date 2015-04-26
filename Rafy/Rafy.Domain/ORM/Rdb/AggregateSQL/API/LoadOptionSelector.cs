/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101230
 * 说明：聚合SQL的简单API
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101230
 * 
*******************************************************/

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rafy.MetaModel;
using Linq = System.Linq.Expressions;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 存储了加载的项
    /// </summary>
    public abstract class LoadOptionSelector
    {
        internal LoadOptionSelector(AggregateDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        private AggregateDescriptor _descriptor;

        internal AggregateDescriptor InnerDescriptor
        {
            get
            {
                return _descriptor;
            }
        }
    }
}