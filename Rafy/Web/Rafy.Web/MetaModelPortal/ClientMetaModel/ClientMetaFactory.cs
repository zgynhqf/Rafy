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
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace Rafy.Web.ClientMetaModel
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

        internal ClientEntityViewMeta ConvertToClientMeta(WebEntityViewMeta evm, bool? isDetail = null)
        {
            var c = new ClientEntityViewMeta();
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
            var mainBlock = aggt.MainBlock;
            meta.mainBlock = ConvertToClientMeta(mainBlock.ViewMeta.AsWebView(), mainBlock.BlockType == BlockType.Detail);
            if (mainBlock is ChildBlock)
            {
                meta.mainBlock.label = (mainBlock as ChildBlock).Label;
            }
            meta.childProperty = childPropertyName;
            meta.layoutClass = aggt.Layout.Class;

            //surrounders
            foreach (var surAggt in aggt.Surrounders)
            {
                var surBlock = surAggt.MainBlock as SurrounderBlock;
                var surroundMeta = new SurrounderMeta
                {
                    surrounderType = surBlock.SurrounderType
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

        private void ConvertToClientMeta(WebEntityViewMeta evm, ClientEntityViewMeta clientMeta, bool? isDetail)
        {
            clientMeta.model = ClientEntities.GetClientName(evm.EntityType);
            clientMeta.viewName = evm.ExtendView;
            clientMeta.label = evm.Label;

            if (isDetail.GetValueOrDefault(this.Option.isDetail))
            {
                clientMeta.formConfig = CreateFormConfig(evm);
            }
            else
            {
                clientMeta.gridConfig = CreateGridConfig(evm);

                if (evm.EntityMeta.IsTreeEntity)
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

        private GridConfig CreateGridConfig(WebEntityViewMeta evm)
        {
            GridConfig grid = new GridConfig();

            var isTree = evm.EntityMeta.IsTreeEntity;

            var showInWhere = this.Option.isLookup ? ShowInWhere.DropDown : ShowInWhere.List;

            //使用list里面的属性生成每一列
            foreach (WebEntityPropertyViewMeta property in evm.OrderedEntityProperties())
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
                        column.dataIndex = property.Name;

                        if (canEdit) { column.editor = ServerTypeHelper.GetTypeEditor(property); }
                    }
                    else
                    {
                        column.dataIndex = EntityModelGenerator.LabeledRefProperty(property.Name);

                        if (canEdit) { column.editor = ServerTypeHelper.CreateComboList(property); }
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

        private FormConfig CreateFormConfig(WebEntityViewMeta evm)
        {
            FormConfig form = new FormConfig();

            //使用list里面的属性生成每一列
            foreach (WebEntityPropertyViewMeta property in evm.OrderedEntityProperties())
            {
                if (property.CanShowIn(ShowInWhere.Detail))
                {
                    bool isReadonly = this.Option.isReadonly || property.IsReadonly || evm.NotAllowEdit;

                    FieldConfig field = null;

                    //对于引用属性需要分开来特殊处理
                    if (!property.IsReference)
                    {
                        field = ServerTypeHelper.GetTypeEditor(property);
                        field.name = property.Name;
                    }
                    else
                    {
                        field = ServerTypeHelper.CreateComboList(property);
                        field.name = EntityModelGenerator.LabeledRefProperty(property.Name);
                    }

                    field.fieldLabel = property.Label;
                    field.anchor = "100%";
                    field.isReadonly = isReadonly;
                    if (property.VisibilityIndicator.IsDynamic)
                    {
                        field.visibilityIndicator = property.VisibilityIndicator.Property.Name;
                    }

                    form.items.Add(field);
                }
            }

            this.AddCommands(evm, form.tbar);

            return form;
        }

        private void AddCommands(WebEntityViewMeta evm, IList<ToolbarItem> toolbar)
        {
            //当不需要忽略命令，或者使用了扩展视图时，都需要加入命令列表。
            if (!this.Option.ignoreCommands || !string.IsNullOrEmpty(this.Option.viewName))
            {
                IEnumerable<WebCommand> commands = evm.Commands;

                //当使用了扩展视图时，只需要显示定制 UI 按钮就行了。
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
                    foreach (var kv in jsCmd.GetExtendedProperties())
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