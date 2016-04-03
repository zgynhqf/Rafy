using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Rafy.Data
{
    /// <summary>
    /// A factory to create IDbDataParameter by some specific conditions
    /// </summary>
    public interface IDbParameterFactory
    {
        /// <summary>
        /// Create a DBParameter
        /// </summary>
        /// <returns></returns>
        IDbDataParameter CreateParameter();
        /// <summary>
        /// Create a DBParameter
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IDbDataParameter CreateParameter(string name);
        /// <summary>
        /// Create a DBParameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        IDbDataParameter CreateParameter(string name, object value);
        /// <summary>
        /// Create a DBParameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        IDbDataParameter CreateParameter(string name, object value, DbType type);
        /// <summary>
        /// Create a DBParameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        IDbDataParameter CreateParameter(string name, object value, ParameterDirection direction);
        /// <summary>
        /// Create a DBParameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        IDbDataParameter CreateParameter(string name, object value, DbType type, int size);
        /// <summary>
        /// Create a DBParameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        IDbDataParameter CreateParameter(string name, object value, DbType type, ParameterDirection direction);
        /// <summary>
        /// Create a DBParameter
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <param name="size"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        IDbDataParameter CreateParameter(string name, object value, DbType type, int size, ParameterDirection direction);
    }
}
