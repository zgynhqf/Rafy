/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120312
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120312
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// <see cref="WPFCommand"/> 的集合。
    /// </summary>
    public class WPFCommandCollection : CommandCollection<WPFCommand>
    {
        /// <summary>
        /// 在命令中查找指定类型的命令。
        /// </summary>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        public WPFCommand Find(Type cmdType)
        {
            return this.FirstOrDefault(c => c.RuntimeType == cmdType);
        }

        /// <summary>
        /// 在集合中删除指定类型的命令。
        /// </summary>
        /// <param name="commands"></param>
        public void Remove(params Type[] commands)
        {
            this.Remove(commands as IEnumerable<Type>);
        }

        /// <summary>
        /// 在集合中删除指定类型的命令。
        /// </summary>
        /// <param name="commands"></param>
        public void Remove(IEnumerable<Type> commands)
        {
            foreach (var cmd in commands)
            {
                var c = this.Find(cmd);
                if (c != null) { this.Remove(c); }
            }
        }
    }
}