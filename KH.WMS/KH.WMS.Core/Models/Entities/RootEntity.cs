using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;

namespace KH.WMS.Core.Models.Entities
{
    public class RootEntity
    {

        /// <summary>
        /// 创建人
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "创建人")]
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 创建人名称
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "创建人名称")]
        public string? CreatedByName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "创建时间")]
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 最后修改人
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "最后修改人")]
        public string? LastModifiedBy { get; set; }

        /// <summary>
        /// 最后修改人名称
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "最后修改人名称")]
        public string? LastModifiedByName { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "最后修改时间")]
        public DateTime? LastModifiedTime { get; set; }
    }
}
