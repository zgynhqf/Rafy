/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130318 15:12
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130318 15:12
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
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;

namespace UT
{
    [ChildEntity]
    public partial class Section : UnitTestEntity
    {
        #region 引用属性

        public static readonly IRefIdProperty ChapterIdProperty =
            P<Section>.RegisterRefId(e => e.ChapterId, ReferenceType.Parent);
        public int ChapterId
        {
            get { return this.GetRefId(ChapterIdProperty); }
            set { this.SetRefId(ChapterIdProperty, value); }
        }
        public static readonly RefEntityProperty<Chapter> ChapterProperty =
            P<Section>.RegisterRef(e => e.Chapter, ChapterIdProperty);
        public Chapter Chapter
        {
            get { return this.GetRefEntity(ChapterProperty); }
            set { this.SetRefEntity(ChapterProperty, value); }
        }

        public static readonly IRefIdProperty SectionOwnerIdProperty =
            P<Section>.RegisterRefId(e => e.SectionOwnerId, ReferenceType.Normal);
        public int? SectionOwnerId
        {
            get { return this.GetRefNullableId(SectionOwnerIdProperty); }
            set { this.SetRefNullableId(SectionOwnerIdProperty, value); }
        }
        public static readonly RefEntityProperty<SectionOwner> SectionOwnerProperty =
            P<Section>.RegisterRef(e => e.SectionOwner, SectionOwnerIdProperty);
        public SectionOwner SectionOwner
        {
            get { return this.GetRefEntity(SectionOwnerProperty); }
            set { this.SetRefEntity(SectionOwnerProperty, value); }
        }

        #endregion

        #region 子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<Section>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    public partial class SectionList : UnitTestEntityList { }

    public partial class SectionRepository : UnitTestEntityRepository
    {
        protected SectionRepository() { }

        [RepositoryQuery]
        public virtual SectionList GetByBookNameOwner(string name, int hasOwner)
        {
            var q = this.CreateLinqQuery();

            if (hasOwner == 1)
            {
                q = q.Where(e => e.Chapter.Book.Name == name && e.SectionOwnerId != null);
            }
            else if (hasOwner == 2)
            {
                q = q.Where(e => e.Chapter.Book.Name == name);
                q = q.Where(e => e.SectionOwnerId == null);
            }
            else
            {
                q = q.Where(e => e.Chapter.Book.Name == name);
            }

            return (SectionList)this.QueryData(q);
        }
    }

    internal class SectionConfig : UnitTestEntityConfig<Section>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapAllProperties();
        }
    }
}