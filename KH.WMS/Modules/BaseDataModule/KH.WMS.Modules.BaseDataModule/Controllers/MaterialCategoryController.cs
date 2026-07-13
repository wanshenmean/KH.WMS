using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.BaseData;
using KH.WMS.Modules.BaseDataModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.BaseDataModule.Controllers
{
    [Route("api/material-category")]
    public class MaterialCategoryController(IMaterialCategoryService materialCategoryService) : CrudController<MdMaterialCategory>(materialCategoryService)
    {
        [HttpGet("tree")]
        public async Task<ApiResponse> GetTreeAsync()
        {
            return await materialCategoryService.GetTreeAsync();
        }
    }
}
