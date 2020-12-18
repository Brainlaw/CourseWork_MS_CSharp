using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS_Lab3
{
    public class NumberGenerator : Random
    {

//	Returns number between 0 and 1 public double Xi()1
        public double Xi()
        {
            return base.Sample();
        }


//	Returns exponentially distributed value based on parametres

//	It uses the inverse cumulative distribution function
        public double Exponential(double lambda)
        {
            return (-Math.Log(Xi()) / lambda);
        }
    }
}
