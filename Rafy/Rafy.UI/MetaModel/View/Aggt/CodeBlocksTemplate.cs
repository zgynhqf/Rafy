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
using Rafy.ManagedProperty;
using Rafy.MetaModel.Attributes;

namespace Rafy.MetaModel.View
{
    /// <summary>
    /// 一个使用代码结构来生成模板结构的模板基类
    /// </summary>
    public class CodeBlocksTemplate : BlocksTemplate
    {
        protected override AggtBlocks DefineBlocks()
        {
            var entityType = this.EntityType;

            var em = CommonModel.Entities.Get(entityType);

            var mainBlock = new Block(entityType);

            return this.ReadCode(em, mainBlock);
        }

        protected AggtBlocks ReadCode(EntityMeta em, Block mainBlock, ReadCodeTemplateOptions op = null)
        {
            op = op ?? new ReadCodeTemplateOptions
            {
                ReadChildren = true,
                ReadQueryPanels = true
            };

            AggtBlocks result = mainBlock;

            if (op.ReadChildren)
            {
                foreach (var property in em.ChildrenProperties)
                {
                    var childBlock = new ChildBlock(property.ManagedProperty as IListProperty);
                    var childAggt = this.ReadCode(property.ChildType, childBlock);

                    result.Children.Add(childAggt);
                }
            }

            if (op.ReadQueryPanels) { this.ReadQueryPanels(em, result); }

            return result;
        }

        private void ReadQueryPanels(EntityMeta em, AggtBlocks result)
        {
            //导航实体的关系对象
            var queryTypes = em.EntityType.GetCustomAttributes(typeof(ConditionQueryTypeAttribute), false);
            foreach (ConditionQueryTypeAttribute conAttri in queryTypes)
            {
                var surBlock = new ConditionBlock
                {
                    EntityType = conAttri.QueryType,
                };
                var surEM = CommonModel.Entities.Get(surBlock.EntityType);
                var surAggt = this.ReadCode(surEM, surBlock);

                result.Surrounders.Add(surAggt);
            }

            queryTypes = em.EntityType.GetCustomAttributes(typeof(NavigationQueryTypeAttribute), false);
            foreach (NavigationQueryTypeAttribute naviAttri in queryTypes)
            {
                var surBlock = new NavigationBlock
                {
                    EntityType = naviAttri.QueryType,
                };
                var surEM = CommonModel.Entities.Get(surBlock.EntityType);
                var surAggt = this.ReadCode(surEM, surBlock);

                result.Surrounders.Add(surAggt);
            }
        }
    }

    public class ReadCodeTemplateOptions
    {
        /// <summary>
        /// 是否需要读取组合子
        /// </summary>
        public bool ReadChildren { get; set; }

        /// <summary>
        /// 是否需要读取查询面板。
        /// </summary>
        public bool ReadQueryPanels { get; set; }
    }
}