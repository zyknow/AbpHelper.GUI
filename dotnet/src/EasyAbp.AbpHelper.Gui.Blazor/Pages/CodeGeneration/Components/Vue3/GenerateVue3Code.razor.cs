using EasyAbp.AbpHelper.Gui.CodeGeneration.Vue3;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace EasyAbp.AbpHelper.Gui.Blazor.Pages.CodeGeneration.Components.Vue3;

public partial class GenerateVue3Code
{
    public GenerateVue3Code()
    {
// #if DEBUG
//
//         Input.EntityDto = "HolidayBetween";
//         Input.VueProjectRootPath = "G:\\Mj\\MjLibrary\\apps\\vue";
//         Input.VueApiDirRelativePath = "\\api\\abp-system\\test";
//         Input.VueCurdTsxDirRelativePath = "\\api\\abp-system\\test";
//         Input.VuePageDirRelativePath = "\\api\\abp-system\\test";
//
// #endif
    }

    [Inject]
    private ICodeGenerationVue3AppService Service { get; set; }

    protected override async Task InternalExecuteAsync()
    {
        await Service.GenerateAsync(Input);
    }
}