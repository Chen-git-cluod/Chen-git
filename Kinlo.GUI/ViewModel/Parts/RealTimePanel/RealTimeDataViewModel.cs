using HandyControl.Controls;

namespace Kinlo.GUI.ViewModel;

public class RealTimeDataViewModel : Screen
{
   public DisplayConfigPanelView ConfigPanelView { get; set; }
   public ProcessRatioDisplay DisplayRatio { get; set; }
   public AppGlobalConfig AppGlobal { get; set; }
   public ParameterConfig Parameter { get; set; }
   IContainer _container;
   UsersStatusConfig _usersStatus;

   public RealTimeDataViewModel(IContainer container)
   {
      FinalInjectInfo = new InjectionVolumeInfoDto(50, 12);
      FirstInjectInfo = new InjectionVolumeInfoDto(16, 5);
      AutoReInjectInfo = new InjectionVolumeInfoDto(16, 5);
      ManualRefillInfo = new InjectionVolumeInfoDto(16, 5);

      _container = container;
      _usersStatus = container.Get<UsersStatusConfig>();
      Parameter = container.Get<ParameterConfig>();
      ConfigPanelView = new DisplayConfigPanelView(container);
      DisplayRatio = container.Get<ProcessRatioDisplay>();
      AppGlobal = container.Get<AppGlobalConfig>();
      DisplayRatio.DisplayRatioChanged += InjectionDisplayPercentage_ProportionAction;
   }

   public async Task ZeroingCMD()
   {
      if (HandyControl.Controls.MessageBox.Show("确定重置统计数据？", "系统提示：", MessageBoxButton.OKCancel) != MessageBoxResult.OK)
         return;
      try
      {
         await DisplayRatio.ResetAsync();
         FinalInjectInfo.Reset();
         FirstInjectInfo.Reset();
         AutoReInjectInfo.Reset();
         ManualRefillInfo.Reset();
         await UIThreadHelper.InvokeOnUiThreadAsync(() => AppGlobal.ShiftSwitchInfo.LastResetTime = DateTime.Now);
         AppGlobal.Save(_usersStatus.LocalLoggedinUser.Account, "重置成功！", true);
      }
      catch (Exception ex)
      {
         Growl.Warning($"重置异常：{ex}");
      }
   }

   #region 注液图表
   /// <summary>
   /// 更新注液图表
   /// </summary>
   /// <param name="isOK"></param>
   private void InjectionDisplayPercentage_ProportionAction(ProcessRatioItem[] data)
   {
      if (data == null)
         return;
      foreach (var item in data)
      {
         if (item.Process == "首注结果")
            GetRatio(FirstInjectInfo, item);
         else if (item.Process == nameof(ProcessTypeEnum.回流补液))
            GetRatio(AutoReInjectInfo, item);
         else if (item.Process == nameof(ProcessTypeEnum.手动补液))
            GetRatio(ManualRefillInfo, item);
         else if (item.Process == "最终注液结果")
            GetRatio(FinalInjectInfo, item);
      }
   }

   private void GetRatio(InjectionVolumeInfoDto info, ProcessRatioItem processRatio)
   {
      info.TotalCount = processRatio.TotalCount;
      var underInject = processRatio.NgDetails.FirstOrDefault(x => x.Name == nameof(ResultTypeEnum.注液量偏少));
      var overInject = processRatio.NgDetails.FirstOrDefault(x => x.Name == nameof(ResultTypeEnum.注液量偏多));
      if (underInject != null)
      {
         info.UnderInjectCount = underInject.Count;
         info.UnderInjectRatio = underInject.Ratio;
      }
      if (overInject != null)
      {
         info.OverInjectCount = overInject.Count;
         info.OverRatio = overInject.Ratio;
      }
      info.OkInjectCount = processRatio.OkTotal;
      info.OkRatio = processRatio.OkRatio;

      ((ObservableCollection<double>)info.InjectSeries[0].Values!)[0] = info.OkRatio;
      ((ObservableCollection<double>)info.InjectSeries[1].Values!)[0] = info.UnderInjectRatio;
      ((ObservableCollection<double>)info.InjectSeries[2].Values!)[0] = info.OverRatio;
   }

   /// <summary>
   /// 最终注液图表
   /// </summary>
   public InjectionVolumeInfoDto FinalInjectInfo { get; set; }

   /// <summary>
   /// 首次注液图表
   /// </summary>
   public InjectionVolumeInfoDto FirstInjectInfo { get; set; }

   /// <summary>
   /// 回流液液图表
   /// </summary>
   public InjectionVolumeInfoDto AutoReInjectInfo { get; set; }

   /// <summary>
   /// 手动补液
   /// </summary>
   public InjectionVolumeInfoDto ManualRefillInfo { get; set; }

   /// <summary>
   /// 解决显示中文乱码问题
   /// </summary>
   public SolidColorPaint TextPaint { get; set; } =
      new SolidColorPaint() { Color = SKColors.DarkSlateGray, SKTypeface = SKFontManager.Default.MatchCharacter('汉') };
   #endregion
}
