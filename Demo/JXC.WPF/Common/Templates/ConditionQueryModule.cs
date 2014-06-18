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
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Reflection;
using JXC.Commands;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;

namespace JXC.WPF.Templates
{
    /// <summary>
    /// 通用的条件查询单据模块模板
    /// </summary>
    public class ConditionQueryModule : ModuleBase
    {
        /// <summary>
        /// 生成条件查询和主体模块
        /// </summary>
        /// <returns></returns>
        protected override AggtBlocks DefineBlocks()
        {
            var entityType = this.EntityType;

            AggtBlocks result = new AggtBlocks
            {
                MainBlock = new Block(entityType),
                Layout = new LayoutMeta(typeof(ConditionQueryLayout))
            };

            var conAttri = entityType.GetSingleAttribute<ConditionQueryTypeAttribute>();
            if (conAttri != null)
            {
                result.Surrounders.Add(new AggtBlocks
                {
                    MainBlock = new ConditionBlock()
                    {
                        EntityType = conAttri.QueryType,
                    },
                    Layout = new LayoutMeta(typeof(HorizentalConditionLayout))
                });
            }

            return result;
        }

        protected override void OnUIGenerated(ControlResult ui)
        {
            base.OnUIGenerated(ui);

            var listView = this.ListView;

            //默认发起一次查询。
            var queryView = listView.ConditionQueryView;
            if (queryView != null) queryView.TryExecuteQuery();

            listView.IsReadOnly = ReadOnlyStatus.ReadOnly;

            //列表双击时，弹出查看窗口
            listView.Control.MouseDoubleClick += (o, e) =>
            {
                var cmd = listView.Commands.Find<ShowBill>();
                if (cmd != null) { cmd.TryExecute(); }
            };
        }
    }
}