/*******************************************************
 * 
 * 作者：杜强
 * 创建时间：20110505
 * 说明：见注释
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 杜强 20110505
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// 命令分组类型
    /// 
    /// 分辨该命令在是属于哪种逻辑的命令，方便系统控制。
    /// </summary>
    public static class CommandGroupType
    {
        /// <summary>
        /// 没有指定
        /// </summary>
        public const int None = 0;
        /// <summary>
        /// 业务类按钮
        /// </summary>
        public const int Business = 1;
        /// <summary>
        /// 公用的查看类型 如展开树形列表，刷新等
        /// </summary>
        public const int View = 2;
        /// <summary>
        /// 公用的编辑类型 如增、删等
        /// </summary>
        public const int Edit = 3;

        /// <summary>
        /// 系统级命令，如：界面配置、报表功能挖掘、导出 Excel 等。
        /// </summary>
        public const int System = -1;
    }
}