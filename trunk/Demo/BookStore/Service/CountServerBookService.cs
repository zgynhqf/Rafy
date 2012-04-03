using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Web.Services;
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

        protected override void Execute()
        {
            var repo = RF.Create<Book>();
            var count = repo.GetAll().Count;
            this.BookCount = count;
        }
    }
}
