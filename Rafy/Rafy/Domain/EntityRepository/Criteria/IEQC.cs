﻿/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：201303
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 201303
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain
{
    /// <summary>
    /// Internal Entity Query Criteria
    /// 实体查询的参数。一个多参数查询的对象容器。
    /// 使用缩写，是因为其名称用于传输，这样可以减少传输量。
    /// </summary>
    [Serializable]
    internal class IEQC : IEntityQueryInvocation
    {
        /// <summary>
        /// 当前查询正在使用的单一条件。
        /// </summary>
        internal static readonly ContextItem<object> CurrentItem = new ThreadContextItem<object>("Rafy.Domain.IEQC.CurrentItem");

        /// <summary>
        /// 当前正在使用的查询参数
        /// </summary>
        internal static IEQC Current
        {
            get
            {
                var ieqc = CurrentItem.Value as IEQC;
                if (ieqc == null) throw new InvalidProgramException("实体查询时必须使用正确的格式，查询方法必须是虚方法，并添加 RepositoryQuery 标记，否则无法判断查询中的返回值。");
                return ieqc;
            }
        }

        private object[] _p;
        private string _m;
        private byte _f = (byte)RepositoryQueryType.List;

        /// <summary>
        /// 所有的参数。
        /// </summary>
        public object[] Parameters
        {
            get { return _p; }
            set { _p = value; }
        }

        /// <summary>
        /// 数据层查询方法。如果为空，表示使用约定的数据层方法。
        /// </summary>
        public string MethodName
        {
            get { return _m; }
            set { _m = value; }
        }

        /// <summary>
        /// 获取数据的类型。
        /// </summary>
        public RepositoryQueryType QueryType
        {
            get { return (RepositoryQueryType)_f; }
            set { _f = (byte)value; }
        }

        IReadOnlyList<object> IEntityQueryInvocation.Parameters
        {
            get { return _p; }
        }
    }

    /// <summary>
    /// 实体查询的参数。
    /// </summary>
    public interface IEntityQueryInvocation
    {
        /// <summary>
        /// 数据层查询方法。如果为空，表示使用约定的数据层方法。
        /// </summary>
        string MethodName { get; }

        /// <summary>
        /// 所有的参数。
        /// </summary>
        IReadOnlyList<object> Parameters { get; }

        /// <summary>
        /// 获取数据的类型
        /// </summary>
        RepositoryQueryType QueryType { get; }
    }

    /// <summary>
    /// 仓库返回数据的类型
    /// </summary>
    public enum RepositoryQueryType
    {
        /// <summary>
        /// 查询实体列表
        /// </summary>
        List = 0,
        /// <summary>
        /// 查询单个实体
        /// </summary>
        First = 1,
        /// <summary>
        /// 查询数据条数统计
        /// </summary>
        Count = 2,
        /// <summary>
        /// 查询数据表格
        /// </summary>
        Table = 3
    }
}