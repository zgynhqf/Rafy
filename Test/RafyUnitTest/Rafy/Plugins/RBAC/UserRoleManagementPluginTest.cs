/*******************************************************
 * 
 * 作者：宋军瑞
 * 创建日期：20161209
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 宋军瑞 20161209 11:39
 * 
*******************************************************/

using System;
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
        }

        [TestMethod]
        public void HasRoleTest()
        {
            this._controller = DomainControllerFactory.Create<UserRoleController>();

            var userRoleRepository = RepositoryFacade.ResolveInstance<UserRoleRepository>();
            var userRepository = RepositoryFacade.ResolveInstance<UserRepository>();
            var roleRepository = RepositoryFacade.ResolveInstance<RoleRepository>();

            using (RepositoryFacade.TransactionScope(userRepository))
            {
                var user = new User {Id = 1, UserName = "Test_001", RealName = "Test_001", Email = "rafy@rafy.org", PhoneNumber = "18666666666"};
                var role = new Role {Id = 1, Code = Guid.NewGuid().ToString("N"), Description = "", Name = "管理员"};

                userRepository.Save(user);
                roleRepository.Save(role);
                userRoleRepository.Save(new UserRole { User = user, Role = role });

                var result1 = this._controller.HasRole(new User { Id = user.Id }, new Role { Id = role.Id });
                var result2 = this._controller.HasRole(new User { Id = user.Id }, new Role { Id = role.Id + 1L });

                Assert.IsTrue(result1);
                Assert.IsFalse(result2);
            }
        }

        [TestMethod]
        public void GetRoleListTest()
        {
            this._controller = DomainControllerFactory.Create<UserRoleController>();

            var userRoleRepository = RepositoryFacade.ResolveInstance<UserRoleRepository>();
            var userRepository = RepositoryFacade.ResolveInstance<UserRepository>();
            var roleRepository = RepositoryFacade.ResolveInstance<RoleRepository>();

            using (RepositoryFacade.TransactionScope(userRoleRepository))
            {
                var user = new User { Id = 1, UserName = "Test_001", RealName = "Test_001", Email = "rafy@rafy.org", PhoneNumber = "18666666666" };
                var role = new Role { Id = 1, Code = Guid.NewGuid().ToString("N"), Description = "", Name = "管理员" };

                userRepository.Save(user);
                roleRepository.Save(role);
                userRoleRepository.Save(new UserRole { User = user, Role = role });

                var roleList = this._controller.GetRoleList(user.Id);

                Assert.IsNotNull(roleList);
                Assert.IsTrue(roleList.Count == 1);
            }
        }

        [TestMethod]
        public void GetUserListTest()
        {
            this._controller = DomainControllerFactory.Create<UserRoleController>();

            var userRoleRepository = RepositoryFacade.ResolveInstance<UserRoleRepository>();
            var userRepository = RepositoryFacade.ResolveInstance<UserRepository>();
            var roleRepository = RepositoryFacade.ResolveInstance<RoleRepository>();

            using (RepositoryFacade.TransactionScope(userRepository))
            {
                var user = new User { Id = 1, UserName = "Test_001", RealName = "Test_001", Email = "rafy@rafy.org", PhoneNumber = "18666666666" };
                var role = new Role { Id = 1, Code = Guid.NewGuid().ToString("N"), Description = "", Name = "管理员" };

                userRepository.Save(user);
                roleRepository.Save(role);
                userRoleRepository.Save(new UserRole { User = user, Role = role });

                var list = this._controller.GetUserList(new Role { Id = role.Id });

                Assert.IsNotNull(list);
                Assert.IsTrue(list.Count == 1);
            }
        }
    }
}
