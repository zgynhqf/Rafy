/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120102
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120102
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.DbMigration.Model
{
    /// <summary>
    /// 更改类型
    /// </summary>
    public enum ChangeType
    {
        UnChanged,
        Added,
        Removed,
        Modified
    }
}