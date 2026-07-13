using SqlSugar;
using Newtonsoft.Json;
using System.Net;
using System.Net.Sockets;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using Microsoft.Extensions.DependencyInjection;
using KH.WMS.Core.Database.Repositories;

namespace KH.WMS.Engines.DataMap.DataMapInterface;

/// <summary>
/// 接口配置服务实现
/// </summary>
[RegisteredService(Lifetime = ServiceLifetime.Scoped)]
public class InterfaceConfigService : IInterfaceConfigService
{
    private readonly IRepository<IntInterfaceConfig, long> _repository;
    public InterfaceConfigService(IRepository<IntInterfaceConfig, long> repository)
    {
        _repository = repository;
    }

    public async Task<List<IntInterfaceConfig>> GetListAsync(string? systemCode = null)
    {
        return (await _repository.GetListAsync(x=> string.IsNullOrEmpty(systemCode)|| x.SystemCode == systemCode))
            .OrderByDescending(x => x.CreatedTime).ToList();
    }

    public async Task<IntInterfaceConfig?> GetByIdAsync(long id)
    {
        return await _repository.GetFirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<long> CreateAsync(IntInterfaceConfig config)
    {
        config.CreatedTime = DateTime.Now;
        config.Status = config.Status ?? DataMapConstants.InterfaceStatus.ACTIVE;

        return await _repository.AddAsync(config);
    }

    public async Task<bool> UpdateAsync(IntInterfaceConfig config)
    {
        config.LastModifiedTime = DateTime.Now;

        return await _repository.UpdateAsync(config);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        return await _repository.DeleteAsync(x => x.Id == id);
    }

    public async Task<(bool Success, string Message)> TestInterfaceAsync(long id)
    {
        var config = await GetByIdAsync(id);
        if (config == null)
            return (false, "接口配置不存在");

        if (string.IsNullOrEmpty(config.RequestPath))
            return (false, "请求路径为空");

        // SSRF 防护：禁止访问内网/本地/非 HTTP(S) 地址（config.RequestPath 为可配置项，存在被诱导访问内网的风险）
        if (!await IsSafeRemoteUrlAsync(config.RequestPath))
            return (false, "目标地址不安全：禁止访问内网/本地/非 HTTP(S) 地址");

        try
        {
            using var httpClient = new HttpClient();
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(config.RequestPath);

            if (!string.IsNullOrEmpty(config.HttpMethod))
            {
                request.Method = new HttpMethod(config.HttpMethod.ToUpper());
            }

            // 设置超时时间
            if (config.Timeout.HasValue && config.Timeout.Value > 0)
            {
                httpClient.Timeout = TimeSpan.FromMilliseconds(config.Timeout.Value);
            }

            var response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return (true, "接口测试成功");
            }
            else
            {
                return (false, $"接口测试失败: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return (false, $"接口测试异常: {ex.Message}");
        }
    }

    /// <summary>
    /// 校验目标 URL 是否安全（SSRF 防护）：仅允许 http/https，且解析出的地址不得落在内网/回环/链路本地段。
    /// </summary>
    private static async Task<bool> IsSafeRemoteUrlAsync(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;
        if (string.IsNullOrWhiteSpace(uri.Host))
            return false;

        var host = uri.Host;
        if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase) || host == "::1" || host == "0.0.0.0")
            return false;

        IPAddress[] addresses;
        try { addresses = await Dns.GetHostAddressesAsync(host); }
        catch { return false; }

        foreach (var addr in addresses)
        {
            if (IsPrivateOrLoopback(addr))
                return false;
        }
        return true;
    }

    /// <summary>
    /// 判断 IP 是否属于内网/回环/链路本地等不应被 SSRF 访问的网段。
    /// </summary>
    private static bool IsPrivateOrLoopback(IPAddress addr)
    {
        if (addr.AddressFamily == AddressFamily.InterNetwork)
        {
            if (IPAddress.IsLoopback(addr)) return true;
            var b = addr.GetAddressBytes();
            if (b[0] == 0) return true;                              // 0.0.0.0/8
            if (b[0] == 10) return true;                             // 10.0.0.0/8
            if (b[0] == 127) return true;                            // 127.0.0.0/8
            if (b[0] == 169 && b[1] == 254) return true;             // 169.254.0.0/16 链路本地
            if (b[0] == 172 && b[1] >= 16 && b[1] <= 31) return true;// 172.16.0.0/12
            if (b[0] == 192 && b[1] == 168) return true;             // 192.168.0.0/16
            return false;
        }

        // IPv6
        if (IPAddress.IsLoopback(addr)) return true;
        if (addr.IsIPv6LinkLocal || addr.IsIPv6SiteLocal) return true;
        return false;
    }
}
