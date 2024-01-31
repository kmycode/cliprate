using ClipRateRecorder.Models.Analysis.Groups;
using ClipRateRecorder.Models.Analysis.Ranges;
using ClipRateRecorder.Models.Goals;
using ClipRateRecorder.Models.Logics;
using ClipRateRecorder.Models.Watching;
using ClipRateRecorder.Utils;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClipRateRecorder.ViewModels
{
  class MainViewModel : ViewModelBase, ICloseEventReceiver
  {
    private readonly MainWatcherModel mainWatcherModel = new();

    public ActivityRange? Range => this.Page == ViewPage.Spot ? this.mainWatcherModel.SpotRange : this.mainWatcherModel.Range;

    public MilestoneRange? MilestoneRange => this.mainWatcherModel.MilestoneRange;

    public DateTime CurrentDay => this.mainWatcherModel.CurrentDay;

    public ViewPage Page
    {
      get => this._page;
      set
      {
        this._page = value;
        this.OnPropertyChanged();

        if (value == ViewPage.Spot || value == ViewPage.Daily)
        {
          this.OnPropertyChanged(nameof(Range));
        }
      }
    }
    private ViewPage _page;

    public bool IsMilestoneEdit
    {
      get => this._isMilestoneEdit;
      set
      {
        if (this._isMilestoneEdit != value)
        {
          this._isMilestoneEdit = value;
          this.OnPropertyChanged();
        }
      }
    }
    private bool _isMilestoneEdit;

    public MainViewModel()
    {
      this.mainWatcherModel.PropertyChanged += this.RaisePropertyChanged;
      this.mainWatcherModel.PropertyChanged += (sender, e) =>
      {
        if (e.PropertyName == nameof(this.mainWatcherModel.SpotRange))
        {
          this.OnPropertyChanged(nameof(Range));
        }
      };
    }

    public void OnWindowClose()
    {
      this.mainWatcherModel.Dispose();
    }

    public ReactiveCommand MoveDailyPageCommand =>
      this._moveDailyPageCommand ??= new ReactiveCommand().WithSubscribe(() => this.Page = ViewPage.Daily);
    private ReactiveCommand? _moveDailyPageCommand;

    public ReactiveCommand MoveSpotPageCommand =>
      this._moveSpotPageCommand ??= new ReactiveCommand().WithSubscribe(() => this.Page = ViewPage.Spot);
    private ReactiveCommand? _moveSpotPageCommand;

    public ReactiveCommand MoveMilestonePageCommand =>
      this._moveMilestonePageCommand ??= new ReactiveCommand().WithSubscribe(() => this.Page = ViewPage.Milestone);
    private ReactiveCommand? _moveMilestonePageCommand;

    public ReactiveCommand StepPrevDayCommand =>
      this._stepPrevDayCommand ??= new ReactiveCommand().WithSubscribe(async () => await this.mainWatcherModel.StepPrewDayAsync());
    private ReactiveCommand? _stepPrevDayCommand;

    public ReactiveCommand StepNextDayCommand =>
      this._stepNextDayCommand ??= new ReactiveCommand().WithSubscribe(async () => await this.mainWatcherModel.StepNextDayAsync());
    private ReactiveCommand? _stepNextDayCommand;

    public ReactiveCommand ResetSpotCommand =>
      this._resetSpotCommand ??= new ReactiveCommand().WithSubscribe(this.mainWatcherModel.ResetSpot);
    private ReactiveCommand? _resetSpotCommand;

    public ReactiveCommand<object[]> SetDefaultEvaluationCommand =>
      this._setDefaultEvaluationCommand ??= new ReactiveCommand<object[]>().WithSubscribe(async (pms) =>
      {
        await this.mainWatcherModel.SetDefaultEvaluationAsync(((ExePathActivityGroup)pms[0]).ExePath, (string)pms[1]);
      });
    private ReactiveCommand<object[]>? _setDefaultEvaluationCommand;

    public ReactiveCommand AddMilestoneCommand =>
      this._addMilestoneCommand ??= new ReactiveCommand().WithSubscribe(async () => await this.mainWatcherModel.AddMilestoneAsync());
    private ReactiveCommand? _addMilestoneCommand;

    public ReactiveCommand<Milestone> RemoveMilestoneCommand =>
      this._removeMilestoneCommand ??= new ReactiveCommand<Milestone>().WithSubscribe(async milestone => await mainWatcherModel.RemoveMilestoneAsync(milestone));
    private ReactiveCommand<Milestone>? _removeMilestoneCommand;
  }
}
