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
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace JXC
{
    [RootEntity, Serializable]
    public partial class AutoCodeInfo : JXCEntity
    {
        #region 构造函数

        public AutoCodeInfo() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected AutoCodeInfo(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

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
    public partial class AutoCodeInfoList : JXCEntityList
    {
    }

    public partial class AutoCodeInfoRepository : JXCEntityRepository
    {
        protected AutoCodeInfoRepository() { }

        public string GetOrCreateAutoCode<TTargetType>(string defaultFormat = null, string beiZhu = null)
            where TTargetType : Entity
        {
            return GetOrCreateAutoCode(typeof(TTargetType), defaultFormat, beiZhu);
        }

        public string GetOrCreateAutoCode(Type targetType, string defaultFormat = null, string beiZhu = null)
        {
            var vm = UIModel.Views.CreateBaseView(targetType);
            string name = vm.Label + "-自动编码规则";

            var item = this.FetchFirst(name);

            if (item == null)
            {
                item = new AutoCodeInfo
                {
                    MingCheng = name,
                    CanShuZhi = defaultFormat ?? "<YEAR><MONTH><DAY>-***",
                    BeiZhu = beiZhu ??
@"自动编码填写规则
***表示自动编号
<YEAR> 表示当前年份
<MONTH> 表示当前月份
<DAY> 表示当前日号
如要编写 年+月+日+自动编号
则值为：<YEAR><MONTH><DAY>-***"
                };
                this.Save(item);
            }

            var format = item.CanShuZhi;

            var t = DateTime.Today;
            var code = format.Replace("<YEAR>", t.Year.ToString("0000"))
                .Replace("<MONTH>", t.Month.ToString("00"))
                .Replace("<DAY>", t.Day.ToString("00"));

            if (code.Contains("***"))
            {
                var count = RF.Find(targetType).CountAll() + 1;
                code = code.Replace("***", count.ToString("0000"));
            }

            return code;
        }

        protected EntityList FetchBy(string name)
        {
            return this.QueryList(q =>
            {
                q.Constrain(AutoCodeInfo.MingChengProperty).Equal(name);
            });
        }
    }

    internal class AutoCodeInfoConfig : EntityConfig<AutoCodeInfo>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapProperties(
                AutoCodeInfo.MingChengProperty,
                AutoCodeInfo.CanShuZhiProperty,
                AutoCodeInfo.BeiZhuProperty
                );
        }

        protected override void ConfigView()
        {
            View.DomainName("自动编码信息").HasDelegate(AutoCodeInfo.MingChengProperty);

            View.UseDefaultCommands();

            using (View.OrderProperties())
            {
                View.Property(AutoCodeInfo.MingChengProperty).HasLabel("参数名称").ShowIn(ShowInWhere.All);
                View.Property(AutoCodeInfo.CanShuZhiProperty).HasLabel("参数值").ShowIn(ShowInWhere.ListDetail);
                View.Property(AutoCodeInfo.BeiZhuProperty).HasLabel("备注").ShowIn(ShowInWhere.ListDetail)
                    .ShowMemoInDetail().Readonly();
            }
        }
    }
}