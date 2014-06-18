/*******************************************************
 * 
 * 作者：CodeProject
 * 创建时间：2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 CodeProject 2009
 * 
*******************************************************/

using System;

namespace Rafy.Domain.ORM
{
    /// <summary>
    /// 数据表列
    /// </summary>
    public interface IColumn
    {
        /// <summary>
        /// 对应的表
        /// </summary>
        ITable Table { get; }

        /// <summary>
        /// 列名
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 数据类型
        /// </summary>
        Type DataType { get; }

        /// <summary>
        /// 是否为自增长主键列。
        /// </summary>
        bool IsIdentity { get; }

        /// <summary>
        /// 根据实体获取该列的值
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        object GetValue(Entity obj);

        /// <summary>
        /// 设置该列的值到实体上。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="val"></param>
        void LoadValue(Entity obj, object val);
    }
}