using KH.WMS.Core.Controllers;
using KH.WMS.Config.Abstractions;
using KH.WMS.Config.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Config.Controllers
{
    [Route("api/cfg-document-type-rule")]
    public class CfgDocumentTypeRuleController(ICfgDocumentTypeRuleService service) : CrudController<CfgDocumentTypeRule>(service)
    {
    }
}
