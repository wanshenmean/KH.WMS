using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.Controllers;
using KH.WMS.Config.Abstractions;
using KH.WMS.Config.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Config.Controllers
{
    [Route("api/warehouse-type")]
    public class CfgWarehouseTypeController(ICfgWarehouseTypeService cfgWarehouseTypeService) : CrudController<CfgWarehouseType>(cfgWarehouseTypeService)
    {
    }
}
