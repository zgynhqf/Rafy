/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110320
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100320
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ORM;

namespace OEA.Library
{
    /// <summary>
    /// TableInfo的驻留器
    /// </summary>
    interface ITableInfoHost
    {
        ITable GetTableInfo();
    }
}