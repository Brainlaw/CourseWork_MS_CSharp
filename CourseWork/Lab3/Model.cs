// Model.cs
using System;
using System.Collections.Generic;

namespace MS_Lab3
{
    public class Model
    {
        public double lambdaForEnqueueing;
        public double lambdaForDuration;
        public double timeCurrent;
        public List<Machine> machines;
        public List<Tuple<double, int, Transaction, Executor>> nextEvents;
        public NumberGenerator generator;

        public Model(double lambda0, double lambda1)
        {
            lambdaForEnqueueing = lambda0;
            lambdaForDuration = lambda1;
            nextEvents = new List<Tuple<double, int, Transaction, Executor>>();
            timeCurrent = 0;
            generator = new NumberGenerator();
            machines = new List<Machine>();
        }

        // Генеруємо наступну транзакцію
        public Transaction GenerateNextTransaction()
        {
            return new Transaction(timeCurrent + generator.Exponential(lambdaForEnqueueing), generator.Exponential(lambdaForDuration));
        }

        // Вставляємо подію в список майбутніх подій
        public void InsertToRightPlace(Tuple<double, int, Transaction, Executor> eventInformation)
        {
            if (nextEvents.Count == 0)
            {
                nextEvents.Add(eventInformation);
                return;
            }

            for (var i = 0; i < nextEvents.Count; i++)
            {
                if (eventInformation.Item1 <= nextEvents[i].Item1)
                {
                    nextEvents.Insert(i, eventInformation);
                    return;
                }
            }

            nextEvents.Add(eventInformation);
        }

        // Створення наступної транзакції та включення її "в чергу" до списку майбутніх подій
        public void PrepareToNextTransaction()
        {
            var futureRequest = GenerateNextTransaction();
            InsertToRightPlace(new Tuple<double, int, Transaction, Executor>(futureRequest.enqueueingTime, 1, futureRequest, machines[0]));
        }

        public void ProcessEnqueueingFully(Machine machine, Tuple<double, int, Transaction, Executor> dataAboutCurrentTime)
        {
            if (machine.isFree)
            {
                // Якщо машина вільна, її слід негайно завантажити
                PrepareToNextTransaction();
                machine.currentRequest = dataAboutCurrentTime.Item3;
                machine.currentRequest.actualStartTime = timeCurrent;
                machine.isFree = false;
                // Console.WriteLine("{0} | the new Transaction is executed on {1}", timeCurrent, machine.name); //time
                var thisEventEnds = new Tuple<double, int, Transaction, Executor>
                    (timeCurrent + machine.currentRequest.executionTime, 3, machine.currentRequest, machine);
                InsertToRightPlace(thisEventEnds);
            }
            else
            {
                // Якщо машина не вільна, її слід або поставити в чергу, або відхилити
                //Console.Write("{0} | an attempt to enqueue a Transaction to {1}: ", timeCurrent, machine.name);

                var newExecutionTime = generator.Exponential(lambdaForDuration);
                var code = machine.TryToEnqueue(dataAboutCurrentTime.Item3);
                if (code != -1)
                    PrepareToNextTransaction();

            }
        }

        // Імітація моделі
        public void Simulate(double modellingTime)
        {
            Tuple<double, int, Transaction, Executor> dataAboutCurrentTime = null;
            var firstRequest = GenerateNextTransaction();
            var dataAboutFutureTime = new Tuple<double, int, Transaction, Executor>(firstRequest.enqueueingTime, 1, firstRequest, machines[0]);
            InsertToRightPlace(dataAboutFutureTime);

            while (timeCurrent < modellingTime)
            {
                if (nextEvents.Count == 0)
                {
                    // Більше нічого моделювати
                    return;
                }

                // Виділення даних про подію, яка повинна відбутися зараз
                dataAboutCurrentTime = nextEvents[0];
                timeCurrent = dataAboutCurrentTime.Item1;
                nextEvents.RemoveAt(0);

                var machine = dataAboutCurrentTime.Item4;

                // Зміна стану системи має сенс лише коли enqueueing/dequeueing to or from the machine
                if (machine is Machine)
                {

                    // Якщо enqueueing
                    if (dataAboutCurrentTime.Item2 == 1)
                        ProcessEnqueueingFully(machine as Machine, dataAboutCurrentTime);
                    else
                    {
                        // Stop 
                        PrepareToNextTransaction();
                        (machine as Machine).currentRequest = null;
                        (machine as Machine).isFree = true;
                        //Console.WriteLine("{0} | the Transaction left {1}", timeCurrent, machine.name);  //time

                        // Якщо в черзі є транзакції, слід почати виконувати наступну
                        if ((machine as Machine).queue.Count != 0)
                        {
                            (machine as Machine).currentRequest = (machine as Machine).queue.Dequeue();
                            (machine as Machine).currentRequest.actualStartTime = timeCurrent;
                            (machine as Machine).isFree = false;
                            // Console.WriteLine("{0} | the new Transaction is executed on {1}", timeCurrent, machine.name); //time

                            var thisEventEnds = new Tuple<double, int, Transaction, Executor>
                                (timeCurrent + (machine as Machine).currentRequest.executionTime, 3, (machine as Machine).currentRequest, machine);
                            InsertToRightPlace(thisEventEnds);
                        }

                        // Ту що перестала працювати, слід відправити на іншу машину
                        var nextDestinationOfRequest = (machine as Machine).NextMachine();
                        if (nextDestinationOfRequest is Machine)
                        {
                            //Console.Write("{0} | attempt to enqueue to {1}: ", timeCurrent, nextDestinationOfRequest.name);
                            (nextDestinationOfRequest as Machine).TryToEnqueue(dataAboutCurrentTime.Item3);

                            (nextDestinationOfRequest as Machine).fullQueueSizeHistory.Add(
                                new Tuple<double, int, bool>(timeCurrent, (nextDestinationOfRequest as Machine).queue.Count,
                                (nextDestinationOfRequest as Machine).isFree));
                        }
                    }
                    (machine as Machine).fullQueueSizeHistory.Add(
                        new Tuple<double, int, bool>(timeCurrent, (machine as Machine).queue.Count, (machine as Machine).isFree));
                }
            }
        }
    }
}
