using EasyAbp.AbpHelper.Gui.CodeGeneration.Vue3.Dtos;
using EasyAbp.AbpHelper.Gui.Shared.Dtos;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EasyAbp.AbpHelper.Gui.CodeGeneration.Vue3;

public interface ICodeGenerationVue3AppService : IApplicationService
{
    Task<ServiceExecutionResult> GenerateAsync(AbpHelperGenerateVue3Input input);
}