using KH.WMS.Core.Controllers;
using KH.WMS.Entities.Inventory;
using KH.WMS.Modules.InventoryModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.InventoryModule.Controllers
{
    [Route("api/inventory-alert-record")]
    public class InvAlertRecordController(IInvAlertRecordService invAlertRecordService) : CrudController<InvAlertRecord>(invAlertRecordService)
    {
    }
}
