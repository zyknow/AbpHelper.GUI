using EasyAbp.AbpHelper.Gui.CodeGeneration.Vue3;
using EasyAbp.AbpHelper.Gui.CodeGeneration.Vue3.Dtos;
using EasyAbp.AbpHelper.Gui.Shared.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp;

namespace EasyAbp.AbpHelper.Gui.Controllers.CodeGeneration;


[RemoteService]
[Route("/api/abp-helper/code-generation/vue3")]
public class CodeGenerationVue3Controller : GuiController, ICodeGenerationVue3AppService
{
    private readonly ICodeGenerationVue3AppService _service;

    public CodeGenerationVue3Controller(ICodeGenerationVue3AppService service)
    {
        _service = service;
    }

    [HttpPost]
    public Task<ServiceExecutionResult> GenerateAsync(AbpHelperGenerateVue3Input input)
    {
        return _service.GenerateAsync(input);
    }
}
