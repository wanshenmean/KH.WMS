using KH.WMS.Config.Abstractions;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.Training;
using KH.WMS.Modules.TrainingModule.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Modules.TrainingModule.Controllers;

[Route("api/training-owner-profile")]
public class TrainingOwnerProfileController(ITrainingOwnerProfileService service)
    : ExtDataCrudController<TrnOwnerProfile>(service)
{
    [HttpGet("form-config")]
    public async Task<IActionResult> GetFormConfig()
    {
        var extService = HttpContext.RequestServices.GetRequiredService<ICfgExtFieldContract>();
        var fields = await extService.GetFieldsAsync("TRN_OWNER_PROFILE", "HEADER");
        var columns = extService.BuildFormColumns(fields);
        return Ok(new { success = true, data = new { columns, lineColumns = Array.Empty<object>() } });
    }
}
