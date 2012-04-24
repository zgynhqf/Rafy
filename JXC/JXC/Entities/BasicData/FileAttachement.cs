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
using System.Linq;
using System.Text;
using OEA;
using OEA.Library;
using OEA.Library.Validation;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;
using OEA.ManagedProperty;
using System.IO;
using JXC.Commands;

namespace JXC
{
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

        protected override void OnInsert()
        {
            base.OnInsert();

            this.SaveToDisk();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            this.SaveToDisk();
        }

        protected override void OnDelete()
        {
            base.OnDelete();

            this.DeleteFromDisk();
        }

        protected override void OnDbLoaded()
        {
            base.OnDbLoaded();

            this.LoadFromDisk();
        }

        private void SaveToDisk()
        {
            var bytes = this.ContentBytes;
            if (bytes == null || bytes.Length == 0)
            {
                throw new InvalidOperationException("附件没有相应的二进制数据。");
            }

            var dir = OEAEnvironment.ToAbsolute(StoreDir);
            Directory.CreateDirectory(dir);

            var name = this.GetDiskFullName();
            File.WriteAllBytes(name, this.ContentBytes);
        }

        private void LoadFromDisk()
        {
            var name = this.GetDiskFullName();
            if (File.Exists(name))
            {
                this.ContentBytes = File.ReadAllBytes(name);
            }
        }

        private void DeleteFromDisk()
        {
            var name = this.GetDiskFullName();
            File.Delete(name);
        }

        private string GetDiskFullName()
        {
            return OEAEnvironment.ToAbsolute(StoreDir + this.Id);
        }

        #endregion
    }

    [Serializable]
    public abstract class FileAttachementList : JXCEntityList { }

    public abstract class FileAttachementRepository : EntityRepository
    {
        protected FileAttachementRepository() { }
    }

    internal class FileAttachementConfig : EntityConfig<FileAttachement>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllPropertiesToTableExcept(
                FileAttachement.ContentBytesProperty
                );
        }

        protected override void ConfigView()
        {
            View.DomainName("附件").HasDelegate(FileAttachement.FileNameProperty);

            View.ClearWPFCommands(false)
                .UseWPFCommands(
                typeof(AddFileAttachement),
                typeof(OpenFileAttachement),
                WPFCommandNames.Delete
                );

            using (View.OrderProperties())
            {
                View.Property(FileAttachement.FileNameProperty).HasLabel("文件名").ShowIn(ShowInWhere.All);
                View.Property(FileAttachement.UploadDateProperty).HasLabel("创建日期").ShowIn(ShowInWhere.ListDetail);
            }
        }
    }
}