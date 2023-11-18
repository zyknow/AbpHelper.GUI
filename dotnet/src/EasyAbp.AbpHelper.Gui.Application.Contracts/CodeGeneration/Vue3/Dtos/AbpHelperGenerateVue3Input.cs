using System.ComponentModel.DataAnnotations;
using EasyAbp.AbpHelper.Gui.CodeGeneration.Shared.Dtos;

namespace EasyAbp.AbpHelper.Gui.CodeGeneration.Vue3.Dtos;

public class AbpHelperGenerateVue3Input : AbpHelperGenerateInput
{
    [Required]
    public virtual string Entity { get; set; }
    public virtual string EntityDto { get; set; }
    public string CreateEntityDto { get; set; }
    public string UpdateEntityDto { get; set; }
    public string GetListEntityDto { get; set; }
    [Required] public virtual string GeneratePath { get; set; }
    // public virtual string VueApiDirRelativePath { get; set; }
    // public virtual string VueCurdTsxDirRelativePath { get; set; }
    // public virtual string VuePageDirRelativePath { get; set; }
}