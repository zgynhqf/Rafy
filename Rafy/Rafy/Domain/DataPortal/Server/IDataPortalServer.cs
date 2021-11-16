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
    /// 数据调用的提供程序
    /// </summary>
    public interface IDataPortalServer
    {
        /// <summary>
        /// 调用指定对象的指定方法，并返回其对应的返回值。
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        DataPortalResult Call(object obj, string method, object[] parameters, DataPortalContext context);
    }
}