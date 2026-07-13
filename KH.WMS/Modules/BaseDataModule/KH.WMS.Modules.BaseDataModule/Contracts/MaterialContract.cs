using KH.WMS.Contracts.BaseData;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Entities.BaseData;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace KH.WMS.Modules.BaseDataModule.Contracts
{
    [RegisteredService(Lifetime = ServiceLifetime.Scoped, ServiceType = typeof(IMaterialContract))]
    public class MaterialContract(ISqlSugarClient db) : IMaterialContract
    {
        /// <inheritdoc />
        public async Task<MaterialInfo?> GetByCodeAsync(string materialCode)
        {
            var material = await db.Queryable<MdMaterial>()
                .FirstAsync(m => m.MaterialCode == materialCode);

            if (material == null) return null;

            return new MaterialInfo
            {
                Id = material.Id,
                MaterialCode = material.MaterialCode,
                MaterialName = material.MaterialName,
                BaseUnitId = material.BaseUnitId,
            };
        }

        /// <inheritdoc />
        public async Task<List<MaterialInfo>> GetByCodesAsync(List<string> materialCodes)
        {
            if (materialCodes == null || materialCodes.Count == 0)
                return [];

            var materials = await db.Queryable<MdMaterial>()
                .Where(m => materialCodes.Contains(m.MaterialCode))
                .ToListAsync();

            return materials.Select(m => new MaterialInfo
            {
                Id = m.Id,
                MaterialCode = m.MaterialCode,
                MaterialName = m.MaterialName,
                BaseUnitId = m.BaseUnitId,
            }).ToList();
        }
    }
}
