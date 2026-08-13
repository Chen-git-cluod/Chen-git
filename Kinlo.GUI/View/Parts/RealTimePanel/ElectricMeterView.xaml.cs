namespace Kinlo.GUI.View
{
   /// <summary>
   /// ElectricMeterView.xaml 的交互逻辑
   /// </summary>
   [AddINotifyPropertyChangedInterface]
   public partial class ElectricMeterView : Border
   {
      IContainer _container;
      public GlobalStaticTemporary Temporary { get; set; }

      public ElectricMeterView(IContainer container)
      {
         InitializeComponent();
         this.DataContext = this;
         _container = container;
         Temporary = container.Get<GlobalStaticTemporary>();
      }
   }
}
