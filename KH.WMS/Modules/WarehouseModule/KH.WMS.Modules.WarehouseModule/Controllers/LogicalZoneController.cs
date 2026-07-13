using KH.WMS.Core.Attributes;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.Warehouse;
using KH.WMS.Modules.WarehouseModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.WarehouseModule.Controllers
{
    [Route("api/logical-zone"), Cache(Duration = 60 * 30)]
    public class LogicalZoneController(ILogicalZoneService logicalZoneService) : CrudController<MdLogicalZone>(logicalZoneService)
    {
    }
}
