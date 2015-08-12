using System;
using System.Collections.Generic;

namespace SampleClient.Dtos
{
    public class ItemDto
    {
        public string Code { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public List<AttributeDto> Attributes { get; set; }
        public List<Identifier> Identifiers { get; set; }
        public string Status { get; set; }
        public List<HierarchyDto> Groups { get; set; }
        public HierarchyDto MerchandiseHierarchy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdateDate { get; set; }
    }
}
