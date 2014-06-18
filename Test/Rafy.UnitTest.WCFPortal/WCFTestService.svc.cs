using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Rafy.Data;
using Rafy.Domain;
using UT;

namespace Rafy.UnitTest.WCFPortal
{
    public class WCFTestService : IWCFTestService
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public LiteDataTable GetTable()
        {
            var repo = RF.Concrete<ArticleRepository>();

            var res = repo.GetAllInTable();

            return res;
        }

        public Article GetFirstArticle()
        {
            return null;
            //return RF.Concrete<ArticleRepository>().GetFirst();
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }
    }
}
