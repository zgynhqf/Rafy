/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130422
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130422 16:46
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.DbMigration.Model;
using Rafy.DbMigration.SqlServer;
using EnvDTE;
using Rafy.Data;
using VSTemplates.Wizards;

namespace Rafy.VSPackage.Commands.MigrateOldDatabase
{
    class CodeFileGenerator
    {
        private ProjectItem _directory;
        private ProjectItem _repoDirectory;
        private string _domainBaseEntityName;

        public string DomainName { get; set; }
        public DbSetting DbSetting { get; set; }
        public ProjectItem Directory
        {
            get { return _directory; }
            set { _directory = value; }
        }
        public ProjectItem RepoDirectory
        {
            get { return _repoDirectory; }
            set { _repoDirectory = value; }
        }
        public int SuccessCount { get; private set; }

        /// <summary>
        /// 执行生成过程，如果产生了一些错误，则返回错误信息，方便提示用户。
        /// </summary>
        /// <returns></returns>
        public string Generate()
        {
            this.SuccessCount = 0;
            _error = null;

            var reader = new SqlServerMetaReader(this.DbSetting);
            var db = reader.Read();

            db.OrderByRelations();

            //类型的前缀是集合空间的最后一个。
            var baseEntityPrefix = this.DomainName;
            if (baseEntityPrefix.Contains('.'))
            {
                baseEntityPrefix = baseEntityPrefix.Substring(baseEntityPrefix.LastIndexOf('.') + 1);
            }
            _domainBaseEntityName = baseEntityPrefix + "Entity";

            foreach (var table in db.Tables)
            {
                var res = this.GenerateClassFile(table);
                if (res)
                {
                    this.SuccessCount++;

                    this.GenerateRepoFile(table);
                }
            }

            return _error;
        }

        private bool GenerateClassFile(Table table)
        {
            if (table.Name == "zzzDbMigrationVersion") return false;

            var entityName = ToEntityName(table);
            var fileName = entityName + ".cs";
            var item = _directory.ProjectItems.FindByName(fileName);
            if (item != null) return false;

            if (!CheckHasPK(table)) return false;

            _tablePropertiesConfig.Clear();

            var parameters = new ItemCodeTemplate.EntityFileCodeParamters()
            {
                domainNamespace = this.DomainName,
                domainEntityName = entityName,
                domainBaseEntityName = _domainBaseEntityName,
            };
            //生成属性、引用属性
            parameters.normalProperties = this.RenderNormalProperties(table);
            parameters.refProperties = this.RenderRefProperties(table);
            parameters.tableConfig = entityName == table.Name ?
                "Meta.MapTable().MapAllProperties();" :
                string.Format(@"Meta.MapTable(""{0}"").MapAllProperties();", table.Name);
            parameters.columnConfig = this.RenderColumnConfig(table);

            //使用模板格式化字符串。
            var code = ItemCodeTemplate.GetEntityFileCode(parameters);

            //写到文件，并加入到项目中。
            var file = Path.Combine(Path.GetDirectoryName(_directory.get_FileNames(1)), fileName);
            File.WriteAllText(file, code);
            _directory.ProjectItems.AddFromFile(file);
            return true;
        }

        private bool GenerateRepoFile(Table table)
        {
            var entityName = ToEntityName(table);
            var fileName = entityName + "Repository.cs";
            var item = _repoDirectory.ProjectItems.FindByName(fileName);
            if (item != null) return false;

            _tablePropertiesConfig.Clear();

            //使用模板格式化字符串。
            var code = ItemCodeTemplate.GetRepositoryFileCode(
                this.DomainName, this.DomainName + ".Repositories", entityName, _domainBaseEntityName
                );

            //写到文件，并加入到项目中。
            var file = Path.Combine(Path.GetDirectoryName(_repoDirectory.get_FileNames(1)), fileName);
            File.WriteAllText(file, code);
            _repoDirectory.ProjectItems.AddFromFile(file);
            return true;
        }

        private bool CheckHasPK(Table table)
        {
            //以下情况必须抛出异常，详情见：RenderNormalProperties 方法。
            var identites = FindIdentityColumns(table);
            var count = identites.Count();
            if (count != 1)
            {
                if (count == 0)
                {
                    AddError(string.Format("{0} 表中没有自增、命名以 Id 结尾的整形列。", table.Name));
                }
                else
                {
                    AddError(string.Format("{0} 表中自增长、命名并以 Id 结尾的整形列过多。", table.Name));
                }

                return false;
            }

            return true;
        }

        private string RenderRefProperties(Table table)
        {
            var code = new StringBuilder();

            foreach (var column in table.Columns)
            {
                if (column.IsForeignKey)
                {
                    //如果属性名以 Id 结尾，则直接去除 Id 即可。
                    //否则，映射失败，直接以列名作为引用属性的名称，添加列名 + Id 与属性名的映射。
                    string propertyName = string.Empty;
                    var columnName = column.Name;
                    if (columnName.ToLower().EndsWith("id"))
                    {
                        propertyName = columnName.Substring(0, columnName.Length - 2);
                    }
                    else
                    {
                        propertyName = columnName;
                    }

                    //处理一些类似于 RefEntity_Id 的字段，去掉最后的下划线。
                    while (propertyName[0] == '_') { propertyName = propertyName.Substring(1); }
                    while (propertyName[propertyName.Length - 1] == '_') { propertyName = propertyName.Substring(0, propertyName.Length - 1); }

                    //如果字段与属性名不同，需要添加属性的配置。
                    var idProperty = propertyName + "Id";
                    if (!idProperty.EqualsIgnoreCase(columnName))
                    {
                        _tablePropertiesConfig.Add(new PropertyConfig
                        {
                            PropertyName = idProperty,
                            ColumnName = columnName
                        });
                    }

                    var refEntity = ToEntityName(column.ForeignConstraint.PKTable);
                    var propertyCode = ItemCodeTemplate.GetRefPropertyCode(
                        ToEntityName(table), refEntity, propertyName,
                        isRequired: column.IsRequired
                        );
                    code.Append(propertyCode);
                }
            }

            return code.ToString();
        }

        #region RenderNormalProperties

        private IEnumerable<Column> FindIdentityColumns(Table table)
        {
            foreach (var column in table.Columns)
            {
                if (column.IsIdentity)
                {
                    var columnNameLower = column.Name.ToLower();
                    if (columnNameLower.EndsWith("id"))
                    {
                        yield return column;
                    }
                }
            }
        }

        private string RenderNormalProperties(Table table)
        {
            var code = new StringBuilder();

            //先输出 Id 字段。
            var idColumn = RenderId(table);

            //Id 字段以外的其它字段，都可以输出了。
            foreach (var column in table.Columns)
            {
                if (!column.IsForeignKey && column != idColumn)
                {
                    string propertyType = string.Empty;
                    #region 转换到属性的类型

                    switch (column.DbType)
                    {
                        case DbType.AnsiString:
                        case DbType.AnsiStringFixedLength:
                        case DbType.StringFixedLength:
                        case DbType.Xml:
                        case DbType.String:
                            propertyType = "string";
                            break;
                        case DbType.Int16:
                        case DbType.Int32:
                        case DbType.Int64:
                        case DbType.SByte:
                        case DbType.UInt16:
                        case DbType.UInt32:
                        case DbType.UInt64:
                            propertyType = "int";
                            if (!column.IsRequired) propertyType += '?';
                            break;
                        case DbType.VarNumeric:
                        case DbType.Single:
                        case DbType.Double:
                            propertyType = "double";
                            if (!column.IsRequired) propertyType += '?';
                            break;
                        case DbType.Decimal:
                            propertyType = "decimal";
                            if (!column.IsRequired) propertyType += '?';
                            break;
                        case DbType.Date:
                        case DbType.DateTime:
                        case DbType.DateTime2:
                        case DbType.DateTimeOffset:
                        case DbType.Time:
                            propertyType = "DateTime";
                            if (!column.IsRequired) propertyType += '?';
                            break;
                        case DbType.Boolean:
                            propertyType = "bool";
                            if (!column.IsRequired) propertyType += '?';
                            break;
                        case DbType.Binary:
                            propertyType = "byte[]";
                            break;
                        case DbType.Byte:
                            propertyType = "byte";
                            break;
                        case DbType.Guid:
                            propertyType = "Guid";
                            if (!column.IsRequired) propertyType += '?';
                            break;
                        case DbType.Object:
                        case DbType.Currency:
                        default:
                            propertyType = "string";
                            break;
                    }

                    #endregion

                    var propertyCode = ItemCodeTemplate.GetNormalPropertyCode(ToEntityName(table), propertyType, column.Name);
                    code.Append(propertyCode);
                }
            }

            return code.ToString();
        }

        private Column RenderId(Table table)
        {
            var identites = FindIdentityColumns(table);
            foreach (var identity in identites)
            {
                //这个属性则直接映射 Entity.Id 属性，不需要添加任何代码。
                //同时，如果在 Id 字样之前还有其它字母，则需要添加字段映射。
                if (identity.Name.ToLower() != "id")
                {
                    _tablePropertiesConfig.Add(new PropertyConfig
                    {
                        PropertyName = "Id",
                        ColumnName = identity.Name,
                        ClearIdPrimaryKey = !identity.IsPrimaryKey
                    });
                }

                //前面已经用 CheckHasPK 方法进行检测，所以必然有且只有一个字段满足这个条件，直接退出循环。
                return identity;
            }

            //之前已经用 CheckHasPK 方法检测过，这一行代码不可能到达。
            throw new NotSupportedException("未知错误。");
        }

        #endregion

        #region RenderColumnConfig

        private List<PropertyConfig> _tablePropertiesConfig = new List<PropertyConfig>();

        private string RenderColumnConfig(Table table)
        {
            var code = new StringBuilder();

            foreach (var config in _tablePropertiesConfig)
            {
                code.AppendFormat(@"
            Meta.Property({0}.{1}Property).MapColumn().HasColumnName(""{2}"")",
                    ToEntityName(table), config.PropertyName, config.ColumnName
                    );
                if (config.ClearIdPrimaryKey)
                {
                    code.Append(@".IsPrimaryKey(false)");
                }
                code.Append(';');
            }

            return code.ToString();
        }

        private class PropertyConfig
        {
            public string PropertyName, ColumnName;
            public bool ClearIdPrimaryKey;
        }

        #endregion

        private string ToEntityName(Table table)
        {
            var name = table.Name;
            if (name.EndsWith("s"))
            {
                if (name.EndsWith("aes") ||
                    name.EndsWith("oes") ||
                    name.EndsWith("ees") ||
                    name.EndsWith("ies") ||
                    name.EndsWith("ues"))
                {
                    //支持 es 后缀
                    name = name.Substring(0, name.Length - 2);
                }
                else
                {
                    //支持 s 后缀
                    name = name.Substring(0, name.Length - 1);
                }
            }
            return name;
        }

        private string _error;

        private void RaiseError(string error)
        {
            throw new MigrateOldDatabaseException(error);
        }

        private void AddError(string error)
        {
            _error += error;
            _error += Environment.NewLine;
        }
    }

    [Serializable]
    public class MigrateOldDatabaseException : Exception
    {
        public MigrateOldDatabaseException() { }
        public MigrateOldDatabaseException(string message) : base(message) { }
        public MigrateOldDatabaseException(string message, Exception inner) : base(message, inner) { }
        protected MigrateOldDatabaseException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}