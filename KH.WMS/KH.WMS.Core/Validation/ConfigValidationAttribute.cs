namespace KH.WMS.Core.Validation;

/// <summary>
/// 标注在业务方法上，声明该方法需要执行哪些校验
/// 拦截器按声明顺序依次执行，任一校验失败则短路返回
/// 仅对返回 Task&lt;ServiceResult&gt; 或 Task&lt;ServiceResult&lt;T&gt;&gt; 的方法生效
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ConfigValidationAttribute : Attribute
{
    public string ValidatorCode { get; }
    public ConfigValidationAttribute(string validatorCode) => ValidatorCode = validatorCode;
}
