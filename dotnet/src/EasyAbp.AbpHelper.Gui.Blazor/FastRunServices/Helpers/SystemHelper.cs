using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace FastRunMicroService.Helpers;

public class SystemHelper
{
    public static int GetPortFromUrl(string url)
    {
        Uri uriResult;
        bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult);
        if (result && uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)
        {
            return uriResult.Port;
        }

        return -1; // 如果URL无效或获取端口失败，则返回-1
    }

    public static int GetProcessIdUsingPort(int port)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "netstat.exe",
            Arguments = "-ano",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };

        using (var process = Process.Start(startInfo))
        {
            using (var reader = process.StandardOutput)
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Trim().StartsWith("TCP") || line.Trim().StartsWith("UDP"))
                    {
                        var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        var localAddress = parts[1];
                        var portIndex = localAddress.LastIndexOf(':');
                        if (portIndex != -1)
                        {
                            var portNumber = localAddress.Substring(portIndex + 1);
                            if (int.TryParse(portNumber, out int localPort) && localPort == port)
                            {
                                return int.Parse(parts[parts.Length - 1]);
                            }
                        }
                    }
                }
            }
        }

        return -1; // 如果没有找到占用端口的进程，则返回-1
    }

    public static void KillProcessById(int processId)
    {
        try
        {
            Process process = Process.GetProcessById(processId);
            process.Kill();
            Console.WriteLine($"进程ID {processId} 已被杀掉。");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"无法杀掉进程ID {processId}: {ex.Message}");
        }
    }
}