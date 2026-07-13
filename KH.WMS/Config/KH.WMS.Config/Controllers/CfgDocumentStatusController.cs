using KH.WMS.Core.Controllers;
using KH.WMS.Config.Abstractions;
using KH.WMS.Config.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Config.Controllers
{
    [Route("api/document-status")]
    public class CfgDocumentStatusController(ICfgDocumentStatusService cfgDocumentStatusService) : CrudController<CfgDocumentStatus>(cfgDocumentStatusService)
    {
    }
}
