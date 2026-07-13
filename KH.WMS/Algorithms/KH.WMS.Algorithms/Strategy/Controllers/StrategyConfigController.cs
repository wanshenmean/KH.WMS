using KH.WMS.Algorithms.Strategy.Configuration;
using KH.WMS.Algorithms.Strategy.Services;
using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Algorithms.Strategy.Controllers
{
    /// <summary>
    /// 策略配置控制器
    /// </summary>
    [ApiController]
    [Route("api/strategy-config")]
    public class StrategyConfigController(IStrategyConfigService service) : CrudController<CfgStrategyConfig>(service)
    {
        private readonly IStrategyConfigService _service = service;

        /// <summary>
        /// 根据编码获取策略配置
        /// </summary>
        [HttpGet("code/{strategyCode}")]
        public async Task<IActionResult> GetByCode(string strategyCode)
        {
            var result = await _service.GetByCodeAsync(strategyCode);
            if (result == null)
                return NotFound(new { success = false, message = "策略配置不存在" });
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// 获取指定类型的已启用策略配置列表
        /// </summary>
        [HttpGet("type/{strategyType}")]
        public async Task<IActionResult> GetByType(string strategyType, [FromQuery] long? warehouseId = null)
        {
            var result = await _service.GetByTypeAsync(strategyType, warehouseId);
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// 验证策略参数JSON格式
        /// </summary>
        [HttpPost("validate-params")]
        public IActionResult ValidateParams([FromBody] string paramsJson)
        {
            var isValid = _service.ValidateParams(paramsJson, out var errorMessage);
            return Ok(new { success = isValid, valid = isValid, message = isValid ? "验证通过" : errorMessage });
        }
    }
}
