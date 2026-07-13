using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Modules.InboundModule.DTOs
{
    public class InboundOrderDto
    {
        public string SourceSystem { get; set; } = "ERP";

        public string SourceDocNo { get; set; }

        public string SourceDocType { get; set; }

        public string? WarehouseCode { get; set; }

        public long? WarehouseId { get; set; }

        public string? SupplierCode { get; set; }

        public long? SuplierId { get; set; }

        public string ExtData { get; set; }

        public List<InboundOrderDetailDto> Details { get; set; }
    }
}
