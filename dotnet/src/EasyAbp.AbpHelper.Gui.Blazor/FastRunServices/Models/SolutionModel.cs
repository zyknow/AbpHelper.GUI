using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FastRunMicroService.Models;

public partial class SolutionModel : BaseModel
{
    private List<string> DefaultNormalServices = new()
    {
        "AuthServer",
        "WebGateway",
        "AdministrationService",
        "IdentityService",
        "SaasService",
        "CirculationService",
        "LibrarianAssistantService",
        "PaymentService",
        "CirculationService",
        "ProcurementService",
        "PublicationService",
        "ReaderService",
    };

    //List<string> NormalServiceNames =>
    //    appSettings.NormalServices.IsNullOrEmpty() ? DefaultNormalServices : appSettings.NormalServices;

    public SolutionModel()
    {
        //var appSettingsFilePath = "appsettings.json";

        //if (File.Exists(appSettingsFilePath))
        //{
        //    appSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(appSettingsFilePath));
        //}
    }

    /// <summary>
    ///  停止运行所有项目并重新构建运行所有正在运行的项目
    /// </summary>
    public async void RebuildAndReRunAllRunning()
    {
        var runningProjects = Children.Where(x => x.IsRunning).ToList();

        await StopAllRunning();

        var compileSucceeded = await Compile();

        if (!compileSucceeded) return;


        var tasks = new List<Task>();

        foreach (var runningProject in runningProjects)
        {
            tasks.Add(new Task(async () => { await runningProject.RunDll(); }));
        }

        foreach (var task in tasks)
        {
            task.Start();
        }

        await Task.WhenAll(tasks);
    }


    public async Task RunNormalServices()
    {
        var services = Children.Where(x => !x.IsRunning && x.IsNormal).ToList();
        foreach (var projectModel in services)
        {
            projectModel.RunDll();
        }
    }

    public async Task StopAllRunning()
    {
        IsRunning = true;

        try
        {
            var runningProjects = Children.Where(x => x.IsRunning).ToList();

            foreach (var runningProject in runningProjects)
            {
                runningProject.Stop();
            }
        }
        catch (System.Exception)
        {

        }
        IsRunning = false;

    }

    public override async Task<bool> Compile()
    {
        await StopAllRunning();

        return await base.Compile();
    }


    //public virtual void SwitchExpander()
    //{
    //    //Expander = !Expander;
    //    foreach (var projectModel in Children)
    //    {
    //        if (!string.IsNullOrWhiteSpace(projectModel.Log))
    //            projectModel.Expander = Expander;
    //    }
    //}
}