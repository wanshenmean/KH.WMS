using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.System;
using KH.WMS.Modules.SystemModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.SystemModule.Controllers
{
    /// <summary>
    /// 附件管理控制器（CRUD）
    /// </summary>
    [ApiController]
    [Route("api/attachment")]
    public class AttachmentController(ISysAttachmentService attachmentService) : CrudController<SysAttachment>(attachmentService)
    {
    }
}
