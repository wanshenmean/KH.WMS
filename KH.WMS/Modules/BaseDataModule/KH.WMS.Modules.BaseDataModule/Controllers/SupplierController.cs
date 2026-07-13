using KH.WMS.Config.Abstractions;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.BaseData;
using KH.WMS.Modules.BaseDataModule.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Modules.BaseDataModule.Controllers
{
    [Route("api/supplier")]
    public class SupplierController(ISupplierService supplierService) : ExtDataCrudController<MdSupplier>(supplierService)
    {
        [HttpGet("form-config")]
        public async Task<IActionResult> GetFormConfig()
        {
            var extService = HttpContext.RequestServices.GetRequiredService<ICfgExtFieldContract>();
            var fields = await extService.GetFieldsAsync("MD_SUPPLIER", "HEADER");
            var columns = extService.BuildFormColumns(fields);
            return Ok(new { success = true, data = new { columns } });
        }
    }
}
