/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120220
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.Web.ClientMetaModel
{
    /// <summary>
    /// 支持的 javascript 数据类型
    /// </summary>
    public enum JavascriptType
    {
        String, Int, Float, Date, Boolean,
        /// <summary>
        /// 自定义的外键引用类型
        /// </summary>
        Reference
    }
}
