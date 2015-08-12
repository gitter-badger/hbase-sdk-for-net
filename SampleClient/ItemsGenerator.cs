using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleClient
{
    using SampleClient.Dtos;

    public class ItemsGenerator
    {
        public ItemDto CreateItem(string code)
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
    }
}
