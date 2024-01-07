/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130107 15:36
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130107 15:36
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace Rafy.DevTools.DbManagement
{
    /// <summary>
    /// 数据库配置项
    /// </summary>
    [RootEntity]
    public partial class DbSettingItem : IntEntity
    {
        #region 一般属性

        public static readonly Property<string> NameProperty = P<DbSettingItem>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        #endregion
    }

    public partial class DbSettingItemList : InheritableEntityList
    {
    }

    public partial class DbSettingItemRepository : EntityRepository
    {
        protected DbSettingItemRepository() { }
    }

    [DataProviderFor(typeof(DbSettingItemRepository))]
    public partial class DbSettingItemDataProvider : RdbDataProvider
    {
        private static readonly string[] IgnoreDatabases = new string[]{
            DbConnectionSchema.DbName_LocalServer,
            DbSettingNames.DbMigrationHistory
        };

        [RepositoryQuery]
        public override object GetAll(PagingInfo paging, LoadOptions loadOptions)
        {
            var list = new DbSettingItemList();

            var settings = ConfigurationManager.ConnectionStrings.OfType<ConnectionStringSettings>().Select(s => s.Name)
                .Union(DbSetting.GetGeneratedSettings().Select(s => s.Name))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToArray();

            var id = 1;
            foreach (var name in settings)
            {
                if (IgnoreDatabases.Contains(name)) continue;

                var entity = new DbSettingItem();
                entity.Id = id++;
                entity.Name = name;

                entity.MarkSaved();

                list.Add(entity);
            }

            return list;
        }
    }

    internal class DbSettingItemConfig : WPFViewConfig<DbSettingItem>
    {
        protected override void ConfigView()
        {
            View.DomainName("数据库配置项").HasDelegate(DbSettingItem.NameProperty);

            using (View.OrderProperties())
            {
                View.Property(DbSettingItem.NameProperty).HasLabel("数据库连接名").ShowIn(ShowInWhere.All);
            }
        }
    }
}