/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121101 11:46
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121101 11:46
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.WPF;
using Rafy.WPF.Controls;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// WPF 下的元数据扩展
    /// </summary>
    public static class WPFMetaExtension
    {
        /// <summary>
        /// 显式指明一个属性是否需要进行合计。
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="value">true: 合计； false: 不合计； null:根据类型来判断。</param>
        /// <returns></returns>
        public static WPFEntityPropertyViewMeta Summarize(this WPFEntityPropertyViewMeta meta, bool? value)
        {
            TreeColumn.SetNeedSummary(meta, value);

            return meta;
        }

        /// <summary>
        /// 显式指明一个列表视图是否需要显示合计行。
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public static WPFEntityViewMeta ShowSummaryRow(this WPFEntityViewMeta meta)
        {
            BlockUIFactory.SetNeedSummary(meta, true);

            return meta;
        }
    }
}