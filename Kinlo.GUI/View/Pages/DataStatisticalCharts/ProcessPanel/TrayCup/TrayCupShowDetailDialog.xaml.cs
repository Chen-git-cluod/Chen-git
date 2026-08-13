using System.Diagnostics.CodeAnalysis;

namespace Kinlo.GUI.View;

/// <summary>
/// TrayCupShowDetailDialog.xaml 的交互逻辑
/// </summary>
[AddINotifyPropertyChangedInterface]
public partial class TrayCupShowDetailDialog : Window
{
   public string TotalInfo { get; set; } = string.Empty;
   public string InjInfo { get; set; } = string.Empty;
   public string LeakInfo { get; set; } = string.Empty;
   public ObservableCollection<ResultTypeEnum> InjectResults { get; set; }
   public ObservableCollection<ResultTypeEnum> LeakResults { get; set; }
   public StatisticsDataItemModel StatisticsData { get; set; }
   public ObservableCollection<InjectDisplay> InjectDisplays { get; set; }

   public TrayCupShowDetailDialog(StatisticsDataItemModel statisticsData)
   {
      InitializeComponent();
      StatisticsData = statisticsData;
      HashSet<ResultTypeEnum> injectSet = new HashSet<ResultTypeEnum>();
      HashSet<ResultTypeEnum> leakSet = new HashSet<ResultTypeEnum>();
      foreach (var item in statisticsData.Datas)
      {
         injectSet.Add(item.FirstInjectResult);
         leakSet.Add(item.LeakResult);
      }
      InjectResults = new ObservableCollection<ResultTypeEnum>();
      LeakResults = new ObservableCollection<ResultTypeEnum>();
      InjectResults.AddRange(injectSet);
      LeakResults.AddRange(leakSet);

      foreach (var item in InjectResults)
         injCmb.SelectedItems.Add(item);
      foreach (var item in LeakResults)
         leakCmb.SelectedItems.Add(item);

      UpdateData();
   }

   private void Button_Click(object sender, RoutedEventArgs e) => UpdateData();

   [MemberNotNull(nameof(InjectDisplays))]
   private void UpdateData()
   {
      if (injCmb.SelectedItems.Count == 0 || leakCmb.SelectedItems.Count == 0)
      {
         HandyControl.Controls.Growl.Warning("注液及测漏各至少需选一项！");
         return;
      }
      if (injCmb.SelectedItems.Count == InjectResults.Count && leakCmb.SelectedItems.Count == LeakResults.Count)
         InjectDisplays = StatisticsData.Datas.ToObservableCollection();
      else
         InjectDisplays = StatisticsData
            .Datas.Where(x => injCmb.SelectedItems.Contains(x.FirstInjectResult) && leakCmb.SelectedItems.Contains(x.LeakResult))
            .ToObservableCollection();

      TotalInfo = $"{InjectDisplays.Count}个";

      StringBuilder stringBuilder = new StringBuilder();
      foreach (var item in injCmb.SelectedItems)
      {
         if (item is ResultTypeEnum res)
         {
            var count = InjectDisplays.Count(x => x.FirstInjectResult == res);
            string name = res == ResultTypeEnum._ ? "注液工序未生产" : res.ToString();
            stringBuilder.Append($"{name}：{count}个； ");
         }
      }
      InjInfo = stringBuilder.ToString();
      stringBuilder.Clear();
      foreach (var item in leakCmb.SelectedItems)
      {
         if (item is ResultTypeEnum res)
         {
            var count = InjectDisplays.Count(x => x.LeakResult == res);
            string name = res == ResultTypeEnum._ ? "测漏工序未生产" : res.ToString();
            stringBuilder.Append($"{name}：{count}个； ");
         }
      }
      LeakInfo = stringBuilder.ToString();
   }
}
