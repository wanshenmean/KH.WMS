using KH.WMS.Core.Controllers;
using KH.WMS.Entities.BaseData;
using KH.WMS.Modules.BaseDataModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.BaseDataModule.Controllers
{
    [Route("api/container")]
    public class ContainerController(IContainerService containerService) : CrudController<MdContainer>(containerService)
    {
    }
}
