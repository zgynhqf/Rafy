/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20170315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20170315 13:41
 * 
*******************************************************/

using System;
using System.Collections.Generic;

namespace Rafy.DataArchiver
{
    /// <summary>
    /// 表示数据归档插件的上下文。
    /// </summary>
    public class AggregationArchiveContext
    {
        /// <summary>
        /// 需要迁移的数据所在的库的配置名。
        /// </summary>
        public string OrignalDataDbSettingName { get; set; }

        /// <summary>
        /// 需要把数据迁移的目标数据库的配置名。
        /// </summary>
        public string BackUpDbSettingName { get; set; }

        /// <summary>
        /// 获取一个时间点，用于确定归档哪天之前的数据。
        /// </summary>
        public DateTime DateOfArchiving { get; set; }

        /// <summary>
        /// 迁移的聚合根类型集合
        /// </summary>
        public List<Type> AggregationsToArchive { get; set; }

        /// <summary>
        /// 每批次迁移实体的个数
        /// </summary>
        public int BatchSize { get; set; }
    }
}