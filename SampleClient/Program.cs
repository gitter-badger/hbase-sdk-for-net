using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.HBase.Client;
using SampleClient.Dtos;

namespace SampleClient
{
    using System.Threading.Tasks;

    class Program
        {

            private static void Main(string[] args)
            {
                const string clusterURL = "http://localhost:5555";
                const string hadoopUsername = "root";
                const string hadoopUserPassword = "hadoop";

                // Create a new instance of an HBase client.
                var hbaseClient = new HBaseClient(new ClusterCredentials(new Uri(clusterURL), hadoopUsername, hadoopUserPassword));
                var itemMapper = new ItemMapper(new JsonDotNetSerializer());
                hbaseClient.DeleteTable("Items");
                var tableSchema = itemMapper.CreateTableSchema("Items");
                hbaseClient.CreateTable(tableSchema);

                var perfCheck = new PerformanceChecksManager(hbaseClient, "Items", itemMapper, new ItemsGenerator(),50,100);


                perfCheck.RunPerformanceLoad();






           


                var tasks = new List<Task>();

                for (int i = 0; i < 10; i++)
                {
               
                }








                //var item = CreateItem("mycode");
              
               
          

                //var itemCellSet = itemMapper.GetCells(item, "en-US");
                //hbaseClient.StoreCells("Items", itemCellSet);

                //itemCellSet = hbaseClient.GetCells("Items", item.Code);
                //var itemFromDb = itemMapper.GetDto(itemCellSet, "en-US");

                //if (!itemFromDb.Groups.First().Code.Equals(item.Groups.First().Code))
                //    throw new Exception();


                #region Old-Helper
                //string clusterURL = "http://localhost:5555";
                //string hadoopUsername = "root";
                //string hadoopUserPassword = "hadoop";
                //string hbaseTableName = "MyCoolTable";

                //// Create a new instance of an HBase client.
                //ClusterCredentials creds = new ClusterCredentials(new Uri(clusterURL), hadoopUsername, hadoopUserPassword);
                //HBaseClient hbaseClient = new HBaseClient(creds);

                //var hbaseHelper = new Helper(hbaseClient, "ThisIsJustATableForShirly1", "DummyKey1ForShirly1");
                //hbaseHelper.CreateTable();
                //var myClass = new Dto()
                //{
                //    Field1 = "1",
                //    Field2 = "2",
                //    Field3 = "3",
                //    Field4 = "4",
                //    Field5 = "5",
                //    NestedData = new NestedDto() { Field1 = "n1", Field2 = "n2" }
                //};

                //hbaseHelper.SaveMyDto(myClass);

                //var dto = hbaseHelper.GetMyDto();
#endregion
            }
        }
    }
