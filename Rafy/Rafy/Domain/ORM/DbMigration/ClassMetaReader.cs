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
using System.Reflection;
using System.Text;
using Rafy.Data;
using Rafy.DbMigration;
using Rafy.DbMigration.Model;
using Rafy.DbMigration.SqlServer;
using Rafy.Domain;
using Rafy.Domain.ORM.SqlServer;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.Reflection;

namespace Rafy.Domain.ORM.DbMigration
{
    /// <summary>
    /// 从 Rafy 元数据中读取整个数据库的元数据。
    /// </summary>
    public class ClassMetaReader : IDestinationDatabaseReader
    {
        private DbSetting _dbSetting;

        internal ClassMetaReader(DbSetting dbSetting)
        {
            this._dbSetting = dbSetting;
            this.IgnoreTables = new List<string>();
            this.EntityDbSettingName = this._dbSetting.Name;
        }

        /// <summary>
        /// 需要忽略的表的表名的集合。
        /// </summary>
        public List<string> IgnoreTables { get; private set; }

        /// <summary>
        /// 是否需要同时读取出相应的注释。
        /// </summary>
        public bool ReadComment { get; set; }

        /// <summary>
        /// 额外的一些属性注释的字典。
        /// Key:属性名。
        /// Value:注释值。
        /// </summary>
        public Dictionary<string, string> AdditionalPropertiesComments { get; set; }

        /// <summary>
        /// 读取整个类型对应的数据库的元数据。
        /// </summary>
        /// <returns></returns>
        public DestinationDatabase Read()
        {
            var tableEntityTypes = this.GetMappingEntityTypes();

            var result = new DestinationDatabase(this._dbSetting.Database);
            foreach (var item in this.IgnoreTables) { result.IgnoreTables.Add(item); }

            if (tableEntityTypes.Count == 0)
            {
                result.Removed = true;
            }
            else
            {
                var reader = new TypesMetaReader
                {
                    Database = result,
                    Entities = tableEntityTypes,
                    ReadComment = this.ReadComment,
                    IsGeneratingForeignKey = this.IsGeneratingForeignKey,
                    AdditionalPropertiesComments = this.AdditionalPropertiesComments
                };
                reader.Read();
            }

            return result;
        }

        private List<EntityMeta> GetMappingEntityTypes()
        {
            var tableEntityTypes = new List<EntityMeta>();

            //程序集列表，生成数据库会反射找到程序集内的实体类型进行数据库映射
            foreach (var assembly in RafyEnvironment.AllPlugins)
            {
                foreach (var type in assembly.Assembly.GetTypes())
                {
                    if (!type.IsAbstract)
                    {
                        //判断实体类型是否映射了某一个数据库
                        var em = CommonModel.Entities.Find(type);
                        if (em != null && em.TableMeta != null)
                        {
                            var entityDb = RdbDataProvider.Get(RF.Find(type)).ConnectionStringSettingName;
                            if (entityDb == EntityDbSettingName)
                            {
                                tableEntityTypes.Add(em);
                            }
                        }
                    }
                }
            }

            return tableEntityTypes;
        }

        DestinationDatabase IDestinationDatabaseReader.Read()
        {
            return this.Read();
        }

        Database IMetadataReader.Read()
        {
            return this.Read();
        }

        private class TypesMetaReader
        {
            private bool _readComment;

            private CommentFinder _commentFinder = new CommentFinder();

            internal DestinationDatabase Database;

            internal List<EntityMeta> Entities;

            internal bool ReadComment
            {
                get { return _readComment; }
                set { _readComment = value; }
            }

            /// <summary>
            /// 额外的一些属性注释的字典。
            /// Key:属性名。
            /// Value:注释值。
            /// </summary>
            internal Dictionary<string, string> AdditionalPropertiesComments { get; set; }

            /// <summary>
            /// 临时存储在这个列表中，最后再整合到 Database 中。
            /// </summary>
            private IList<ForeignConstraintInfo> _foreigns = new List<ForeignConstraintInfo>();

            internal void Read()
            {
                _commentFinder.AdditionalPropertiesComments = this.AdditionalPropertiesComments;

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
            /// <param name="em">The memory.</param>
            /// <exception cref="System.ArgumentNullException"></exception>
            /// <exception cref="System.InvalidOperationException">refMeta.ReferenceInfo == null</exception>
            private void BuildTable(EntityMeta em)
            {
                var tableMeta = em.TableMeta;

                //视图类不需要支持数据库迁移。
                if (tableMeta.IsMappingView) { return; }

                var table = new Table(tableMeta.TableName, this.Database);

                //读取实体的注释
                if (_readComment)
                {
                    table.Comment = _commentFinder.TryFindComment(em.EntityType);
                }

                var metaProperties = em.EntityProperties;

                //var managedProperties = ManagedPropertyRepository.Instance
                //    .GetTypePropertiesContainer(em.EntityType)
                //    .GetNonReadOnlyCompiledProperties();

                foreach (var property in metaProperties)
                {
                    var columnMeta = property.ColumnMeta;
                    if (columnMeta == null) continue;

                    var mp = property.ManagedProperty;
                    if (mp == null) { throw new ArgumentNullException(string.Format("{0}.{1} 属性需要使用托管属性进行编写。", em.Name, mp.Name)); }

                    //列名
                    var propertyName = property.Name;
                    var columnName = columnMeta.ColumnName;
                    if (string.IsNullOrWhiteSpace(columnName)) columnName = propertyName;

                    //类型
                    var propertyType = property.PropertyType;
                    bool isNullableRef = false;

                    #region 引用关系

                    if (columnMeta.HasFKConstraint)
                    {
                        var refProperty = mp as IRefProperty;
                        if (refProperty != null)
                        {
                            isNullableRef = refProperty.Nullable;

                            //是否生成外键
                            // 默认 IsGeneratingForeignKey 为 true
                            if (IsGeneratingForeignKey)
                            {
                                var refMeta = em.Property(refProperty.RefEntityProperty);
                                if (refMeta.ReferenceInfo == null)
                                    throw new InvalidOperationException("refMeta.ReferenceInfo == null");

                                //引用实体的类型。
                                var refTypeMeta = refMeta.ReferenceInfo.RefTypeMeta;
                                if (refTypeMeta != null)
                                {
                                    var refTableMeta = refTypeMeta.TableMeta;
                                    if (refTableMeta != null)
                                    {
                                        //如果主键表已经被忽略，那么到这个表上的外键也不能建立了。
                                        //这是因为被忽略的表的结构是未知的，不一定是以这个字段为主键。
                                        if (!this.Database.IsIgnored(refTableMeta.TableName))
                                        {
                                            var id = refTypeMeta.Property(Entity.IdProperty);
                                            //有时一些表的 Id 只是自增长，但并不是主键，不能创建外键。
                                            if (id.ColumnMeta.IsPrimaryKey)
                                            {
                                                this._foreigns.Add(new ForeignConstraintInfo()
                                                {
                                                    FkTableName = tableMeta.TableName,
                                                    PkTableName = refTableMeta.TableName,
                                                    FkColumn = columnName,
                                                    PkColumn = id.ColumnMeta.ColumnName,
                                                    NeedDeleteCascade =
                                                        refProperty.ReferenceType == ReferenceType.Parent
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (IsGeneratingForeignKey && mp == Entity.TreePIdProperty)
                        {
                            var id = em.Property(Entity.IdProperty);
                            //有时一些表的 Id 只是自增长，但并不是主键，不能创建外键。
                            if (id.ColumnMeta.IsPrimaryKey)
                            {
                                this._foreigns.Add(new ForeignConstraintInfo()
                                {
                                    FkTableName = tableMeta.TableName,
                                    PkTableName = tableMeta.TableName,
                                    FkColumn = columnName,
                                    PkColumn = id.ColumnMeta.ColumnName,
                                    NeedDeleteCascade = false
                                });
                            }
                        }
                    }

                    #endregion

                    var dataType = TypeHelper.IgnoreNullable(propertyType);
                    //对于支持多数据类型的 Id、TreePId 属性进行特殊处理。
                    if (mp == Entity.IdProperty || mp == Entity.TreePIdProperty)
                    {
                        dataType = em.IdType;
                    }
                    var dbType = columnMeta.DataType.GetValueOrDefault(DbTypeHelper.ConvertFromCLRType(dataType));
                    var column = new Column(columnName, dbType, columnMeta.DataTypeLength, table);
                    if (columnMeta.IsRequired.HasValue)
                    {
                        column.IsRequired = columnMeta.IsRequired.Value;
                    }
                    else
                    {
                        column.IsRequired = !isNullableRef && !propertyType.IsClass && !TypeHelper.IsNullable(propertyType);
                    }
                    //IsPrimaryKey 的设置放在 IsRequired 之后，可以防止在设置可空的同时把列调整为非主键。
                    column.IsPrimaryKey = columnMeta.IsPrimaryKey;
                    column.IsIdentity = columnMeta.IsIdentity;

                    table.Columns.Add(column);

                    //读取属性的注释。
                    if (_readComment)
                    {
                        var commentProperty = mp;
                        var refProperty = commentProperty as IRefProperty;
                        if (refProperty != null)
                        {
                            commentProperty = refProperty.RefEntityProperty;
                        }

                        column.Comment = _commentFinder.TryFindComment(commentProperty);
                    }
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
                            existingTable.Columns.Add(new Column(newColumn.Name, newColumn.DataType, newColumn.Length, existingTable)
                            {
                                IsRequired = newColumn.IsRequired,
                                IsPrimaryKey = newColumn.IsPrimaryKey,
                                IsIdentity = newColumn.IsIdentity
                            });
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
                    //外键表必须找到，否则这个外键不会加入到集合中。
                    var fkTable = this.Database.FindTable(foreign.FkTableName);
                    var fkColumn = fkTable.FindColumn(foreign.FkColumn);

                    var pkTable = this.Database.FindTable(foreign.PkTableName);
                    //有可能这个引用的表并不在这个数据库中，此时不需要创建外键。
                    if (pkTable != null)
                    {
                        //找到主键列，创建引用关系
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

            private bool _isGeneratingForeignKey = true;

            /// <summary>
            /// 是否生成外键，默认true 
            /// </summary>
            internal bool IsGeneratingForeignKey
            {
                get { return _isGeneratingForeignKey; }
                set { _isGeneratingForeignKey = value; }
            }
        }

        /// <summary>
        /// 此属性用于指定需要读取的实体集合对应的数据库配置名称。
        /// 默认值：将要生成的数据库的配置名。
        /// 当需要生成的数据库的配置名与实体集合的数据库配置名不一致时，可以摄者此属性来指定实体集合对应的数据库配置名称。
        /// </summary>
        public string EntityDbSettingName { get; set; }

        private bool _isGeneratingForeignKey = true;

        /// <summary>
        /// 是否生成外键，默认true 
        /// </summary>
        public bool IsGeneratingForeignKey
        {
            get { return _isGeneratingForeignKey; }
            set { _isGeneratingForeignKey = value; }
        }
    }
}