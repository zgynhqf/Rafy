using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;

namespace FM
{
    [Serializable]
    public abstract class FMEntity : Entity
    {
        public static readonly string ConnectionString = "FinanceManagement";

        protected override string ConnectionStringSettingName
        {
            get { return ConnectionString; }
        }

        #region 扩展字段

        public static readonly Property<string> Extend1Property = P<FMEntity>.Register(e => e.Extend1);
        public string Extend1
        {
            get { return this.GetProperty(Extend1Property); }
            set { this.SetProperty(Extend1Property, value); }
        }

        public static readonly Property<string> Extend2Property = P<FMEntity>.Register(e => e.Extend2);
        public string Extend2
        {
            get { return this.GetProperty(Extend2Property); }
            set { this.SetProperty(Extend2Property, value); }
        }

        public static readonly Property<string> Extend3Property = P<FMEntity>.Register(e => e.Extend3);
        public string Extend3
        {
            get { return this.GetProperty(Extend3Property); }
            set { this.SetProperty(Extend3Property, value); }
        }

        #endregion
    }

    [Serializable]
    public abstract class FMEntityList : EntityList { }
}