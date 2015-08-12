using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using org.apache.hadoop.hbase.rest.protobuf.generated;
using SampleClient.Dtos;

namespace SampleClient
{
    class ItemMapper
    {
        private readonly JsonDotNetSerializer _serializer;

        public ItemMapper(JsonDotNetSerializer serializer)
        {
            _serializer = serializer;
        }

        public TableSchema CreateTableSchema(string name)
        {
            //Create a new HBase table.
            TableSchema testTableSchema = new TableSchema();
            testTableSchema.name = name;
            testTableSchema.columns.Add(new ColumnSchema() { name = "DATA" });
            testTableSchema.columns.Add(new ColumnSchema() { name = "Hierarchy" });
            testTableSchema.columns.Add(new ColumnSchema() { name = "Descriptions" });
            testTableSchema.columns.Add(new ColumnSchema() { name = "Groups" });
            testTableSchema.columns.Add(new ColumnSchema() { name = "Identifiers" });
            testTableSchema.columns.Add(new ColumnSchema() { name = "Attributes" });
            return testTableSchema;
        }

        public CellSet GetCells(ItemDto item, string culture)
        {
            var cellSetRow = new CellSet.Row { key = Encoding.UTF8.GetBytes(item.Code) };
            var cells = new List<Cell>();
            cells.Add(new Cell { column = Encoding.UTF8.GetBytes("DATA:Status"), data = Encoding.UTF8.GetBytes(item.Status) });
            //cells.Add(new Cell { column = Encoding.UTF8.GetBytes("DATA:Status"), data = _serializer.SerializeToBson(item.Status) });
            cells.Add(new Cell { column = Encoding.UTF8.GetBytes("Hierarchy:" + item.MerchandiseHierarchy.Code), data = _serializer.SerializeToBson(item.MerchandiseHierarchy) });
            cells.Add(new Cell { column = Encoding.UTF8.GetBytes("DATA:CreatedDate"), data = Encoding.UTF8.GetBytes(DateTime.Now.ToString()) });
            cells.Add(new Cell { column = Encoding.UTF8.GetBytes("DATA:UpdatedDate"), data = Encoding.UTF8.GetBytes(DateTime.Now.ToString()) });
            cells.Add(new Cell { column = Encoding.UTF8.GetBytes("Descriptions:Long_" + culture), data = Encoding.UTF8.GetBytes(item.Description) });
            cells.Add(new Cell { column = Encoding.UTF8.GetBytes("Descriptions:Short_" + culture), data = Encoding.UTF8.GetBytes(item.ShortDescription) });

            foreach (var group in item.Groups)
                cells.Add(new Cell { column = Encoding.UTF8.GetBytes("Groups:" + group.Code), data = _serializer.SerializeToBson(group) });

            foreach (var identifier in item.Identifiers)
                cells.Add(new Cell { column = Encoding.UTF8.GetBytes("Identifiers:" + identifier.Type), data = Encoding.UTF8.GetBytes(identifier.Value) });

            foreach (var attributes in item.Attributes)
                cells.Add(new Cell { column = Encoding.UTF8.GetBytes("Attributes:" + culture + "_" + attributes.Id), data = _serializer.SerializeToBson(attributes.Value) });
            cellSetRow.values.AddRange(cells);

            var cellSet = new CellSet();
            cellSet.rows.Add(cellSetRow);
            return cellSet;
        }

        public ItemDto GetDto(CellSet cellSet, string culture)
        {
            var itemRow = cellSet.rows.First();
            //Retrieve a cell by its key.
            var columnNameMapping = itemRow.values.ToDictionary(o => Encoding.UTF8.GetString(o.column), o => o.data);

            var item = new ItemDto();
            item.Code = Encoding.UTF8.GetString(itemRow.key);
            item.Status = Encoding.UTF8.GetString(columnNameMapping["DATA:Status"]);
            //item.Status = (string)_serializer.DeseralizeFromBson(columnNameMapping["DATA:Status"],typeof(string));

            var hierarchyColumnKey = columnNameMapping.Keys.Where(o => o.Contains("Hierarchy"));
            item.MerchandiseHierarchy = (HierarchyDto)_serializer.DeseralizeFromBson(columnNameMapping[hierarchyColumnKey.First()], typeof(HierarchyDto));

            item.CreatedDate =DateTime.Parse(Encoding.UTF8.GetString(columnNameMapping["DATA:CreatedDate"]));
            item.LastUpdateDate = DateTime.Parse(Encoding.UTF8.GetString(columnNameMapping["DATA:UpdatedDate"]));
            item.Description = Encoding.UTF8.GetString(columnNameMapping["Descriptions:Long_" + culture]);
            item.ShortDescription = Encoding.UTF8.GetString(columnNameMapping["Descriptions:Short_" + culture]);

            var groupColumnKeys = columnNameMapping.Keys.Where(o => o.StartsWith("Groups:"));
            item.Groups = new List<HierarchyDto>();
            foreach (var groupKey in groupColumnKeys)
                item.Groups.Add((HierarchyDto)_serializer.DeseralizeFromBson(columnNameMapping[groupKey], typeof(HierarchyDto)));

            var identifiersColumnKeys = columnNameMapping.Keys.Where(o => o.StartsWith("Identifiers:"));
            item.Identifiers = new List<Identifier>();
            foreach (var identifierKey in identifiersColumnKeys)
                item.Identifiers.Add(new Identifier{ Value= Encoding.UTF8.GetString(columnNameMapping[identifierKey]),Type= identifierKey.Substring(13)});

            var attributesColumnKeys = columnNameMapping.Keys.Where(o => o.StartsWith("Attributes:" + culture));
            item.Attributes = new List<AttributeDto>();
            foreach (var attributeKey in attributesColumnKeys)
                item.Attributes.Add(new AttributeDto { Id = attributeKey.Substring(17), Value = (AttributeValue)_serializer.DeseralizeFromBson(columnNameMapping[attributeKey], typeof(AttributeValue)) });

            

            return item;
        }


    }


}

