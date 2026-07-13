using KH.WMS.Core.Controllers;
using KH.WMS.Entities.Task;
using KH.WMS.Modules.TaskModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.TaskModule.Controllers
{
    [Route("api/task-confirm")]
    public class TaskConfirmController(ITaskConfirmService taskConfirmService) : CrudController<TaskConfirm>(taskConfirmService)
    {
    }
}
