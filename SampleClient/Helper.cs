using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using Microsoft.HBase.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Serialization;
using org.apache.hadoop.hbase.rest.protobuf.generated;

namespace SampleClient
{
    public class NestedDto
    {
        public string Field1 { get; set; }
        public string Field2 { get; set; }
    }

    public class Dto
    {
        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string Field3 { get; set; }
        public string Field4 { get; set; }
        public string Field5 { get; set; }
        public NestedDto NestedData { get; set; }

    }



    public class Helper
    {
        //Probably not thread safe.

        private readonly HBaseClient _client;
        private readonly string _sampleTableName;
        private readonly string _sampleRowKey;
        private readonly Dictionary<string, byte[]> _columnNameMapping;
        private readonly JsonSerializer _serializer;


        public Helper(HBaseClient client, string sampleTableName, string sampleRowKey)
        {
            _client = client;
            _sampleTableName = sampleTableName;
            _sampleRowKey = sampleRowKey;

            _serializer = new JsonSerializer
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver =
                    new DefaultContractResolver { DefaultMembersSearchFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance },
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            };


            _columnNameMapping = new Dictionary<string, byte[]>();
        }
        public void CreateTable()
        {
            //Create a new HBase table.
            TableSchema testTableSchema = new TableSchema();
            testTableSchema.name = _sampleTableName;
            testTableSchema.columns.Add(new ColumnSchema() { name = "CF1" });
            testTableSchema.columns.Add(new ColumnSchema() { name = "CF2" });
            _client.CreateTable(testTableSchema);
        }

        public void SaveMyDto(Dto dto)
        {
            CellSet cellSet = new CellSet();
            CellSet.Row cellSetRow = new CellSet.Row { key = Encoding.UTF8.GetBytes(_sampleRowKey) };
            cellSet.rows.Add(cellSetRow);

            Cell value1 = new Cell { column = Encoding.UTF8.GetBytes("CF1:field1"), data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dto.Field1)) };
            Cell value2 = new Cell { column = Encoding.UTF8.GetBytes("CF1:field2"), data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dto.Field2)) };
            Cell value3 = new Cell { column = Encoding.UTF8.GetBytes("CF1:field3"), data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dto.Field3)) };
            Cell value4 = new Cell { column = Encoding.UTF8.GetBytes("CF1:field4"), data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dto.Field4)) };
            Cell value5 = new Cell { column = Encoding.UTF8.GetBytes("CF1:field5"), data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dto.Field5)) };
            //     Cell value6 = new Cell { column = Encoding.UTF8.GetBytes("CF1:NestedData"), data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dto.NestedData))};

            byte[] nestedDataBlob;

            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new BsonWriter(memoryStream))
                {
                    _serializer.Serialize(writer, dto.NestedData);
                    nestedDataBlob = memoryStream.ToArray();
                }
            }

            Cell value6 = new Cell { column = Encoding.UTF8.GetBytes("CF1:NestedData"), data = nestedDataBlob };
            cellSetRow.values.AddRange(new List<Cell>() { value1, value2, value3, value4, value5, value6 });
            _client.StoreCells(_sampleTableName, cellSet);
        }

        public Dto GetMyDto()
        {
            //Retrieve a cell by its key.
            var cellSet = _client.GetCells(_sampleTableName, _sampleRowKey);

            foreach (var value in cellSet.rows.Single().values)
            {
                //maps everything 
                var columnName = Encoding.UTF8.GetString(value.column);
                _columnNameMapping.Add(columnName, value.data);
            }

            //converting
            var composedDto = BuildUpDto();
            return composedDto;

        }

        private Dto BuildUpDto()
        {
            var myDto = new Dto();
            myDto.Field1 = JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(_columnNameMapping["CF1:field1"]));
            myDto.Field2 = JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(_columnNameMapping["CF1:field2"]));
            myDto.Field3 = JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(_columnNameMapping["CF1:field3"]));
            myDto.Field4 = JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(_columnNameMapping["CF1:field4"]));
            myDto.Field5 = JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(_columnNameMapping["CF1:field5"]));
            //myDto.NestedData=JsonConvert.DeserializeObject<NestedDto>(Encoding.UTF8.GetString(_columnNameMapping["CF1:NestedData"]));

            var ms = new MemoryStream(_columnNameMapping["CF1:NestedData"]);
            using (var reader = new BsonReader(ms))
            {
                myDto.NestedData = _serializer.Deserialize<NestedDto>(reader);

            }

            return myDto;
       
        }
    }
}
