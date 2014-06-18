/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2010
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2010
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Rafy.Domain
{
    partial class EntityList
    {
        /// <summary>
        /// 通过 Id 来查找某个实体。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Entity Find(object id)
        {
            for (int i = 0, c = this.Count; i < c; i++)
            {
                var e = this[i];
                if (e.Id.Equals(id)) return e;
            }
            return null;
        }
    }
}
