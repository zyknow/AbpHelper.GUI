using FastRunMicroService.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FastRunMicroService.Models;

public partial class BaseModel : INotifyPropertyChanged
{
    event Action ScrollToEndEvent;
    public event PropertyChangedEventHandler PropertyChanged;

    string _log;
    public string Log
    {
        get => _log;
        set
        {
            _log = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Log)));
        }
    }


    public string Name { get; set; }

    public string DisplayName => Name.Replace("MjLibrary.", "").Replace(".HttpApi.Host", "");

    public string Path { get; set; }

    public bool IsNormal { get; set; }

    bool _isRunning;

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            _isRunning = value;

            if (value)
            {
                HasError = false;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsRunning)));
        }
    }

    bool _isBudding;

    public bool IsBudding
    {
        get => _isBudding;
        set
        {
            _isBudding = value;

            if (value)
            {
                HasError = false;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBudding)));
        }
    }


    public string Url { get; set; }

    public bool HasError { get; set; }



    //public bool expander;
    //private IBrush _titleColor = Brushes.Black;


    public Process Process { get; set; }

    public ObservableCollection<ProjectModel> Children { get; set; } = new ObservableCollection<ProjectModel>();

    public ObservableCollection<ProjectModel> Gateways =>
        new ObservableCollection<ProjectModel>(Children.Where(x => x.DisplayName.Contains("Gateway")));

    public ObservableCollection<ProjectModel> Services =>
        new ObservableCollection<ProjectModel>(Children.Where(x => x.DisplayName.Contains("Service")));

    public ObservableCollection<ProjectModel> Others =>
        new ObservableCollection<ProjectModel>(Children.Where(x =>
            !x.DisplayName.Contains("Service") && !x.DisplayName.Contains("Gateway")));



    public virtual async void ClearLog()
    {
        Log = "";
        logBuilder.Clear();

        if (Children.Any())
        {
            foreach (var projectModel in Children)
            {
                projectModel.ClearLog();
            }
        }
    }

    StringBuilder logBuilder = new StringBuilder();

    /// <summary>
    /// 根据Path找到项目文件，编译该csproj，并且log输出全部放入到当前对象的Log中
    /// </summary>
    public virtual async Task<bool> Compile()
    {
        Stop();
        Log = "";
        // Expander = true;
        IsBudding = true;


        var succeeded = true;

        try
        {
            Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"build \"{Path}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    // StandardOutputEncoding = Encoding.ASCII,
                    StandardOutputEncoding = Encoding.UTF8, // 设置标准输出的编码
                    StandardErrorEncoding = Encoding.UTF8, // 设置标准错误的编码
                }
            };


            // 创建一个 StringBuilder 用于保存构建输出
            logBuilder = new StringBuilder();

            bool isSuccessful = false;
            // 为 OutputDataReceived 和 ErrorDataReceived 事件添加事件处理程序
            Process.OutputDataReceived += (sender, e) =>
            {
                if (isSuccessful) return;
                if (e.Data != null)
                {
                    Console.WriteLine(e.Data.ToString());

                    logBuilder.AppendLine(e.Data);
                    Log = logBuilder.ToString();

                    ScrollToEndEvent?.Invoke();

                    if (e.Data.Contains("Build succeeded.") || e.Data.Contains("已成功生成。"))
                    {
                        //Expander = false;
                        //Log = "";
                        isSuccessful = true;
                        //TitleColor = Brushes.Green;
                    }
                }
            };
            Process.ErrorDataReceived += (sender, e) =>
            {
                if (isSuccessful) return;
                if (e.Data != null)
                {
                    logBuilder.AppendLine(e.Data);
                    Log = logBuilder.ToString();
                }
            };

            // 启动进程
            Process.Start();

            // 开始异步读取构建输出和错误
            Process.BeginOutputReadLine();
            Process.BeginErrorReadLine();


            // 等待进程完成
            await Process.WaitForExitAsync();

            // 检查进程退出代码以确定构建是否成功
            if (Process.ExitCode == 0)
            {
                // 构建成功
                Log = logBuilder.ToString();
            }
            else
            {
                // 构建失败
                Log = $"Build failed with exit code {Process.ExitCode}:\n{logBuilder}";
                //TitleColor = Brushes.Red;
                succeeded = false;
            }
        }
        catch (Exception ex)
        {
            // 处理异常
            Log = $"Error: {ex.Message}";
            //TitleColor = Brushes.Red;
            succeeded = false;
        }

        IsBudding = false;

        //return !Equals(TitleColor, Brushes.Red);
        HasError = !succeeded;

        return succeeded;
    }


    private string GetBindingUrl()
    {
        var bindUrl = "";
        var projectPath = System.IO.Path.GetDirectoryName(Path);

        var launchSettingsPath = System.IO.Path.Combine(projectPath, "Properties", "launchSettings.json");

        if (File.Exists(launchSettingsPath))
        {
            var launchSettingsJson = File.ReadAllText(launchSettingsPath);
            using var jsonDoc = JsonDocument.Parse(launchSettingsJson);
            var root = jsonDoc.RootElement;

            // 检查 "iisSettings" 是否存在，并且 "iisExpress" 下的 "applicationUrl" 是否存在
            if (root.TryGetProperty("iisSettings", out var iisSettings) &&
                iisSettings.TryGetProperty("iisExpress", out var iisExpress) &&
                iisExpress.TryGetProperty("applicationUrl", out var applicationUrlElement))
            {
                bindUrl = applicationUrlElement.GetString();
            }
            // 你也可以遍历 "profiles" 以找到具体的启动配置
            else if (root.TryGetProperty("profiles", out var profiles))
            {
                foreach (var profile in profiles.EnumerateObject())
                {
                    if (profile.Value.TryGetProperty("applicationUrl", out var profileApplicationUrlElement))
                    {
                        bindUrl = profileApplicationUrlElement.GetString();
                        // 如果有多个配置，你可能需要根据名称或其他条件选择一个
                        break;
                    }
                }
            }
        }

        return bindUrl;
    }

    public async Task RunDll()
    {
        Log = "";
        // Expander = true;
        IsRunning = true;
        //TitleColor = Brushes.DarkCyan;
        logBuilder = new StringBuilder();


        var dllName = @$"{Name}.dll";

        var projectPath = System.IO.Path.GetDirectoryName(Path);

        var debugPath = System.IO.Path.Combine(projectPath, "bin", "Debug");

        var nets = new DirectoryInfo(debugPath);


        var dllPath = "";

        foreach (var directoryInfo in nets.GetDirectories().OrderByDescending(x => x.Name))
        {
            var file = directoryInfo.GetFiles().FirstOrDefault(x => x.Name == dllName || x.Name == Name);
            if (file != null)
            {
                dllPath = file.FullName;
            }
        }

        var bindUrl = GetBindingUrl();


        if (!string.IsNullOrEmpty(bindUrl))
        {
            int port = SystemHelper.GetPortFromUrl(bindUrl);
            if (port != -1)
            {
                var processId = SystemHelper.GetProcessIdUsingPort(port);
                if (processId != -1)
                {
                    SystemHelper.KillProcessById(processId);
                }
            }

            Url = bindUrl;

            bindUrl = @$"--urls={bindUrl}";


        }

        try
        {
            Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = projectPath,
                    FileName = "dotnet",
                    Arguments = $"{dllPath} --environment=Development {bindUrl}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    // StandardOutputEncoding = Encoding.UTF8, // 设置标准输出的编码
                    // StandardErrorEncoding = Encoding.UTF8, // 设置标准错误的编码
                }
            };


            // 创建一个 StringBuilder 用于保存构建输出

            // 为 OutputDataReceived 和 ErrorDataReceived 事件添加事件处理程序
            Process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    if (Log.Length >= 200000)
                    {
                        Log = "";
                        logBuilder.Clear();
                    }

                    logBuilder.AppendLine(e.Data);
                    Log = logBuilder.ToString();
                    ScrollToEndEvent?.Invoke();
                }
            };
            Process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    logBuilder.AppendLine(e.Data);
                    Log = logBuilder.ToString();
                }
            };

            // 启动进程
            Process.Start();

            // 开始异步读取构建输出和错误
            Process.BeginOutputReadLine();
            Process.BeginErrorReadLine();

            // Process.Exited += (sender, args) => { Dispatcher.UIThread.InvokeAsync(() =>
            // {
            //     IsRunning = false;
            //     TitleColor = Brushes.Black;
            // }); };
            // 等待进程完成
            await Process.WaitForExitAsync();

            // 检查进程退出代码以确定构建是否成功
            if (Process.ExitCode == 0)
            {
                // 构建成功
                Log = logBuilder.ToString();
            }
            else
            {
                // 构建失败
                Log = $"Build failed with exit code {Process.ExitCode}:\n{logBuilder}";
                HasError = true;
                //TitleColor = Brushes.Red;
            }
        }
        catch (Exception ex)
        {
            // 处理异常
            Log = $"Error: {ex.Message}";
            HasError = true;
            //TitleColor = Brushes.Red;
        }
        Url = "";
        IsRunning = false;
    }

    public virtual void Stop()
    {
        try
        {
            var bindUrl = GetBindingUrl();


            if (!string.IsNullOrEmpty(bindUrl))
            {
                int port = SystemHelper.GetPortFromUrl(bindUrl);
                if (port != -1)
                {
                    var processId = SystemHelper.GetProcessIdUsingPort(port);
                    if (processId != -1)
                    {
                        SystemHelper.KillProcessById(processId);
                    }
                }
            }

            // if (Process != null)
            //     await Process.WaitForExitAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        try
        {
            if (Process != null)
            {
                Process.Kill();
                Process.Dispose();
            }
        }
        catch (Exception e)
        {
        }


        Url = "";
        //Expander = false;
        IsRunning = false;
        Task.Delay(100).GetAwaiter().GetResult();
        //TitleColor = Brushes.Black;
    }
}