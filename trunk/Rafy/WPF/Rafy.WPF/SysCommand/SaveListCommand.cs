/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111215
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111215
 * 
*******************************************************/

using System;
using System.Linq;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;

namespace Rafy.WPF.Command
{
    /// <summary>
    /// 保存整个列表
    /// </summary>
    [Command(ImageName = "Save.bmp", Label = "保存", GroupType = CommandGroupType.Edit)]
    public class SaveListCommand : BaseSaveCommand
    {
        public override void Execute(LogicalView view)
        {
            var listView = view.CastTo<ListLogicalView>();

            var list = listView.Data;
            if (list.IsDirty)
            {
                //检测条件
                if (!this.ValidateData(listView)) return;

                //如果元数据中指定了列表保存的服务，则尝试使用这个服务。
                var svcType = view.Meta.EntityMeta.GetSaveListServiceType();
                if (svcType != null)
                {
                    var svc = Activator.CreateInstance(svcType) as ISaveListService;
                    if (svc == null) throw new InvalidProgramException(string.Format("{0} 服务应该实现 ISaveListService 接口。", svcType));
                    svc.EntityList = list;
                    svc.Invoke();
                    if (!svc.Result)
                    {
                        App.MessageBox.Show(svc.Result.Message, "保存出错".Translate());
                        return;
                    }
                    else
                    {
                        listView.Data = svc.EntityList;
                    }
                }
                else
                {
                    RF.Save(list);
                }

                this.OnSaveSuccessed();
                this.RefreshAll(listView);
            }
        }

        /// <summary>
        /// 验证所有的数据。返回是否成功。
        /// </summary>
        /// <param name="listView"></param>
        /// <returns></returns>
        protected virtual bool ValidateData(ListLogicalView listView)
        {
            var list = listView.Data;

            if (listView.IsShowingTree)
            {
                var error = list.EachNode(item =>
                {
                    var brokenRules = item.Validate();
                    if (brokenRules.Count > 0)
                    {
                        var msg = string.Format("节点索引为 {0} 的数据验证不通过：\r\n{1}".Translate(), item.TreeIndex, brokenRules.ToString());
                        App.MessageBox.Show(msg, "保存出错".Translate());
                        return true;
                    }
                    return false;
                });
                return error == null;
            }
            else
            {
                for (int i = 0, c = list.Count; i < c; i++)
                {
                    var item = list[i];
                    var brokenRules = item.Validate();
                    if (brokenRules.Count > 0)
                    {
                        var msg = string.Format("第 {0} 行数据验证不通过：\r\n{1}".Translate(), i + 1, brokenRules.ToString());
                        App.MessageBox.Show(msg, "保存出错".Translate());
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 列表保存后，新加的实体的 Id 已经变化，需要重新刷新表格控件。
        /// </summary>
        /// <param name="view"></param>
        private void RefreshAll(LogicalView view)
        {
            view.RefreshControl();

            var current = view.Current;
            if (current != null)
            {
                foreach (var childView in view.ChildrenViews)
                {
                    this.RefreshAll(childView);
                }
            }
        }
    }
}