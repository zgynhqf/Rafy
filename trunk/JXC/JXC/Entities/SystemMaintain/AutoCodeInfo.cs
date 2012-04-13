/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120413
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120413
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;

namespace JXC
{
    [RootEntity, Serializable]
    public class AutoCodeInfo : JXCEntity
    {
        public static readonly Property<string> MingChengProperty = P<AutoCodeInfo>.Register(e => e.MingCheng);
        public string MingCheng
        {
            get { return this.GetProperty(MingChengProperty); }
            set { this.SetProperty(MingChengProperty, value); }
        }

        public static readonly Property<string> CanShuZhiProperty = P<AutoCodeInfo>.Register(e => e.CanShuZhi);
        public string CanShuZhi
        {
            get { return this.GetProperty(CanShuZhiProperty); }
            set { this.SetProperty(CanShuZhiProperty, value); }
        }

        public static readonly Property<string> BeiZhuProperty = P<AutoCodeInfo>.Register(e => e.BeiZhu);
        public string BeiZhu
        {
            get { return this.GetProperty(BeiZhuProperty); }
            set { this.SetProperty(BeiZhuProperty, value); }
        }
    }

    [Serializable]
    public class AutoCodeInfoList : JXCEntityList
    {
        protected void QueryBy(string name)
        {
            this.QueryDb(q =>
            {
                q.Constrain(AutoCodeInfo.MingChengProperty).Equal(name);
            });
        }
    }

    public class AutoCodeInfoRepository : EntityRepository
    {
        protected AutoCodeInfoRepository() { }

        public string GetOrCreateAutoCode(string name, string defaultFormat = null, string beiZhu = null)
        {
            var item = this.FetchFirstAs<AutoCodeInfo>(name);

            if (item == null)
            {
                item = new AutoCodeInfo
                {
                    MingCheng = name,
                    CanShuZhi = defaultFormat ?? "<YEAR><MONTH><DAY>***",
                    BeiZhu = beiZhu ?? @"
自动编码填写规则
***表示随机编号
<YEAR> 表示当前年份
<MONTH> 表示当前月份
<DAY> 表示当前日号
如要编写 年+月+日+随机编号
则值为：<YEAR><MONTH><DAY>***"
                };
                RF.Save(item);
            }

            var format = item.CanShuZhi;

            var t = DateTime.Today;
            var value = format.Replace("<YEAR>", t.Year.ToString())
                .Replace("<MONTH>", t.Month.ToString())
                .Replace("<DAY>", t.Day.ToString())
                .Replace("***", new Random().Next(1000, 9999).ToString());

            return value;
        }
    }

    internal class AutoCodeInfoConfig : EntityConfig<AutoCodeInfo>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().HasColumns(
                AutoCodeInfo.MingChengProperty,
                AutoCodeInfo.CanShuZhiProperty,
                AutoCodeInfo.BeiZhuProperty
                );
        }

        protected override void ConfigView()
        {
            View.HasLabel("自动编码信息").HasTitle(AutoCodeInfo.MingChengProperty);

            View.Property(AutoCodeInfo.MingChengProperty).HasLabel("参数名称").ShowIn(ShowInWhere.All);
            View.Property(AutoCodeInfo.CanShuZhiProperty).HasLabel("参数值").ShowIn(ShowInWhere.ListDetail);
            View.Property(AutoCodeInfo.BeiZhuProperty).HasLabel("备注").ShowIn(ShowInWhere.ListDetail);
        }
    }
}