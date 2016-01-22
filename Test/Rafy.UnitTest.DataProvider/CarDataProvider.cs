/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140508
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140508 09:51
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.UnitTest.IDataProvider;
using Rafy.UnitTest.Repository;
using UT;

namespace Rafy.UnitTest.DataProvider
{
    [DataProviderFor(typeof(CarRepository))]
    public class CarDataProvider : UnitTestEntityRepositoryDataProvider, ICarDataProvider
    {
        public CarList GetByStartDate(DateTime time)
        {
            var q = this.CreateLinqQuery<Car>();
            q = q.Where(e => e.AddTime > time);
            return this.QueryList(q) as CarList;
        }

        public CarList DA_GetByReplacableDAL()
        {
            return new CarList
            {
                new Car { Name = "ImplementationReplaced" }
            };
        }
    }
}
