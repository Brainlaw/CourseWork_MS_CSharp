using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MS_Lab3
{
    class Program
    {
        private static void ModelMultipleTimes(int queueSize, double lambda0, double lambda1, double time)
        {
            double[] fractionOfDeclines = { 0, 0, 0, 0 }; // ймовірність відмови в обслуговуванні
            double[] fractionOfBusiness = { 0, 0, 0, 0 }; // спостережуване значення завантаження пристроїв
            double[] maximumFractionOfBusiness = { 0, 0, 0, 0 }; // МАХ спостережуване значення завантаження пристроїв
            double[] averageQueueLength = { 0, 0, 0, 0 }; // спостережуване значення черг
            int[] maximumQueueLength = { 0, 0, 0, 0 }; // МАХ спостережуване значення черг
            double[] averageWaitingTime = { 0, 0, 0, 0 }; // середній час очікування

            for (var i = 0; i < 10; i++) // верифікація моделі (10 ітерацій)
            {
                Model model = new Model(lambda0, lambda1);

                // створення компонентів згідно наданої схеми моделі
                Dispose Despose = new Dispose(model, "Despose");
                Machine process4 = new Machine(model, queueSize, "PROCESS 4", Despose);
                Machine process3 = new Machine(model, queueSize, "PROCESS 3", process4);
                Machine process2 = new Machine(model, queueSize, "PROCESS 2", Despose);
                Machine process1 = new Machine(model, queueSize, "PROCESS 1", process2, process3);
                process4.nextPossibleMachines.Add(process1);

                model.Simulate(time);

                for (var j = 0; j < 4; j++)
                {
                    var statisticsForMachine = model.machines[j].Statistics();

                    fractionOfDeclines[j] += (statisticsForMachine.Item1 * 0.1);
                    fractionOfBusiness[j] += (statisticsForMachine.Item2 * 0.1);
                    averageQueueLength[j] += (statisticsForMachine.Item3 * 0.1);
                    averageWaitingTime[j] += (statisticsForMachine.Item5 * 0.1);

                    if (statisticsForMachine.Item2 > maximumFractionOfBusiness[j])
                        maximumFractionOfBusiness[j] = statisticsForMachine.Item2;

                    if (statisticsForMachine.Item4 > maximumQueueLength[j])
                        maximumQueueLength[j] = statisticsForMachine.Item4;
                }
            }

            Console.WriteLine("{0,5} | {1,10:F2} | {2,10:F2} | {3,8:F2} | ", queueSize, lambda0, lambda1, time); // вхідні параметри моделі


            for (var j = 0; j < 4; j++)
            {
                // вихідні параметри моделі
                Console.WriteLine("                                           | {0,8:F3} | {1,8:F3} | {2,12:F3} | {3,8:F3} | {4,11:F3} | {5,10:F3}",
                    fractionOfDeclines[j],
                    fractionOfBusiness[j],
                    maximumFractionOfBusiness[j],
                    averageQueueLength[j],
                    maximumQueueLength[j],
                    averageWaitingTime[j]);
            }
        }


        static void Analysis()
        {
            //                  0           1           2           3           4       5           6           7               8           9
            //Console.WriteLine("queue | lambda-enq | lambda-dur | mod.time | declines | business | max.business | av.queue | max.waiting | av.waiting");
            Console.WriteLine("current time      |");
            //Console.WriteLine("\n(Reference)");
            ModelMultipleTimes(10, 1, 1, 100);

            //Console.WriteLine("\n(Increasing the queue size)");
            for (var i = 1; i < 10; i++)
                ModelMultipleTimes((int)(10 * Math.Pow(2, i)), 1, 1, 100);

            //Console.WriteLine("\n(Increasing the lambda of exponential distribution for enqueueing time)");
            for (var i = 1; i < 10; i++)
                ModelMultipleTimes(10, (1 * Math.Pow(2, i)), 1, 100);

            //Console.WriteLine("\n(Increasing the lambda of exponential distribution for duration time)");
            for (var i = 1; i < 10; i++)
                ModelMultipleTimes(10, 1, (1 * Math.Pow(2, i)), 100);

            //Console.WriteLine("\n(Increasing the modelling time)");
            for (var i = 1; i < 10; i++)
                ModelMultipleTimes(10, 1, 1, (100 * Math.Pow(2, i)));
        }

        static void Main(string[] args)
        {
            Analysis();
        }
    }
}
