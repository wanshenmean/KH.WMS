using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.Controllers;
using KH.WMS.Config.Abstractions;
using KH.WMS.Config.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Config.Controllers
{
    [Route("api/location-status")]
    public class CfgLocationStatusController(ICfgLocationStatusService cfgLocationStatusService) : CrudController<CfgLocationStatus>(cfgLocationStatusService)
    {
        /// <summary>
        /// 校验状态流转是否合法
        /// </summary>
        [HttpGet("can-transition")]
        public async Task<bool> CanTransition([FromQuery] string fromStatus, [FromQuery] string toStatus)
        {
            return await cfgLocationStatusService.CanTransitionAsync(fromStatus, toStatus);
        }

        /// <summary>
        /// 获取指定状态允许转入的所有目标状态
        /// </summary>
        [HttpGet("available-transitions")]
        public async Task<List<string>> GetAvailableTransitions([FromQuery] string fromStatus)
        {
            return await cfgLocationStatusService.GetAvailableTransitionsAsync(fromStatus);
        }
    }
}
