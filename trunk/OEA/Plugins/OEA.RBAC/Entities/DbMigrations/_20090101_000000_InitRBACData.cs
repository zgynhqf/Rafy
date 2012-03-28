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
using DbMigration;
using OEA;
using System.Transactions;
using OEA.RBAC;

namespace OEA.Library.DbMigrations
{
    public class _20090101_000000_InitRBACData : ManualDbMigration
    {
        public override string DbSetting
        {
            get { return ConnectionStringNames.OEA; }
        }

        protected override void Up()
        {
            this.RunCode(db =>
            {
                //由于本类没有支持 Down 操作，所以这里面的 Up 需要防止重入。
                var orgRepository = RF.Concreate<OrgRepository>();
                var list = orgRepository.GetAll();
                if (list.Count == 0)
                {
                    using (var tran = new TransactionScope())
                    {
                        //Position
                        var positionRepo = RF.Concreate<PositionRepository>();
                        var p = positionRepo.New().CastTo<Position>();
                        p.Code = "01";
                        p.Name = "系统管理员";
                        positionRepo.Save(p);

                        //User
                        var repo = RF.Concreate<UserRepository>();
                        var admin = repo.New().CastTo<User>();
                        admin.Code = "admin";
                        admin.Name = "admin";
                        repo.Save(admin);

                        //Org
                        var org = orgRepository.New().CastTo<Org>();
                        org.TreeCode = "01";
                        org.Name = "IT系统管理部";
                        var op = org.OrgPositionList.AddNew().CastTo<OrgPosition>();
                        op.PositionId = p.Id;
                        var opu = op.OrgPositionUserList.AddNew().CastTo<OrgPositionUser>();
                        opu.User = admin;
                        orgRepository.Save(org);

                        tran.Complete();
                    }

                    ////User
                    //var userRepo = RF.Concreate<UserRepository>();
                    //for (int i = 0; i < 100; i++)
                    //{
                    //    var user = userRepo.New().CastTo<User>();
                    //    user.Code = "testUserCode" + i;
                    //    user.Name = "testUserName" + i;
                    //    userRepo.Save(user);
                    //}

                    ////Position
                    //var pRepo = RF.Concreate<PositionRepository>();
                    //for (int i = 0; i < 100; i++)
                    //{
                    //    var p = pRepo.New().CastTo<Position>();
                    //    p.Code = "01" + i;
                    //    p.Name = "系统管理员" + i;
                    //    pRepo.Save(p);
                    //}
                }
            });
        }

        protected override void Down() { }

        protected override string GetDescription()
        {
            return "添加 RBAC 库中的初始数据。";
        }

        public override ManualMigrationType Type
        {
            get { return ManualMigrationType.Data; }
        }
    }
}