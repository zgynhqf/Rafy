/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130420
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130420 17:34
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Rafy.EntityObjectModel
{
    /// <summary>
    /// 枚举集合
    /// </summary>
    public class EnumTypeCollection : Collection<EnumType>
    {
        /// <summary>
        /// 通过全名称查找某个枚举。
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public EnumType Find(string fullName)
        {
            return this.FirstOrDefault(e => e.TypeFullName == fullName);
        }
    }
}
