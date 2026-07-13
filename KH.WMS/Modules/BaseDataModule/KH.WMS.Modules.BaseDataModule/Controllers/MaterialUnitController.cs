using KH.WMS.Core.Controllers;
using KH.WMS.Entities.BaseData;
using KH.WMS.Modules.BaseDataModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.BaseDataModule.Controllers
{
    [Route("api/material-unit")]
    public class MaterialUnitController(IMaterialUnitService materialUnitService) : CrudController<MdMaterialUnit>(materialUnitService)
    {
    }
}
