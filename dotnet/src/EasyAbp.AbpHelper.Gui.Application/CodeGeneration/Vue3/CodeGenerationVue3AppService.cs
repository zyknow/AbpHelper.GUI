using EasyAbp.AbpHelper.Core.Commands.Generate.Vue3;
using EasyAbp.AbpHelper.Gui.CodeGeneration.Vue3.Dtos;
using EasyAbp.AbpHelper.Gui.Shared.Dtos;
using System.Threading.Tasks;

namespace EasyAbp.AbpHelper.Gui.CodeGeneration.Vue3;

public class CodeGenerationVue3AppService : CodeGenerationAppService, ICodeGenerationVue3AppService
{
    private readonly Vue3Command _vue3Command;

    public CodeGenerationVue3AppService(Vue3Command vue3Command)
    {
        _vue3Command = vue3Command;
    }

    public async Task<ServiceExecutionResult> GenerateAsync(AbpHelperGenerateVue3Input input)
    {
        await _vue3Command.RunCommand(ObjectMapper.Map<AbpHelperGenerateVue3Input, Vue3CommandOption>(input));

        return new ServiceExecutionResult(true);
    }
}