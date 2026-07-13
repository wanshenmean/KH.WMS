using KH.WMS.Core.Api.Responses;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.Inventory;
using KH.WMS.Modules.InventoryModule.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace KH.WMS.Server.BackgroundServices
{
    /// <summary>
    /// 每日库存快照后台服务
    /// 每小时检查一次，如果当天尚未生成 DAILY 类型快照则自动创建
    /// </summary>
    public class DailySnapshotBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<DailySnapshotBackgroundService> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("每日库存快照后台服务已启动");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndCreateDailySnapshotAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "每日库存快照检查失败");
                }

                // 每小时检查一次
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task CheckAndCreateDailySnapshotAsync(CancellationToken cancellationToken)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();
            var snapshotService = scope.ServiceProvider.GetRequiredService<IInvSnapshotHeaderService>();

            var today = DateOnly.FromDateTime(DateTime.Now);

            // 检查今天是否已有 DAILY 类型的快照
            var exists = await db.Queryable<InvSnapshotHeader>()
                .Where(h => h.SnapshotType == "DAILY" && h.SnapshotDate == today)
                .AnyAsync();

            if (exists)
            {
                logger.LogDebug("今日库存快照已存在，跳过");
                return;
            }

            var snapshotName = $"每日库存快照 {today:yyyy-MM-dd}";
            var result = await snapshotService.CreateSnapshotAsync(snapshotName, BizConstants.SnapshotTypes.DAILY, "系统自动生成");

            if (result.Code == ResponseCode.SUCCESS)
            {
                logger.LogInformation("每日库存快照创建成功: {SnapshotName}", snapshotName);
            }
            else
            {
                logger.LogWarning("每日库存快照创建失败: {Message}", result.Message);
            }
        }
    }
}
