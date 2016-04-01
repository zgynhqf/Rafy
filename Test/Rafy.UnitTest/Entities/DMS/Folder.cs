using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace UT
{
    /// <summary>
    /// 文件夹
    /// </summary>
    [RootEntity, Serializable]
    public partial class Folder : UnitTestEntity
    {
        #region 构造函数

        public Folder() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected Folder(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        #endregion

        #region 组合子属性

        public static readonly ListProperty<FileList> FileListProperty = P<Folder>.RegisterList(e => e.FileList);
        public FileList FileList
        {
            get { return this.GetLazyList(FileListProperty); }
        }

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<Folder>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 文件夹 列表类。
    /// </summary>
    [Serializable]
    public partial class FolderList : UnitTestEntityList { }

    /// <summary>
    /// 文件夹 仓库类。
    /// 负责 文件夹 类的查询、保存。
    /// </summary>
    public partial class FolderRepository : UnitTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected FolderRepository() { }

        [RepositoryQuery]
        public virtual FolderList GetForIgnoreTest()
        {
            var q = this.CreateLinqQuery();
            q = q.Where(e => e.Name != "1.1");
            return (FolderList)this.QueryData(q);
        }
    }

    /// <summary>
    /// 文件夹 配置类。
    /// 负责 文件夹 类的实体元数据的配置。
    /// </summary>
    internal class FolderConfig : UnitTestEntityConfig<Folder>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            Meta.SupportTree();

            //配置实体的所有属性都映射到数据表中。
            Meta.MapTable().MapAllProperties();
        }
    }
}