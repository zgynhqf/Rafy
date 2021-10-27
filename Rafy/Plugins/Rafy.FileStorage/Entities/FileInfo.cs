/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160502
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160502 01:49
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.ComponentModel;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;

namespace Rafy.FileStorage
{
    /// <summary>
    /// 文件信息
    /// </summary>
    [RootEntity]
    public partial class FileInfo : FileStorageEntity
    {
        #region 引用属性

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<FileInfo>.Register(e => e.Name);
        /// <summary>
        /// 文件名（带扩展名）
        /// </summary>
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> DescriptionProperty = P<FileInfo>.Register(e => e.Description);
        /// <summary>
        /// 描述信息
        /// </summary>
        public string Description
        {
            get { return this.GetProperty(DescriptionProperty); }
            set { this.SetProperty(DescriptionProperty, value); }
        }

        public static readonly Property<double> LengthProperty = P<FileInfo>.Register(e => e.Length);
        /// <summary>
        /// 文件的大小(Bytes)
        /// </summary>
        public double Length
        {
            get { return this.GetProperty(LengthProperty); }
            internal set { this.SetProperty(LengthProperty, value); }
        }

        internal static readonly Property<string> StorageProperty = P<FileInfo>.Register(e => e.Storage);
        /// <summary>
        /// 存储的具体信息。
        /// 不同的协议决定文件存储的方式。（可以是文件夹路径，也可能存储在云端等。）
        /// </summary>
        internal string Storage
        {
            get { return this.GetProperty(StorageProperty); }
            set { this.SetProperty(StorageProperty, value); }
        }

        public static readonly Property<byte[]> ContentProperty = P<FileInfo>.Register(e => e.Content, new PropertyMetadata<byte[]>
        {
            //不能不影响实体的状态，否则在更新的文件内容后，实体无法保存。
            //AffectStatus = false,
            CoerceGetValueCallBack = (o, v) => (o as FileInfo).CoerceGetContent(v)
        });
        /// <summary>
        /// 文件内容
        /// 注意：使用这个属性会自动懒加载文件的内容到内存中。如果是使用大文件，请使用 <see cref="Controllers.FileStorageController"/> 来完成相关操作。
        /// </summary>
        public byte[] Content
        {
            get { return this.GetProperty(ContentProperty); }
            set { this.SetProperty(ContentProperty, value); }
        }
        protected virtual byte[] CoerceGetContent(byte[] value)
        {
            if (value == null || value.Length == 0)
            {
                value = FileStoragePlugin.ContentProvider.ReadContent(this);
                this.Content = value;
            }

            return value;
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 文件信息 列表类。
    /// </summary>
    public partial class FileInfoList : FileStorageEntityList { }

    /// <summary>
    /// 文件信息 仓库类。
    /// 负责 文件信息 类的查询、保存。
    /// </summary>
    public partial class FileInfoRepository : FileStorageEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected FileInfoRepository() { }
    }

    [DataProviderFor(typeof(FileInfoRepository))]
    public partial class FileInfoRepositoryDataProvider : FileStorageEntityRepositoryDataProvider
    {
        protected override void Submit(SubmitArgs e)
        {
            var entity = e.Entity as FileInfo;

            switch (e.Action)
            {
                case SubmitAction.Delete:
                    FileStoragePlugin.ContentProvider.DeleteContent(entity);
                    break;
                case SubmitAction.ChildrenOnly:
                case SubmitAction.Insert:
                case SubmitAction.Update:
                    if (entity.HasLocalValue(FileInfo.ContentProperty))
                    {
                        FileStoragePlugin.ContentProvider.SaveContent(entity, entity.Content);
                    }
                    break;
                default:
                    break;
            }

            base.Submit(e);
        }
    }

    /// <summary>
    /// 文件信息 配置类。
    /// 负责 文件信息 类的实体元数据的配置。
    /// </summary>
    internal class FileInfoConfig : FileStorageEntityConfig<FileInfo>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            //配置实体的属性映射到数据表中。
            Meta.MapTable().MapAllPropertiesExcept(
                FileInfo.ContentProperty
                );
        }

        protected override void AddValidations(IValidationDeclarer rules)
        {
            base.AddValidations(rules);

            rules.AddRule(FileInfo.NameProperty, new RequiredRule());
            //rules.AddRule(FileInfo.NameProperty, new NotDuplicateRule());
        }
    }
}