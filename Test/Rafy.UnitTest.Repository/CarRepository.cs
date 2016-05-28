using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.UnitTest.IDataProvider;
using UT;

namespace Rafy.UnitTest.Repository
{
    /// <summary>
    /// Car 仓库类。
    /// 负责 Car 类的查询、保存。
    /// </summary>
    [RepositoryFor(typeof(Car))]
    public partial class CarRepository : UnitTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected CarRepository() { }

        private new ICarDataProvider DataProvider
        {
            get { return base.DataProvider as ICarDataProvider; }
        }

        [RepositoryQuery]
        public virtual CarList GetByStartDate(DateTime startTime)
        {
            return (CarList)this.DataProvider.GetByStartDate(startTime);
        }

        [RepositoryQuery]
        public virtual long CountByStartDate(DateTime startTime)
        {
            return (long)this.DataProvider.GetByStartDate(startTime);
        }

        [RepositoryQuery]
        public virtual Car GetByReplacableDAL()
        {
            return new Car { Name = "RawImplementation" };
        }
    }
}