using System;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy.Accounts;
using Rafy.Domain;
using Rafy.RBAC.RoleManagement;
using Rafy.RBAC.UserRoleManagement;
using Rafy.RBAC.UserRoleManagement.Controllers;

namespace RafyUnitTest
{
    [TestClass]
    public class UserRoleManagementPluginTest
    {
        private UserRoleController _controller;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);

            PrepareData();
        }

        private static void PrepareData()
        {
            
        }

        [TestMethod]
        public void SaveTest()
        {
            this._controller = DomainControllerFactory.Create<UserRoleController>();

            var role = new Role {Code = Guid.NewGuid().ToString("N"), Description = "", Name = "管理员" };
            RepositoryFacade.Save(role);

            this._controller.Save(new UserRole {
                User = new User { Id = 1, UserName = "Test_001", RealName = "Test_001", Email = "rafy@rafy.org", PhoneNumber = "18666666666"},
                Role = role
            });
        }

        [TestMethod]
        public void HasRoleTest()
        {
            this._controller = DomainControllerFactory.Create<UserRoleController>();

            var result1 = this._controller.HasRole(new User {Id = 1}, new Role {Id = 1});
            var result2 = this._controller.HasRole(new User {Id = 1}, new Role {Id = 2});

            Assert.IsTrue(result1);
            Assert.IsFalse(result2);
        }

        [TestMethod]
        public void GetRoleListTest()
        {
            this._controller = DomainControllerFactory.Create<UserRoleController>();

            var roleList = this._controller.GetRoleList(new User {Id = 1});

            Assert.IsNotNull(roleList);
            Assert.IsTrue(roleList.Count == 1);
        }

        [TestMethod]
        public void GetResourceOperationListTest()
        {
            this._controller = DomainControllerFactory.Create<UserRoleController>();

            var resourceOperation = new ResourceOperation { Name = "Add", Code = Guid.NewGuid().ToString("N")};
            RepositoryFacade.Save(resourceOperation);
            var roleRepository = RepositoryFacade.ResolveInstance<RoleRepository>();
            var role = roleRepository.GetFirst();
            var roleOperation = new RoleOperation { Role = role, Operation = resourceOperation};
            RepositoryFacade.Save(roleOperation);

            var list = this._controller.GetResourceOperationList(role);

            Assert.IsNotNull(list);
            Assert.IsTrue(list.Count == 1);
        }
    }
}
