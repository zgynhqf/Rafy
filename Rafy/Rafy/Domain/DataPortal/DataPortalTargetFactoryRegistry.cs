/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211115
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211115 07:42
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy.Domain.DataPortal
{
    /// <summary>
    /// <see cref="IDataPortalTargetFactory"/> 的注册表。
    /// </summary>
    public static class DataPortalTargetFactoryRegistry
    {
        private static Dictionary<string, IDataPortalTargetFactory> targetFactories = new Dictionary<string, IDataPortalTargetFactory>();

        private static IDataPortalTargetFactory _lastFactoryCache;

        /// <summary>
        /// 注册一个 <see cref="IDataPortalTargetFactory"/> 的对象。
        /// </summary>
        /// <param name="targetFactory"></param>
        public static void Register(IDataPortalTargetFactory targetFactory)
        {
            targetFactories.Add(targetFactory.Name, targetFactory);
        }

        /// <summary>
        /// 通过名称来获取对应的工厂对象。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IDataPortalTargetFactory Get(string name)
        {
            var last = _lastFactoryCache;
            if (_lastFactoryCache.Name == name) return last;

            last = targetFactories[name];
            _lastFactoryCache = last;
            return last;
        }
    }
}
