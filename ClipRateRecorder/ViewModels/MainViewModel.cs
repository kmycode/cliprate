﻿using ClipRateRecorder.Models.Analysis.Groups;
using ClipRateRecorder.Models.Analysis.Ranges;
using ClipRateRecorder.Models.Logics;
using ClipRateRecorder.Models.Watching;
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

    public ActivityRange? Range => this.IsSpot ? this.mainWatcherModel.SpotRange : this.mainWatcherModel.Range;

    public DateTime Today => DateTime.Today;

    public bool IsSpot
    {
      get => this._isSpot;
      set
      {
        this._isSpot = value;
        this.OnPropertyChanged();
        this.OnPropertyChanged(nameof(Range));
      }
    }
    private bool _isSpot;

    public MainViewModel()
    {
      this.mainWatcherModel.PropertyChanged += this.RaisePropertyChanged;
    }

    public void OnWindowClose()
    {
      this.mainWatcherModel.Dispose();
    }

    public ReactiveCommand ResetSpotCommand =>
      this._resetSpotCommand ??= new ReactiveCommand().WithSubscribe(() => {
        this.mainWatcherModel.ResetSpot();
        this.OnPropertyChanged(nameof(Range));
        });
    private ReactiveCommand _resetSpotCommand;
  }
}
