/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130307 11:34
 * 说明：TestUserRepository 类的查询扩展。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130307 11:34
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Domain;
using Rafy.Domain.ORM.Query;

namespace UT
{
    public class TestUserRepositoryExt : EntityRepositoryExt<TestUserRepository>
    {
        public static TestUserList GetByAge(TestUserRepository repository, int age)
        {
            return FetchList(repository, new GetByAgeCriteria { Age = age }) as TestUserList;
        }

        protected EntityList FetchBy(GetByAgeCriteria criteria)
        {
            var q = QueryFactory.Instance.Query(this.Repository);
            q.AddConstraintIf(TestUser.AgeProperty, PropertyOperator.Equal, criteria.Age);

            return this.QueryList(q);
        }
    }

    public class TestUserRepositoryExt2 : EntityRepositoryExt<TestUserRepository>
    {
        public static TestUserList GetByAge(TestUserRepository repository, int age)
        {
            return FetchList(repository, new GetByAge2Criteria { Age = age }) as TestUserList;
        }

        protected EntityList FetchBy(GetByAge2Criteria criteria)
        {
            var q = QueryFactory.Instance.Query(this.Repository);
            q.AddConstraintIf(TestUser.AgeProperty, PropertyOperator.Equal, criteria.Age);

            return this.QueryList(q);
        }
    }

    [Serializable]
    public class GetByAgeCriteria { public int Age; }

    [Serializable]
    public class GetByAge2Criteria { public int Age; }

    [Serializable]
    public class NotImplementCriteria { }
}