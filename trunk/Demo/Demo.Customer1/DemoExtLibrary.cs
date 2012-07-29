using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.ORM.DbMigration;
using OEA.MetaModel.View;
using OEA.MetaModel;
using Demo.WPF;

namespace Demo
{
    class DemoExtLibrary : LibraryPlugin
    {
        public override ReuseLevel ReuseLevel
        {
            get { return ReuseLevel.Customized; }
        }

        public override void Initialize(IApp app)
        {
        }
    }
}