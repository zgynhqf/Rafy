/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20170921
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20170921 23:24
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rafy.Reflection;

namespace Rafy.DbMigration
{
    /// <summary>
    /// 数据库字段类型的转换器。
    /// </summary>
    public abstract class DbTypeConverter
    {
        /// <summary>
        /// 将 DbType 转换为数据库中的列的类型名称。
        /// </summary>
        /// <param name="fieldType"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public abstract string ConvertToDatabaseTypeName(DbType fieldType, string length = null);

        /// <summary>
        /// 将从数据库 Schema Meta 中读取出来的列的类型名称，转换为其对应的 DbType。
        /// </summary>
        /// <param name="databaseTypeName">从数据库 Schema Meta 中读取出来的列的类型名称。</param>
        /// <returns></returns>
        public abstract DbType ConvertToDbType(string databaseTypeName);

        /// <summary>
        /// 返回 CLR 类型默认映射的数据库的类型。
        /// </summary>
        /// <param name="clrType"></param>
        /// <returns></returns>
        public virtual DbType FromClrType(Type clrType)
        {
            if (clrType.IsEnum) { return DbType.Int32; }
            if (clrType == typeof(string)) { return DbType.String; }
            if (clrType == typeof(int)) { return DbType.Int32; }
            if (clrType == typeof(long)) { return DbType.Int64; }
            if (clrType == typeof(bool)) { return DbType.Boolean; }
            if (clrType == typeof(DateTime)) { return DbType.DateTime; }
            if (clrType == typeof(DateTimeOffset)) { return DbType.DateTimeOffset; }
            if (clrType == typeof(Guid)) { return DbType.Guid; }
            if (clrType == typeof(double)) { return DbType.Double; }
            if (clrType == typeof(byte)) { return DbType.Byte; }
            if (clrType == typeof(short)) { return DbType.Int16; }
            if (clrType == typeof(char)) { return DbType.StringFixedLength; }
            if (clrType == typeof(decimal)) { return DbType.Decimal; }
            if (clrType == typeof(float)) { return DbType.Single; }
            if (clrType == typeof(uint)) { return DbType.UInt32; }
            if (clrType == typeof(ulong)) { return DbType.UInt64; }
            if (clrType == typeof(ushort)) { return DbType.UInt16; }
            if (clrType == typeof(sbyte)) { return DbType.SByte; }
            if (clrType == typeof(byte[])) { return DbType.Binary; }

            if (TypeHelper.IsNullable(clrType))
            {
                return this.FromClrType(TypeHelper.IgnoreNullable(clrType));
            }

            return DbType.String;
        }

        /// <summary>
        /// 将指定的值转换为一个兼容数据库类型的值。
        /// 该值可用于与下层的 ADO.NET 交互。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual object ToDbParameterValue(object value)
        {
            return value ?? DBNull.Value;
        }

        /// <summary>
        /// 将指定的值转换为一个 CLR 类型的值。
        /// </summary>
        /// <param name="dbValue">The database value.</param>
        /// <param name="clrType">Type of the color.</param>
        /// <returns></returns>
        public virtual object ToClrValue(object dbValue, Type clrType)
        {
            return dbValue == DBNull.Value ? null : dbValue;
        }

        /// <summary>
        /// 获取指定的数据库字段类型所对应的默认值。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal virtual object GetDefaultValue(DbType type)
        {
            switch (type)
            {
                case DbType.String:
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.Xml:
                    return string.Empty;
                case DbType.Date:
                case DbType.Time:
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return new DateTime(2000, 1, 1, 0, 0, 0);
                case DbType.Guid:
                    return Guid.Empty;
                case DbType.Boolean:
                case DbType.Byte:
                case DbType.Int32:
                case DbType.Int64:
                case DbType.Single:
                case DbType.Double:
                case DbType.Decimal:
                case DbType.VarNumeric:
                case DbType.UInt16:
                case DbType.UInt32:
                case DbType.UInt64:
                case DbType.SByte:
                case DbType.Currency:
                    return 0;
                case DbType.Binary:
                    return new byte[0];
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 由于不同的 DbType 映射到库中后的类型可能是相同的，所以这里需要对类型进行兼容性判断。
        /// </summary>
        /// <param name="oldColumnType"></param>
        /// <param name="newColumnType"></param>
        /// <returns></returns>
        internal virtual bool IsCompatible(DbType oldColumnType, DbType newColumnType)
        {
            if (oldColumnType == newColumnType) return true;

            //如果两个列都属性同一类型的数据库类型，这里也表示库的类型没有变化。
            for (int i = 0, c = CompatibleTypes.Length; i < c; i++)
            {
                var sameTypes = CompatibleTypes[i];
                if (sameTypes.Contains(oldColumnType) && sameTypes.Contains(newColumnType))
                {
                    return true;
                }
            }

            return false;
        }

        private static DbType[][] CompatibleTypes = new DbType[][]{
            new DbType[]{ DbType.String, DbType.AnsiString, DbType.Xml },
            new DbType[]{ DbType.Int64, DbType.Double, DbType.Decimal },
            new DbType[]{ DbType.Date, DbType.Time, DbType.DateTime, DbType.DateTimeOffset },
        };
    }
}
