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
    public static class PhantomContext
    {
        /// <summary>
        /// 是否需要同时查询幽灵实体。
        /// </summary>
        internal static readonly AppContextItem<bool> NeedPhantomData =
            new AppContextItem<bool>("Rafy.Domain.EntityPhantom.PhantomContext.NeedPhantomData");

        /// <summary>
        /// 关闭自动过滤幽灵数据。
        /// 调用此方法来声明一个需要查询幽灵实体的代码段。
        /// </summary>
        /// <returns></returns>
        public static IDisposable DontFilterPhantoms()
        {
            return NeedPhantomData.UseScopeValue(true);
        }

        /// <summary>
        /// 是否需要禁用幽灵功能，而是采用删除真实数据的行为。
        /// </summary>
        internal static readonly AppContextItem<bool> DeleteRealData =
            new AppContextItem<bool>("Rafy.Domain.EntityPhantom.PhantomContext.DeleteRealData");

        /// <summary>
        /// 关闭使用幽灵模式来保存数据，而是采用删除真实数据的行为。
        /// 调用此方法来声明一个需要使用真实模式来删除数据的代码段。
        /// </summary>
        /// <returns></returns>
        public static IDisposable DisablePhantomDeleting()
        {
            return DeleteRealData.UseScopeValue(true);
        }
    }
}