/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20131212
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20131212 16:34
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Data;
using Rafy.Domain.ORM;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;

namespace UT
{
    /// <summary>
    /// 文章
    /// </summary>
    [RootEntity, Serializable]
    //[KnownType(typeof(BlogUser))]
    public partial class Article : UnitTestEntity
    {
        #region 构造函数

        public Article() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Article(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        public static readonly IRefIdProperty UserIdProperty =
            P<Article>.RegisterRefId(e => e.UserId, ReferenceType.Normal);
        public int UserId
        {
            get { return this.GetRefId(UserIdProperty); }
            set { this.SetRefId(UserIdProperty, value); }
        }
        public static readonly RefEntityProperty<BlogUser> UserProperty =
            P<Article>.RegisterRef(e => e.User, UserIdProperty);
        /// <summary>
        /// 作者
        /// </summary>
        public BlogUser User
        {
            get { return this.GetRefEntity(UserProperty); }
            set { this.SetRefEntity(UserProperty, value); }
        }

        public static readonly IRefIdProperty AdministratorIdProperty =
            P<Article>.RegisterRefId(e => e.AdministratorId, ReferenceType.Normal);
        public int? AdministratorId
        {
            get { return this.GetRefNullableId(AdministratorIdProperty); }
            set { this.SetRefNullableId(AdministratorIdProperty, value); }
        }
        public static readonly RefEntityProperty<BlogUser> AdministratorProperty =
            P<Article>.RegisterRef(e => e.Administrator, AdministratorIdProperty);
        /// <summary>
        /// 文章的管理员
        /// </summary>
        public BlogUser Administrator
        {
            get { return this.GetRefEntity(AdministratorProperty); }
            set { this.SetRefEntity(AdministratorProperty, value); }
        }

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> CodeProperty = P<Article>.Register(e => e.Code);
        /// <summary>
        /// 编码
        /// </summary>
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<DateTime> CreateDateProperty = P<Article>.Register(e => e.CreateDate);
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreateDate
        {
            get { return this.GetProperty(CreateDateProperty); }
            set { this.SetProperty(CreateDateProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 文章 列表类。
    /// </summary>
    [Serializable]
    public partial class ArticleList : UnitTestEntityList { }

    /// <summary>
    /// 文章 仓库类。
    /// 负责 文章 类的查询、保存。
    /// </summary>
    public partial class ArticleRepository : UnitTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected ArticleRepository() { }

        [RepositoryQuery]
        public virtual ArticleList GetForTwoJoinTest()
        {
            var q = this.CreateLinqQuery();
            //对同时引用 BlogUser 表的两个引用实体（一个可空，一个非空），进行查询。
            q = q.Where(e => (e.Administrator.UserName == "admin" || e.AdministratorId == null) && e.User.UserName == "huqf").OrderBy(p=>p.Id);
            return (ArticleList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual LiteDataTable GetAllInTable()
        {
            return (this.DataQueryer as RdbDataQueryer).QueryTable(@"
select * from Article
");
        }
    }

    /// <summary>
    /// 文章 配置类。
    /// 负责 文章 类的实体元数据的配置。
    /// </summary>
    internal class ArticleConfig : UnitTestEntityConfig<Article>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            //配置实体的所有属性都映射到数据表中。
            Meta.MapTable().MapAllProperties();
        }
    }
}