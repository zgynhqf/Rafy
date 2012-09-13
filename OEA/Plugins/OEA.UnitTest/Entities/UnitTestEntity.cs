using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace OEA.Library._Test
{
    [Serializable]
    public abstract class UnitTestEntity : Entity
    {
        public const string DbSetting = "OEAUnitTest";

        protected UnitTestEntity() { }

        protected override string ConnectionStringSettingName
        {
            get { return DbSetting; }
        }
    }
}
