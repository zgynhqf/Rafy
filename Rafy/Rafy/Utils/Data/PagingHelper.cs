/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130122 16:43
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130122 16:43
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Rafy.Data
{
    /// <summary>
    /// 分页算法帮助类
    /// </summary>
    public static class PagingHelper
    {  /// <summary>
       /// 使用 IDataReader 的内存分页读取方案。
       /// </summary>
       /// <param name="reader"></param>
       /// <param name="rowReader">每一行数据，会调用此方法进行调取。</param>
       /// <param name="pagingInfo">分页信息。如果这个参数不为空，则使用其中描述的分页规则进行内存分页查询。</param>
       /// <param name="maxCount"></param>
        public static void MemoryPaging(IDataReader reader, Action<IDataReader> rowReader, PagingInfo pagingInfo, int maxCount = int.MaxValue)
        {
            bool isPaging = !PagingInfo.IsNullOrEmpty(pagingInfo);
            bool needCount = isPaging && pagingInfo.IsNeedCount;
            long totalCount = 0;
            long startRow = 1;//从一开始的行号
            long endRow = int.MaxValue;

            if (isPaging)
            {
                startRow = pagingInfo.PageSize * (pagingInfo.PageNumber - 1) + 1;
                endRow = startRow + pagingInfo.PageSize - 1;
            }

            var itemsCount = 0;
            while (reader.Read())
            {
                totalCount++;

                if (totalCount >= startRow)
                {
                    if (totalCount <= endRow)
                    {
                        itemsCount++;
                        if (itemsCount <= maxCount)
                        {
                            rowReader(reader);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (needCount)
            {
                while (reader.Read()) { totalCount++; }

                pagingInfo.TotalCount = totalCount;
            }
        }
    }
}