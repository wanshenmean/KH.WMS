using KH.WMS.Core.Controllers;
using KH.WMS.Entities.BaseData;
using KH.WMS.Modules.BaseDataModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.BaseDataModule.Controllers
{
    [Route("api/container-type")]
    public class ContainerTypeController(IContainerTypeService containerTypeService) : CrudController<MdContainerType>(containerTypeService)
    {
    }
}
