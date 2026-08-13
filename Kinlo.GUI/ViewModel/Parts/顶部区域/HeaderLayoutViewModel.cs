namespace Kinlo.GUI.ViewModel;

[UIDisplayAttribute(true)]
public class HeaderLayoutViewModel : Screen
{
   public Action<int> HandlerWindow;
   public HelperViewModel HelpMenuVM { get; set; }
   public UserStatusViewModel UserStatus { get; set; }
   public ThemeViewModel Theme { get; set; }
   public StartDeviceViewModel StartDevice { get; set; }
   public OtherParameterConfig OtherParameter { get; set; }
   public ParameterConfig Parameter { get; set; }

   UsersStatusConfig _usersStatus;
   public PLCResectionViewModel PLCResectionVM { get; set; }

   IContainer _container;

   public HeaderLayoutViewModel(IContainer container)
   {
      _container = container;
      Theme = container.Get<ThemeViewModel>();
      Parameter = container.Get<ParameterConfig>();
      UserStatus = container.Get<UserStatusViewModel>();
      StartDevice = container.Get<StartDeviceViewModel>();
      OtherParameter = container.Get<OtherParameterConfig>();
      _usersStatus = container.Get<UsersStatusConfig>();
      PLCResectionVM = container.Get<PLCResectionViewModel>();
      HelpMenuVM = container.Get<HelperViewModel>();

      #region 测试切除显示
      //测试切除显示
      //Task.Run(() =>
      //{
      //    Random random = new Random();
      //    while (true)
      //    {
      //        var i = random.Next();
      //        foreach (var item in container.Get<PLCSignalConfig>().PLCScanSignals)
      //        {
      //            for (int k = 0; k < item.PLCResections.Count; k++)
      //            {
      //                var p = item.PLCResections[k];
      //                p.IsEnabled = (i+k) % 3 == 0 ? true : false;
      //                p.IsExcision = (i + k) % 3 == 0 ? true : false;
      //            }
      //        }

      //        Thread.Sleep(2000);
      //    }

      //});
      #endregion

      #region 导出现有字典 弃用
      //List<tempClass> tempClasses = new List<tempClass>();
      //string requestedCulture = $@"pack://application:,,,/Kinlo.GUI;component/Languages/zh_cn.xaml";
      //string requestedCulture2 = $@"Languages\zh_cn.xaml";
      //ResourceDictionary _dictionary =
      //    Application.Current.Resources.MergedDictionaries.FirstOrDefault(d =>
      //    d.Source != null && d.Source.OriginalString.Equals(requestedCulture));
      //if (_dictionary != null)
      //{
      //    foreach (var kvp in _dictionary)
      //    {
      //        System.Collections.DictionaryEntry k = (System.Collections.DictionaryEntry)kvp;
      //        tempClasses.Add(new tempClass
      //        {
      //            Key = k.Key.ToString(),
      //            zh_cn = k.Value.ToString(),
      //        });
      //    }
      //}
      //var ff = Application.GetResourceStream(new Uri(requestedCulture));
      // string requestedCulture2 = $@"Languages\zh_tw.xaml";
      // ResourceDictionary _dictionary2 =
      //     Application.Current.Resources.MergedDictionaries.FirstOrDefault(d =>
      //     d.Source != null && d.Source.OriginalString.Equals(requestedCulture2));
      // if (_dictionary2 != null)
      // {
      //     foreach (var kvp in _dictionary2)
      //     {
      //         System.Collections.DictionaryEntry k = (System.Collections.DictionaryEntry)kvp;
      //         var vla = tempClasses.FirstOrDefault(x=>x.Key == k.Key.ToString());
      //         if (vla != null)
      //             vla.zh_tw = k.Value.ToString();
      //     }
      // }
      // string requestedCulture3 = $@"Languages\en_us.xaml";
      // ResourceDictionary _dictionary3 =
      //     Application.Current.Resources.MergedDictionaries.FirstOrDefault(d =>
      //     d.Source != null && d.Source.OriginalString.Equals(requestedCulture3));
      // if (_dictionary3 != null)
      // {
      //     foreach (var kvp in _dictionary3)
      //     {
      //         System.Collections.DictionaryEntry k = (System.Collections.DictionaryEntry)kvp;
      //         var vla = tempClasses.FirstOrDefault(x => x.Key == k.Key.ToString());
      //         if (vla != null)
      //             vla.en_us = k.Value.ToString();
      //     }
      // }
      // tempClasses = tempClasses.OrderBy(x => x.Key).ToList();
      // StringBuilder stringBuilder = new StringBuilder();
      // foreach (var item in tempClasses)
      // {
      //     stringBuilder.AppendLine($"{item.Key}={item.zh_cn}={item.zh_tw}={item.en_us}");
      // }
      // File.WriteAllText("E:\\language.csv", stringBuilder.ToString());
      #endregion
   }

   /// <summary>
   /// 窗口最小化
   /// </summary>
   public void Minimized()
   {
      HandlerWindow?.Invoke(1);
   }

   /// <summary>
   /// 窗口最大化
   /// </summary>
   public void MaximizedCMD()
   {
      HandlerWindow?.Invoke(2);
   }

   /// <summary>
   /// 关闭窗口
   /// </summary>
   public void CloseCMD()
   {
      HandlerWindow?.Invoke(3);
   }
}
