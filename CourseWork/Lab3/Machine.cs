// Machine.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS_Lab3
{
    public class Machine : Executor
    {
        public bool isFree;
        public List<Executor> nextPossibleMachines;
        public Queue<Transaction> queue;
        public int queueSize;
        public List<Tuple<double, int, bool>> fullQueueSizeHistory;
        public Transaction currentRequest;
        public int totalNumberOfEnqueuedRequests;
        public int totalNumberOfDeclines;

        public Machine(Model thisModel, int futureQueueSize, string newName, params Executor[] nextPossible)
        {
            model = thisModel;
            name = newName;
            isFree = true;
            nextPossibleMachines = new List<Executor>();
            foreach (var machine in nextPossible)
                nextPossibleMachines.Add(machine);
            queueSize = futureQueueSize;
            queue = new Queue<Transaction>();
            fullQueueSizeHistory = new List<Tuple<double, int, bool>>();
            currentRequest = null;
            totalNumberOfEnqueuedRequests = 0;
            totalNumberOfDeclines = 0;

            thisModel.machines.Insert(0, this);
        }

        public Executor NextMachine()
        {
            int numberOfNextMachines = nextPossibleMachines.Count;
            int machineIndex = (int)Math.Floor(model.generator.Xi() * numberOfNextMachines);
            return nextPossibleMachines[machineIndex];
        }

        // Обчислення загального часу, витраченого на кожну транзакцію в черзі
        public double TotalTimeInQueue()
        {
            double totalTimeInQueue = 0;
            for (var i = 0; i < fullQueueSizeHistory.Count - 1; i++)
            {
                var timeDifference = fullQueueSizeHistory[i + 1].Item1 - fullQueueSizeHistory[i].Item1;
                totalTimeInQueue += (timeDifference * fullQueueSizeHistory[i].Item2);
            }
            return totalTimeInQueue;
        }

        public double TotalTimeWhenBusy()
        {
            double totalTimeWhenBusy = 0;

            for (var i = 0; i < fullQueueSizeHistory.Count - 1; i++)
            {
                double thisPeriodOfTime = fullQueueSizeHistory[i + 1].Item1 - fullQueueSizeHistory[i].Item1;
                if (!fullQueueSizeHistory[i].Item3)
                    totalTimeWhenBusy += thisPeriodOfTime;
            }

            return totalTimeWhenBusy;
        }

        public int MaxQueueSize()
        {
            int maxQueueSize = 0;
            for (var i = 0; i < fullQueueSizeHistory.Count; i++)
                if (fullQueueSizeHistory[i].Item2 > maxQueueSize)
                    maxQueueSize = fullQueueSizeHistory[i].Item2;

            return maxQueueSize;
        }

        // Розрахунок вихідних статистичних параметрів моделі після завершення виконання
        public Tuple<double, double, double, int, double> Statistics()
        {
            double totalTimeInQueue = TotalTimeInQueue();
            double totalTimeWhenBusy = TotalTimeWhenBusy();

            double fractionOfDeclines = (double)totalNumberOfDeclines / (double)(totalNumberOfEnqueuedRequests + totalNumberOfDeclines);
            double fractionOfBusiness = (double)totalTimeWhenBusy / (double)fullQueueSizeHistory[fullQueueSizeHistory.Count - 1].Item1;
            double averageQueueLength = (double)totalTimeInQueue / (double)fullQueueSizeHistory[fullQueueSizeHistory.Count - 1].Item1;
            int maximumQueueLength = MaxQueueSize();
            double averageWaitingTime = (double)totalTimeInQueue / (double)totalNumberOfEnqueuedRequests;

            return new Tuple<double, double, double, int, double>
                (fractionOfDeclines, fractionOfBusiness, averageQueueLength, maximumQueueLength, averageWaitingTime);
        }

        // Спроба поставити транзакцію в чергу
        // Повертає код успіху (1 або 2) або невдачі (-1)

        // -1 = відхилено
        //  0 = нічого не відбувається
        //  1 = в черзі
        //  2 = в процесі
        //  3 = виконано

        public int TryToEnqueue(Transaction Transaction)
        {
            if (isFree && queue.Count == 0)
            {
                totalNumberOfEnqueuedRequests++;
                Transaction.status = 2;
                Transaction.actualStartTime = model.timeCurrent;
                model.InsertToRightPlace(new Tuple<double, int, Transaction, Executor>
                    (Transaction.actualStartTime + Transaction.executionTime, 3, Transaction, this));
                isFree = false;
                //Console.WriteLine("status --> loaded immediately");
            }
            else if (queue.Count < queueSize)
            {
                totalNumberOfEnqueuedRequests++;
                Transaction.status = 1;
                queue.Enqueue(Transaction);
                //Console.WriteLine("status --> enqueued");
            }
            else
            {
                Transaction.status = -1;
                totalNumberOfDeclines++;
                //Console.WriteLine("status --> declined");
            }

            return Transaction.status;
        }
    }
}
