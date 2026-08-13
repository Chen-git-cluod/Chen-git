using System.ComponentModel;

namespace Kinlo.Common.Dto;

[AddINotifyPropertyChangedInterface]
public class DisplayDataDto
{
   public int Index { get; set; }
   public bool IsSelected { get; set; }
   public ProcessTypeEnum Processes { get; set; }

   /// <summary>
   /// 单工序原始类
   /// </summary>
   [JsonIgnore]
   public Type? OriginalClass { get; set; }

   /// <summary>
   /// 运行时完整电池类
   /// </summary>
   [JsonIgnore]
   public Type? RuntimeBatteryType { get; set; }

   /// <summary>
   /// 描述
   /// </summary>
   [JsonIgnore]
   public string Description { get; set; } = string.Empty;

   /// <summary>
   /// 运行时完整工序属性
   /// </summary>
   public ObservableRangeCollection<DisplayPropertyBindingDto> PropertyBindings { get; set; } = new();

   /// <summary>
   /// UI显示数据，勿直接添加，插入数据用下面 AddDatas 方法
   /// </summary>
   [JsonIgnore]
   public ObservableRangeCollection<object> Datas { get; private set; } = new();

   /// <summary>
   /// UI展示数据控件
   /// </summary>
   [JsonIgnore]
   public FrameworkElement? DataDisplayControl { get; set; }

   /// <summary>
   /// 数据View
   /// </summary>
   [JsonIgnore]
   public ICollectionView? DisplayView { get; private set; }

   /// <summary>
   /// 初始化View
   /// </summary>
   public void InitView(string sortDescription)
   {
      DisplayView = CollectionViewSource.GetDefaultView(Datas);
      DisplayView.SortDescriptions.Add(new SortDescription(sortDescription, ListSortDirection.Descending));
   }

   /// <summary>
   /// 添加数据
   /// </summary>
   /// <param name="datas"></param>
   public async Task AddDisplayData(params IBatMainModel[] batterys)
   {
      await Task.Run(async () =>
      {
         try
         {
            IBatMainModel[] list = new IBatMainModel[batterys.Length];
            for (int i = 0; i < batterys.Length; i++)
            {
               var entity = Activator.CreateInstance(RuntimeBatteryType); //复制一个新的
               batterys[i].EntityAssign(entity);
               list[i] = (IBatMainModel)entity;
            }
            await UIThreadHelper.Dispatcher.BeginInvoke(() =>
            {
               if (Datas.Count >= 200)
               {
                  var count = list.Length + (Datas.Count - 200);
                  Datas.RemoveAtRange(0, count);
                  // DisplayView.Refresh(); // UI刷新
               }
               Datas.AddRange(list);
            });
         }
         catch (Exception ex)
         {
            $"添加界面数据异常：{ex}".LogRun(Log4NetLevelEnum.错误);
         }
      });
   }
}
