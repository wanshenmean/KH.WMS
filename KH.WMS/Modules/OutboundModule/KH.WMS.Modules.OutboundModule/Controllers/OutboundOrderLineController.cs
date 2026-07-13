using KH.WMS.Core.Controllers;
using KH.WMS.Entities.Outbound;
using KH.WMS.Modules.OutboundModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.OutboundModule.Controllers
{
    [Route("api/outbound-order-line")]
    public class OutboundOrderLineController(IOutboundOrderLineService outboundOrderLineService) : CrudController<OutboundOrderLine>(outboundOrderLineService)
    {
    }
}
