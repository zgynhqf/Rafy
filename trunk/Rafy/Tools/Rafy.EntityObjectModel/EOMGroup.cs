/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130410 10:46
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130410 10:46
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.EntityObjectModel
{
    /// <summary>
    /// 表示一组实体类的实体对象模型
    /// </summary>
    public class EOMGroup
    {
        public EOMGroup()
        {
            this.EntityTypes = new EntityTypeCollection();
            this.EnumTypes = new EnumTypeCollection();
        }

        /// <summary>
        /// 所有的实体类型
        /// </summary>
        public EntityTypeCollection EntityTypes { get; private set; }

        /// <summary>
        /// 所有的枚举类型。
        /// </summary>
        public EnumTypeCollection EnumTypes { get; private set; }
    }
}
