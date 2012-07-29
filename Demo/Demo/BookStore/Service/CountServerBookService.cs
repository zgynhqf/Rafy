using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Web;
using OEA.Library;
using OEA;

namespace Demo
{
    [Serializable]
    [ClientServiceName("CountServerBookService")]
    public class CountServerBookJsonService : Service
    {
        [ServiceOutput]
        public int BookCount { get; set; }

        /// <summary>
        /// 客户端会把这个列表传输过来。
        /// </summary>
        [ServiceInput]
        public EntityList Books { get; set; }

        protected override void Execute()
        {
            //var repo = RF.Create<Book>();
            //var count = repo.GetAll().Count;
            //this.BookCount = count;
            this.BookCount = this.Books.Count;
        }
    }
}
