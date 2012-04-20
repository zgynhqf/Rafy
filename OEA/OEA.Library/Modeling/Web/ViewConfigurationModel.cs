using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.Web;
using OEA.MetaModel.XmlConfig;
using OEA.MetaModel.XmlConfig.Web;

namespace OEA
{
    [Serializable]
    [RootEntity]
    public class ViewConfigurationModel : Entity
    {
        /// <summary>
        /// 实体的客户端名称
        /// </summary>
        public static readonly Property<string> EntityTypeProperty = P<ViewConfigurationModel>.Register(e => e.EntityType);
        public string EntityType
        {
            get { return this.GetProperty(EntityTypeProperty); }
            set { this.SetProperty(EntityTypeProperty, value); }
        }

        public static readonly Property<string> ViewNameProperty = P<ViewConfigurationModel>.Register(e => e.ViewName, "全局界面");
        public string ViewName
        {
            get { return this.GetProperty(ViewNameProperty); }
            set { this.SetProperty(ViewNameProperty, value); }
        }

        public static readonly Property<string> GroupByProperty = P<ViewConfigurationModel>.Register(e => e.GroupBy);
        public string GroupBy
        {
            get { return this.GetProperty(GroupByProperty); }
            set { this.SetProperty(GroupByProperty, value); }
        }

        public static readonly Property<int> PageSizeProperty = P<ViewConfigurationModel>.Register(e => e.PageSize);
        public int PageSize
        {
            get { return this.GetProperty(PageSizeProperty); }
            set { this.SetProperty(PageSizeProperty, value); }
        }

        public static readonly ListProperty<ViewConfigurationPropertyList> ViewConfigurationPropertyListProperty = P<ViewConfigurationModel>.RegisterList(e => e.ViewConfigurationPropertyList);
        public ViewConfigurationPropertyList ViewConfigurationPropertyList
        {
            get { return this.GetLazyList(ViewConfigurationPropertyListProperty); }
        }

        public static readonly ListProperty<ViewConfigurationCommandList> ViewConfigurationCommandListProperty = P<ViewConfigurationModel>.RegisterList(e => e.ViewConfigurationCommandList);
        public ViewConfigurationCommandList ViewConfigurationCommandList
        {
            get { return this.GetLazyList(ViewConfigurationCommandListProperty); }
        }

        #region 保存为 Xml

        internal void Read(EntityViewMeta evm)
        {
            this.Id = GetRuntimeUniqueId(evm);
            if (evm.GroupBy != null) this.GroupBy = evm.GroupBy.Name;
            this.PageSize = evm.PageSize;
            this.EntityType = ClientEntityConverter.ToClientName(evm.EntityType);
            if (!string.IsNullOrEmpty(evm.ExtendView)) this.ViewName = evm.ExtendView;
        }

        protected override void OnUpdate()
        {
            var io = GetInputOutput(this.Id);
            if (io.InputView == null) return;

            var blockConfig = new BlockConfig
            {
                Key = io.OutputBlockConfigKey
            };

            if (this.PageSize != io.InputView.PageSize) { blockConfig.PageSize = this.PageSize; }
            var hasGroup = !string.IsNullOrEmpty(this.GroupBy);
            if (io.InputView.GroupBy == null)
            {
                if (hasGroup) { blockConfig.GroupBy = this.GroupBy; }
            }
            else
            {
                if (hasGroup)
                {
                    if (io.InputView.GroupBy.Name != this.GroupBy)
                    {
                        blockConfig.GroupBy = this.GroupBy;
                    }
                }
                else
                {
                    blockConfig.GroupBy = BlockConfig.NullString;
                }
            }

            this.SerializeEntityProperties(io, blockConfig);
            this.SerializeCommands(io, blockConfig);

            UIModel.XmlConfigMgr.Save(blockConfig);
        }

        private void SerializeEntityProperties(ConfigInputOutput io, BlockConfig blockConfig)
        {
            var properties = blockConfig.EntityProperties;
            foreach (ViewConfigurationProperty item in this.ViewConfigurationPropertyList)
            {
                bool changed = false;
                var propertyDiff = new BlockPropertyConfig() { Name = item.Name };
                var property = io.InputView.Property(item.Name);

                var siw = (ShowInWhere)item.ShowInWhere;
                if (siw != property.ShowInWhere)
                {
                    changed = true;
                    propertyDiff.ShowInWhere = siw;
                }
                if (item.OrderNo != property.OrderNo)
                {
                    changed = true;
                    propertyDiff.OrderNo = item.OrderNo;
                }
                if (item.Label != property.Label && item.Label != property.Name)
                {
                    changed = true;
                    propertyDiff.Label = item.Label;
                }

                if (changed) { properties.Add(propertyDiff); }
            }
        }

        private void SerializeCommands(ConfigInputOutput io, BlockConfig blockConfig)
        {
            if (OEAEnvironment.IsWeb)
            {
                foreach (ViewConfigurationCommand item in this.ViewConfigurationCommandList)
                {
                    bool changed = false;
                    var commandDiff = new BlockCommandConfig() { Name = item.Name };
                    var cmd = io.InputView.WebCommands.Find(item.Name);

                    if (item.Label != cmd.Label)
                    {
                        changed = true;
                        commandDiff.Label = item.Label;
                    }
                    if (item.IsVisible != cmd.IsVisible)
                    {
                        changed = true;
                        commandDiff.IsVisible = item.IsVisible;
                    }

                    if (changed) { blockConfig.Commands.Add(commandDiff); }
                }
            }
            else
            {
                foreach (ViewConfigurationCommand item in this.ViewConfigurationCommandList)
                {
                    bool changed = false;
                    var commandDiff = new BlockCommandConfig() { Name = item.Name };
                    var cmd = io.InputView.WPFCommands.Find(item.Name);

                    if (item.Label != cmd.Label)
                    {
                        changed = true;
                        commandDiff.Label = item.Label;
                    }
                    if (item.IsVisible != cmd.IsVisible)
                    {
                        changed = true;
                        commandDiff.IsVisible = item.IsVisible;
                    }

                    if (changed) { blockConfig.Commands.Add(commandDiff); }
                }
            }
        }

        private static ConfigInputOutput GetInputOutput(int id)
        {
            /*********************** 代码块解释 *********************************
            *   此文档说明四种配置模式下的：输入视图及输出视图，以及相应的方法：
            *
            *   ConfigDefaultView:
            *       输入：通过某类型代码反射生成的默认视图。
            *           GetDefaultView(needConfig: false, needCustomization: false)
            *       输出：主干版本中某类型的默认视图。
            *           GetDefaultView(needConfig: true, needCustomization: false)
            *
            *   ConfigExtendView:
            *       输入：主干版本中某类型的默认视图。
            *           GetDefaultView(needConfig: true, needCustomization: false)
            *       输出：主干版本中某类型的自定义扩展视图。
            *           GetExtendView(valueName, needConfig: true, needCustomization: false)
            *
            *   CustomizeDefaultView:
            *       输入：主干版本中某类型的默认视图。
            *           GetDefaultView(needConfig: true, needCustomization: false)
            *       输出：分支版本中某类型的默认视图。
            *           GetDefaultView(needConfig: true, needCustomization: true)
            *
            *   CustomizeExtendView:
            *       输入：主干版本中某类型的自定义扩展视图。
            *           GetExtendView(valueName, needConfig: true, needCustomization: false)
            *       输出：分支版本中某类型的自定义扩展视图。
            *           GetExtendView(valueName, needConfig: true, needCustomization: true)
            **********************************************************************/

            var viewId = TryGetViewUniqueId(id);
            var res = new ConfigInputOutput()
            {
                OutputBlockConfigKey = new BlockConfigKey()
                {
                    EntityType = viewId.EntityType,
                    ExtendView = viewId.ViewName
                }
            };

            if (!OEAEnvironment.CustomerProvider.IsCustomizing)
            {
                res.OutputBlockConfigKey.Type = BlockConfigType.Config;

                //ConfigDefaultView
                if (string.IsNullOrEmpty(viewId.ViewName))
                {
                    res.InputView = UIModel.Views.CreateDefaultView(viewId.EntityType, null);
                }
                //ConfigExtendView
                else
                {
                    res.InputView = UIModel.Views.CreateDefaultView(viewId.EntityType, BlockConfigType.Config);
                }

                ////ConfigDefaultView
                //if (extendViewId == null)
                //{
                //    var entityMeta = CommonModel.Entities.Find(id);
                //    if (entityMeta != null)
                //    {
                //        res.InputView = WebModel.Views.GetDefaultView(entityMeta.EntityType, null);
                //    }
                //}
                ////ConfigExtendView
                //else
                //{
                //    res.InputView = WebModel.Views.GetDefaultView(extendViewId.EntityType, BlockConfigType.Config);
                //}
            }
            else
            {
                res.OutputBlockConfigKey.Type = BlockConfigType.Customization;

                //CustomizeDefaultView，CustomizeExtendView
                res.InputView = UIModel.Views.Create(viewId.EntityType, viewId.ViewName, BlockConfigType.Config);

                ////CustomizeDefaultView
                //if (extendViewId == null)
                //{
                //    res.InputView = WebModel.Views.GetDefaultView(extendViewId.EntityType, BlockConfigType.Config);
                //}
                ////CustomizeExtendView
                //else
                //{
                //    res.InputView = WebModel.Views.GetExtendView(extendViewId.EntityType, extendViewId.ViewName, BlockConfigType.Config);
                //}
            }

            return res;
        }

        private class ConfigInputOutput
        {
            public EntityViewMeta InputView;

            public BlockConfigKey OutputBlockConfigKey;
        }

        #endregion

        #region GetRuntimeUniqueId

        private static readonly List<EVMUniqueId> _viewIds = new List<EVMUniqueId>();

        /// <summary>
        /// 尝试为 evm 生成一个运行时的 Id，以方便这个没有数据库的类也能运行在 OEA 界面框架上。
        /// </summary>
        /// <param name="evm"></param>
        /// <returns></returns>
        private static int GetRuntimeUniqueId(EntityViewMeta evm)
        {
            var viewName = evm.ExtendView;
            //if (string.IsNullOrEmpty(viewName)) return evm.EntityMeta.Id;

            var entityType = evm.EntityType;

            var res = _viewIds.FirstOrDefault(i => i.EntityType == entityType && i.ViewName == viewName);
            if (res == null)
            {
                lock (_viewIds)
                {
                    res = _viewIds.FirstOrDefault(i => i.EntityType == entityType && i.ViewName == viewName);
                    if (res == null)
                    {
                        res = new EVMUniqueId
                        {
                            EntityType = entityType,
                            ViewName = viewName,
                            Id = OEAEnvironment.NewLocalId()
                        };
                        _viewIds.Add(res);
                    }
                }
            }

            return res.Id;
        }

        internal static EVMUniqueId TryGetViewUniqueId(int id)
        {
            return _viewIds.FirstOrDefault(i => i.Id == id);
        }

        internal static EntityViewMeta GetEVMByParentId(int parentId)
        {
            EntityViewMeta evm = null;

            var dest = OEAEnvironment.CustomerProvider.IsCustomizing ? BlockConfigType.Customization : BlockConfigType.Config;

            var viewId = TryGetViewUniqueId(parentId);
            if (viewId != null)
            {
                evm = UIModel.Views.Create(viewId.EntityType, viewId.ViewName, dest);
            }
            //else
            //{
            //    var entityMeta = CommonModel.Entities.Find(parentId);
            //    if (entityMeta != null)
            //    {
            //        evm = WebModel.Views.GetDefaultView(entityMeta.EntityType, dest);
            //    }
            //}

            return evm;
        }

        internal class EVMUniqueId
        {
            public Type EntityType;
            public string ViewName;
            public int Id;
        }

        #endregion
    }

    [Serializable]
    public class ViewConfigurationModelList : EntityList
    {
        /// <summary>
        /// 导航面板查询
        /// </summary>
        /// <param name="criteria"></param>
        protected void QueryBy(ViewConfigurationModelNameCriteria criteria)
        {
            var viewName = criteria.ViewName;
            Type entityType = ClientEntityConverter.ToClientType(criteria.EntityType);

            var evm = UIModel.Views.Create(entityType, viewName);

            var m = this.AddNew().CastTo<ViewConfigurationModel>();
            m.Read(evm);
            m.MarkOld();
        }
    }

    public class ViewConfigurationModelRepository : EntityRepository
    {
        protected ViewConfigurationModelRepository() { }

        public ViewConfigurationModel GetByName(ViewConfigurationModelNameCriteria c)
        {
            return this.FetchFirstAs<ViewConfigurationModel>(c);
        }
    }

    [Criteria, Serializable]
    public class ViewConfigurationModelNameCriteria : Criteria
    {
        public static readonly Property<string> ClientEntityProperty = P<ViewConfigurationModelNameCriteria>.Register(e => e.EntityType);
        /// <summary>
        /// 实体的客户端名称
        /// </summary>
        public string EntityType
        {
            get { return this.GetProperty(ClientEntityProperty); }
            set { this.SetProperty(ClientEntityProperty, value); }
        }

        public static readonly Property<string> ViewNameProperty = P<ViewConfigurationModelNameCriteria>.Register(e => e.ViewName);
        public string ViewName
        {
            get { return this.GetProperty(ViewNameProperty); }
            set { this.SetProperty(ViewNameProperty, value); }
        }
    }

    internal class ViewConfigurationModelConfig : EntityConfig<ViewConfigurationModel>
    {
        protected override void ConfigView()
        {
            base.ConfigView();

            View.HasDelegate(ViewConfigurationModel.ViewNameProperty);//.HasLabel("界面配置信息");
            View.PageSize = 10000;

            View.Property(ViewConfigurationModel.EntityTypeProperty).HasLabel("实体").ShowIn(ShowInWhere.All).Readonly();
            View.Property(ViewConfigurationModel.ViewNameProperty).HasLabel("界面名称").ShowIn(ShowInWhere.All).Readonly();
            View.Property(ViewConfigurationModel.GroupByProperty).HasLabel("按属性分组").ShowIn(ShowInWhere.All);

            if (OEAEnvironment.IsWeb)
            {
                View.Property(ViewConfigurationModel.PageSizeProperty).HasLabel("分页条数").ShowIn(ShowInWhere.All);
                //使用自定义的聚合保存按钮。
                View.UseWebCommands("SaveViewConfig", "BackupViewConfig", "RestoreViewConfig", "OpenConfigFile")
                    .RemoveWebCommands(WebCommandNames.CommonCommands.ToArray());
            }
            else
            {
                //使用自定义的聚合保存按钮。
                View.ClearWPFCommands()
                    .UseWPFCommands(WPFCommandNames.SaveBill, WPFCommandNames.Cancel)
                    .UseWPFCommands(
                        "OEA.WPF.Command.BackupViewConfig",
                        "OEA.WPF.Command.RestoreViewConfig",
                        "OEA.WPF.Command.OpenConfigFile"
                        );
            }
        }
    }
}