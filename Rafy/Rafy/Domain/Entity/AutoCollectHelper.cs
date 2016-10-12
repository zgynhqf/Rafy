/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130429
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130429 11:01
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// 组合实体自动进行汇总数据的帮助类
    /// </summary>
    public static class AutoCollectHelper
    {
        /// <summary>
        /// 当实体的某个属性变更时，自动向父级实体的指定属性汇总。
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="changedPropertyArgs">实体属性变更时的参数</param>
        /// <param name="toTreeParent">
        /// 如果实体是树型对象，那么这个参数表明是否需要把值汇总到树中的父对象的同一个属性值上。
        /// </param>
        /// <param name="toParentProperty">
        /// 指示需要把值汇总到组合父对象的哪一个属性上。这个属性只支持直接父对象，不支持多级父对象。
        /// </param>
        /// <threadsafety static="true" instance="true"/>
        public static void AutoCollectAsChanged(
            Entity entity, ManagedPropertyChangedEventArgs changedPropertyArgs,
            bool toTreeParent = true,
            IManagedProperty toParentProperty = null
            )
        {
            if (toTreeParent)
            {
                var treeEntity = entity as ITreeEntity;
                if (treeEntity.IsTreeParentLoaded && treeEntity.TreeParent != null)
                {
                    CalculateCollectValue(treeEntity.TreeParent, changedPropertyArgs.Property, changedPropertyArgs);
                    //如果已经向树型父汇总，则不向父对象汇总，直接返回
                    return;
                }
            }

            if (toParentProperty != null)
            {
                var parent = (entity as IEntity).FindParentEntity();
                if (parent != null)
                {
                    CalculateCollectValue(parent, toParentProperty, changedPropertyArgs);
                }
            }
        }

        private static void CalculateCollectValue(Entity entity, IManagedProperty property, ManagedPropertyChangedEventArgs args)
        {
            var distance = Convert.ToDouble(args.NewValue) - Convert.ToDouble(args.OldValue);
            var oldValue = Convert.ToDouble(entity.GetProperty(property));
            entity.SetProperty(property, oldValue + distance);
        }
    }
}
