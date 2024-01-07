/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101017
 * 说明：Rafy缓存中服务器端数据库中存储的版本号
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101017
 * 修改Value为DateTime类型 胡庆访 20101222
 * 
*******************************************************/

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Rafy;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;

namespace Rafy.Domain.Caching
{
    /// <summary>
    /// Rafy缓存中服务器端数据库中存储的版本号
    /// </summary>
    [Serializable]
    [RootEntity]
    [DebuggerDisplay("{ClassRegion} {ScopeClass} {ScopeId}")]
    public partial class ScopeVersion : IntEntity
    {
        /// <summary>
        /// 以此类作为缓存的“区域”划分
        /// </summary>
        public static readonly Property<string> ClassRegionProperty = P<ScopeVersion>.Register(e => e.ClassRegion);
        public string ClassRegion
        {
            get { return GetProperty(ClassRegionProperty); }
            set { SetProperty(ClassRegionProperty, value); }
        }

        /// <summary>
        /// 所处范围的类名
        /// 可以为null，表示整个表作为缓存的范围。
        /// </summary>
        public static readonly Property<string> ScopeClassProperty = P<ScopeVersion>.Register(e => e.ScopeClass);
        public string ScopeClass
        {
            get { return GetProperty(ScopeClassProperty); }
            set { SetProperty(ScopeClassProperty, value); }
        }

        /// <summary>
        /// 范围的Id号
        /// </summary>
        public static ManagedProperty<string> ScopeIdProperty = P<ScopeVersion>.Register(e => e.ScopeId);
        public string ScopeId
        {
            get { return GetProperty(ScopeIdProperty); }
            set { SetProperty(ScopeIdProperty, value); }
        }

        /// <summary>
        /// 精确的版本号值。
        /// </summary>
        public static readonly Property<long> AccurateValueProperty = P<ScopeVersion>.Register(e => e.AccurateValue);
        public long AccurateValue
        {
            get { return this.GetProperty(AccurateValueProperty); }
            set { this.SetProperty(AccurateValueProperty, value); }
        }

        /// <summary>
        /// 只是方便显示、不用于比较的版本号。
        /// </summary>
        public static readonly Property<DateTime> ValueProperty = P<ScopeVersion>.Register(e => e.Value, new PropertyMetadata<DateTime>
        {
            PropertyChangedCallBack = (o, e) => (o as ScopeVersion).OnValueChanged(e)
        });
        public DateTime Value
        {
            get { return this.GetProperty(ValueProperty); }
            set { this.SetProperty(ValueProperty, value); }
        }
        protected virtual void OnValueChanged(ManagedPropertyChangedEventArgs e)
        {
            this.AccurateValue = ((DateTime)e.NewValue).Ticks;
        }
    }

    public partial class ScopeVersionList : InheritableEntityList
    {
        public DateTime ServerTime { get; set; }
    }

    public partial class ScopeVersionRepository : EntityRepository
    {
        public ScopeVersionRepository() { }

        [RepositoryQuery]
        public virtual ScopeVersionList GetList(string classRegion)
        {
            var q = QueryFactory.Instance.Query(this);
            q.AddConstraint(ScopeVersion.ClassRegionProperty, PropertyOperator.Equal, classRegion);

            return (ScopeVersionList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual ScopeVersionList GetList(string classRegion, string scopeClass, string scopeId)
        {
            var q = QueryFactory.Instance.Query(this);
            q.AddConstraint(ScopeVersion.ClassRegionProperty, PropertyOperator.Equal, classRegion);
            q.AddConstraint(ScopeVersion.ScopeClassProperty, PropertyOperator.Equal, scopeClass);
            q.AddConstraint(ScopeVersion.ScopeIdProperty, PropertyOperator.Equal, scopeId);

            return (ScopeVersionList)this.QueryData(q);
        }

        /// <summary>
        /// 获取最新的更改集
        /// </summary>
        /// <param name="lastTime"></param>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual ScopeVersionList GetList(DateTime lastTime)
        {
            var q = QueryFactory.Instance.Query(this);
            q.AddConstraint(ScopeVersion.AccurateValueProperty, PropertyOperator.Greater, lastTime.Ticks);

            return (ScopeVersionList)this.QueryData(q);
        }
    }

    [DataProviderFor(typeof(ScopeVersionRepository))]
    public partial class ScopeVersionRepositoryDataProvider : RdbDataProvider
    {
        public ScopeVersionRepositoryDataProvider()
        {
            this.DataSaver = new ScopeVersionSaver();
            this.DataQueryer = new ScopeVersionQueryer();
        }

        private class ScopeVersionSaver : RdbDataSaver
        {
            protected override void Submit(SubmitArgs e)
            {
                //把Value更新为服务端时间，再保存到库中。
                (e.Entity as ScopeVersion).Value = DateTime.Now;

                base.Submit(e);
            }
        }

        private class ScopeVersionQueryer : RdbDataQueryer
        {
            protected override void OnDbLoaded(Entity entity)
            {
                var sv = entity as ScopeVersion;
                sv.LoadProperty(ScopeVersion.ValueProperty, new DateTime(sv.AccurateValue));
            }

            protected override void OnEntityQueryed(EntityQueryArgs args)
            {
                base.OnEntityQueryed(args);

                var list = args.EntityList as ScopeVersionList;
                list.ServerTime = DateTime.Now;
            }
        }
    }

    internal class ScopeVersionConfig : EntityConfig<ScopeVersion>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable("CacheScopedVersions").MapProperties(
                ScopeVersion.ClassRegionProperty,
                ScopeVersion.ScopeClassProperty,
                ScopeVersion.ScopeIdProperty,
                ScopeVersion.AccurateValueProperty,
                ScopeVersion.ValueProperty
                );
            Meta.Property(ScopeVersion.ScopeClassProperty).MapColumn().IsNullable();
            Meta.Property(ScopeVersion.ScopeIdProperty).MapColumn().IsNullable();
        }
    }
}
