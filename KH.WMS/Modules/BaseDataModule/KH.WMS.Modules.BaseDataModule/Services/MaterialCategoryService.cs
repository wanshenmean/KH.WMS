using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Entities.BaseData;
using KH.WMS.Entities.System;
using KH.WMS.Modules.BaseDataModule.DTOs;
using KH.WMS.Modules.BaseDataModule.Interfaces;
using Newtonsoft.Json;

namespace KH.WMS.Modules.BaseDataModule.Services
{
    [RegisteredService(ServiceType = typeof(IMaterialCategoryService))]
    public class MaterialCategoryService(
        IRepository<MdMaterialCategory, long> repository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<MdMaterialCategory>(repository, unitOfWork, detailSaveService), IMaterialCategoryService
    {
        private readonly IRepository<MdMaterialCategory, long> _materialCategoryRepository = repository;

        public async Task<ApiResponse> GetTreeAsync()
        {
            var categories = await _materialCategoryRepository.GetAllAsync();
            var tree = BuildMaterialCategoryTree(categories, 0);
            return ApiResponse.Ok(tree);
        }

        private List<MaterialCategoryTreeDTO> BuildMaterialCategoryTree(List<MdMaterialCategory> all, long parentId)
        {
            return all
                .Where(p => p.ParentId == parentId)
                .OrderBy(p => p.SortNo)
                .Select(MapToTreeNode)
                .Select(node =>
                {
                    node.Children = BuildMaterialCategoryTree(all, node.Id);
                    return node;
                }).ToList();
        }

        private static MaterialCategoryTreeDTO MapToTreeNode(MdMaterialCategory p)
        {
            return new MaterialCategoryTreeDTO
            {
                Id = p.Id,
                CategoryCode = p.CategoryCode,
                CategoryName = p.CategoryName,
                ParentId = p.ParentId,
                Path = p.Path,
                SortNo = p.SortNo,
                Status = p.Status,
                Level = p.Level
            };
        }
    }
}
