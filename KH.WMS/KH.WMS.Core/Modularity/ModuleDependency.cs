using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Core.Modularity
{

    /// <summary>
    /// 模块依赖
    /// </summary>
    public class ModuleDependency
    {
        public string ModuleName { get; set; } = string.Empty;
        public Version MinVersion { get; set; } = new(1, 0, 0, 0);
        public Version MaxVersion { get; set; } = new(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
    }

}
