using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.Web;
using Rafy.Domain;
using Rafy;

namespace Demo
{
    [Serializable]
    [JsonService("CountServerBookService")]
    [Contract, ContractImpl]
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
