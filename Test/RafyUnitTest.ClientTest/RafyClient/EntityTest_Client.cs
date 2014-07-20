using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy;
using Rafy.Domain;
using Rafy.Domain.Validation;
using UT;

namespace RafyUnitTest.ClientTest
{
    [TestClass]
    public class EntityTest_Client
    {
        [ClassInitialize]
        public static void ET_ClassInitialize(TestContext context)
        {
            ClientTestHelper.ClassInitialize(context);
        }

        [TestMethod]
        public void CT_ET_Validation_OnClient()
        {
            var user = new TestUser();
            var brokenRules = user.Validate();
            Assert.AreEqual(brokenRules.Count, 1);
            Assert.AreEqual(brokenRules[0].Description, "编码 里没有输入值。");
        }

        [TestMethod]
        public void CT_ET_LazyRef_Serialization_OnClient()
        {
            Assert.IsFalse(
                TestRole.TestUserProperty.DefaultMeta.Serializable,
                "默认在客户端，应该是不可以序列化实体的。"
                );
        }
    }
}
