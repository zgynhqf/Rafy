/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120102
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120102
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbMigration.Model;
using System.Reflection;
using OEA.MetaModel;
using OEA.ManagedProperty;
using OEA.ORM.SqlServer;
using DbMigration.SqlServer;
using hxy.Common.Data;
using DbMigration;

namespace OEA.Library.ORM.DbMigration
{
    /// <summary>
    /// 从 OEA 元数据中读取整个数据库的元数据。
    /// </summary>
    public class ClassMetaReader : IDestinationDatabaseReader
    {
        private DbSetting _dbSetting;

        public ClassMetaReader(DbSetting dbSetting)
        {
            this._dbSetting = dbSetting;
            this.IgnoreTables = new List<string>();
        }

        public List<string> IgnoreTables { get; private set; }

        public DestinationDatabase Read()
        {
            var tableEntityTypes = this.GetMappingEntityTypes();

            var result = new DestinationDatabase(this._dbSetting.Database);

            if (tableEntityTypes.Count == 0)
            {
                result.Removed = true;
            }
            else
            {
                var reader = new TypesMetaReader
                {
                    Database = result,
                    Entities = tableEntityTypes
                };
                reader.Read();
            }

            foreach (var item in this.IgnoreTables) { result.IgnoreTables.Add(item); }

            return result;
        }

        private List<EntityMeta> GetMappingEntityTypes()
        {
            var tableEntityTypes = new List<EntityMeta>();

            //程序集列表，生成数据库会反射找到程序集内的实体类型进行数据库映射
            foreach (var assembly in OEAEnvironment.GetAllLibraries())
            {
                foreach (var type in assembly.Assembly.GetTypes())
                {
                    if (!type.IsAbstract)
                    {
                        //判断实体类型是否映射了某一个数据库
                        var em = CommonModel.Entities.Find(type);
                        if (em != null)
                        {
                            if (em.TableMeta != null)
                            {
                                var entityDb = RF.Create(type).DbSetting.Name;
                                if (entityDb == this._dbSetting.Name)
                                {
                                    tableEntityTypes.Add(em);
                                }
                            }
                        }
                    }
                }
            }

            return tableEntityTypes;
        }

        Database IMetadataReader.Read()
        {
            return this.Read();
        }

        private class TypesMetaReader
        {
            internal DestinationDatabase Database;

            internal List<EntityMeta> Entities;

            /// <summary>
            /// 临时存储在这个列表中，最后再整合到 Database 中。
            /// </summary>
            private IList<ForeignConstraintInfo> _foreigns = new List<ForeignConstraintInfo>();

            internal DestinationDatabase Result
            {
                get { return this.Database; }
            }

            internal void Read()
            {
                foreach (var meta in Entities)
                {
                    this.BuildTable(meta);
                }

                //在所有关系完成之后在创建外键的元数据
                this.BuildFKRelations();
            }

            /// <summary>
            /// 根据实体类型创建表的描述信息，并添加到数据库中
            /// </summary>
            /// <param name="meta"></param>
            private void BuildTable(EntityMeta em)
            {
                var tableMeta = em.TableMeta;
                if (!tableMeta.SupportMigrating) { this.Database.IgnoreTables.Add(tableMeta.TableName); }

                var table = new Table(tableMeta.TableName, this.Database);

                var metaProperties = em.EntityProperties;

                var managedProperties = ManagedPropertyRepository.Instance
                    .GetTypePropertiesContainer(em.EntityType)
                    .GetNonReadOnlyCompiledProperties();

                foreach (var property in metaProperties)
                {
                    var columnMeta = property.ColumnMeta;
                    if (columnMeta == null) continue;
                    var propertyName = property.Name;

                    var mp = managedProperties.FirstOrDefault(p => p.GetMetaPropertyName(em.EntityType) == propertyName);
                    if (mp == null) { throw new ArgumentNullException(string.Format("{0}.{1} 属性需要使用托管属性进行编写。", em.Name, propertyName)); }

                    //列名
                    var columnName = columnMeta.ColumnName;
                    if (string.IsNullOrWhiteSpace(columnName)) columnName = propertyName;

                    //类型
                    var dataType = mp.PropertyType;

                    //引用关系
                    if (property.ReferenceInfo != null)
                    {
                        var refProperty = mp.CastTo<IRefProperty>();
                        dataType = refProperty.GetMeta(em.EntityType).Nullable ? typeof(int?) : typeof(int);

                        var refTypeMeta = property.ReferenceInfo.RefTypeMeta;
                        if (refTypeMeta != null)
                        {
                            var refTableMeta = refTypeMeta.TableMeta;
                            if (refTableMeta != null)
                            {
                                this._foreigns.Add(new ForeignConstraintInfo()
                                {
                                    FkTableName = tableMeta.TableName,
                                    PkTableName = refTableMeta.TableName,
                                    FkColumn = columnName,
                                    PkColumn = DBConvention.FieldName_Id,
                                    NeedDeleteCascade = property.ReferenceInfo.Type == ReferenceType.Parent
                                });
                            }
                        }
                    }
                    else if (columnName == DBConvention.FieldName_PId || columnName == DBConvention.FieldName_TreePId)
                    {
                        this._foreigns.Add(new ForeignConstraintInfo()
                        {
                            FkTableName = tableMeta.TableName,
                            PkTableName = tableMeta.TableName,
                            FkColumn = columnName,
                            PkColumn = DBConvention.FieldName_Id,
                            NeedDeleteCascade = false
                        });
                    }

                    var column = new Column(DbTypeHelper.ConvertFromCLRType(dataType), columnName, table)
                    {
                        IsPrimaryKey = columnMeta.IsPKID
                    };
                    if (columnMeta.IsRequired.HasValue)
                    {
                        column.IsRequired = columnMeta.IsRequired.Value;
                    }
                    else
                    {
                        //字符串都是可空的。
                        column.IsRequired = dataType != typeof(string) &&
                            (!dataType.IsGenericType || dataType.GetGenericTypeDefinition() != typeof(Nullable<>));
                    }

                    table.Columns.Add(column);
                }

                table.SortColumns();

                this.AddTable(table);
            }

            /// <summary>
            /// 将表添加到数据库中，对于已经存在的表进行全并
            /// </summary>
            /// <param name="table"></param>
            private void AddTable(Table table)
            {
                var existingTable = this.Database.FindTable(table.Name);
                if (existingTable != null)
                {
                    //由于有类的继承关系存在，合并两个表的所有字段。
                    foreach (var newColumn in table.Columns)
                    {
                        if (existingTable.FindColumn(newColumn.Name) == null)
                        {
                            existingTable.Columns.Add(newColumn);
                        }
                    }
                }
                else
                {
                    this.Database.Tables.Add(table);
                }
            }

            /// <summary>
            /// 构造外键的描述，并创建好与数据库、表、列的相关依赖关系
            /// </summary>
            private void BuildFKRelations()
            {
                foreach (var foreign in this._foreigns)
                {
                    var fkTable = this.Database.FindTable(foreign.FkTableName);
                    var fkColumn = fkTable.FindColumn(foreign.FkColumn);

                    var pkTable = this.Database.FindTable(foreign.PkTableName);
                    //有可能这个引用的表并不在这个数据库中，此时不需要创建外键。
                    if (pkTable != null)
                    {
                        var pkColumn = pkTable.FindColumn(foreign.PkColumn);

                        fkColumn.ForeignConstraint = new ForeignConstraint(pkColumn)
                        {
                            NeedDeleteCascade = foreign.NeedDeleteCascade
                        };
                    }
                }
            }

            /// <summary>
            /// 简单描述外键约束的信息，在表构建完成后，用些信息构造外键约束
            /// </summary>
            private class ForeignConstraintInfo
            {
                public string FkTableName, PkTableName, FkColumn, PkColumn;
                public bool NeedDeleteCascade;
            }
        }
    }
}