using KH.WMS.Core.Controllers;
using KH.WMS.Config.Abstractions;
using KH.WMS.Config.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Config.Controllers
{
    [Route("api/document-field")]
    public class CfgDocumentFieldController(ICfgDocumentFieldService cfgDocumentFieldService) : CrudController<CfgDocumentField>(cfgDocumentFieldService)
    {
    }
}
