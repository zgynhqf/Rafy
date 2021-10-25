/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140515
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140515 20:18
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
    public abstract class StringTestEntity : StringEntity
    {
    }

    public abstract class StringTestIntEntity : IntEntity
    {
    }

    public abstract class StringTestLongEntity : LongEntity
    {
    }

    public abstract class StringTestEntityList : EntityList { }

    public abstract class StringTestEntityRepository : EntityRepository
    {
        protected StringTestEntityRepository() { }
    }

    [DataProviderFor(typeof(StringTestEntityRepository))]
    public class StringTestEntityDataProvider : RdbDataProvider
    {
        public static readonly string DbSettingName = "Test_StringEntityTest";

        protected override string ConnectionStringSettingName
        {
            get { return DbSettingName; }
        }
    }

    public abstract class StringTestEntityConfig<TEntity> : EntityConfig<TEntity> { }
}