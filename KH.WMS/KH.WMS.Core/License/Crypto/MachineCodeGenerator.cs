using System;
using System.Collections.Generic;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace KH.WMS.Core.License.Crypto
{
    /// <summary>
    /// 机器码生成器 - 基于硬件指纹
    /// </summary>
    public static class MachineCodeGenerator
    {
        /// <summary>
        /// 获取当前服务器的机器码
        /// 通过采集 CPU ID、主板序列号、硬盘序列号，
        /// 拼接后做 SHA256 哈希生成唯一机器码
        /// </summary>
        public static string GetMachineCode()
        {
            var components = new List<string>();

            // 1. CPU ProcessorId
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                foreach (var obj in searcher.Get())
                {
                    var processorId = obj["ProcessorId"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(processorId))
                    {
                        components.Add(processorId.Trim());
                        break;
                    }
                }
            }
            catch
            {
                // WMI 查询失败时忽略
            }

            // 2. 主板序列号
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                foreach (var obj in searcher.Get())
                {
                    var serialNumber = obj["SerialNumber"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(serialNumber))
                    {
                        components.Add(serialNumber.Trim());
                        break;
                    }
                }
            }
            catch
            {
                // WMI 查询失败时忽略
            }

            // 3. 第一块硬盘序列号
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_DiskDrive");
                foreach (var obj in searcher.Get())
                {
                    var serialNumber = obj["SerialNumber"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(serialNumber))
                    {
                        components.Add(serialNumber.Trim());
                        break;
                    }
                }
            }
            catch
            {
                // WMI 查询失败时忽略
            }

            // 拼接所有硬件信息并计算 SHA256 哈希
            var rawCode = string.Join("|", components);

            if (string.IsNullOrWhiteSpace(rawCode))
            {
                throw new InvalidOperationException("无法获取硬件信息，请检查系统权限");
            }

            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawCode));
            return Convert.ToHexString(hashBytes).ToUpperInvariant();
        }
    }
}
