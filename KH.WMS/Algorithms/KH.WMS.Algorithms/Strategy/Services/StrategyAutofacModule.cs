using Autofac;
using KH.WMS.Algorithms.Strategy.Interfaces;

namespace KH.WMS.Algorithms.Strategy.Services;

/// <summary>
/// 策略模块：扫描所有 IPolicyStrategy 实现并注册到 PolicyRegistry
/// </summary>
public class StrategyAutofacModule : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // 扫描当前程序集中所有非抽象的 IPolicyStrategy 实现类
        var strategyTypes = typeof(StrategyAutofacModule).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IPolicyStrategy).IsAssignableFrom(t))
            .ToList();

        // 将每个策略实现注册为 Singleton（策略无状态，可共享实例）
        foreach (var type in strategyTypes)
        {
            builder.RegisterType(type)
                .As<IPolicyStrategy>()
                .SingleInstance();
        }

        // 容器构建完成后，将所有策略注入 PolicyRegistry
        builder.RegisterBuildCallback(c =>
        {
            var registry = c.Resolve<IPolicyRegistry>();
            var strategies = c.Resolve<IEnumerable<IPolicyStrategy>>();

            foreach (var strategy in strategies)
            {
                registry.RegisterStrategy(strategy);
            }

            Console.WriteLine($"[策略注册] 已注册 {strategies.Count()} 个策略实现到 PolicyRegistry");
        });
    }
}
