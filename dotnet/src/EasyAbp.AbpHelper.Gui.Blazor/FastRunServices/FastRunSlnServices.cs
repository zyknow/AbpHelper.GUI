using FastRunMicroService.Models;
using Microsoft.Build.Construction;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Volo.Abp.DependencyInjection;

namespace EasyAbp.AbpHelper.Gui.Blazor.FastRunServices;

public class FastRunSlnServices : ISingletonDependency
{
    public SolutionModel Solution { get; set; }

    string[] whiteProjectNameList = { ".AuthServer", "DbMigrator", ".Host", "Gateway" };

    string[] exincludeProjectNameList = { "Shared" };

    public FastRunSlnServices()
    {
    }

    public void LoadSolution(string path)
    {
        var solutionFile = SolutionFile.Parse(path);
        var solution = new SolutionModel { Path = path, Name = System.IO.Path.GetFileNameWithoutExtension(path) };
        solution.Children = new ObservableCollection<ProjectModel>(GetProjects(solutionFile.ProjectsInOrder));

        Solution = solution;

        // FilterEmptyNode(solution.Children);
    }



    void FilterEmptyNode(ObservableCollection<ProjectModel> nodes)
    {
        var removeNodes = new List<ProjectModel>();

        foreach (var node in nodes)
        {
            if (node.Children.Count == 0 && !whiteProjectNameList.Any(name => node.Name.Contains(name)))
            {
                removeNodes.Add(node);
            }
            else FilterEmptyNode(node.Children);
        }

        foreach (var removeNode in removeNodes)
        {
            nodes.Remove(removeNode);
        }
    }

    private List<ProjectModel> GetProjects(IReadOnlyList<ProjectInSolution> projects, string? parentId)
    {
        var projectModels = new List<ProjectModel>();

        if (projects == null || !projects.Any()) return new List<ProjectModel>();

        foreach (var projectInSolution in projects.Where(p => p.ParentProjectGuid == parentId))
        {
            var project = new ProjectModel
            {
                Name = $@"{projectInSolution.ProjectName}",
                Path = projectInSolution.AbsolutePath
            };
            if (projectInSolution.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat)
            {
                if (!whiteProjectNameList.Any(name => projectInSolution.ProjectName.Contains(name)))
                    continue;
                else
                {
                    project.IsWhite = true;
                }
            }


            projectModels.Add(project);

            project.Children =
                new ObservableCollection<ProjectModel>(GetProjects(projects, projectInSolution.ProjectGuid));
        }

        return projectModels;
    }

    private List<ProjectModel> GetProjects(IReadOnlyList<ProjectInSolution> projects)
    {
        var projectModels = new List<ProjectModel>();

        if (projects == null || !projects.Any()) return new List<ProjectModel>();

        foreach (var projectInSolution in projects)
        {
            var project = new ProjectModel
            {
                Name = $@"{projectInSolution.ProjectName}",
                Path = projectInSolution.AbsolutePath
            };
            if (projectInSolution.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat)
            {
                if (!whiteProjectNameList.Any(name => projectInSolution.ProjectName.Contains(name)) ||
                    exincludeProjectNameList.Any(name => projectInSolution.ProjectName.Contains(name)))
                    continue;
                else
                {
                    project.IsWhite = true;
                    projectModels.Add(project);
                }
            }


            // project.Children =
            //     new ObservableCollection<ProjectModel>(GetProjects(projects, projectInSolution.ProjectGuid));
        }

        return projectModels;
    }


}
