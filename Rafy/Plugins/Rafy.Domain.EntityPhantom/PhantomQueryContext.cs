/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20151024
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20151024 12:42
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Domain.EntityPhantom
{
    /// <summary>
    /// 幽灵数据查询上下文
    /// </summary>
    public static class PhantomQueryContext
    {
        private const string NeedPhantomDataKey = "Rafy.Domain.EntityPhantom.PhantomQueryContext.NeedPhantomData";

        /// <summary>
        /// 是否需要同时查询幽灵实体。
        /// </summary>
        internal static bool NeedPhantomData
        {
            get
            {
                object value = null;
                AppContext.Items.TryGetValue(NeedPhantomDataKey, out value);
                return value == null ? false : (bool)value;
            }
            private set { AppContext.Items[NeedPhantomDataKey] = BooleanBoxes.Box(value); }
        }

        /// <summary>
        /// 关闭不自动过滤幽灵数据。
        /// 调用此方法来声明一个需要查询幽灵实体的代码段。
        /// </summary>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        public static IDisposable DontFilterPhantoms()
        {
            var scope = new PhantomDataScopeAgent
            {
                OldValue = NeedPhantomData
            };

            NeedPhantomData = true;

            return scope;
        }

        private class PhantomDataScopeAgent : IDisposable
        {
            internal bool OldValue;

            public void Dispose()
            {
                PhantomQueryContext.NeedPhantomData = OldValue;
            }
        }
    }
}