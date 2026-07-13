using KH.WMS.Core.Database.SqlSugar;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using SqlSugar;

namespace KH.WMS.Core.Services;

/// <summary>
/// 编码生成服务实现
/// 根据 CfgCodeRule 规则表 + CfgCodeSequence 序列表生成各类业务编码
/// 通过原生 SQL 访问配置库中的规则表和序列表
/// </summary>
[RegisteredService(ServiceType = typeof(ICodeGeneratorService))]
public class CodeGeneratorService(ISqlSugarClient db) : ICodeGeneratorService
{
    /// <summary>
    /// 配置库连接（cfg_code_rule / cfg_code_sequence 在 SQLite 配置库中）
    /// </summary>
    private ISqlSugarClient ConfigDb => ((SqlSugarClient)db).GetConnection(SqlSugarSetup.ConfigDb);

    /// <summary>
    /// 编码生成进程级串行锁。
    /// 取号是"读取当前值 → +1 → 写回"的非原子序列，并发请求会读到相同值导致撞号/丢号。
    /// 本系统以单实例（Windows 服务）部署，进程内串行即可保证唯一；
    /// 若未来多实例部署，需改为数据库层的原子自增（UPDATE...RETURNING）+ 序列表 (RuleId,SequenceKey) 唯一约束。
    /// </summary>
    private static readonly SemaphoreSlim _generateLock = new(1, 1);

    /// <summary>
    /// 根据规则类型生成编码
    /// </summary>
    public async Task<string> GenerateAsync(string ruleType)
    {
        await _generateLock.WaitAsync();
        try
        {
            return await GenerateInternalAsync(ruleType);
        }
        finally
        {
            _generateLock.Release();
        }
    }

    private async Task<string> GenerateInternalAsync(string ruleType)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        // 1. 查找编码规则
        var rule = await ConfigDb.Ado.SqlQueryAsync<CodeRuleRecord>(
            @"SELECT * FROM cfg_code_rule
              WHERE RuleType = @ruleType AND IsActive = 1 AND IsDefault = 1
              AND (EffectiveDate IS NULL OR EffectiveDate <= @today)
              AND (ExpiryDate IS NULL OR ExpiryDate >= @today)
              LIMIT 1",
            new { ruleType, today });

        if (rule == null || rule.Count == 0)
            throw new InvalidOperationException($"未找到规则类型为 '{ruleType}' 的有效编码规则");

        var ruleRecord = rule[0];

        // 2. 生成序列键
        var sequenceKey = BuildSequenceKey(ruleType, ruleRecord.SequenceType, today);

        // 3. 获取或创建序列记录
        var sequence = await GetOrCreateSequenceAsync(ruleRecord.Id, sequenceKey, ruleRecord.SequenceType, today);

        // 4. 自增序列值
        sequence.CurrentValue++;
        sequence.GeneratedCount++;
        sequence.LastGeneratedTime = DateTime.Now;

        await ConfigDb.Ado.ExecuteCommandAsync(
            @"UPDATE cfg_code_sequence
              SET CurrentValue = @CurrentValue, GeneratedCount = @GeneratedCount, LastGeneratedTime = @LastGeneratedTime
              WHERE Id = @Id",
            new
            {
                sequence.CurrentValue,
                sequence.GeneratedCount,
                sequence.LastGeneratedTime,
                sequence.Id
            });

        // 5. 拼装编码
        return BuildCode(ruleRecord, today, sequence.CurrentValue);
    }

    private async Task<CodeSequenceRecord> GetOrCreateSequenceAsync(
        long ruleId, string sequenceKey, string sequenceType, DateOnly today)
    {
        var existing = await ConfigDb.Ado.SqlQueryAsync<CodeSequenceRecord>(
            @"SELECT * FROM cfg_code_sequence WHERE RuleId = @ruleId AND SequenceKey = @sequenceKey LIMIT 1",
            new { ruleId, sequenceKey });

        if (existing != null && existing.Count > 0)
        {
            var record = existing[0];
            if (record.PeriodEnd.HasValue && today > record.PeriodEnd.Value)
            {
                var periodEnd = CalculatePeriodEnd(sequenceType, today);
                await ConfigDb.Ado.ExecuteCommandAsync(
                    @"UPDATE cfg_code_sequence
                      SET CurrentValue = 0, PeriodStart = @PeriodStart, PeriodEnd = @PeriodEnd, GeneratedCount = 0
                      WHERE Id = @Id",
                    new { PeriodStart = today, PeriodEnd = periodEnd, record.Id });
                record.CurrentValue = 0;
                record.PeriodStart = today;
                record.PeriodEnd = periodEnd;
                record.GeneratedCount = 0;
            }
            return record;
        }

        var periodEndNew = CalculatePeriodEnd(sequenceType, today);
        await ConfigDb.Ado.ExecuteCommandAsync(
            @"INSERT INTO cfg_code_sequence (RuleId, SequenceKey, CurrentValue, PeriodStart, PeriodEnd, GeneratedCount, CreatedTime)
              VALUES (@RuleId, @SequenceKey, 0, @PeriodStart, @PeriodEnd, 0, @CreatedTime)",
            new { RuleId = ruleId, SequenceKey = sequenceKey, PeriodStart = today, PeriodEnd = periodEndNew, CreatedTime = DateTime.Now });

        var newRecord = (await ConfigDb.Ado.SqlQueryAsync<CodeSequenceRecord>(
            @"SELECT * FROM cfg_code_sequence WHERE RuleId = @ruleId AND SequenceKey = @sequenceKey LIMIT 1",
            new { ruleId, sequenceKey }))[0];
        newRecord.PeriodEnd = periodEndNew;
        return newRecord;
    }

    private static string BuildSequenceKey(string ruleType, string sequenceType, DateOnly today)
    {
        return sequenceType switch
        {
            "DAILY" => $"{ruleType}_{today:yyyyMMdd}",
            "MONTHLY" => $"{ruleType}_{today:yyyyMM}",
            "YEARLY" => $"{ruleType}_{today:yyyy}",
            _ => $"{ruleType}_NONE"
        };
    }

    private static DateOnly CalculatePeriodEnd(string sequenceType, DateOnly periodStart)
    {
        return sequenceType switch
        {
            "DAILY" => periodStart,
            "MONTHLY" => new DateOnly(periodStart.Year, periodStart.Month, DateTime.DaysInMonth(periodStart.Year, periodStart.Month)),
            "YEARLY" => new DateOnly(periodStart.Year, 12, 31),
            _ => DateOnly.MaxValue
        };
    }

    private static string BuildCode(CodeRuleRecord rule, DateOnly today, long sequenceValue)
    {
        var parts = new List<string>();

        if (!string.IsNullOrEmpty(rule.Prefix))
            parts.Add(rule.Prefix);

        if (!string.IsNullOrEmpty(rule.DateFormat))
        {
            var dateStr = today.ToString(rule.DateFormat);
            if (!string.IsNullOrEmpty(rule.Separator) && parts.Count > 0)
                parts.Add(rule.Separator);
            parts.Add(dateStr);
        }

        if (parts.Count > 0 && !string.IsNullOrEmpty(rule.Separator))
            parts.Add(rule.Separator);

        parts.Add(sequenceValue.ToString().PadLeft(rule.SequenceLength, '0'));

        return string.Join("", parts);
    }

    private class CodeRuleRecord
    {
        public long Id { get; set; }
        public string RuleCode { get; set; } = string.Empty;
        public string RuleType { get; set; } = string.Empty;
        public string? Prefix { get; set; }
        public string? DateFormat { get; set; }
        public int SequenceLength { get; set; }
        public string SequenceType { get; set; } = "DAILY";
        public string? Separator { get; set; }
    }

    private class CodeSequenceRecord
    {
        public long Id { get; set; }
        public long RuleId { get; set; }
        public string SequenceKey { get; set; } = string.Empty;
        public long CurrentValue { get; set; }
        public DateOnly? PeriodStart { get; set; }
        public DateOnly? PeriodEnd { get; set; }
        public int GeneratedCount { get; set; }
        public DateTime? LastGeneratedTime { get; set; }
    }
}
