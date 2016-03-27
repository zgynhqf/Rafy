/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120103
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120103
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.DbMigration;
using Rafy;
using System.Transactions;
using Rafy.RBAC.Old;

namespace Rafy.Domain.DbMigrations
{
    public class _20090101_000000_InitRBACData : ManualDbMigration
    {
        public override string DbSetting
        {
            get { return DbSettingNames.RafyPlugins; }
        }

        protected override void Up()
        {
            this.RunCode(db =>
            {
                //由于本类没有支持 Down 操作，所以这里面的 Up 需要防止重入。
                var orgRepository = RF.Concrete<OrgRepository>();
                var list = orgRepository.GetAll();
                if (list.Count == 0)
                {
                    using (var tran = RF.TransactionScope(orgRepository))
                    {
                        //Position
                        var pos = new Position();
                        pos.Code = "01";
                        pos.Name = "系统管理员";
                        RF.Save(pos);

                        //User
                        var admin = new User();
                        admin.Code = "admin";
                        admin.Name = "admin";
                        RF.Save(admin);

                        //Org
                        var org = new Org
                        {
                            TreeIndex = "001.",
                            Name = "IT系统管理部",
                            OrgPositionList =
                            {
                                new OrgPosition
                                {
                                    Position = pos,
                                    OrgPositionUserList = 
                                    {
                                        new OrgPositionUser
                                        {
                                            User = admin
                                        }
                                    }
                                }
                            }
                        };
                        orgRepository.Save(org);

                        tran.Complete();
                    }

                    ////User
                    //var userRepo = RF.Concreate<UserRepository>();
                    //for (int i = 0; i < 100; i++)
                    //{
                    //    var user = new User();
                    //    user.Code = "testUserCode" + i;
                    //    user.Name = "testUserName" + i;
                    //    userRepo.Save(user);
                    //}

                    ////Position
                    //var pRepo = RF.Concreate<PositionRepository>();
                    //for (int i = 0; i < 100; i++)
                    //{
                    //    var p = new Position();
                    //    p.Code = "01" + i;
                    //    p.Name = "系统管理员" + i;
                    //    pRepo.Save(p);
                    //}
                }
            });
        }

        protected override void Down() { }

        public override string Description
        {
            get { return "添加 RBAC 库中的初始数据。"; }
        }

        public override ManualMigrationType Type
        {
            get { return ManualMigrationType.Data; }
        }
    }
}