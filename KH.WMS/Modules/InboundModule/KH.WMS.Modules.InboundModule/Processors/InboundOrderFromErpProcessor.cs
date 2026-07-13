using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.Attributes;
using KH.WMS.Core.Factories;
using KH.WMS.Modules.InboundModule.DTOs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace KH.WMS.Modules.InboundModule
{
    public class InboundOrderFromErpProcessor : BusinessProcessorBase
    {
        public override string ProcessorType => "ReceiveInboundOrer";


        [LogInterceptor(LogParameters = true, LogReturnValue = true, LogLevel = Microsoft.Extensions.Logging.LogLevel.Information)]
        public override async Task<(bool Success, object? Data, string? ErrorMessage)> ProcessAsync(JToken jsonData, IServiceProvider serviceProvider)
        {
            try
            {
                var logger = serviceProvider.GetRequiredService<ILogger<InboundOrderFromErpProcessor>>();
                var receiptService = serviceProvider.GetRequiredService<IInboundOrderService>();

                (bool Success, string? ErrorMessage, InboundOrderDto? Data) mappingResult = (false, null, null);

                try
                {
                    var json = JsonConvert.SerializeObject(jsonData, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });

                    // 3. 直接反序列化为目标类型（自动处理数组）
                    var data = JsonConvert.DeserializeObject<InboundOrderDto>(json, new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });

                    mappingResult = (true, null, data);
                }
                catch(Exception ex)
                {
                    mappingResult = (false, $"反序列化失败: {ex.Message}", null);
                }
               
                if (!mappingResult.Success || mappingResult.Data == null)
                {
                    return (false, null, $"数据映射失败: {mappingResult.ErrorMessage}");
                }

                var request = mappingResult.Data;

                // 步骤2: 直接使用强类型对象调用业务服务（无硬编码）
                var result = await receiptService.ReceiveInboundOrder(request);

                if (result.Success)
                {
                    logger.LogInformation("ERP入库单处理成功");
                    return (true, new { result.Message }, null);
                }
                else
                {
                    logger.LogWarning("ERP入库单处理失败: {Message}", result.Message);
                    return (false, null, result.Message);
                }
            }
            catch (Exception ex)
            {
                return (false, null, $"处理失败: {ex.Message}");
            }
        }
    }
}
