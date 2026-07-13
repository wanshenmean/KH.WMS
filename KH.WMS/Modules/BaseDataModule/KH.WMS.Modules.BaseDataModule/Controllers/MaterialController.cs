using KH.WMS.Config.Abstractions;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.BaseData;
using KH.WMS.Modules.BaseDataModule.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Modules.BaseDataModule.Controllers
{
    [Route("api/material")]
    public class MaterialController(IMaterialService materialService) : ExtDataCrudController<MdMaterial>(materialService)
    {
        [HttpGet("form-config")]
        public async Task<IActionResult> GetFormConfig()
        {
            var extService = HttpContext.RequestServices.GetRequiredService<ICfgExtFieldContract>();
            var fields = await extService.GetFieldsAsync("MD_MATERIAL", "HEADER");
            var columns = extService.BuildFormColumns(fields);
            return Ok(new { success = true, data = new { columns } });
        }
    }
}
