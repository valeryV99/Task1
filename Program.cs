using System;
using System.Collections.Concurrent;
using System.Threading;

//Создать класс на языке C#, который: 
//- называется TaskQueue и реализует логику пула потоков;
//- создает указанное количество потоков пула в конструкторе;
//- содержит очередь задач в виде делегатов с одним параметром:
//- delegate void TaskDelegate(object obj);
//- обеспечивает постановку в очередь и последующее выполнение делегатов с помощью метода 
//  void EnqueueTask(TaskDelegate task);
//-     обеспечивает ожидание завершения всех поставленных в очередь делегатов и
//-   уничтожает все созданные потоки в пуле.



namespace Task1
{
    public delegate void TaskDelegate(TaskDelegateParam obj);

    public class TaskDelegateParam
    {
        public int tasksCount { get; set; } 
    }
    public class TaskQueue
    {
        private readonly BlockingCollection<TaskDelegate> _blockingQueue
            = new BlockingCollection<TaskDelegate>(new ConcurrentQueue<TaskDelegate>());

        public TaskQueue(int threadCount)
        {
            for (var i = 1; i < threadCount + 1; i++)
            {
                var thread = new Thread(DoThreadWork);
                thread.Start();
            }
        }

        public void EnqueueTask(TaskDelegate task)
        {
            _blockingQueue.Add(task);
        }

        private void DoThreadWork(object param)
        {
            while (true)
            {
                if (_blockingQueue.Count == 0)
                {
                    break;
                }
                TaskDelegateParam someParam = new TaskDelegateParam();
                someParam.tasksCount = _blockingQueue.Count;
                var task = _blockingQueue.Take();
                try
                {
                    task(someParam);
                }
                catch (ThreadAbortException)
                {
                    Thread.ResetAbort();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            Console.WriteLine("Finished Thread #" + Thread.CurrentThread.ManagedThreadId );
        }
    }

    static class Program
    {
        private static void TestTask(TaskDelegateParam obj)
        {
            Thread.Sleep(1000);
            Console.WriteLine("completed task of " + obj.tasksCount);
        }

        static void Main(string[] args)
        {
            var taskQueue = new TaskQueue(3);
            for (var i = 0; i < 10; i++)
            {
                taskQueue.EnqueueTask(TestTask);
            }
            
        }
    }
}