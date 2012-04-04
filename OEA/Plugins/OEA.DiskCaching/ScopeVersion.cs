/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101017
 * 说明：OEA缓存中服务器端数据库中存储的版本号
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
using OEA;
using OEA.ORM;
using OEA.MetaModel.Attributes;
using System.Linq;
using OEA.ManagedProperty;
using OEA.MetaModel;

namespace OEA.Library.Caching
{
    /// <summary>
    /// OEA缓存中服务器端数据库中存储的版本号
    /// </summary>
    [Serializable]
    [RootEntity]
    [DebuggerDisplay("{ClassRegion} {ScopeClass} {ScopeId}")]
    public class ScopeVersion : Entity
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
        /// 当前的版本号
        /// </summary>
        public static ManagedProperty<DateTime> ValueProperty = P<ScopeVersion>.Register(e => e.Value);
        public DateTime Value
        {
            get { return GetProperty(ValueProperty); }
            set { SetProperty(ValueProperty, value); }
        }

        protected override void OnUpdate()
        {
            //把Value更新为服务端时间，再保存到库中。
            this.Value = DateTime.Now;

            base.OnUpdate();
        }

        protected override void OnInsert()
        {
            //把Value更新为服务端时间，再保存到库中。
            this.Value = DateTime.Now;

            base.OnInsert();
        }
    }

    [Serializable]
    internal class ScopeVersionList : EntityList
    {
        public DateTime ServerTime { get; set; }

        protected override void OnGetAll()
        {
            base.OnGetAll();

            this.ServerTime = DateTime.Now;
        }

        protected override void OnGetByParentId(int parentId)
        {
            base.OnGetByParentId(parentId);

            this.ServerTime = DateTime.Now;
        }

        protected void QueryBy(DateTime lastTime)
        {
            this.QueryDb(q => q.Constrain(ScopeVersion.ValueProperty).Greater(lastTime));

            this.ServerTime = DateTime.Now;
        }

        protected void QueryBy(string classRegion)
        {
            this.QueryDb(q => q.Constrain(ScopeVersion.ClassRegionProperty).Equal(classRegion));

            this.ServerTime = DateTime.Now;
        }

        protected void QueryBy(GetByPKCriteria criteria)
        {
            this.QueryDb(q => q.Constrain(ScopeVersion.ClassRegionProperty).Equal(criteria.ClassRegion)
                .And().Constrain(ScopeVersion.ScopeClassProperty).Equal(criteria.ScopeClass)
                .And().Constrain(ScopeVersion.ScopeIdProperty).Equal(criteria.ScopeId)
                );

            this.ServerTime = DateTime.Now;
        }
    }

    internal class ScopeVersionRepository : EntityRepository
    {
        public ScopeVersionList GetList(string classRegion)
        {
            return FetchListCast<ScopeVersionList>(classRegion);
        }

        public ScopeVersionList GetList(string classRegion, string scopeClass, string scopeId)
        {
            return FetchListCast<ScopeVersionList>(new GetByPKCriteria()
            {
                ClassRegion = classRegion,
                ScopeClass = scopeClass ?? string.Empty,
                ScopeId = scopeId ?? string.Empty
            });
        }

        /// <summary>
        /// 获取最新的更改集
        /// </summary>
        /// <param name="lastTime"></param>
        /// <returns></returns>
        public ScopeVersionList GetList(DateTime lastTime)
        {
            return FetchListCast<ScopeVersionList>(lastTime);
        }
    }

    internal class ScopeVersionConfig : EntityConfig<ScopeVersion>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().HasColumns(
                ScopeVersion.ClassRegionProperty,
                ScopeVersion.ScopeClassProperty,
                ScopeVersion.ScopeIdProperty,
                ScopeVersion.ValueProperty
                );
            Meta.Property(ScopeVersion.ScopeClassProperty).MapColumn().IsRequired(false);
            Meta.Property(ScopeVersion.ScopeIdProperty).MapColumn().IsRequired(false);
        }
    }

    [Serializable]
    internal class GetByPKCriteria
    {
        public string ClassRegion { get; set; }
        public string ScopeClass { get; set; }
        public string ScopeId { get; set; }
    }
}
