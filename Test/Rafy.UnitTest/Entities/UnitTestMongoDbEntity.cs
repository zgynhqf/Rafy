/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20230303
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20230303 23:18
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
using Rafy.MongoDb;

namespace UT
{
    /// <summary>
    /// 使用 Mongodb 数据库的单元测试实体类
    /// </summary>
    public abstract class UnitTestMongoDbEntity : StringEntity { }

    public abstract class UnitTestMongoDbEntityList : InheritableEntityList { }

    public abstract class UnitTestMongoDbEntityRepository : EntityRepository
    {
        protected UnitTestMongoDbEntityRepository() { }
    }

    [DataProviderFor(typeof(UnitTestMongoDbEntityRepository))]
    public class UnitTestMongoDbEntityRepositoryDataProvider : MongoDbDataProvider
    {
        public static string DbSettingName = "Test_MongoDb";

        protected override string ConnectionStringSettingName
        {
            get { return DbSettingName; }
        }

        public UnitTestMongoDbEntityRepositoryDataProvider()
        {
            this.EnumSerializationMode = Rafy.Domain.Serialization.Json.EnumSerializationMode.String;
        }
    }

    public abstract class UnitTestMongoDbEntityConfig<TEntity> : EntityConfig<TEntity> { }
}
