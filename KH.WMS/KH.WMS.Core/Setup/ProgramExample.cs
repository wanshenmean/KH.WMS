//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;

//namespace Infrastructure.Setup;

///// <summary>
///// 应用启动示例
///// </summary>
//public class StartupExample
//{
//    /// <summary>
//    /// 配置服务示例（在 Program.cs 中使用）
//    /// </summary>
//    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
//    {
//        // 方式1：使用统一入口
//        services.AddInfrastructure(configuration);

//        // 方式2：单独配置各个模块
//        // services.AddDatabaseSetup(configuration);
//        // services.AddCacheSetup(configuration);
//        // services.AddAuthenticationSetup(configuration);
//        // ...

//        // 添加控制器
//        services.AddControllers();

//        // 添加 API 探索器
//        services.AddEndpointsApiExplorer();
//    }

//    /// <summary>
//    /// 配置中间件示例（在 Program.cs 中使用）
//    /// </summary>
//    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//    {
//        // 使用统一中间件配置
//        app.UseCustomMiddleware(env);
//    }
//}

///// <summary>
///// Program.cs 示例
///// </summary>
//public class ProgramExample
//{
//    public static void Main(string[] args)
//    {
//        var builder = WebApplication.CreateBuilder(args);

//        // 配置主机
//        builder.Host.AddCustomConfiguration();

//        // 配置服务
//        builder.Services.AddInfrastructure(builder.Configuration);
//        builder.Services.AddControllers();
//        builder.Services.AddEndpointsApiExplorer();

//        var app = builder.Build();

//        // 配置中间件
//        app.UseCustomMiddleware(app.Environment);

//        app.Run();
//    }

//    /// <summary>
//    /// 最小化 API 示例
//    /// </summary>
//    public static void MinimalApiExample(string[] args)
//    {
//        var builder = WebApplication.CreateBuilder(args);

//        // 添加基础设施
//        builder.Services.AddInfrastructure(builder.Configuration);

//        var app = builder.Build();
//        app.UseCustomMiddleware(app.Environment);

//        // 定义端点
//        app.MapGet("/", () => "Hello World!");
//        app.MapControllers();

//        app.Run();
//    }
//}
