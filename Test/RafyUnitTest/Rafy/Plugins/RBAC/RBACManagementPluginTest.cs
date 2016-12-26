/*******************************************************
 * 
 * 作者：吴中坡
 * 创建日期：20161215
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 吴中坡 20161215 16:50
 * 
*******************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rafy.Accounts;
using Rafy.Domain;
using Rafy.RBAC.DataPermissionManagement;
using Rafy.RBAC.GroupManagement;
using Rafy.RBAC.RoleManagement;
using Rafy.RBAC.RoleManagement.Controllers;
using Rafy.RBAC.UserRoleManagement;
using Rafy.RBAC.UserRoleManagement.Controllers;
using Rafy.Reflection;
using Rafy.UnitTest;

namespace RafyUnitTest.Rafy.Plugins.RBAC
{
    [TestClass]
    public class RBACManagementPluginTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            ServerTestHelper.ClassInitialize(context);
        }


        private Tuple<User, Role, Group, Resource> TestInitRBAC()
        {
            var user = new User() { UserName = "admin" };
            RF.Save(user);
            var role = new Role() { Name = "管理员", Code = "admin" };
            RF.Save(role);
            var userRole = new UserRole() { User = user, Role = role };
            RF.Save(userRole);
            var group = new Group()
            {
                Name = "研发部",
                Code = "abc",
                TreeChildren = { new Group()
                {
                    Name = "测试组",
                     Code = "def"
                } }
            };
            RF.Save(group);
            var groupUser = new GroupUser() { Group = group, User = user };
            RF.Save(groupUser);
            var groupRole = new GroupRole() { Group = group, Role = role };
            RF.Save(groupRole);
            var resource = new Resource()
            {
                Name = "待开明细",
                Code = "Transcation",
                Description = "Transcation"
            }.SetIsSupportDataPermission(true).SetResourceEntityType(typeof(TestDataPermission).FullName);
            var operation = new ResourceOperation()
            {
                Name = "新增",
                Code = "Add"
            };
            resource.ResourceOperationList.Add(operation);
            resource.ResourceOperationList.Add(
                 new ResourceOperation()
                 {
                     Name = "删除",
                     Code = "Delete"
                 }
                );
            RF.Save(resource);
            RF.Save(new RoleOperation()
            {
                Role = role,
                Operation = operation
            });
            DataPermission dataPermission = new DataPermission()
            {
                Resource = resource,
                Role = role
            };
            dataPermission.SetBuilder(new CurrentGroupPermissionConstraintBuilder()
            {
                IsIncludeChildGroup = false,
                GroupIdProperty = "GroupId"
            });
            RF.Save(dataPermission);

            TestDataPermission testDataPermission = new TestDataPermission();
            testDataPermission.Group = group;
            testDataPermission.Name = "test";
            RF.Save(testDataPermission);
            return Tuple.Create(user, role, group, resource);
        }

        [TestMethod]
        public void TestGetOperationByRole()
        {
            var repo = RF.ResolveInstance<RoleRepository>();
            using (RF.TransactionScope(repo))
            {
                var tuple = TestInitRBAC();
                Assert.IsTrue(RF.ResolveInstance<ResourceOperationRepository>().GetOperationByRoleList(new List<long>() { tuple.Item2.Id }).Count == 1);

            }
        }

        [TestMethod]
        public void TestCurrentGroupDataPermisssion()
        {
            var repo = RF.ResolveInstance<TestDataPermissionRepository>();
            using (RF.TransactionScope(repo))
            {
                var tuple = TestInitRBAC();
                using (DataPermissionFacade.EnableDataPermission(tuple.Item4))
                {
                    AccountContext.CurrentUser = tuple.Item1;
                    Assert.IsTrue(repo.GetAll().Count == 1);
                }
            }
        }

        [TestMethod]
        public void TestCurrentGroupAndLowerDataPermisssion()
        {
            var repo = RF.ResolveInstance<TestDataPermissionRepository>();
            using (RF.TransactionScope(repo))
            {
                var tuple = TestInitRBAC();

                var role = new Role() { Name = "包含下级管理员", Code = "admin" };
                RF.Save(role);
                var userRole = new UserRole() { User = tuple.Item1, Role = role };
                RF.Save(userRole);

                DataPermission dataPermission1 = new DataPermission()
                {
                    Resource = tuple.Item4,
                    Role = role
                };
                dataPermission1.SetBuilder(new CurrentGroupPermissionConstraintBuilder()
                {
                    IsIncludeChildGroup = true,
                    GroupIdProperty = "GroupId"
                });
                RF.Save(dataPermission1);

                TestDataPermission testDataPermission = new TestDataPermission();
                testDataPermission.Group = tuple.Item3.TreeChildren[0] as Group;
                testDataPermission.Name = "test";
                RF.Save(testDataPermission);
                using (DataPermissionFacade.EnableDataPermission(tuple.Item4))
                {
                    AccountContext.CurrentUser = tuple.Item1;
                    Assert.IsTrue(repo.GetAll().Count == 2);
                }
                Assert.IsTrue(repo.GetAll().Count == 2);
            }
        }

        [TestMethod]
        public void TestGetAllGroupList()
        {
            var repo = RF.ResolveInstance<GroupRepository>();
            using (RF.TransactionScope(repo))
            {
                TestInitRBAC();
                var groupList = TreeHelper.ConvertToList<Group>(repo.GetAll());
                Assert.IsTrue(groupList.Count == 2);
            }
        }

        [TestMethod]
        public void TestGetResourceOperation()
        {
            var repo = RF.ResolveInstance<ResourceOperationRepository>();
            using (RF.TransactionScope(repo))
            {
                var tuple = TestInitRBAC();
                var userRoleDc = DomainControllerFactory.Create<UserRoleController>();
                Assert.IsTrue(repo.GetByParentId(tuple.Item4.Id).Count == 2);
                Assert.IsTrue(userRoleDc.GetResourceOperation(tuple.Item1.Id, tuple.Item4.Id).Count == 1);
            }
        }

        [TestMethod]
        public void TestSetRoleOperation()
        {
            RoleController controller = DomainControllerFactory.Create<RoleController>();
            var repo = RF.ResolveInstance<RoleOperationRepository>();
            using (RF.TransactionScope(repo))
            {
                var tuple = TestInitRBAC();
                var originId = repo.GetAll()[0].OperationId;
                var role = tuple.Item2;
                var resource = tuple.Item4;
                var delopId = Convert.ToInt64(resource.ResourceOperationList[1].Id);
                controller.SetRoleOperation(role.Id, new List<long>() { delopId });
                var savedId= repo.GetAll()[0].OperationId;
                Assert.AreNotEqual(originId,savedId);

            }

        }
    }
}
