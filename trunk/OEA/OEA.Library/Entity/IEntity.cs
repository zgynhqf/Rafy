using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SimpleCsla.Core;
using System.ComponentModel;
using SimpleCsla.Security;
using OEA.MetaModel;

namespace OEA.Library
{
    /// <summary>
    /// “实体”
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        int Id { get; set; }
    }

    /// <summary>
    /// 用以表示报表实体的唯一身份
    /// </summary>
    public interface IReportEntity
    {
        Object Id { get; }
    }
}
