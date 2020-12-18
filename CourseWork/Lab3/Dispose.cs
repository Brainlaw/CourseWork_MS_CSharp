using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS_Lab3
{
    public class Dispose : Executor
    {
        public Dispose(Model actualModel, string newName)
        {
            model = actualModel;

            name = newName;
        }
    }
}
