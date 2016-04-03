/*******************************************************
 * 
 * 作者：CSLA
 * 创建日期：2008
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 周金根 2008
 * 
*******************************************************/

using System;

namespace Rafy.Domain.DataPortal
{
    /// <summary>
    /// Interface implemented by server-side data portal
    /// components.
    /// </summary>
    public interface IDataPortalServer
    {
        /// <summary>
        /// Get an existing business object.
        /// </summary>
        /// <param name="objectType">Type of business object to retrieve.</param>
        /// <param name="criteria">Criteria object describing business object.</param>
        /// <param name="context"><see cref="DataPortalContext" /> object passed to the server.</param>
        /// <returns></returns>
        DataPortalResult Fetch(Type objectType, object criteria, DataPortalContext context);

        /// <summary>
        /// Update a business object.
        /// </summary>
        /// <param name="obj">Business object to update.</param>
        /// <param name="context"><see cref="DataPortalContext" /> object passed to the server.</param>
        /// <returns></returns>
        DataPortalResult Update(object obj, DataPortalContext context);
    }
}