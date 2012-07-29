/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120629 11:04
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120629 11:04
 * 
*******************************************************/

using System.Collections.Generic;

namespace OEA.ORM
{
    /// <summary>
    /// Where 条件项
    /// </summary>
    internal interface IWhereConstraint
    {
        /// <summary>
        /// 获取 Where 条件对应的 SQL
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        string GetSql(FormatSqlParameter paramaters);
    }
}