using Microsoft.Extensions.Hosting.WindowsServices;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using KH.WMS.Core.Database;
using KH.WMS.Core.Setup;
using KH.WMS.Core.AOP;
using KH.WMS.Core.Api.Documentation.Swagger;
using KH.WMS.Core.DependencyInjection;
using KH.WMS.Core.Filters.Exception;
using KH.WMS.Core.Logging.Serilog;
using KH.WMS.Server.BackgroundServices;
using KH.WMS.Core.Middlewares;
using KH.WMS.Core.Monitoring.MiniProfiler;
using StackExchange.Profiling;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// 注册为 Windows 服务（非服务环境下自动退化为普通 WebHost，不影响开发调试）
builder.Host.UseWindowsService(options =>
{
    options.ServiceName = "KH.WMS";
});

builder.Services.AddMemoryCache();

builder.Host.AddSerilog();


builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureContainer<ContainerBuilder>(builder =>
{
    builder.RegisterModule(new ServiceExtensions());//带有接口层的服务注入
    builder.RegisterModule(new KH.WMS.Algorithms.Strategy.Services.StrategyAutofacModule());//策略注册到PolicyRegistry
});

// 添加 HttpContextAccessor（用于日志中获取用户信息）
builder.Services.AddHttpContextAccessor();

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

// 注册每日库存快照后台服务
builder.Services.AddHostedService<DailySnapshotBackgroundService>();

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GlobalExceptionFilter>(); // 全局异常过滤器
    options.Filters.Add<KH.WMS.Core.Filters.Result.TraceIdResultFilter>(); // TraceId 注入
    // 事务管理通过 [Transaction] 特性触发（IFilterFactory），无需全局注册
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    options.JsonSerializerOptions.Converters.Add(new KH.WMS.Core.Serialization.JsonConverters.DateTimeConverter());
    options.JsonSerializerOptions.Converters.Add(new KH.WMS.Core.Serialization.JsonConverters.NullableDateTimeConverter());
    options.JsonSerializerOptions.Converters.Add(new KH.WMS.Core.Serialization.JsonConverters.EnumConverter());
    options.JsonSerializerOptions.Converters.Add(new KH.WMS.Core.Serialization.JsonConverters.BoolToIntConverter());
    options.JsonSerializerOptions.Converters.Add(new KH.WMS.Core.Serialization.JsonConverters.NullableBoolToIntConverter());
})
.ConfigureApplicationPartManager(apm =>
{
    // 手动注册模块程序集，确保模块中的控制器能被 MVC 发现并出现在 Swagger 中
    var moduleAssemblies = AssemblyService.GetReferencedAssemblies()
        .Where(a => a.GetName().Name?.Contains(".Modules.") == true || a.GetName().Name == "KH.WMS.Config");
    foreach (var assembly in moduleAssemblies)
    {
        apm.ApplicationParts.Add(new AssemblyPart(assembly));
    }
});
builder.Services.AddRazorPages();  // MiniProfiler UI 可能需要
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// 同步执行数据库初始化（建库 + 建表），确保后续中间件和数据操作依赖的表已存在
using (var scope = app.Services.CreateScope())
{
    var dbInit = scope.ServiceProvider.GetRequiredService<IDatabaseInitService>();
    //dbInit.InitDatabase();

    // 预加载全局配置到缓存，避免首次请求时的数据库查询延迟
    var configService = scope.ServiceProvider.GetRequiredService<KH.WMS.Config.Abstractions.IConfigResolverContract>();
    try { await configService.WarmUpAsync(); } catch { }

    // 预加载系统参数到缓存（sys_parameter 表），带默认值兜底
    var paramService = scope.ServiceProvider.GetRequiredService<KH.WMS.Modules.SystemModule.Interfaces.ISysParameterService>();
    try { await paramService.WarmUpAsync(); } catch { }

    // A2: 出库已简化为单步 InventoryAllocation（移除 OutboundAllocation 选区分区层），
    // 原"OutboundAllocation+InventoryAllocation 两步链组成"校验不再适用，已移除。
}

// 启用请求体缓冲，允许 Body 流被多次读取（ExtDataCrudController 需要在模型绑定后重读原始 JSON）
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    KH.WMS.Core.Logging.ErrorLogScope.Begin();
    try
    {
        await next();
    }
    finally
    {
        KH.WMS.Core.Logging.ErrorLogScope.Clear();
    }
});

app.UseCustomMiddleware(app.Environment);

// 启用静态文件服务（附件上传目录）
var uploadPath = builder.Configuration["FileStorage:UploadPath"] ?? "Uploads";
var uploadsDir = Path.Combine(AppContext.BaseDirectory, uploadPath);
if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsDir),
    RequestPath = "/" + uploadPath,
});

// 托管前端 SPA：把 `npm run build` 产出的 dist 内容复制到运行目录的 wwwroot 后，
// 由后端统一提供服务（前端 config.js 的 API_BASE_URL 留空即可，前后端同源、无需 CORS）
app.UseDefaultFiles();
app.UseStaticFiles();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation(builder.Configuration);
}

//app.UseInterfaceMiddleware(app.Services);

// SPA 回退：未匹配控制器路由/静态文件的请求（如 /system/user）返回 index.html，
// 支持 vue-router history 模式刷新（独立 nginx/IIS 部署时由对应服务器的 rewrite 兜底，效果相同）
app.MapFallbackToFile("index.html");

app.Run();
