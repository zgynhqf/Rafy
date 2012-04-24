using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbMigration;
using OEA.Library;

namespace Demo.DbMigrations
{
    public class _20120404_131000_InitAddress : ManualDbMigration
    {
        public override string DbSetting
        {
            get { return DemoEntity.ConnectionString; }
        }

        public override ManualMigrationType Type
        {
            get { return ManualMigrationType.Data; }
        }

        protected override void Up()
        {
            this.RunCode(db =>
            {
                //由于本类没有支持 Down 操作，所以这里面的 Up 需要防止重入。
                var pvcRepo = RF.Concreate<ProvinceRepository>();
                var list = pvcRepo.GetAll(false);
                if (list.Count == 0)
                {
                    var yn = new Province
                    {
                        Name = "云南",
                        CityList =
                        {
                            new City
                            {
                                Name = "昭通",
                                CountryList= 
                                {
                                    new Country { Name = "镇雄" },
                                    new Country { Name = "威信" }
                                }
                            },
                            new City
                            {
                                Name = "昆明",
                                CountryList= 
                                {
                                    new Country { Name = "昆明市" }
                                }
                            },
                            new City
                            {
                                Name = "丽江",
                                CountryList= 
                                {
                                    new Country { Name = "丽江市" }
                                }
                            }
                        }
                    };

                    pvcRepo.Save(yn);
                }
            });
        }

        protected override void Down() { }

        protected override string GetDescription()
        {
            return "添加 省市县 的初始数据。";
        }
    }
}
