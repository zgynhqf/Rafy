/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120423
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120423
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace JXC
{
    /// <summary>
    /// 附件
    /// </summary>
    [Serializable]
    public abstract class FileAttachement : JXCEntity
    {
        public static readonly Property<string> FileNameProperty = P<FileAttachement>.Register(e => e.FileName);
        public string FileName
        {
            get { return this.GetProperty(FileNameProperty); }
            set { this.SetProperty(FileNameProperty, value); }
        }

        public static readonly Property<byte[]> ContentBytesProperty = P<FileAttachement>.Register(e => e.ContentBytes);
        public byte[] ContentBytes
        {
            get { return this.GetProperty(ContentBytesProperty); }
            set { this.SetProperty(ContentBytesProperty, value); }
        }

        public static readonly Property<DateTime> UploadDateProperty = P<FileAttachement>.Register(e => e.UploadDate);
        public DateTime UploadDate
        {
            get { return this.GetProperty(UploadDateProperty); }
            set { this.SetProperty(UploadDateProperty, value); }
        }

        #region 把二进制存放到硬盘上

        /*********************** 代码块解释 *********************************
         * 
         * 附件在数据库中不需要存储其对应的二进制数据，而只是存放几个常用的字段。
         * 二进制数据是以文件的形式存放在某个指定的目录下。
         * 
        **********************************************************************/

        protected virtual string StoreDir
        {
            get { return "FileAttachements\\"; }
        }

        internal void SaveToDisk()
        {
            var bytes = this.ContentBytes;
            if (bytes == null || bytes.Length == 0)
            {
                throw new InvalidOperationException("附件没有相应的二进制数据。");
            }

            var dir = RafyEnvironment.MapAbsolutePath(StoreDir);
            Directory.CreateDirectory(dir);

            var name = this.GetDiskFullName();
            File.WriteAllBytes(name, this.ContentBytes);
        }

        internal void LoadFromDisk()
        {
            var name = this.GetDiskFullName();
            if (File.Exists(name))
            {
                this.ContentBytes = File.ReadAllBytes(name);
            }
        }

        internal void DeleteFromDisk()
        {
            var name = this.GetDiskFullName();
            File.Delete(name);
        }

        private string GetDiskFullName()
        {
            return RafyEnvironment.MapAbsolutePath(StoreDir + this.Id);
        }

        #endregion
    }

    public abstract class FileAttachementList : JXCEntityList { }

    public abstract class FileAttachementRepository : JXCEntityRepository
    {
        protected FileAttachementRepository() { }
    }

    [DataProviderFor(typeof(FileAttachementRepository))]
    public class FileAttachementDataProvider : JXCEntityDataProvider
    {
        protected override void Submit(SubmitArgs e)
        {
            if (e.Action != SubmitAction.Delete)
            {
                (e.Entity as FileAttachement).SaveToDisk();
            }

            base.Submit(e);

            if (e.Action == SubmitAction.Delete)
            {
                (e.Entity as FileAttachement).DeleteFromDisk();
            }
        }
    }

    internal class FileAttachementConfig : EntityConfig<FileAttachement>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesExcept(
                FileAttachement.ContentBytesProperty
                );
        }
    }
}