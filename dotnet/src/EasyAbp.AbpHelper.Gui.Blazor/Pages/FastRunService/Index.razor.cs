using Blazored.LocalStorage;
using EasyAbp.AbpHelper.Gui.Blazor.FastRunServices;
using FastRunMicroService.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EasyAbp.AbpHelper.Gui.Blazor.Pages.FastRunService;

public partial class Index : IDisposable
{
    public Index()
    {

    }

    public string Path { get; set; }

    [Inject]
    public FastRunSlnServices _service { get; set; }

    [Inject]
    public ILocalStorageService localStorage { get; set; }

    [Inject]
    public IJSRuntime _jSRuntime { get; set; }

    public SolutionModel Sln { get => _service.Solution; }

    public BaseModel CurrentLogProject { get; set; }

    public bool FollowLog { get; set; } = true;
    public bool FollowCurrentActionLog { get; set; } = true;

    public void LoadSolution(string path)
    {
        _service.LoadSolution(path);

        foreach (var item in Sln.Children)
        {
            item.PropertyChanged += (s, e) =>
            {
                InvokeAsync(() =>
                {
                    StateHasChanged();

                });
            };
        }

    }

    public async Task ChangeCurrentLogProject(BaseModel project)
    {

        // 移除之前的监听
        if (CurrentLogProject != null)
        {
            CurrentLogProject.PropertyChanged -= CurrentLogProject_PropertyChanged;
        }

        CurrentLogProject = project;

        CurrentLogProject.PropertyChanged += CurrentLogProject_PropertyChanged;


        await ScrollToBottomAsync();

        StateHasChanged();
    }

    private async void CurrentLogProject_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (FollowLog)
            await ScrollToBottomAsync();

        await InvokeAsync(() =>
    {
        // 这里放置更新 UI 的代码
        StateHasChanged();  // 例如，调用 StateHasChanged 方法
    });
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        try
        {
            Path = await localStorage.GetItemAsync<string>("MjLibrarySlnPath") ?? Path;
            LoadSolution(Path);
            await LoadLocalStorageAsync();

        }
        catch (System.Exception)
        {
            _service.Solution = null;
        }

        StateHasChanged();

    }

    private async Task SaveLocalStorageAsync()
    {
        if (Sln != null)
        {
            await localStorage.SetItemAsync(nameof(FollowLog), FollowLog);
            await localStorage.SetItemAsync(nameof(FollowCurrentActionLog), FollowCurrentActionLog);
            await localStorage.SetItemAsync("MjLibraryNomalServices", Sln.Children.Where(x => x.IsNormal).Select(x => x.DisplayName).ToList());
            await localStorage.SetItemAsync("MjLibrarySlnPath", Path);
        }


    }

    async Task LoadLocalStorageAsync()
    {
        if (Sln != null)
        {
            FollowLog = await localStorage.GetItemAsync<bool?>(nameof(FollowLog)) ?? true;
            FollowCurrentActionLog = await localStorage.GetItemAsync<bool?>(nameof(FollowCurrentActionLog)) ?? true;
            var nomalServices = await localStorage.GetItemAsync<List<string>>("MjLibraryNomalServices");
            if (nomalServices != null)
            {
                foreach (var item in Sln.Children)
                {
                    if (nomalServices.Contains(item.DisplayName))
                    {
                        item.IsNormal = true;
                    }
                }
            }
        }
    }





    private async Task ScrollToBottomAsync()
    {
        if (FollowLog)
        {
            try
            {
                await _jSRuntime.InvokeVoidAsync("scrollToBottom", "logCardId");
            }
            catch (System.Exception)
            {
            }
        }
    }

    public void Dispose()
    {
        if (Sln != null)
        {
            Sln.StopAllRunning().GetAwaiter().GetResult();
        }
    }
}
