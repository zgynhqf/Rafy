//*******************************************************
// * 
// * 作者：胡庆访
// * 创建时间：20110317
// * 说明：此文件只包含一个类，具体内容见类型注释。
// * 运行环境：.NET 4.0
// * 版本号：1.0.0
// * 
// * 历史记录：
// * 创建文件 胡庆访 20100317
// * 
//*******************************************************/

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
////
//namespace Rafy.Library
//{
//    internal interface IDbSeeder
//    {
//        DbCreationType DbCreationType { get; }

//        void Seed(DbContext context);
//    }

//    /// <summary>
//    /// 数据库配置类
//    /// </summary>
//    /// <typeparam name="TEntity"></typeparam>
//    public abstract class ORMapping<TEntity> : EntityTypeConfiguration<TEntity>, IDbSeeder
//        where TEntity : Entity
//    {
//        protected ORMapping()
//        {
//            this.MapToDb();
//        }

//        /// <summary>
//        /// 同一数据库，只需要任意一个子配置类重写此属性即可。
//        /// 默认：CreateIfNotExists
//        /// </summary>
//        public virtual DbCreationType DbCreationType
//        {
//            get
//            {
//                return DbCreationType.Ignore;
//            }
//        }

//        protected virtual void MapToDb()
//        {
//            this.Ignore(e => e.BrokenRulesCollection);
//            //this.Ignore(e => e.IsBusy);
//            //this.Ignore(e => e.IsDeleted);
//            //this.Ignore(e => e.IsNew);
//            //this.Ignore(e => e.IsObjectLazy);
//            //this.Ignore(e => e.IsSavable);
//            //this.Ignore(e => e.IsSelfBusy);
//            this.Ignore(e => e.Status);
//            this.Ignore(e => e.IsDirty);
//            this.Ignore(e => e.IsSelfDirty);
//            this.Ignore(e => e.IsSelfValid);
//            this.Ignore(e => e.IsValid);
//            this.Ignore(e => e.Parent);
//            this.Ignore(e => e.ValidationRules);
//            this.Ignore(e => e.SupportTree);
//            this.Ignore(e => e.TreePId);
//            this.Ignore(e => e.TreeParentEntity);
//            this.Ignore(e => e.ChildrenNodes);
//            this.Ignore(e => e.OrderNo);
//            this.Ignore(e => e.DisableReference);

//            this.HasKey(e => e.Id);

//            this.MapToTable(typeof(TEntity).Name);

//            //var isTree = RF.Create<TEntity>().SupportTree;
//            //if (!isTree)
//            //{
//            //    this.Ignore(e => e.OrderNo);
//            //}
//        }

//        /// <summary>
//        /// 子类使用 TPC 模式来进行映射。
//        /// </summary>
//        protected void MapInheritance()
//        {
//            this.Map(m => m.MapInheritedProperties())
//                .MapToTable(typeof(TEntity).Name);
//        }

//        protected virtual void Seed(ICollection<TEntity> table, Func<TEntity> creatEntity, DbContext context) { }

//        void IDbSeeder.Seed(DbContext context)
//        {
//            Func<TEntity> creator = () =>
//            {
//                return RF.Create<TEntity>().New() as TEntity;
//                //以下方案没有默认值
//                //return Activator.CreateInstance(typeof(TEntity), true) as TEntity;
//            };

//            this.Seed(context.Set<TEntity>().Local, creator, context);
//        }
//    }
//}