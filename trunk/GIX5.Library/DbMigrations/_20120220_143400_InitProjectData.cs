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
using OEA.Library;

namespace GIX5.Library.DbMigrations
{
    public class _20120220_143400_InitProjectData : ManualDbMigration
    {
        public override string DbSetting
        {
            get { return GEntity.ConnectionString; }
        }

        protected override void Up()
        {
            this.RunCode(db =>
            {
                //由于本类没有支持 Down 操作，所以这里面的 Up 需要防止重入。
                var cateRepo = RF.Concreate<ProjectCategoryRepository>();
                var list = cateRepo.GetAll();
                if (list.Count == 0)
                {
                    using (var tran = new TransactionScope())
                    {
                        #region 添加 ProjectCategories

                        var c1 = new ProjectCategory();
                        c1.Name = "市政工程";
                        cateRepo.Save(c1);
                        c1 = new ProjectCategory();
                        c1.Name = "园林工程";
                        cateRepo.Save(c1);
                        c1 = new ProjectCategory();
                        c1.Name = "建筑工程";
                        cateRepo.Save(c1);

                        var c11 = new ProjectCategory();
                        c11.Name = "住宅";
                        c11.TreeParent = c1;
                        cateRepo.Save(c11);
                        var c12 = new ProjectCategory();
                        c12.Name = "写字楼";
                        c12.TreeParent = c1;
                        cateRepo.Save(c12);

                        var c111 = new ProjectCategory();
                        c111.Name = "高层";
                        c111.TreeParent = c11;
                        cateRepo.Save(c111);
                        var c112 = new ProjectCategory();
                        c112.Name = "小高层";
                        c112.TreeParent = c11;
                        cateRepo.Save(c112);
                        var c113 = new ProjectCategory();
                        c113.Name = "超高层";
                        c113.TreeParent = c11;
                        cateRepo.Save(c113);

                        #endregion

                        #region 添加用户

                        var userRepo = RF.Concreate<UserRepository>();

                        var user1 = new User();
                        user1.Name = "张工";
                        userRepo.Save(user1);
                        var user2 = new User();
                        user2.Name = "李工";
                        userRepo.Save(user2);

                        #endregion

                        #region 添加项目

                        var projectRepo = RF.Concreate<ProjectRepository>();
                        var p = new Project();
                        p.FileName = "广联达大厦";
                        p.ProjectName = "广联达大厦";
                        p.ProjectCategory = c12;
                        p.Mask = 1;
                        //p.Mask = MaskType.Mask1;
                        projectRepo.Save(p);

                        p = new Project();
                        p.FileName = "住宅1";
                        p.ProjectName = "住宅1";
                        p.ProjectCategory = c111;
                        p.Mask = 2;
                        //p.Mask = MaskType.Mask2;
                        p.Opener = user1;
                        projectRepo.Save(p);

                        p = new Project();
                        p.FileName = "住宅2";
                        p.ProjectName = "住宅2";
                        p.ProjectCategory = c112;
                        p.Mask = 3;
                        //p.Mask = MaskType.Mask3;
                        projectRepo.Save(p);

                        #endregion

                        tran.Complete();
                    }
                }
            });
        }

        protected override void Down() { }

        protected override string GetDescription()
        {
            return "添加 GIX5 库中项目相关的初始数据。";
        }

        public override ManualMigrationType Type
        {
            get { return ManualMigrationType.Data; }
        }
    }
}