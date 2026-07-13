using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.Attributes;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.Warehouse;
using KH.WMS.Modules.WarehouseModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.WarehouseModule.Controllers
{
    [Route("api/conveyor-line"), Cache(Duration = 60 * 30)]
    public class ConveyorLineController(IConveyorLineService conveyorLineService) : CrudController<MdConveyorLine>(conveyorLineService)
    {
    }
}
