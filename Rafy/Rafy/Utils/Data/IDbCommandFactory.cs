/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20150825
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.5
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20150825 10:57
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.Data
{
    /// <summary>
    /// A factory to create IDbCommand.
    /// </summary>
    public interface IDbCommandFactory
    {
        /// <summary>
        /// Create a command by sql,type and parameters
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="type"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IDbCommand CreateCommand(string sql, CommandType type, params IDbDataParameter[] parameters);
    }
}
