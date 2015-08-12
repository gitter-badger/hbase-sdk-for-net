using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.HBase.Client;
 using SampleClient.Dtos;
using System.Linq;
using System.Collections;
using System.Collections.Concurrent;

namespace SampleClient
{
    

    class PerformanceChecksManager
    {
        private readonly IHBaseClient _hBaseClient;
        private readonly string _tableName;
        private readonly ItemMapper _itemMapper;
        private readonly ItemsGenerator _itemsGenerator;
        private readonly int _numOfThreads;
        private readonly int _requestsPerThread;
        private readonly ConcurrentBag<string> _exceptions; 

        public PerformanceChecksManager(IHBaseClient hBaseClient,string tableName, ItemMapper itemMapper, ItemsGenerator itemsGenerator, int numOfThreads, int requestsPerThread)
        {
            _hBaseClient = hBaseClient;
            _tableName = tableName;
            _itemMapper = itemMapper;
            _itemsGenerator = itemsGenerator;
            _numOfThreads = numOfThreads;
            _requestsPerThread = requestsPerThread;
            _exceptions = new ConcurrentBag<string>();
        }
        private IEnumerable<Task>  GenerateTasks()
        {
            var tasks = new List<Task>();

            for (int i = 0; i < _numOfThreads; i++)
            {
                var task = new Task(() =>
                {
                    for (int j = 0; j< _requestsPerThread; j++)
                    {
                        var item = _itemsGenerator.CreateItem("code" + Guid.NewGuid().ToString());

                        Console.WriteLine("Thread " + Environment.CurrentManagedThreadId.ToString() + " sending request number " + j.ToString());
                        try
                        {
                            _hBaseClient.StoreCells(_tableName, _itemMapper.GetCells(item, "en-US"));
                        }
                        catch (Exception ex)
                        {
                            _exceptions.Add(
                                "Thread number : " + Environment.CurrentManagedThreadId.ToString() + "- request number " + j.ToString() +
                                "failed with the following error: " + ex.Message);
                        }
                       
                        Console.WriteLine("Request number " + j.ToString() + " of thread id " + Environment.CurrentManagedThreadId.ToString() + " finished");
                    }
                });

                tasks.Add(task);
            }

            return tasks;

        }

        public void RunPerformanceLoad()
        {
            var tasks = GenerateTasks();
            tasks.ToList().ForEach(t => t.Start());

            Task.WaitAll(tasks.ToArray());




        }
    }
}
