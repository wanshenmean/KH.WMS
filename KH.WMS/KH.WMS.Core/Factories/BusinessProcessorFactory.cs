using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.DependencyInjection;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;

namespace KH.WMS.Core.Factories
{
    /// <summary>
    /// 业务处理器工厂（无接口，Singleton）
    /// </summary>
    [SelfRegisteredService(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton, WithoutInterceptor = true)]
    public class BusinessProcessorFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, IBusinessProcessor> _processors = new();

        public BusinessProcessorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            AutoRegisterProcessors();
        }

        /// <summary>
        /// 注册处理器
        /// </summary>
        public void RegisterProcessor(IBusinessProcessor processor)
        {
            _processors.TryAdd(processor.ProcessorType, processor);
        }

        /// <summary>
        /// 批量注册处理器
        /// </summary>
        public void RegisterProcessors(IEnumerable<IBusinessProcessor> processors)
        {
            foreach (var processor in processors)
            {
                RegisterProcessor(processor);
            }
        }

        /// <summary>
        /// 获取处理器
        /// </summary>
        public IBusinessProcessor? GetProcessor(string processorType)
        {
            if (string.IsNullOrEmpty(processorType))
                return null;

            return _processors.TryGetValue(processorType, out var processor) ? processor : null;
        }

        /// <summary>
        /// 获取所有已注册的处理器
        /// </summary>
        public IEnumerable<IBusinessProcessor> GetAllProcessors()
        {
            return _processors.Values;
        }

        /// <summary>
        /// 自动扫描并注册所有处理器
        /// 使用反射扫描所有程序集，查找继承自 BusinessProcessorBase 的类并自动注册
        /// </summary>
        public void AutoRegisterProcessors()
        {
            // 扫描所有加载的程序集，找到包含处理器的程序集
            var assemblies = AssemblyService.GetReferencedAssemblies();

            var registeredCount = 0;

            foreach (var assembly in assemblies)
            {
                try
                {
                    // 查找所有实现了 IBusinessProcessor 接口的类型
                    var processorTypes = assembly.GetTypes()
                        .Where(t => typeof(IBusinessProcessor).IsAssignableFrom(t)    // 实现了 IBusinessProcessor
                            && !t.IsInterface                                          // 不是接口
                            && !t.IsAbstract);                                         // 不是抽象类

                    foreach (var processorType in processorTypes)
                    {
                        try
                        {
                            // 使用 Activator.CreateInstance 创建实例
                            var processor = (IBusinessProcessor?)Activator.CreateInstance(processorType);
                            if (processor != null)
                            {
                                RegisterProcessor(processor);
                                registeredCount++;
                                Console.WriteLine($"成功注册处理器: {processor.ProcessorType} ({processorType.Name})");
                            }
                        }
                        catch (Exception ex)
                        {
                            // 记录单个处理器注册失败，但继续注册其他处理器
                            Console.WriteLine($"注册处理器 {processorType.Name} 失败: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 记录程序集扫描失败，但继续扫描其他程序集
                    Console.WriteLine($"扫描程序集 {assembly.GetName().Name} 失败: {ex.Message}");
                }
            }

            Console.WriteLine($"处理器自动注册完成，共注册 {registeredCount} 个处理器");
        }
    }
}
