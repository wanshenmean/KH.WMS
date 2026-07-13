using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Warehouse;

namespace KH.WMS.Modules.WarehouseModule.Interfaces
{
    public interface ILocationService : ICrudService<MdLocation>
    {
        Task<ApiResponse> GetStatData();
        Task<ApiResponse> GetAvailableByZoneAsync(long warehouseId, string zoneCode);
    }
}
