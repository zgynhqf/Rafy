/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Rafy.Utils;

namespace Rafy.MetaModel
{
    /// <summary>
    /// 属性对应的列的元数据
    /// </summary>
    public class ColumnMeta : Freezable
    {
        private bool _HasFKConstraint;
        /// <summary>
        /// 如果这是一个引用属性的列，则这个属性表示数据库中是否有对应的外键存在（引用属性也可以不映射外键）。
        /// </summary>
        public bool HasFKConstraint
        {
            get { return this._HasFKConstraint; }
            set { this.SetValue(ref this._HasFKConstraint, value); }
        }

        private bool _IsIdentity;
        /// <summary>
        /// 是否自增长列
        /// </summary>
        public bool IsIdentity
        {
            get { return this._IsIdentity; }
            set { this.SetValue(ref this._IsIdentity, value); }
        }

        private bool _IsPrimaryKey;
        /// <summary>
        /// 是否主键。主键有以下区别：
        /// * 数据库迁移框架，生成表时，将为其生成主键约束；
        /// * 数据库迁移框架，生成外键时，将需要使用到被引用表的主键（无主键不生成）；
        /// * 在实体更新时，主键的值不支持被更新，而是作为 where 条件使用；
        /// * 生成 Sql 语句中的 JOIN 时，需要使用到主键；
        /// * 生成 Sql 语句时，将作为默认的排序语句；
        /// * 查询实体的某个值时，将使用主键作为条件来查询；
        /// * 删除某个实体时，将使用主键作为条件来过滤；
        /// </summary>
        public bool IsPrimaryKey
        {
            get { return this._IsPrimaryKey; }
            set
            {
                this.SetValue(ref this._IsPrimaryKey, value);

                //是主键，则肯定是必须的
                if (value)
                {
                    this.IsRequired = true;
                }
            }
        }

        private bool? _IsRequired;
        /// <summary>
        /// 是否必填列。如果没有赋值，则按照默认的类型计算方法来计算该值。
        /// </summary>
        public bool? IsRequired
        {
            get { return this._IsRequired; }
            set
            {
                this.SetValue(ref this._IsRequired, value);

                //如果不是必须的，则肯定不是主键
                if (value.HasValue && !value.Value)
                {
                    this.IsPrimaryKey = false;
                }
            }
        }

        private bool _HasIndex;
        /// <summary>
        /// 该列是否拥有索引。
        /// 对于有索引的列，在查询时，会设置查询的参数的类型为 <see cref="DbType"/>，这样索引才会起作用。
        /// </summary>
        public bool HasIndex
        {
            get { return this._HasIndex; }
            set { this.SetValue(ref this._HasIndex, value); }
        }

        private string _ColumnName;
        /// <summary>
        /// 映射数据库中的字段名。此属性不为空。
        /// </summary>
        public string ColumnName
        {
            get { return this._ColumnName; }
            set { this.SetValue(ref this._ColumnName, value); }
        }

        private DbType? _DbType;
        /// <summary>
        /// 映射数据库中的字段的类型。
        /// 如果没有设置，则使用默认的映射规则。
        /// </summary>
        public DbType? DbType
        {
            get { return this._DbType; }
            set { this.SetValue(ref this._DbType, value); }
        }

        private string _DbTypeLength;
        /// <summary>
        /// 映射数据库中的字段的长度、精度等信息。
        /// 可以是数字，也可以是 MAX 等字符串。
        /// 如果是空，则表示使用默认的长度。
        /// </summary>
        public string DbTypeLength
        {
            get { return this._DbTypeLength; }
            set { this.SetValue(ref this._DbTypeLength, value); }
        }

        private ReferenceValuePath _RefValuePath;
        /// <summary>
        /// 如果这个属性不为 null，表示该属性从哪个引用路径获取值。
        /// </summary>
        public ReferenceValuePath RefValuePath
        {
            get { return this._RefValuePath; }
            set { this.SetValue(ref this._RefValuePath, value); }
        }

        private ReferenceValueDataMode? _RefValueDataMode;
        /// <summary>
        /// 如果属性 <see cref="RefValuePath"/> 不为 null，则这里表示引用值的获取方式
        /// </summary>
        public ReferenceValueDataMode? RefValueDataMode
        {
            get { return this._RefValueDataMode; }
            set { this.SetValue(ref this._RefValueDataMode, value); }
        }

        /// <summary>
        /// 返回这个对象，是否真正在映射表的字段。
        /// 如果是 true，则在生成表、插入、更新时，都会有相应的列。
        /// </summary>
        /// <returns></returns>
        internal bool MappingRealColumn()
        {
            return _RefValueDataMode != ReferenceValueDataMode.ReadJoinTable;
        }

        internal bool IsFromJoin()
        {
            return _RefValueDataMode == ReferenceValueDataMode.ReadJoinTable;
        }
    }

    /// <summary>
    /// 引用关系中的引用值属性，它的值的处理、获取方式。
    /// </summary>
    public enum ReferenceValueDataMode
    {
        /// <summary>
        /// 采用实时读取的方式。
        /// 这种模式下，当前实体对应的表中没有字段。
        /// 在读取实体时，会在 sql 中生成 join 语句，并把关系表中的值读取过来。
        /// </summary>
        ReadJoinTable,
        /// <summary>
        /// 采用冗余的方式。
        /// 这种模式下，会在本实体对应的表中生成字段，并将引用关系表属性的值直接拷贝到字段中，作为一份冗余。
        /// 这个冗余属性的值，会在引用关系表属性的值发生改变时，自动更新。（自动更新只在通过框架变更值时触发；开发者通过其它方式直接修改数据库时，无法触发。）
        /// </summary>
        Redundancy
    }
}
