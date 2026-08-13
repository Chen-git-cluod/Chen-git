using System.ComponentModel;

namespace Kinlo.Common.Tools;

/// <summary>
/// 用于处理 绑定代表资源键(key)的变量
/// </summary>
/// <example>
/// <code>
/// (Image Source="{local:ResourceBinding ImageResourceKey}"/>
/// </code>
/// </example>
public class ResourceBinding : MarkupExtension
{
   #region Helper properties

   public static object GetResourceBindingKeyHelper(DependencyObject obj)
   {
      return (object)obj.GetValue(ResourceBindingKeyHelperProperty);
   }

   public static void SetResourceBindingKeyHelper(DependencyObject obj, object value)
   {
      obj.SetValue(ResourceBindingKeyHelperProperty, value);
   }

   // 使用DependencyProperty作为ResourceBindingKeyHelper的后备存储。这可以实现动画、样式、绑定等。。。
   public static readonly DependencyProperty ResourceBindingKeyHelperProperty = DependencyProperty.RegisterAttached(
      "ResourceBindingKeyHelper",
      typeof(object),
      typeof(ResourceBinding),
      new System.Windows.PropertyMetadata(null, ResourceKeyChanged)
   );

   static void ResourceKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
   {
      var target = d as FrameworkElement;
      var newVal = e.NewValue as Tuple<object, DependencyProperty>;

      if (target == null || newVal == null)
         return;

      var dp = newVal.Item2;

      if (newVal.Item1 == null)
      {
         target.SetValue(dp, dp.GetMetadata(target).DefaultValue);
         return;
      }

      target.SetResourceReference(dp, newVal.Item1 is Enum ? newVal.Item1.ToString() : newVal.Item1);
   }

   #endregion

   public ResourceBinding() { }

   public ResourceBinding(string path)
   {
      Path = new PropertyPath(path);
   }

   public override object ProvideValue(IServiceProvider serviceProvider)
   {
      var provideValueTargetService = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
      if (provideValueTargetService == null)
         return null;

      if (
         provideValueTargetService.TargetObject != null
         && provideValueTargetService.TargetObject.GetType().FullName == "System.Windows.SharedDp"
      )
         return this;

      var targetObject = provideValueTargetService.TargetObject as FrameworkElement;
      var targetProperty = provideValueTargetService.TargetProperty as DependencyProperty;
      if (targetObject == null || targetProperty == null)
         return null;

      #region binding

      Binding binding = new Binding
      {
         Path = Path,
         XPath = XPath,
         Mode = Mode,
         UpdateSourceTrigger = UpdateSourceTrigger,
         Converter = Converter,
         ConverterParameter = ConverterParameter,
         ConverterCulture = ConverterCulture,
         FallbackValue = FallbackValue,
      };

      if (RelativeSource != null)
         binding.RelativeSource = RelativeSource;

      if (ElementName != null)
         binding.ElementName = ElementName;

      if (Source != null)
         binding.Source = Source;

      #endregion

      var multiBinding = new MultiBinding { Converter = HelperConverter.Current, ConverterParameter = targetProperty };

      multiBinding.Bindings.Add(binding);
      multiBinding.NotifyOnSourceUpdated = true;

      targetObject.SetBinding(ResourceBindingKeyHelperProperty, multiBinding);

      if (targetProperty.PropertyType == typeof(string))
         return string.Empty;

      return default;
      //  return null;
   }

   #region Binding Members

   /// <summary>
   /// 源路径（用于CLR绑定）。
   /// </summary>
   public object Source { get; set; }

   /// <summary>
   ///  源路径（用于CLR绑定）。
   /// </summary>
   public PropertyPath Path { get; set; }

   /// <summary>
   /// XPath路径（用于XML绑定）。
   /// </summary>
   [DefaultValue(null)]
   public string XPath { get; set; }

   /// <summary>
   /// Binding mode
   /// </summary>
   [DefaultValue(BindingMode.Default)]
   public BindingMode Mode { get; set; }

   /// <summary>
   /// Update type
   /// </summary>
   [DefaultValue(UpdateSourceTrigger.Default)]
   public UpdateSourceTrigger UpdateSourceTrigger { get; set; }

   /// <summary>
   /// 要应用的转换器
   /// </summary>
   [DefaultValue(null)]
   public IValueConverter Converter { get; set; }

   /// <summary>
   /// 传递给转换器的参数。
   /// </summary>
   /// <value></value>
   [DefaultValue(null)]
   public object ConverterParameter { get; set; }

   /// <summary>
   /// 转换器info
   /// </summary>
   [DefaultValue(null)]
   [TypeConverter(typeof(System.Windows.CultureInfoIetfLanguageTagConverter))]
   public CultureInfo ConverterCulture { get; set; }

   /// <summary>
   /// 相对于目标元素，用作源的对象的描述。
   /// </summary>
   [DefaultValue(null)]
   public RelativeSource RelativeSource { get; set; }

   /// <summary>
   /// 用作源的元素的名称
   /// </summary>
   [DefaultValue(null)]
   public string ElementName { get; set; }

   #endregion

   #region BindingBase Members

   /// <summary>
   /// 当源无法提供值时使用的值
   /// </summary>
   /// <remarks>
   /// 已初始化为DependencyProperty。未设定值；如果未设置FallbackValue，则BindingExpression
   /// 当Binding无法获得实际值时，将返回目标属性的默认值。
   /// </remarks>
   public object FallbackValue { get; set; }

   #endregion

   #region Nested types

   private class HelperConverter : IMultiValueConverter
   {
      public static readonly HelperConverter Current = new HelperConverter();

      public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
      {
         var _rs = Tuple.Create(values[0], (DependencyProperty)parameter);
         return _rs;
      }

      public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
      {
         throw new NotImplementedException();
      }
   }

   #endregion
}
