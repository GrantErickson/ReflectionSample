using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReflectionSample
{
    internal class PrivateAdder: IAdder
    {
        private int lastResult;
        public int Add(int a, int b)
        {
            lastResult = a + b;
            return lastResult;
        }
    }
}
