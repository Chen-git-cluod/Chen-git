using System.Windows.Input;

namespace Kinlo.GUI.ViewModel
{
   [UIDisplay(true)]
   public class EntityPropertyVisibleViewModel : Screen
   {
      public bool IsEntityPropertyVisible { get; set; }
      public DisplayDataCollection DisplayDatas { get; set; }

      // public ObservableCollection<DisplayPropertyBindingDto> PropertyBindings { get; set; } = new ObservableCollection<DisplayPropertyBindingDto>();
      public DisplayDataDto DisplayData { get; set; } = new DisplayDataDto();

      IContainer _container;
      UsersStatusConfig _usersStatusConfig;

      public EntityPropertyVisibleViewModel(IContainer container)
      {
         _container = container;
         DisplayDatas = container.Get<DisplayDataCollection>();
         _usersStatusConfig = container.Get<UsersStatusConfig>();
      }

      public void SaveProperty()
      {
         _container.Get<ProductionHistoryViewModel>().CreateBatteryDataGrid();
         DisplayDatas.CreateControl();
         DisplayDatas.Save(_usersStatusConfig.LocalLoggedinUser.Account, "修改显示属性", false);
      }

      /// <summary>
      /// 全选
      /// </summary>
      /// <param name="e"></param>
      public void AllSelected(RoutedEventArgs e)
      {
         foreach (var item in DisplayData.PropertyBindings)
         {
            item.IsVisible = item.IsExport = !item.IsVisible;
         }
      }

      /// <summary>
      /// 拖动完成
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void TextBlock_Drop(object sender, DragEventArgs e)
      {
         // HCC.Tag TextBlock but = sender as TextBlock;
         Border but = sender as Border;
         // var send = e.Data.GetData(typeof(EntityPropertyVisibleModel)) as EntityPropertyVisibleModel;
         var sends = DisplayData.PropertyBindings.Where(x => x.IsSelected).ToList();
         var receive = but.DataContext as DisplayPropertyBindingDto;
         int receiveIndex = DisplayData.PropertyBindings.IndexOf(receive);
         ++receiveIndex;
         for (int i = 0; i < sends.Count; i++)
         {
            int sendIndex = DisplayData.PropertyBindings.IndexOf(sends[i]);

            if (sendIndex < receiveIndex)
            {
               DisplayData.PropertyBindings.Insert(receiveIndex, sends[i]);
               DisplayData.PropertyBindings.RemoveAt(sendIndex);
            }
            else
            {
               DisplayData.PropertyBindings.RemoveAt(sendIndex);
               DisplayData.PropertyBindings.Insert(receiveIndex, sends[i]);
               ++receiveIndex;
            }

            sends[i].IsSelected = false;
         }

         for (int i = 0; i < DisplayData.PropertyBindings.Count; i++)
         {
            DisplayData.PropertyBindings[i].Index = i;
         }
      }

      /// <summary>
      /// 按下鼠标
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         Border but = sender as Border;
         DragDrop.DoDragDrop(but, but.DataContext, DragDropEffects.Move);
      }

      /// <summary>
      /// 按下鼠标
      /// </summary>
      /// <param name="sender"></param>
      /// <param name="e"></param>
      public void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
      {
         Border but = sender as Border;
         DisplayPropertyBindingDto _propertyBinding = (DisplayPropertyBindingDto)but.DataContext;
         _propertyBinding.IsSelected = !_propertyBinding.IsSelected;
      }

      /// <summary>
      /// 重置
      /// </summary>
      /// <param name="processesType"></param>
      public void ResetCmd()
      {
         DisplayData.PropertyBindings.Clear();
         DisplayDatas.UpdateProcessProperty(DisplayData);
         SaveProperty();
      }
   }
}
