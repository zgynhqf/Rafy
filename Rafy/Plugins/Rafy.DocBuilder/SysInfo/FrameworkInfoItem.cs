/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130110 11:45
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130110 11:45
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain.ORM;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;
using Rafy.MultiLanguages;

namespace Rafy.DevTools.SysInfo
{
    /// <summary>
    /// Rafy 系统信息
    /// </summary>
    [RootEntity, Serializable]
    public partial class FrameworkInfoItem : IntEntity
    {
        #region 构造函数

        public FrameworkInfoItem() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected FrameworkInfoItem(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        #endregion

        #region 子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> KeyProperty = P<FrameworkInfoItem>.Register(e => e.Key);
        public string Key
        {
            get { return this.GetProperty(KeyProperty); }
            set { this.SetProperty(KeyProperty, value); }
        }

        public static readonly Property<string> ValueProperty = P<FrameworkInfoItem>.Register(e => e.Value);
        public string Value
        {
            get { return this.GetProperty(ValueProperty); }
            set { this.SetProperty(ValueProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    [Serializable]
    public partial class FrameworkInfoItemList : EntityList
    {
        internal void CreateItems()
        {
            this.CountModules();

            this.CountEntities();

            this.CountDevLanguages();
        }

        #region 统计模块个数

        private void CountModules()
        {
            int sum = 0;
            foreach (var root in CommonModel.Modules.Roots)
            {
                CountModule(root, ref sum);
            }

            this.AddItem("模块个数", sum);
        }

        private static void CountModule(ModuleMeta module, ref int sum)
        {
            sum++;

            foreach (var child in module.Children)
            {
                CountModule(child, ref sum);
            }
        }

        #endregion

        private void CountEntities()
        {
            this.AddItem("实体元数据", CommonModel.Entities.Count());

            int tablesCount = 0;
            int viewsCount = 0;
            int fieldsCount = 0;
            foreach (var em in CommonModel.Entities)
            {
                if (em.TableMeta != null)
                {
                    if (em.TableMeta.IsMappingView)
                    {
                        viewsCount++;
                    }
                    else
                    {
                        tablesCount++;
                        fieldsCount += em.EntityProperties.Count(ep => ep.ColumnMeta != null);
                    }
                }
            }
            this.AddItem("视图查询类", viewsCount);
            this.AddItem("数据表", tablesCount);
            this.AddItem("数据字段数", fieldsCount);

            var mpCount = 0;
            foreach (var kv in ManagedPropertyRepository.Instance)
            {
                mpCount += kv.Value.GetCompiledProperties().Count();
            }
            this.AddItem("托管属性数", mpCount);
        }

        private void CountDevLanguages()
        {
            this.AddItem("语言项", RF.Concrete<DevLanguageItemRepository>().CountAll());
        }

        private void AddItem(string key, object value)
        {
            var item = new FrameworkInfoItem
            {
                Key = key,
                Value = value.ToString()
            };
            item.MarkUnchanged();

            this.Add(item);
        }
    }

    public partial class FrameworkInfoItemRepository : EntityRepository
    {
        protected FrameworkInfoItemRepository() { }

        public EntityList GetAllOnClient()
        {
            var list = new FrameworkInfoItemList();
            list.CreateItems();

            this.NotifyLoaded(list);

            return list;
        }
    }

    [DataProviderFor(typeof(FrameworkInfoItemRepository))]
    public partial class FrameworkInfoItemDataProvider : RepositoryDataProvider
    {
        public override EntityList GetAll(PagingInfo paging, EagerLoadOptions eagerLoad)
        {
            var list = new FrameworkInfoItemList();

            list.CreateItems();

            return list;
        }
    }

    internal class FrameworkInfoItemConfig : WPFViewConfig<FrameworkInfoItem>
    {
        protected override void ConfigView()
        {
            View.DomainName("Rafy 系统信息").HasDelegate(FrameworkInfoItem.KeyProperty);

            View.DisableEditing();

            View.UseCommands(
                typeof(GetServerInfoCommand),
                typeof(GetClientInfoCommand),
                typeof(LoadAllEntitiesCommand),
                typeof(CheckErrorRedundancyCommand)
                );

            using (View.OrderProperties())
            {
                View.Property(FrameworkInfoItem.KeyProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
                View.Property(FrameworkInfoItem.ValueProperty).HasLabel("值").ShowIn(ShowInWhere.All);
            }
        }
    }
}