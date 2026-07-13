using KH.WMS.Algorithms.Strategy.Configuration;
using KH.WMS.Algorithms.Strategy.Services;
using KH.WMS.Core.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Algorithms.Strategy.Controllers
{
    /// <summary>
    /// 策略链配置控制器
    /// </summary>
    [ApiController]
    [Route("api/strategy-chain")]
    public class StrategyChainController(IStrategyChainService service) : CrudController<CfgStrategyChainConfig>(service)
    {
        private readonly IStrategyChainService _service = service;

        /// <summary>
        /// 获取策略链完整详情（含步骤列表）
        /// </summary>
        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetDetail(long id)
        {
            var result = await _service.GetChainDetailAsync(id);
            if (result == null)
                return NotFound(new { success = false, message = "策略链配置不存在" });
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// 根据编码获取策略链配置
        /// </summary>
        [HttpGet("code/{chainCode}")]
        public async Task<IActionResult> GetByCode(string chainCode)
        {
            var result = await _service.GetByCodeAsync(chainCode);
            if (result == null)
                return NotFound(new { success = false, message = "策略链配置不存在" });
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// 获取指定类型的已启用策略链列表
        /// </summary>
        [HttpGet("type/{chainType}")]
        public async Task<IActionResult> GetByType(string chainType, [FromQuery] long? warehouseId = null)
        {
            var result = await _service.GetByTypeAsync(chainType, warehouseId);
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// 获取策略链的步骤列表
        /// </summary>
        [HttpGet("{chainId}/steps")]
        public async Task<IActionResult> GetSteps(long chainId)
        {
            var result = await _service.GetStepsByChainIdAsync(chainId);
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// 创建策略链（含步骤）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StrategyChainCreateRequest request)
        {
            try
            {
                var id = await _service.CreateAsync(request);
                return Ok(new { success = true, data = new { id }, message = "创建成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 更新策略链（含步骤，全量替换）
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] StrategyChainUpdateRequest request)
        {
            try
            {
                if (id != request.Chain.Id)
                    return BadRequest(new { success = false, message = "ID不匹配" });

                var result = await _service.UpdateAsync(request);
                return Ok(new { success = true, message = result ? "更新成功" : "更新失败" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        ///// <summary>
        ///// 删除策略链（含步骤）
        ///// </summary>
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(long id)
        //{
        //    try
        //    {
        //        var result = await _service.DeleteAsync(id);
        //        return Ok(new { success = true, message = result ? "删除成功" : "删除失败" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { success = false, message = ex.Message });
        //    }
        //}
    }
}
