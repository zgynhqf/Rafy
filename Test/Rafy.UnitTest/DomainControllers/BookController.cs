/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211121
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211121 23:54
 * 
*******************************************************/

using Rafy.DataPortal;
using Rafy.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using UT;

namespace Rafy.UnitTest
{
    public interface IBookController : IDomainControllerContract
    {
        long CountBook();
    }

    public class BookController : DomainController, IBookController
    {
        [DataPortalCall]
        public virtual bool IsRunningAtServer()
        {
            return DataPortalApi.IsRunning;
        }

        [DataPortalCall]
        public virtual int GetThreadPortalCount()
        {
            var repo = RF.ResolveInstance<BookRepository>();
            var count = repo.GetThreadPortalCount();
            return count;
        }

        [DataPortalCall]
        public virtual long CountBook()
        {
            var repo = RF.ResolveInstance<BookRepository>();
            var count = repo.CountAll();
            return count;
        }

        [DataPortalCall]
        public virtual BookList GetBooks(PagingInfo pi)
        {
            var repo = RF.ResolveInstance<BookRepository>();
            var all = repo.GetAll(pi);
            return all;
        }
    }
}