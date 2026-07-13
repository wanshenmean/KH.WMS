using KH.WMS.Core.Controllers;
using KH.WMS.Entities.BaseData;
using KH.WMS.Modules.BaseDataModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.BaseDataModule.Controllers
{
    [Route("api/turnover-class")]
    public class CfgTurnoverClassController(ICfgTurnoverClassService cfgTurnoverClassService) : CrudController<CfgTurnoverClass>(cfgTurnoverClassService)
    {
    }
}
