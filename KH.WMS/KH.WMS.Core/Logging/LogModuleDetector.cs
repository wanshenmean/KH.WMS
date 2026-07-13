using System.Diagnostics;
using System.Runtime.CompilerServices;
using KH.WMS.Core.Logging.LogEnums;

namespace KH.WMS.Core.Logging;

/// <summary>
/// 日志模块自动识别器
/// </summary>
public static class LogModuleDetector
{
    /// <summary>
    /// 模块命名空间映射配置
    /// </summary>
    private static readonly Dictionary<string, LogModule> NamespaceModuleMapping = new(StringComparer.OrdinalIgnoreCase)
    {
        // 基础设施层
        ["KH.WMS.Core.Database"] = LogModule.Database,
        ["KH.WMS.Core.Database.Repositories"] = LogModule.Repositories,
        ["KH.WMS.Core.Security"] = LogModule.Auth,
        ["KH.WMS.Core.Controllers"] = LogModule.Api,
        ["KH.WMS.Core.Logging"] = LogModule.Logging,
        ["KH.WMS.Core.Services"] = LogModule.Core,
        ["KH.WMS.Core.ImportExport"] = LogModule.File,
        ["KH.WMS.Core.AOP"] = LogModule.Core,

        // 算法模块 - 策略
        ["KH.WMS.Algorithms"] = LogModule.WMS_Strategy,

        // WMS 业务模块 - 入库管理
        ["KH.WMS.Modules.InboundModule"] = LogModule.WMS_Inbound,

        // WMS 业务模块 - 出库管理
        ["KH.WMS.Modules.OutboundModule"] = LogModule.WMS_Outbound,

        // WMS 业务模块 - 库存管理
        ["KH.WMS.Modules.InventoryModule"] = LogModule.WMS_Inventory,

        // WMS 业务模块 - 任务管理
        ["KH.WMS.Modules.TaskModule"] = LogModule.WMS_Transfer,

        // WMS 业务模块 - 仓库管理（仓库、库区、库位）
        ["KH.WMS.Modules.WarehouseModule"] = LogModule.WMS_Warehouse,

        // WMS 业务模块 - 基础数据
        ["KH.WMS.Modules.BaseDataModule"] = LogModule.WMS_Product,

        // WMS 业务模块 - 配置管理
        ["KH.WMS.Modules.ConfigModule"] = LogModule.WMS_Order,

        // WMS 业务模块 - 系统管理
        ["KH.WMS.Modules.SystemModule"] = LogModule.Core,
    };

    /// <summary>
    /// 从调用栈中自动识别模块
    /// </summary>
    /// <param name="skipFrames">跳过的帧数（默认跳过 LoggerService 内部调用）</param>
    /// <returns>识别出的模块，默认返回 Core</returns>
    public static LogModule DetectModule(int skipFrames = 2)
    {
        try
        {
            var stackTrace = new StackTrace(skipFrames, false);

            for (int i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                if (frame == null) continue;

                var namespaceName = frame.GetMethod()?.DeclaringType?.Namespace;
                if (string.IsNullOrEmpty(namespaceName)) continue;

                // 跳过日志系统本身的命名空间
                if (namespaceName.StartsWith("KH.WMS.Core.Logging", StringComparison.OrdinalIgnoreCase) ||
                    namespaceName.StartsWith("Microsoft.Extensions.Logging", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // 查找匹配的模块
                var module = FindModuleByNamespace(namespaceName);
                if (module != LogModule.Core)
                {
                    return module;
                }
            }

            return LogModule.Core;
        }
        catch
        {
            return LogModule.Core;
        }
    }

    /// <summary>
    /// 根据命名空间查找对应的模块
    /// </summary>
    private static LogModule FindModuleByNamespace(string namespaceName)
    {
        // 完全匹配
        if (NamespaceModuleMapping.TryGetValue(namespaceName, out var module))
        {
            return module;
        }

        // 前缀匹配
        foreach (var mapping in NamespaceModuleMapping)
        {
            if (namespaceName.StartsWith(mapping.Key, StringComparison.OrdinalIgnoreCase))
            {
                return mapping.Value;
            }
        }

        return LogModule.Core;
    }

    /// <summary>
    /// 从类型推断模块
    /// </summary>
    public static LogModule DetectModuleFromType(Type type)
    {
        if (type == null) return LogModule.Core;

        var namespaceName = type.Namespace;
        return string.IsNullOrEmpty(namespaceName) ? LogModule.Core : FindModuleByNamespace(namespaceName);
    }

    /// <summary>
    /// 添加自定义命名空间映射
    /// </summary>
    /// <param name="namespacePrefix">命名空间前缀</param>
    /// <param name="module">对应的模块</param>
    public static void AddMapping(string namespacePrefix, LogModule module)
    {
        NamespaceModuleMapping[namespacePrefix] = module;
    }

    /// <summary>
    /// 批量添加命名空间映射
    /// </summary>
    public static void AddMappings(Dictionary<string, LogModule> mappings)
    {
        foreach (var mapping in mappings)
        {
            NamespaceModuleMapping[mapping.Key] = mapping.Value;
        }
    }
}
