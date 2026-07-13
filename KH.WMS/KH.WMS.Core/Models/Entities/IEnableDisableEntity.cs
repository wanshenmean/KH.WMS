namespace KH.WMS.Core.Models.Entities;

/// <summary>
/// 支持启用/禁用管理的实体标记接口
/// 实体实现此接口后，CrudController 自动暴露 SetStatus 端点
/// <para>
/// CrudService 按以下优先级查找状态属性：
/// 1. [StatusFieldName] 特性指定的字段名
/// 2. <see cref="StatusFieldNames.Status"/>（默认）
/// 3. <see cref="StatusFieldNames.IsActive"/>（备选）
/// </para>
/// </summary>
public interface IEnableDisableEntity { }
