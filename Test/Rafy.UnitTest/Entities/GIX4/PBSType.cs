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

namespace UT
{
    [RootEntity, Serializable]
    public partial class PBSType : UnitTestEntity
    {
        #region 构造函数

        public PBSType() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected PBSType(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

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

    [Serializable]
    public partial class PBSTypeList : UnitTestEntityList { }

    public partial class PBSTypeRepository : UnitTestEntityRepository
    {
        protected PBSTypeRepository() { }

        public PBSTypeList LinqBySingleName(string name)
        {
            return this.FetchList(name);
        }
        protected EntityList FetchBy(string name)
        {
            var queryable = this.CreateLinqQuery()
                .Where(t => t.Name == name);
            return this.QueryList(queryable);
        }

        public PBSTypeList LinqBySingleNameReverse(string name)
        {
            return this.FetchList(r => r.DA_LinqBySingleNameReverse(name));
        }
        private EntityList DA_LinqBySingleNameReverse(string name)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => name == e.Name);
            return this.QueryList(q);
        }

        public PBSTypeList LinqByCodeAndName(string code, string name, bool andOr)
        {
            return this.FetchList(code, name, andOr);
        }
        protected EntityList FetchBy(string code, string name, bool andOr)
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
            return this.QueryList(queryable);
        }

        public PBSTypeList LinqByWhereOnWHere(string code, string name)
        {
            return this.FetchList(new LinqByWhereOnWhereCriteira { Code = code, Name = name });
        }
        protected EntityList FetchBy(LinqByWhereOnWhereCriteira c)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.Code == c.Code);
            if (!string.IsNullOrEmpty(c.Name))
            {
                q = q.Where(e => e.Name == c.Name);
            }
            return this.QueryList(q);
        }

        public PBSTypeList LinqOrderByCode(OrderDirection dir, OrderDirection? thenByName = null)
        {
            return this.FetchList(new LinqOrderByCodeCriteira { Direction = dir, ThenByNameDirection = thenByName });
        }
        protected EntityList FetchBy(LinqOrderByCodeCriteira c)
        {
            var q = this.CreateLinqQuery() as IOrderedQueryable<PBSType>;

            if (c.Direction == OrderDirection.Ascending)
            {
                q = q.OrderBy(t => t.Code);
            }
            else
            {
                q = q.OrderByDescending(t => t.Code);
            }

            if (c.ThenByNameDirection.HasValue)
            {
                if (c.ThenByNameDirection.Value == OrderDirection.Ascending)
                {
                    q = q.ThenBy(t => t.Name);
                }
                else
                {
                    q = q.ThenByDescending(t => t.Name);
                }
            }

            return this.QueryList(q);
        }

        public PBSTypeList LinqByBoolean(bool isDefault)
        {
            return this.FetchList(r => r.DA_LinqByBoolean(isDefault));
        }
        private EntityList DA_LinqByBoolean(bool isDefault)
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
            return this.QueryList(q);
        }

        public PBSTypeList LinqByBoolean_Raw(bool isDefault)
        {
            return this.FetchList(r => r.DA_LinqByBoolean_Raw(isDefault));
        }
        private EntityList DA_LinqByBoolean_Raw(bool isDefault)
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.IsDefault == isDefault);
            return this.QueryList(q);
        }

        public PBSTypeList LinqByBoolean_InBinary(string name, bool isDefault)
        {
            return this.FetchList(r => r.DA_LinqByBoolean_InBinary(name, isDefault));
        }
        private EntityList DA_LinqByBoolean_InBinary(string name, bool isDefault)
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
            return this.QueryList(q);
        }
    }

    internal class PBSTypeConfig : UnitTestEntityConfig<PBSType>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();

            Meta.EnableClientCache(100);
            Meta.EnableServerCache();
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