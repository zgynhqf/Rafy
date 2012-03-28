using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;

namespace Demo
{
    [Serializable]
    public abstract class DemoEntity : Entity
    {
        public static readonly string ConnectionString = "Demo";

        protected override string ConnectionStringSettingName
        {
            get { return ConnectionString; }
        }
    }

    [Serializable]
    public abstract class DemoEntityList : EntityList { }
}