using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Web.Services;
using OEA.Library;

namespace Demo
{
    [Serializable]
    [ClientServiceName("CountServerBookService")]
    internal class CountServerBookJsonService : JsonService
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
