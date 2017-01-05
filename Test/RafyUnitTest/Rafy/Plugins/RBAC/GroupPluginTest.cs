
/*******************************************************
 * 
 * 作者：刘雷
 * 创建日期：20161207
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 刘雷 20161207 10:47
 * 
*******************************************************/


using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy.RBAC.GroupManagement.Controllers;
using System.Transactions;
using Rafy.RBAC.RoleManagement;
using Rafy.RBAC.GroupManagement;
using Rafy.Domain;

namespace RafyUnitTest
{
    [TestClass]
    public class GroupPluginTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }

        [TestMethod]
        public void RBAC_GetPermissionEntry_Success()
        {
            var repo = RepositoryFacade.ResolveInstance<GroupRepository>();
            using (RepositoryFacade.TransactionScope(repo))
            {
                var controller = new PermissionFacadeController();
                Assert.IsNotNull(controller.GetPermissionEntry(1));
            }
        }
    }
}