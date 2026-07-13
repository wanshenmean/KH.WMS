using KH.WMS.Core.Controllers;
using KH.WMS.Config.Abstractions;
using KH.WMS.Config.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Config.Controllers
{
    [Route("api/document-type")]
    public class CfgDocumentTypeController(ICfgDocumentTypeService cfgDocumentTypeService) : CrudController<CfgDocumentType>(cfgDocumentTypeService)
    {
    }
}
