using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;

namespace OEA.UnitTest
{
    class UnitTestLibrary : ILibrary
    {
        public ReuseLevel ReuseLevel
        {
            get { return ReuseLevel.Main; }
        }

        public void Initialize(IApp app)
        {
        }
    }
}