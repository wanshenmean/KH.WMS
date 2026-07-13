using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace KH.WMS.Modules.BaseDataModule.DTOs
{
    public class MaterialCategoryTreeDTO
    {
        public long Id { get; set; }

        /// <summary>
        /// 分类编码
        /// </summary>
        public string CategoryCode { get; set; } = string.Empty;

        /// <summary>
        /// 分类名称
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>
        /// 父级ID
        /// </summary>
        public long ParentId { get; set; } = 0;

        /// <summary>
        /// 层级
        /// </summary>
        public int Level { get; set; } = 1;

        /// <summary>
        /// 层级路径
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortNo { get; set; } = 0;

        /// <summary>
        /// 状态（1启用 0禁用）
        /// </summary>
        public byte Status { get; set; } = 1;

        public List<MaterialCategoryTreeDTO> Children { get; set; } = new List<MaterialCategoryTreeDTO>();
    }
}
