using KH.WMS.Algorithms.Strategy.Services;
using KH.WMS.Core.Api.Responses;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Algorithms.Strategy.Controllers
{
    /// <summary>
    /// 策略查询控制器
    /// 查询已注册的策略实现信息，供前端配置时选择
    /// </summary>
    [ApiController]
    [Route("api/strategy/query")]
    public class StrategyQueryController : ControllerBase
    {
        private readonly IStrategyQueryService _service;

        public StrategyQueryController(IStrategyQueryService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取所有已注册的策略列表
        /// 前端配置策略时可选择具体的策略实现
        /// </summary>
        [HttpGet("strategies")]
        public ApiResponse GetRegisteredStrategies([FromQuery] string? strategyType = null)
        {
            var result = _service.GetRegisteredStrategies(strategyType);
            return ApiResponse.Ok(result);
        }

        /// <summary>
        /// 获取所有已注册的策略链列表
        /// </summary>
        [HttpGet("chains")]
        public ApiResponse GetRegisteredChains()
        {
            var result = _service.GetRegisteredChains();
            return ApiResponse.Ok(result);
        }

        /// <summary>
        /// 获取所有策略类型及其已注册的策略数量
        /// </summary>
        [HttpGet("types")]
        public ApiResponse GetStrategyTypes()
        {
            var result = _service.GetStrategyTypes();
            return ApiResponse.Ok(result);
        }

        /// <summary>
        /// 获取前端配置页面所需的策略选项聚合数据
        /// 返回策略类型列表 + 按类型分组的规则编码映射，供策略配置和策略链配置页面的下拉选择使用
        /// </summary>
        [HttpGet("options")]
        public ApiResponse GetStrategyOptions()
        {
            var result = _service.GetStrategyOptions();
            return ApiResponse.Ok(result);
        }
    }
}
