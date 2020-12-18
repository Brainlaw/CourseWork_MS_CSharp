using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS_Lab3
{
    public class Transaction
    {
        public double enqueueingTime;
        public double actualStartTime;
        public double executionTime;

        public int status; // -1 = declined, 0 = nothing so far, 1 = enqueued, 2 = in process, 3 = done
    public int totalNumberOfEnqueuedRequests;
        public int totalNumberOfDeclines;

        public Transaction(double enqueueing, double execution)
        {
            enqueueingTime = enqueueing;
            executionTime = execution;
            actualStartTime = double.MaxValue;
            status = 0;

        }

    }
}
