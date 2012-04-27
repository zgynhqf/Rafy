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

namespace OEA.MetaModel.View
{
    /// <summary>
    /// 环绕块
    /// 
    /// 支持 XML 序列化
    /// </summary>
    public class SurrounderBlock : Block
    {
        public SurrounderBlock(Type entityType, string surrounderType)
        {
            this.EntityType = entityType;
            this.SurrounderType = surrounderType;
        }

        public SurrounderBlock() { }

        /// <summary>
        /// 环绕类型
        /// </summary>
        public string SurrounderType { get; set; }
    }

    /// <summary>
    /// 条件面板块
    /// </summary>
    public class ConditionBlock : SurrounderBlock
    {
        public const string Type = "condition";

        public ConditionBlock(Type entityType)
            : base(entityType, Type)
        {
            this.BlockType = BlockType.Detail;
        }

        public ConditionBlock()
        {
            this.SurrounderType = Type;
            this.BlockType = BlockType.Detail;
        }
    }

    /// <summary>
    /// 导航面板块
    /// </summary>
    public class NavigationBlock : SurrounderBlock
    {
        public const string Type = "navigation";

        public NavigationBlock(Type entityType)
            : base(entityType, Type)
        {
            this.BlockType = BlockType.Detail;
        }

        public NavigationBlock()
        {
            this.SurrounderType = Type;
            this.BlockType = BlockType.Detail;
        }
    }
}
