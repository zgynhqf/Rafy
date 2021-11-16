/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：2007
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 2007
 * 
*******************************************************/

using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Security.Permissions;
using System.Runtime.Serialization;
using Rafy.Serialization;
using Rafy.DataPortal;

namespace Rafy
{
    /// <summary>
    /// this indicates a pager info,
    /// includes page number, page size, and total count;
    /// Note!
    /// Don't use null to indicates a empty paging information, use <see cref="PagingInfo.Empty" /> instead.
    /// </summary>
    [Serializable]
    [DataContract]
    public class PagingInfo : IDataPortalOutArgument, ICustomSerializationObject
    {
        private const int TotalCount_NeedCount = -1;

        /// <summary>
        /// this field value will be set which at thr first time TotalCount is set.
        /// and its value will be restore to default(false) when PagingInfo object is transferred;
        /// </summary>
        internal bool NeedTransferToClent;
        private long _pageNumber;
        private int _pageSize;
        /// <summary>
        /// If this value is positive or zero, it indicates the count.
        /// Otherwise, it means "need count all items".
        /// </summary>
        private long _totalCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="PagingInfo"/> class.
        /// Its pageNumber will be set to 1, and pageSize will be set to 10.
        /// </summary>
        public PagingInfo() : this(1, 10) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagingInfo"/> class.
        /// this constructor indicates that does not retrieve count information from persistence.
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        public PagingInfo(long pageNumber, int pageSize) : this(pageNumber, pageSize, false) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagingInfo"/> class.
        /// this constructor indicates whether to retrieve count information from persistence.
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="isNeedCount">is need retrieve count of all records(if it is true,it will retrieve count info from persistence)</param>
        public PagingInfo(long pageNumber, int pageSize, bool isNeedCount)
        {
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
            this.IsNeedCount = isNeedCount;
        }

        /// <summary>
        /// construct with a totalCount
        /// </summary>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalCount"></param>
        public PagingInfo(long pageNumber, int pageSize, long totalCount)
        {
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
            this.TotalCount = totalCount;
        }

        /// <summary>
        /// Whether need to retrieve count of all records
        /// (if it's true, it means the DAL should retrieve count info from database.)
        /// </summary>
        [DataMember]
        public bool IsNeedCount
        {
            get { return _totalCount == TotalCount_NeedCount; }
            set
            {
                if (value)
                {
                    _totalCount = TotalCount_NeedCount;
                }
                else
                {
                    if (_totalCount == TotalCount_NeedCount)
                    {
                        _totalCount = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Count of all records
        /// </summary>
        public long TotalCount
        {
            get
            {
                if (this._totalCount == -1) { return 0; }

                return this._totalCount;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "value can not be negative.");
                }
                this._totalCount = value;
                this.NeedTransferToClent = true;//第一次设置时，需要传回客户端。后续就不再需要了。
            }
        }

        /// <summary>
        /// size of a page
        /// </summary>
        [DataMember]
        public int PageSize
        {
            get { return this._pageSize; }
            set
            {
                if (value <= 0) { throw new ArgumentOutOfRangeException("value", "value should be positive."); }

                this._pageSize = value;
            }
        }

        /// <summary>
        /// current page number.
        /// start from 1.
        /// </summary>
        [DataMember]
        public long PageNumber
        {
            get { return this._pageNumber; }
            set
            {
                if (value < 1) { throw new ArgumentOutOfRangeException("value", "value should be positive."); }

                this._pageNumber = value;
            }
        }

        /// <summary>
        /// Gets the total page count, if <see cref="TotalCount"/> has positive value.
        /// </summary>
        public long PageCount
        {
            get
            {
                var count = _totalCount / _pageSize;
                if (_totalCount % _pageSize != 0) count++;
                return count;
            }
        }

        /// <summary>
        /// Indicates whether current page is not the first one.
        /// </summary>
        public bool HasPreviousPage
        {
            get
            {
                return _pageNumber > 1;
            }
        }

        /// <summary>
        /// Indicates whether current page is not the last one.
        /// </summary>
        public bool HasNextPage
        {
            get
            {
                return _pageNumber < this.PageCount;
            }
        }

        #region Empty

        /// <summary>
        /// A singleton instance indicates there is no paging action.
        /// </summary>
        public static readonly PagingInfo Empty = new EmptyPagingInfo();

        /// <summary>
        /// Indicates is this pagingInfo a nonsence.
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return this is EmptyPagingInfo;
        }

        /// <summary>
        /// Indicates is this pagingInfo a nonsence.
        /// </summary>
        /// <returns></returns>
        public static bool IsNullOrEmpty(PagingInfo pagingInfo)
        {
            return pagingInfo == null || pagingInfo is EmptyPagingInfo;
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("pn", _pageNumber);
            info.AddValue("s", _pageSize);
            info.AddValue("tc", _totalCount);
        }

        void ICustomSerializationObject.SetObjectData(SerializationInfo info, StreamingContext context)
        {
            _pageNumber = info.GetInt64("pn");
            _pageSize = info.GetInt32("s");
            _totalCount = info.GetInt64("tc");
        }

        bool IDataPortalOutArgument.NeedTransferToClient()
        {
            return this.NeedTransferToClent;
        }

        #endregion
    }

    /// <summary>
    /// 不进行分页查询的分页信息。
    /// 
    /// 一般情况下，效果等同于传入 null 值的 PagingInfo。
    /// 在使用多参数查询时，则只能使用这个对象，而不能使用 null 查询。
    /// </summary>
    [Serializable]
    internal class EmptyPagingInfo : PagingInfo, ISerializable
    {
        internal EmptyPagingInfo() : base(1, 1) { }

        #region 单例序列化

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.SetType(typeof(SSH));
        }

        /// <summary>
        /// Singleton Serialization Helper
        /// </summary>
        [Serializable]
        private sealed class SSH : IObjectReference
        {
            public object GetRealObject(StreamingContext context)
            {
                return PagingInfo.Empty;
            }
        }

        #endregion
    }
}