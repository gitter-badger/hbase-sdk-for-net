using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.HBase.Client;
using SampleClient.Dtos;

namespace SampleClient
{
    class Program
        {

         private static ItemDto CreateItem(string code)
         {
             return new ItemDto
             {
                 Code = code,
                 Attributes =
                     new List<AttributeDto>()
                     {
                         new AttributeDto { Id = "attributeId", Value = new AttributeValue { Name = "attributeName", Value = "attributeValue" } }
                     },
                 MerchandiseHierarchy = new HierarchyDto { Code = "hierarchyCode", Title = "hierarchyTitle" },
                 Description = "longDescription",
                 ShortDescription = "shortDescription",
                 Groups =
                     new List<HierarchyDto>
                     {
                         new HierarchyDto { Code = "hierarchyCode1", Title = "hierarchyTitle1" },
                         new HierarchyDto { Code = "hierarchyCode2", Title = "hierarchyTitle2" }
                     },
                 Identifiers = new List<Identifier> { new Identifier { Type = "identifierType", Value = "identifierValue" } },
                 Status = "Enabled"
             };
         }

            private static void Main(string[] args)
            {
                const string clusterURL = "http://localhost:5555";
                const string hadoopUsername = "root";
                const string hadoopUserPassword = "hadoop";

                // Create a new instance of an HBase client.
                var hbaseClient = new HBaseClient(new ClusterCredentials(new Uri(clusterURL), hadoopUsername, hadoopUserPassword));

                var item = CreateItem("mycode");
                var itemMapper = new ItemMapper(new JsonDotNetSerializer());
                var tableSchema = itemMapper.CreateTableSchema("Items");
                hbaseClient.CreateTable(tableSchema);
          

                var itemCellSet = itemMapper.GetCells(item, "en-US");
                hbaseClient.StoreCells("Items", itemCellSet);

                itemCellSet = hbaseClient.GetCells("Items", item.Code);
                var itemFromDb = itemMapper.GetDto(itemCellSet, "en-US");

                if (!itemFromDb.Groups.First().Code.Equals(item.Groups.First().Code))
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
