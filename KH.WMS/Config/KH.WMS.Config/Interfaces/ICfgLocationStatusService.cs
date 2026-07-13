using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.Services;
using KH.WMS.Config.Abstractions;

namespace KH.WMS.Config.Interfaces
{
    public interface ICfgLocationStatusService : ICrudService<CfgLocationStatus>
    {
        /// <summary>
        /// 校验状态流转是否合法
        /// </summary>
        /// <param name="fromStatus">来源状态编码</param>
        /// <param name="toStatus">目标状态编码</param>
        /// <returns>是否允许流转</returns>
        Task<bool> CanTransitionAsync(string fromStatus, string toStatus);

        /// <summary>
        /// 获取指定状态允许转入的所有目标状态
        /// </summary>
        /// <param name="fromStatus">来源状态编码</param>
        /// <returns>可转入的目标状态编码列表</returns>
        Task<List<string>> GetAvailableTransitionsAsync(string fromStatus);
    }
}
