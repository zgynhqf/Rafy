/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120416
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120416
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.Module.WPF;
using OEA.Module;

namespace JXC.WPF.Templates
{
    /// <summary>
    /// 通用的条件查询单据模块模板
    /// </summary>
    public class ConditionQueryModule : ModuleBase
    {
        /// <summary>
        /// 生成条件查询和主体查询
        /// </summary>
        /// <returns></returns>
        protected override AggtBlocks DefineBlocks()
        {
            var entityType = this.EntityType;

            AggtBlocks result = new AggtBlocks
            {
                MainBlock = new Block(entityType),
                Layout = new LayoutMeta(typeof(TraditionalLayoutMethod<ConditionQueryLayout>))
            };

            var conAttri = entityType.GetSingleAttribute<ConditionQueryTypeAttribute>();
            if (conAttri != null)
            {
                result.Surrounders.Add(new AggtBlocks
                {
                    MainBlock = new SurrounderBlock
                    {
                        EntityType = conAttri.QueryType,
                        SurrounderType = SurrounderType.Condition
                    },
                    Layout = new LayoutMeta(typeof(TraditionalLayoutMethod<HorizentalConditionLayout>))
                });
            }

            return result;
        }

        protected override void OnUIGenerated(ControlResult ui)
        {
            base.OnUIGenerated(ui);

            //默认发起一次查询。
            var queryView = this.ListView.CondtionQueryView;
            if (queryView != null) queryView.TryExecuteQuery();

            this.ListView.IsReadOnly = true;
        }
    }
}
