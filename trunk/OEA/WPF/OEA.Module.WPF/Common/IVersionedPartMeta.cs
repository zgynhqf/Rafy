/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110401
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100401
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.ComponentModel.Composition.Hosting;

namespace OEA.Module.WPF
{
    /// <summary>
    /// MEF 中组合的 Part 的版本信息。
    /// </summary>
    public interface IVersionedPartMeta
    {
        /// <summary>
        /// 版本号
        /// </summary>
        [DefaultValue(1)]
        int Version { get; }
    }

    public static class CompositionContainerExtension
    {
        /// <summary>
        /// 获取指定的契约的 ExportValue 信息，并返回其中最高版本的一个。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="container"></param>
        /// <param name="contractName"></param>
        /// <returns></returns>
        public static T TryGetExportedLastVersionValue<T>(this CompositionContainer container, string contractName)
            where T : class
        {
            var windows = container.GetExports<T, IVersionedPartMeta>(contractName);
            var lastVersionItem = windows.OrderByDescending(w => w.Metadata.Version).FirstOrDefault();

            if (lastVersionItem != null) { return lastVersionItem.Value; }

            return null;
        }
    }
}
