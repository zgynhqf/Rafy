/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120220
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120220
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.MetaModel;
using OEA.MetaModel.View;

namespace OEA.Web.ClientMetaModel
{
    /// <summary>
    /// 客户端元数据的生成类
    /// </summary>
    internal class ClientMetaFactory
    {
        public ClientMetaFactory()
        {
            this.Option = new ConvertOption();
        }

        public ConvertOption Option { get; private set; }

        internal ClientEntityMeta ConvertToClientMeta(EntityViewMeta evm, bool? isDetail = null)
        {
            var c = new ClientEntityMeta();
            ConvertToClientMeta(evm, c, isDetail);
            return c;
        }

        internal ClientAggtMeta ConvertToAggtMeta(AggtBlocks aggt, string childPropertyName = null)
        {
            var meta = new ClientAggtMeta();
            ConvertToAggtMeta(aggt, meta, childPropertyName);
            return meta;
        }

        #region ConvertToAggtMeta

        private void ConvertToAggtMeta(AggtBlocks aggt, ClientAggtMeta meta, string childPropertyName = null)
        {
            var evm = UIModel.Views.Create(aggt.MainBlock.EntityType, aggt.MainBlock.ExtendView);
            meta.mainBlock = ConvertToClientMeta(evm, aggt.MainBlock.BlockType == BlockType.Detail);
            if (aggt.MainBlock is ChildBlock)
            {
                meta.mainBlock.label = (aggt.MainBlock as ChildBlock).Label;
            }
            meta.childProperty = childPropertyName;
            meta.layoutClass = aggt.Layout.Class;

            //surrounders
            foreach (var surAggt in aggt.Surrounders)
            {
                var surBlock = surAggt.MainBlock as SurrounderBlock;
                var surroundMeta = new SurrounderMeta
                {
                    surrounderType = surBlock.SurrounderType.GetDescription()
                };
                ConvertToAggtMeta(surAggt, surroundMeta);
                meta.surrounders.Add(surroundMeta);
            }

            //children
            foreach (var childAggt in aggt.Children)
            {
                var childBlock = childAggt.MainBlock as ChildBlock;
                var child = ConvertToAggtMeta(childAggt, childBlock.ChildrenPropertyName);
                meta.children.Add(child);
            }
        }

        #endregion

        #region ClientEntityMeta

        private void ConvertToClientMeta(EntityViewMeta evm, ClientEntityMeta clientMeta, bool? isDetail)
        {
            var isTree = evm.EntityMeta.IsTreeEntity;

            clientMeta.model = ClientEntities.GetClientName(evm.EntityType);
            clientMeta.viewName = evm.ExtendView;
            clientMeta.isTree = isTree;

            if (isDetail.GetValueOrDefault(this.Option.isDetail))
            {
                clientMeta.formConfig = CreateFormConfig(evm);
            }
            else
            {
                clientMeta.gridConfig = CreateGridConfig(evm);

                if (isTree)
                {
                    clientMeta.storeConfig = new TreeStoreConfig();
                }
                else
                {
                    var groupBy = evm.GroupBy;
                    if (groupBy != null)
                    {
                        var n = groupBy.Name;
                        if (groupBy.IsReference)
                        {
                            n = EntityModelGenerator.LabeledRefProperty(n);
                        }
                        clientMeta.groupBy = n;
                    }

                    clientMeta.storeConfig = new StoreConfig
                    {
                        pageSize = evm.PageSize
                    };
                }
            }
        }

        private GridConfig CreateGridConfig(EntityViewMeta evm)
        {
            GridConfig grid = new GridConfig();

            var isTree = evm.EntityMeta.IsTreeEntity;

            var showInWhere = this.Option.isLookup ? ShowInWhere.DropDown : ShowInWhere.List;

            //使用list里面的属性生成每一列
            foreach (var property in evm.OrderedEntityProperties())
            {
                if (property.CanShowIn(showInWhere))
                {
                    bool canEdit = !this.Option.isReadonly && !evm.NotAllowEdit && !property.IsReadonly;

                    var column = new GridColumnConfig
                    {
                        header = property.Label,
                        dataIndex = property.Name,
                        //flex = 1,
                    };

                    if (evm.LockedProperties.Contains(property))
                    {
                        column.locked = true;
                    }

                    //对于引用属性需要分开来特殊处理
                    if (!property.IsReference)
                    {
                        var ct = ServerTypeHelper.GetServerType(property.PropertyMeta.Runtime.PropertyType);

                        column.dataIndex = property.Name;

                        if (canEdit) { column.editor = ServerTypeHelper.GetTypeEditor(ct); }
                    }
                    else
                    {
                        var ct = ServerTypeHelper.GetServerType(typeof(int));
                        ct.JSType = JavascriptType.Reference;

                        column.dataIndex = EntityModelGenerator.LabeledRefProperty(property.Name);

                        if (canEdit)
                        {
                            column.editor = ServerTypeHelper.GetTypeEditor(ct);

                            var model = ClientEntities.GetClientName(property.ReferenceViewInfo.RefType);
                            column.editor.SetProperty("model", model);

                            var title = property.ReferenceViewInfo.RefTypeDefaultView.TitleProperty;
                            if (title != null) { column.editor.SetProperty("displayField", title.Name); }
                        }
                    }

                    grid.columns.Add(column);
                }
            }

            if (isTree && grid.columns.Count > 0)
            {
                grid.columns[0].xtype = "treecolumn";
            }

            this.AddCommands(evm, grid.tbar);

            return grid;
        }

        private FormConfig CreateFormConfig(EntityViewMeta evm)
        {
            FormConfig form = new FormConfig();

            //使用list里面的属性生成每一列
            foreach (var property in evm.OrderedEntityProperties())
            {
                if (property.CanShowIn(ShowInWhere.Detail))
                {
                    //对于引用属性需要分开来特殊处理
                    if (!property.IsReference)
                    {
                        var ct = ServerTypeHelper.GetServerType(property.PropertyMeta.Runtime.PropertyType);

                        var field = ServerTypeHelper.GetTypeEditor(ct);
                        field.name = property.Name;
                        field.fieldLabel = property.Label;
                        field.anchor = "100%";
                        field.isReadonly = property.IsReadonly;

                        form.items.Add(field);
                    }
                    else
                    {
                        var ct = ServerTypeHelper.GetServerType(typeof(int));
                        ct.JSType = JavascriptType.Reference;

                        var field = ServerTypeHelper.GetTypeEditor(ct);
                        field.name = EntityModelGenerator.LabeledRefProperty(property.Name);
                        field.fieldLabel = property.Label;
                        field.anchor = "100%";
                        field.isReadonly = property.IsReadonly;

                        var model = ClientEntities.GetClientName(property.ReferenceViewInfo.RefType);
                        field.SetProperty("model", model);

                        var title = property.ReferenceViewInfo.RefTypeDefaultView.TitleProperty;
                        if (title != null) { field.SetProperty("displayField", title.Name); }

                        form.items.Add(field);
                    }
                }
            }

            this.AddCommands(evm, form.tbar);

            return form;
        }

        private void AddCommands(EntityViewMeta evm, IList<ToolbarItem> toolbar)
        {
            if (!this.Option.ignoreCommands || !string.IsNullOrEmpty(this.Option.viewName))
            {
                IEnumerable<WebCommand> commands = evm.WebCommands;
                if (this.Option.ignoreCommands)
                {
                    var customUI = commands.FirstOrDefault(c => c.Name == WebCommandNames.CustomizeUI);
                    if (customUI != null)
                    {
                        commands = new WebCommand[] { customUI };
                    }
                    else
                    {
                        commands = Enumerable.Empty<WebCommand>();
                    }
                }

                foreach (var jsCmd in commands)
                {
                    if (!jsCmd.IsVisible) continue;

                    var ti = new ToolbarItem
                    {
                        command = jsCmd.Name
                    };

                    if (jsCmd.LabelModified)
                    {
                        ti.text = jsCmd.Label;
                    }

                    //自定义属性都直接添加到 toolBarItem 的属性上。
                    foreach (var kv in jsCmd.GetAllCustomParams())
                    {
                        var name = kv.Key;
                        var value = kv.Value;

                        ti.SetProperty(name, value);
                    }

                    toolbar.Add(ti);
                }
            }
        }

        #endregion
    }

    internal class ConvertOption
    {
        public bool isReadonly;
        public bool ignoreCommands;
        public bool isDetail;
        public bool isLookup;
        public string viewName;
    }
}