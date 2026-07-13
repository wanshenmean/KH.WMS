using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Modules.InboundModule.DTOs
{
    public class InboundOrderDetailDto
    {
        public string MaterialCode { get; set; }

        public decimal OrderedQty { get; set; }

        public long? UnitId { get; set; }

        public string BatchNo { get; set; }

        public DateOnly? ManufactureDate { get; set; }

        public DateOnly? ExpiryDate { get; set; }

        public string ExtData { get; set; }
    }
}
