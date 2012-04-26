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
using OEA.MetaModel.Attributes;

namespace OEA.MetaModel.View
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

            return this.Read(em, mainBlock);
        }

        private AggtBlocks Read(EntityMeta em, Block mainBlock)
        {
            var result = new AggtBlocks
            {
                MainBlock = mainBlock
            };

            foreach (var property in em.ChildrenProperties)
            {
                var childBlock = new ChildBlock
                {
                    ChildrenPropertyName = property.Name,
                    EntityType = property.ChildType.EntityType
                };
                var childAggt = this.Read(property.ChildType, childBlock);

                result.Children.Add(childAggt);
            }

            //导航实体的关系对象
            var conAttri = em.EntityType.GetSingleAttribute<ConditionQueryTypeAttribute>();
            if (conAttri != null)
            {
                var surBlock = new SurrounderBlock
                {
                    EntityType = conAttri.QueryType,
                    SurrounderType = SurrounderType.Condition
                };
                var surEM = CommonModel.Entities.Get(surBlock.EntityType);
                var surAggt = this.Read(surEM, surBlock);

                result.Surrounders.Add(surAggt);
            }
            var naviAttri = em.EntityType.GetSingleAttribute<NavigationQueryTypeAttribute>();
            if (naviAttri != null)
            {
                var surBlock = new SurrounderBlock
                {
                    EntityType = naviAttri.QueryType,
                    SurrounderType = SurrounderType.Navigation
                };
                var surEM = CommonModel.Entities.Get(surBlock.EntityType);
                var surAggt = this.Read(surEM, surBlock);

                result.Surrounders.Add(surAggt);
            }

            return result;
        }
    }
}