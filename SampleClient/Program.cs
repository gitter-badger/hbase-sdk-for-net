using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.HBase.Client;
using SampleClient.Dtos;

namespace SampleClient
{
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using org.apache.hadoop.hbase.rest.protobuf.generated;

    class Program
        {

            private static void Main(string[] args)
            {
                const string clusterURL = "http://localhost:5555";
                const string hadoopUsername = "root";
                const string hadoopUserPassword = "hadoop";
                var serializer = new JsonDotNetSerializer();
                // Create a new instance of an HBase client.
                var hbaseClient = new HBaseClient(new ClusterCredentials(new Uri(clusterURL), hadoopUsername, hadoopUserPassword));
                var itemMapper = new ItemMapper(serializer);
                hbaseClient.DeleteTable("Items");
                var tableSchema = itemMapper.CreateTableSchema("Items");
                hbaseClient.CreateTable(tableSchema);

                //var perfCheck = new PerformanceChecksManager(hbaseClient, "Items", itemMapper, new ItemsGenerator(),50,100);
                //perfCheck.RunPerformanceLoad();
                
                //Generating item.
                var itemsGenerator = new ItemsGenerator();
                var item = itemsGenerator.CreateItem("mycode");

                //Generating cells relevant to the generate item and saving the entity to Hbae.
                var itemCellSet = itemMapper.GetCells(item, "en-US");
                var stausCell = itemCellSet.rows.Single().values.Single(cell => Encoding.UTF8.GetString(cell.column).Equals("CF1:Status"));
                stausCell.data = new byte[0];
                hbaseClient.StoreCells("Items", itemCellSet);

                //making sure item stored well.
                var originalItemCells = hbaseClient.GetCells("Items", item.Code);
                var originalItemFetchedFromDb = itemMapper.GetDto(originalItemCells, "en-US");

                if (!string.IsNullOrEmpty(originalItemFetchedFromDb.Status))
                    throw new Exception();

                //Describing the conditional update expression.
                var cellToCheck = new Cell { column = Encoding.UTF8.GetBytes("CF1:Status"), data = new byte[0] };

                //manipulating original item.
                stausCell = itemCellSet.rows.Single().values.Single(cell => Encoding.UTF8.GetString(cell.column).Equals("CF1:Status"));
                var newStatusValue = Encoding.UTF8.GetBytes("new");
                stausCell.data = newStatusValue;
                
                //Testing new functionality...

                hbaseClient.CheckAndPutCells("Items", itemCellSet, cellToCheck);
                //Thread.Sleep(1);
                itemCellSet = hbaseClient.GetCells("Items", item.Code);
                var itemFromDb = itemMapper.GetDto(itemCellSet, "en-US");

                if (!itemFromDb.Status.Equals(Encoding.UTF8.GetString(newStatusValue)))
                    throw new Exception();


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
