/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130412 19:43
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130412 19:43
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.DomainModeling.Controls
{
    /// <summary>
    /// 标题的格式化接口
    /// </summary>
    public interface ITitleFormatter
    {
        /// <summary>
        /// 返回某个元素格式化后的标题
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        string Format(IModelingDesignerComponent component);
    }
}
