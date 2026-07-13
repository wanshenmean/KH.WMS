using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyModel;

namespace KH.WMS.Core.DependencyInjection
{
    public class AssemblyService
    {
        /// <summary>
        /// 获取项目内的程序集
        /// </summary>
        /// <returns></returns>
        public static List<Assembly> GetReferencedAssemblies()
        {
            List<Assembly> assemblies = new();
            string basePath = AppContext.BaseDirectory;
            try
            {
                DependencyContext? dependencyContext = DependencyContext.Default;
                if (dependencyContext != null)
                {
                    List<RuntimeLibrary> compilationLibraries = [.. dependencyContext.RuntimeLibraries.Where(x => (!x.Serviceable && x.Type == "project") || x.Name.StartsWith("KH.WMS."))];
                    foreach (var library in compilationLibraries)
                    {
                        try
                        {
                            string path = Path.Combine(basePath, $"{library.Name}.dll");
                            if (!File.Exists(path))
                            {
                                var msg = $"{library.Name}.dll丢失，因为项目解耦了，所以需要先F6编译，再F5运行，请检查 bin 文件夹，并拷贝。";
                                //log.Error(msg);
                                throw new Exception(msg);
                            }
                            assemblies.Add(Assembly.LoadFrom(path));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(library.Name + ex.Message);
                        }
                    }
                }
                else
                {
                    DirectoryInfo directoryInfo = new(basePath);
                    directoryInfo.GetFiles("*.dll").ToList().ForEach(file =>
                    {
                        try
                        {
                            if (file.Name.StartsWith("KH.WMS."))
                                assemblies.Add(Assembly.LoadFrom(file.FullName));
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(file.Name + ex.Message);
                        }
                    });
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: 获取程序集失败: {ex.Message}");
            }

            return assemblies;
        }
    }
}
