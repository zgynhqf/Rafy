/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130319 15:44
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130319 15:44
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
using Rafy.Domain.ORM.Linq;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;
using System.Linq.Expressions;
using Rafy.Data;

namespace UT
{
    [RootEntity]
    public partial class PBSType : UnitTestEntity
    {
        #region 引用属性

        #endregion

        #region 子属性

        public static readonly ListProperty<PBSList> PBSListProperty = P<PBSType>.RegisterList(e => e.PBSList);
        public PBSList PBSList
        {
            get { return this.GetLazyList(PBSListProperty); }
        }

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<PBSType>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> CodeProperty = P<PBSType>.Register(e => e.Code);
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<int> AmountProperty = P<PBSType>.Register(e => e.Amount);
        public int Amount
        {
            get { return this.GetProperty(AmountProperty); }
            set { this.SetProperty(AmountProperty, value); }
        }

        public static readonly Property<DateTime> CreateTimeProperty = P<PBSType>.Register(e => e.CreateTime);
        public DateTime CreateTime
        {
            get { return this.GetProperty(CreateTimeProperty); }
            set { this.SetProperty(CreateTimeProperty, value); }
        }

        public static readonly Property<bool> IsDefaultProperty = P<PBSType>.Register(e => e.IsDefault);
        public bool IsDefault
        {
            get { return this.GetProperty(IsDefaultProperty); }
            set { this.SetProperty(IsDefaultProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    public partial class PBSTypeList : UnitTestEntityList { }

    public partial class PBSTypeRepository : UnitTestEntityRepository
    {
        protected PBSTypeRepository() { }

        [RepositoryQuery]
        public virtual PBSTypeList LinqBySingleName(string name)
        {
            var q = this.CreateLinqQuery()
                .Where(t => t.Name == name);
            return (PBSTypeList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual PBSTypeList LinqBySingleNameReverse(string name)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => name == e.Name);
            return (PBSTypeList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual PBSTypeList LinqByCodeAndName(string code, string name, bool andOr)
        {
            var queryable = this.CreateLinqQuery();
            if (andOr)
            {
                queryable = queryable.Where(t => t.Code == code && t.Name == name);
            }
            else
            {
                queryable = queryable.Where(t => t.Code == code || t.Name == name);
            }

            return (PBSTypeList)this.QueryData(queryable);
        }

        [RepositoryQuery]
        public virtual PBSTypeList LinqByWhereOnWHere(string code, string name)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.Code == code);
            if (!string.IsNullOrEmpty(name))
            {
                q = q.Where(e => e.Name == name);
            }

            return (PBSTypeList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual PBSTypeList LinqOrderByCode(OrderDirection dir, OrderDirection? thenByName = null)
        {
            var q = this.CreateLinqQuery() as IOrderedQueryable<PBSType>;

            if (dir == OrderDirection.Ascending)
            {
                q = q.OrderBy(t => t.Code);
            }
            else
            {
                q = q.OrderByDescending(t => t.Code);
            }

            if (thenByName.HasValue)
            {
                if (thenByName.Value == OrderDirection.Ascending)
                {
                    q = q.ThenBy(t => t.Name);
                }
                else
                {
                    q = q.ThenByDescending(t => t.Name);
                }
            }

            return (PBSTypeList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual PBSTypeList LinqByBoolean(bool isDefault)
        {
            var q = this.CreateLinqQuery();
            if (isDefault)
            {
                q = q.Where(e => e.IsDefault);
            }
            else
            {
                q = q.Where(e => !e.IsDefault);
            }
            return (PBSTypeList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual PBSTypeList LinqByBoolean_Raw(bool isDefault)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.IsDefault == isDefault);
            return (PBSTypeList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual PBSTypeList LinqByBoolean_InBinary(string name, bool isDefault)
        {
            var q = this.CreateLinqQuery();
            if (isDefault)
            {
                q = q.Where(e => e.IsDefault && e.Name == name);
            }
            else
            {
                q = q.Where(e => !e.IsDefault && e.Name == name);
            }
            return (PBSTypeList)this.QueryData(q);
        }
    }

    internal class PBSTypeConfig : UnitTestEntityConfig<PBSType>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();

            Meta.EnableClientCache(100);
            Meta.EnableServerCache();

            var dbSettings = DbSetting.FindOrCreate(UnitTestEntityRepositoryDataProvider.DbSettingName);
            if (dbSettings.ProviderName == DbSetting.Provider_SQLite)
            {
                Meta.DeletingChildrenInMemory = true;
            }
        }
    }

    [Serializable]
    public partial class LinqByWhereOnWhereCriteira
    {
        public string Code, Name;
    }
    [Serializable]
    public partial class LinqOrderByCodeCriteira
    {
        public OrderDirection Direction = OrderDirection.Ascending;
        public OrderDirection? ThenByNameDirection;
    }
}