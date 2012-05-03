using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Library;

namespace JXC
{
    [Serializable]
    public abstract class JXCEntity : Entity
    {
        public static string ConnectionString = "Demo";

        protected override string ConnectionStringSettingName
        {
            get { return ConnectionString; }
        }
    }

    [Serializable]
    public abstract class JXCEntityList : EntityList { }
}