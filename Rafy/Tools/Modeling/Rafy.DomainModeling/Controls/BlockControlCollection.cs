/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130401 23:25
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130401 23:25
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Rafy;

namespace Rafy.DomainModeling.Controls
{
    /// <summary>
    /// 实体/枚举的集合。
    /// </summary>
    public class BlockControlCollection : NotifyChangedCollection<BlockControl>
    {
        //public BlockControl Find(string title)
        //{
        //    return this.FirstOrDefault(i => i.Title == title);
        //}

        /// <summary>
        /// 通过类型全名称，查找实体/枚举。
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public BlockControl Find(string fullName)
        {
            return this.FirstOrDefault(i => i.TypeFullName == fullName);
        }
    }
}