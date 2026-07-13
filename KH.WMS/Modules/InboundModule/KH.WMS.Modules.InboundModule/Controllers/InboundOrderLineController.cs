using KH.WMS.Core.Controllers;
using KH.WMS.Entities.Inbound;
using KH.WMS.Modules.InboundModule;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.InboundModule.Controllers
{
    [Route("api/inbound-order-line")]
    public class InboundOrderLineController(IInboundOrderLineService inboundOrderLineService) : CrudController<InboundOrderLine>(inboundOrderLineService)
    {
    }
}
