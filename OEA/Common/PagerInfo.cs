using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace hxy
{
    /// <summary>
    /// this indicates a pager info,
    /// include page index, page size, and total count;
    /// </summary>
    public class PagerInfo
    {
        private int _pageIndex;

        private int _pageSize;

        /// <summary>
        /// If this value is positive or zero, it indicates the count.
        /// Otherwise, it means "need count all items".
        /// </summary>
        private int _totalCount;

        /// <summary>
        /// Whether need to retrieve count of all records
        /// (if it is true,it means should retrieve count info from database)
        /// </summary>
        public bool IsNeedCount
        {
            get
            {
                return _totalCount < 0;
            }
            set
            {
                _totalCount = value ? -1 : 1;
            }
        }

        /// <summary>
        /// Count of all records
        /// </summary>
        public int TotalCount
        {
            get
            {
                if (_totalCount == -1)
                {
                    return 0;
                }
                return _totalCount;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", "value should be positive.");
                }
                _totalCount = value;
            }
        }

        /// <summary>
        /// size of a page
        /// </summary>
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        /// <summary>
        /// current page index(from 1 to infinite)
        /// </summary>
        public int PageIndex
        {
            get { return _pageIndex; }
            set { _pageIndex = value; }
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="needRetrieveTotalCount">is need retrieve count of all records(if it is true,it will retrieve count info from database)</param>
        public PagerInfo(int pageIndex, int pageSize, bool isNeedCount)
        {
            this._pageIndex = pageIndex;
            this._pageSize = pageSize;
            this.IsNeedCount = isNeedCount;
        }

        /// <summary>
        /// this constructor indicate that no need to retrieve count info from database
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        public PagerInfo(int pageIndex, int pageSize) : this(pageIndex, pageSize, false) { }

        public PagerInfo(int pageIndex, int pageSize, int totalCount)
        {
            this._pageIndex = pageIndex;
            this._pageSize = pageSize;
            this.TotalCount = totalCount;
        }
    }
}