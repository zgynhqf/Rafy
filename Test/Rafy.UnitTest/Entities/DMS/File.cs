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
    /// 文件
    /// </summary>
    [ChildEntity]
    public partial class File : UnitTestEntity
    {
        #region 引用属性

        public static readonly IRefIdProperty FolderIdProperty =
            P<File>.RegisterRefId(e => e.FolderId, ReferenceType.Parent);
        public int FolderId
        {
            get { return (int)this.GetRefId(FolderIdProperty); }
            set { this.SetRefId(FolderIdProperty, value); }
        }
        public static readonly RefEntityProperty<Folder> FolderProperty =
            P<File>.RegisterRef(e => e.Folder, FolderIdProperty);
        public Folder Folder
        {
            get { return this.GetRefEntity(FolderProperty); }
            set { this.SetRefEntity(FolderProperty, value); }
        }

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<File>.Register(e => e.Name);
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
    /// 文件 列表类。
    /// </summary>
    public partial class FileList : UnitTestEntityList { }

    /// <summary>
    /// 文件 仓库类。
    /// 负责 文件 类的查询、保存。
    /// </summary>
    public partial class FileRepository : UnitTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected FileRepository() { }
    }

    /// <summary>
    /// 文件 配置类。
    /// 负责 文件 类的实体元数据的配置。
    /// </summary>
    internal class FileConfig : UnitTestEntityConfig<File>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            //配置实体的所有属性都映射到数据表中。
            Meta.MapTable("Files").MapAllProperties();
        }
    }
}