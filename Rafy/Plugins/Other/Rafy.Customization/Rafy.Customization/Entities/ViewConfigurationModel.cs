using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.Web;
using Rafy.MetaModel.XmlConfig;
using System.Security.Permissions;
using System.Runtime.Serialization;
using Rafy.Domain.ORM;
using Rafy.UI;

namespace Rafy.Customization
{
    [Serializable]
    [RootEntity]
    public partial class ViewConfigurationModel : IntEntity
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

        public static readonly Property<string> ViewNameProperty = P<ViewConfigurationModel>.Register(e => e.ViewName, "基础界面");
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
            if (evm is WebEntityViewMeta)
            {
                this.PageSize = evm.AsWebView().PageSize;
            }
            this.EntityType = ClientEntities.GetClientName(evm.EntityType);
            if (!string.IsNullOrEmpty(evm.ExtendView)) this.ViewName = evm.ExtendView;
        }

        internal void SaveToXml()
        {
            var io = GetInputOutput(this.Id);
            if (io.InputView == null) return;

            //先从 xml 文件中读取当前的配置。
            var blockConfig = UIModel.XmlConfigMgr.GetBlockConfig(io.OutputBlockConfigKey);
            if (blockConfig == null)
            {
                blockConfig = new BlockConfig
                {
                    Key = io.OutputBlockConfigKey
                };
            }

            //把所有所有变更的差异都存储到 blockConfig 中。
            this.SerializeProperties(io, blockConfig);
            this.SerializeEntityProperties(io, blockConfig);
            this.SerializeCommands(io, blockConfig);

            UIModel.XmlConfigMgr.Save(blockConfig);
        }

        private void SerializeProperties(ConfigInputOutput io, BlockConfig blockConfig)
        {
            if (io.InputView is WebEntityViewMeta)
            {
                blockConfig.PageSize = (this.PageSize != io.InputView.AsWebView().PageSize) ? (int?)this.PageSize : null;
            }
            var hasGroup = !string.IsNullOrEmpty(this.GroupBy);
            if (io.InputView.GroupBy == null)
            {
                blockConfig.GroupBy = hasGroup ? this.GroupBy : null;
            }
            else
            {
                if (hasGroup)
                {
                    blockConfig.GroupBy = io.InputView.GroupBy.Name != this.GroupBy ? this.GroupBy : null;
                }
                else
                {
                    blockConfig.GroupBy = BlockConfig.NullString;
                }
            }
        }

        private void SerializeEntityProperties(ConfigInputOutput io, BlockConfig blockConfig)
        {
            var properties = blockConfig.EntityProperties;
            foreach (var propertyChanged in this.ViewConfigurationPropertyList)
            {
                //查找或添加一个新的属性配置项。
                bool exsits = true;
                var propertyCfg = properties.FirstOrDefault(p => p.Name == propertyChanged.Name);
                if (propertyCfg == null)
                {
                    propertyCfg = new BlockPropertyConfig() { Name = propertyChanged.Name };
                    exsits = false;
                }

                //重算所有属性值。
                var propertyVM = io.InputView.Property(propertyChanged.Name);
                var siw = (ShowInWhere)propertyChanged.ShowInWhere;
                propertyCfg.ShowInWhere = siw != propertyVM.ShowInWhere ? (ShowInWhere?)siw : null;
                propertyCfg.OrderNo = propertyChanged.OrderNo != propertyVM.OrderNo ? (double?)propertyChanged.OrderNo : null;
                propertyCfg.Label = propertyChanged.Label != propertyVM.Label ? propertyChanged.Label : null;

                //如果配置项有用，则加入到列表中，否则应该从列表中移除。
                if (propertyCfg.IsChanged())
                {
                    if (!exsits)
                    {
                        properties.Add(propertyCfg);
                    }
                }
                else
                {
                    if (exsits)
                    {
                        properties.Remove(propertyCfg);
                    }
                }
            }
        }

        private void SerializeCommands(ConfigInputOutput io, BlockConfig blockConfig)
        {
            bool isWeb = UIEnvironment.IsWebUI;

            foreach (var cmdChanged in this.ViewConfigurationCommandList)
            {
                //查找或添加一个新的属性配置项。
                bool exsits = true;
                var cmdCfg = blockConfig.Commands.FirstOrDefault(p => p.Name == cmdChanged.Name);
                if (cmdCfg == null)
                {
                    cmdCfg = new BlockCommandConfig() { Name = cmdChanged.Name };
                    exsits = false;
                }

                //重算所有属性值。
                if (isWeb)
                {
                    var cmdVM = io.InputView.AsWebView().Commands.Find(cmdChanged.Name);
                    cmdCfg.Label = cmdChanged.Label != cmdVM.Label ? cmdChanged.Label : null;
                    cmdCfg.IsVisible = cmdChanged.IsVisible != cmdVM.IsVisible ? (bool?)cmdChanged.IsVisible : null;
                }
                else
                {
                    var cmdVM = io.InputView.AsWPFView().Commands.Find(cmdChanged.Name);
                    cmdCfg.Label = cmdChanged.Label != cmdVM.Label ? cmdChanged.Label : null;
                    cmdCfg.IsVisible = cmdChanged.IsVisible != cmdVM.IsVisible ? (bool?)cmdChanged.IsVisible : null;
                }

                //如果配置项有用，则加入到列表中，否则应该从列表中移除。
                if (cmdCfg.IsChanged())
                {
                    if (!exsits)
                    {
                        blockConfig.Commands.Add(cmdCfg);
                    }
                }
                else
                {
                    if (exsits)
                    {
                        blockConfig.Commands.Remove(cmdCfg);
                    }
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

            if (!UIEnvironment.BranchProvider.HasBranch)
            {
                res.OutputBlockConfigKey.Type = BlockConfigType.Config;

                //ConfigDefaultView/ConfigExtendView
                res.InputView = UIModel.Views.Create(viewId.EntityType, viewId.ViewName, null);
            }
            else
            {
                res.OutputBlockConfigKey.Type = BlockConfigType.Customization;

                //CustomizeDefaultView，CustomizeExtendView
                res.InputView = UIModel.Views.Create(viewId.EntityType, viewId.ViewName, BlockConfigType.Config);
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
        /// 尝试为 evm 生成一个运行时的 Id，以方便这个没有数据库的类也能运行在 Rafy 界面框架上。
        /// </summary>
        /// <param name="evm"></param>
        /// <returns></returns>
        private static int GetRuntimeUniqueId(EntityViewMeta evm)
        {
            var viewName = evm.ExtendView;
            //if (string.IsNullOrEmpty(viewName)) return evm.EntityMeta.Id;

            var entityType = evm.EntityType;

            lock (_viewIds)
            {
                var res = _viewIds.FirstOrDefault(i => i.EntityType == entityType && i.ViewName == viewName);
                if (res == null)
                {
                    res = new EVMUniqueId
                    {
                        EntityType = entityType,
                        ViewName = viewName,
                        Id = RafyEnvironment.NewLocalId()
                    };
                    _viewIds.Add(res);
                }
                return res.Id;
            }
        }

        internal static EVMUniqueId TryGetViewUniqueId(int id)
        {
            return _viewIds.FirstOrDefault(i => i.Id == id);
        }

        internal static EntityViewMeta GetEVMByParentId(int parentId)
        {
            EntityViewMeta evm = null;

            var dest = UIEnvironment.BranchProvider.HasBranch ? BlockConfigType.Customization : BlockConfigType.Config;

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

    public partial class ViewConfigurationModelList : EntityList { }

    public partial class ViewConfigurationModelRepository : EntityRepository
    {
        protected ViewConfigurationModelRepository()
        {
            this.DataPortalLocation = Rafy.DataPortal.DataPortalLocation.Local;
        }

        [RepositoryQuery]
        public virtual ViewConfigurationModelList GetByEntity(Type entityType, string viewName)
        {
            var list = this.NewList();

            var evm = UIModel.Views.Create(entityType, viewName);

            var m = new ViewConfigurationModel();
            list.Add(m);
            m.Read(evm);
            (m as IDirtyAware).MarkSaved();

            return list;
        }

        /// <summary>
        /// 导航面板查询
        /// </summary>
        /// <param name="criteria"></param>
        [RepositoryQuery]
        public virtual ViewConfigurationModel GetBy(ViewConfigurationModelNameCriteria criteria)
        {
            Type entityType = ClientEntities.Find(criteria.EntityType).EntityType;

            var evm = UIModel.Views.Create(entityType, criteria.ViewName);

            var m = new ViewConfigurationModel();
            m.Read(evm);
            (m as IDirtyAware).MarkSaved();

            return m;
        }
    }

    [DataProviderFor(typeof(ViewConfigurationModelRepository))]
    public partial class ViewConfigurationModelDataProvider : RdbDataProvider
    {
        public ViewConfigurationModelDataProvider()
        {
            this.DataSaver = new ViewConfigurationModelSaver();
        }

        private class ViewConfigurationModelSaver : RdbDataSaver
        {
            protected override void Submit(SubmitArgs e)
            {
                if (e.Action == SubmitAction.Update || e.Action == SubmitAction.ChildrenOnly)
                {
                    (e.Entity as ViewConfigurationModel).SaveToXml();
                }
                else
                {
                    base.Submit(e);
                }
            }
        }
    }

    [QueryEntity, Serializable]
    public partial class ViewConfigurationModelNameCriteria : Criteria
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
}