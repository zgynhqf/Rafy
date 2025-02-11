using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.MetaModel.XmlConfig;
using Rafy.UI;

namespace Rafy.Customization
{
    [ChildEntity]
    public partial class ViewConfigurationCommand : IntEntity
    {
        public static readonly Property<int> ViewConfigurationModelIdProperty =
            P<ViewConfigurationCommand>.Register(e => e.ViewConfigurationModelId);
        public int ViewConfigurationModelId
        {
            get { return this.GetProperty(ViewConfigurationModelIdProperty); }
            set { this.SetProperty(ViewConfigurationModelIdProperty, value); }
        }
        public static readonly RefEntityProperty<ViewConfigurationModel> ViewConfigurationModelProperty =
            P<ViewConfigurationCommand>.RegisterRef(e => e.ViewConfigurationModel, ViewConfigurationModelIdProperty, ReferenceType.Parent);
        public ViewConfigurationModel ViewConfigurationModel
        {
            get { return this.GetRefEntity(ViewConfigurationModelProperty); }
            set { this.SetRefEntity(ViewConfigurationModelProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<ViewConfigurationCommand>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> LabelProperty = P<ViewConfigurationCommand>.Register(e => e.Label);
        public string Label
        {
            get { return this.GetProperty(LabelProperty); }
            set { this.SetProperty(LabelProperty, value); }
        }

        public static readonly Property<bool> IsVisibleProperty = P<ViewConfigurationCommand>.Register(e => e.IsVisible);
        public bool IsVisible
        {
            get { return this.GetProperty(IsVisibleProperty); }
            set { this.SetProperty(IsVisibleProperty, value); }
        }
    }

    public partial class ViewConfigurationCommandList : InheritableEntityList
    {
        internal void Add(int parentId, IEnumerable<WebCommand> webCommands)
        {
            foreach (var jsCmd in webCommands)
            {
                if (jsCmd.Name != WebCommandNames.CustomizeUI)
                {
                    var m = new ViewConfigurationCommand();

                    m.Id = RafyEnvironment.NewLocalId();
                    m.ViewConfigurationModelId = parentId;
                    m.Name = jsCmd.Name;
                    m.Label = jsCmd.Label;
                    m.IsVisible = jsCmd.IsVisible;

                    this.Add(m);
                }
            }
        }

        internal void Add(int parentId, IEnumerable<WPFCommand> wpfCommands)
        {
            foreach (var cmd in wpfCommands)
            {
                if (cmd.RuntimeType != CustomizationPlugin.CustomizeUICommand)
                {
                    var m = new ViewConfigurationCommand();

                    m.Id = RafyEnvironment.NewLocalId();
                    m.ViewConfigurationModelId = parentId;
                    m.Name = cmd.Name;
                    m.Label = cmd.Label;
                    m.IsVisible = cmd.IsVisible;

                    this.Add(m);
                }
            }
        }
    }

    public partial class ViewConfigurationCommandRepository : EntityRepository
    {
        protected ViewConfigurationCommandRepository()
        {
            this.DataPortalLocation = Rafy.DataPortal.DataPortalLocation.Local;
        }
    }

    [DataProviderFor(typeof(ViewConfigurationCommandRepository))]
    public partial class ViewConfigurationCommandDataProvider : RdbDataProvider
    {
        [RepositoryQuery]
        public override object GetByParentId(object pId, PagingInfo paging = null, LoadOptions loadOptions = null)
        {
            int parentId = (int)pId;

            var list = new ViewConfigurationCommandList();

            var evm = ViewConfigurationModel.GetEVMByParentId(parentId);

            if (UIEnvironment.IsWebUI)
            {
                list.Add(parentId, evm.AsWebView().Commands);
            }
            else
            {
                list.Add(parentId, evm.AsWPFView().Commands);
            }

            return list;
        }
    }
}