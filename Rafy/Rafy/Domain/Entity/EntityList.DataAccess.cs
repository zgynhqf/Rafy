/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2010
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2010
 * 
*******************************************************/

using System;
using System.Collections;
using System.Linq;
using Rafy.Domain.ORM;
using Rafy.MetaModel;
using System.Linq.Expressions;
using Rafy.MetaModel.Attributes;
using System.Collections.Generic;
using Rafy;

namespace Rafy.Domain
{
    partial class EntityList
    {
        #region Count

        private long _totalCount = -1;

        /// <summary>
        /// 查询出来的当前列表在数据库中存在的总数据条数。
        /// 
        /// 一是用于统计数据条数查询的数据传输。
        /// 二是是分页时保存所有数据的行数。
        /// </summary>
        public long TotalCount
        {
            get { return this._totalCount; }
        }

        /// <summary>
        /// 当查询 Count 时，调用此方法设置最终查询出的总条数。
        /// </summary>
        /// <param name="value"></param>
        public void SetTotalCount(long value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException("value");

            this._totalCount = value;
        }

        #endregion
    }
}