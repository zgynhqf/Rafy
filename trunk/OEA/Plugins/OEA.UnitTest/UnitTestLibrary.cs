using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;

namespace OEA.UnitTest
{
    class UnitTestLibrary : LibraryPlugin
    {
        public override ReuseLevel ReuseLevel
        {
            get { return ReuseLevel.Main; }
        }

        public override void Initialize(IApp app)
        {
        }
    }
}