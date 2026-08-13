namespace Kinlo.Common.Tools;

public static class CreateControlHelper
{
   #region ClockPicker
   public static ClockPicker CreateClockPicher(
      string bindingText,
      string editlLevelBinding,
      ControlInfoModel controlInfo,
      double[]? margin = null
   )
   {
      if (margin == null)
         margin = [5];
      ClockPicker _clockPicker = new();
      _clockPicker.VerticalAlignment = VerticalAlignment.Center;
      _clockPicker.Margin = margin.ToThickness();
      //_textBox.BorderThickness = new Thickness(1);
      _clockPicker.SetResourceReference(ClockPicker.TitleProperty, controlInfo.DisplayName);
      _clockPicker.SetValue(ClockPicker.HorizontalContentAlignmentProperty, HorizontalAlignment.Left);
      _clockPicker.SetValue(ClockPicker.VerticalContentAlignmentProperty, VerticalAlignment.Center);
      _clockPicker.SetValue(ClockPicker.BorderBrushProperty, Application.Current.FindResource("BorderBrush"));
      _clockPicker.SetValue(ClockPicker.IconBackgroundProperty, Application.Current.FindResource("PrimaryBrush"));
      _clockPicker.SetValue(ClockPicker.BorderThicknessProperty, new Thickness(1));
      _clockPicker.SetValue(ClockPicker.PaddingProperty, new Thickness(5, 0, 2, 0));
      _clockPicker.SetValue(ClockPicker.TitleWidthProperty, new GridLength(120d));
      _clockPicker.SetBinding(
         ClockPicker.TimeProperty,
         new Binding()
         {
            Path = new PropertyPath(bindingText),
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
         }
      );

      _clockPicker.SetBinding(ClockPicker.IsEnabledProperty, CreateLevelToBoolMultiConverter(bindingText, editlLevelBinding));

      return _clockPicker;
   }
   #endregion

   #region TextBox
   public static TextBox CreateTextBox(
      string bindingText,
      string editlLevelBinding,
      ControlInfoModel controlInfo,
      double titleWidth,
      double[]? margin = null
   )
   {
      if (margin == null)
         margin = [5];
      HCC.TextBox textBox = new HCC.TextBox();
      textBox.VerticalAlignment = VerticalAlignment.Center;
      textBox.Margin = margin.ToThickness();
      //_textBox.BorderThickness = new Thickness(1);
      textBox.SetResourceReference(HCC.TitleElement.TitleProperty, controlInfo.DisplayName);
      // _textBox.SetValue(HCC.TitleElement.TitleProperty, controlInfo.DisplayName);
      textBox.SetValue(HCC.TitleElement.TitlePlacementProperty, HC.Data.TitlePlacementType.Left);
      textBox.SetValue(HCC.TitleElement.TitleWidthProperty, new GridLength(titleWidth));
      // textBox.SetValue(HCC.TitleElement.TitleWidthProperty, new GridLength(120d));
      textBox.SetBinding(
         HCC.TextBox.TextProperty,
         new Binding()
         {
            Path = new PropertyPath(bindingText),
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
         }
      );

      textBox.SetBinding(HCC.TextBox.IsEnabledProperty, CreateLevelToBoolMultiConverter(bindingText, editlLevelBinding));
      textBox.SetBinding(
         HCC.TextBox.VisibilityProperty,
         new Binding
         {
            Converter = new ProductionTypeToVIsibilityConverter(),
            Path = new PropertyPath($"Parameter.AdvancedConfig.ProductionType"),
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            ConverterParameter = controlInfo.ProductVisibility,
         }
      );
      return textBox;
   }
   #endregion

   #region Careate ComboBox
   public static ComboBox CreateComboBox(
      string bindingText,
      string editlLevelBinding,
      ControlInfoModel controlInfo,
      Type type,
      double titleWidth,
      double[]? margin = null,
      bool isdynamic = false
   )
   {
      if (margin == null)
         margin = [5];
      HCC.ComboBox comboBox = new HCC.ComboBox();
      comboBox.VerticalAlignment = VerticalAlignment.Center;
      comboBox.Margin = margin.ToThickness();
      comboBox.SetResourceReference(HCC.TitleElement.TitleProperty, controlInfo.DisplayName);
      //_comboBox.SetValue(HCC.TitleElement.TitleProperty, controlInfo.DisplayName);
      comboBox.SetValue(HCC.TitleElement.TitlePlacementProperty, HC.Data.TitlePlacementType.Left);
      comboBox.SetValue(HCC.TitleElement.TitleWidthProperty, new GridLength(titleWidth));

      comboBox.ItemsSource = type.GetEnumValues();
      comboBox.SetBinding(
         HCC.ComboBox.TextProperty,
         new Binding()
         {
            Path = new PropertyPath(bindingText),
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
         }
      );

      comboBox.SetBinding(HCC.ComboBox.IsEnabledProperty, CreateLevelToBoolMultiConverter(bindingText, editlLevelBinding));
      comboBox.SetBinding(
         HCC.TextBox.VisibilityProperty,
         new Binding
         {
            Converter = new ProductionTypeToVIsibilityConverter(),
            Path = new PropertyPath($"Parameter.AdvancedConfig.ProductionType"),
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            ConverterParameter = controlInfo.ProductVisibility,
         }
      );

      string xaml =
         $@"
<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
 xmlns:tools='clr-namespace:Kinlo.Common.Tools;assembly=Kinlo.Common'> 
    <TextBlock Text='{{tools:ResourceBinding .}}' />
</DataTemplate>";
      var template = (DataTemplate)XamlReader.Parse(xaml);
      comboBox.ItemTemplate = template;
      return comboBox;
   }
   #endregion

   #region CheckBox
   public static CheckBox CreateCheckBox(
      string bindingText,
      string editlLevelBinding,
      ControlInfoModel controlInfo,
      double[]? margin = null
   )
   {
      if (margin == null)
         margin = [5, 10, 5, 10];
      System.Windows.Controls.CheckBox checkBox = new System.Windows.Controls.CheckBox();
      checkBox.VerticalAlignment = VerticalAlignment.Center;
      checkBox.Margin = margin.ToThickness();
      checkBox.SetResourceReference(CheckBox.ContentProperty, controlInfo.DisplayName);
      //_checkBox.SetValue(CheckBox.ContentProperty, controlInfo.DisplayName);
      checkBox.SetBinding(
         CheckBox.IsCheckedProperty,
         new Binding()
         {
            Path = new PropertyPath(bindingText),
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
         }
      );

      checkBox.SetBinding(CheckBox.IsEnabledProperty, CreateLevelToBoolMultiConverter(bindingText, editlLevelBinding));
      checkBox.SetBinding(
         HCC.TextBox.VisibilityProperty,
         new Binding
         {
            Converter = new ProductionTypeToVIsibilityConverter(),
            Path = new PropertyPath($"Parameter.AdvancedConfig.ProductionType"),
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            ConverterParameter = controlInfo.ProductVisibility,
         }
      );
      return checkBox;
   }
   #endregion

   #region CreateListView

   /// <summary>
   ///
   /// </summary>
   /// <param name="bindingItemsSource">绑定路径</param>
   /// <param name="entityPropertys"></param>
   /// <param name="isIntToEnum">是否为Int转换Enum实际值</param>
   /// <param name="isNoAscending">序号是否正序显示</param>
   /// <returns></returns>
   public static ListView CreateListView(
      string bindingItemsSource,
      IEnumerable<DisplayPropertyBindingDto> entityPropertys,
      bool isIntToEnum,
      bool isNoAscending
   )
   {
      try
      {
         GridView gridView = new GridView();

         gridView.Columns.Add(CreateNoColumn(isNoAscending)); //添加序列号

         foreach (var property in entityPropertys)
         {
            if (!property.IsVisible)
               continue;
            FrameworkElementFactory _headerText = new FrameworkElementFactory(typeof(TextBlock));
            _headerText.SetResourceReference(TextBlock.TextProperty, property.Description);
            var _headerDataTemplate = new DataTemplate();
            _headerDataTemplate.VisualTree = _headerText;

            if (property.PropertyType.Name == nameof(ResultTypeEnum))
            {
               var dataTemplate = new DataTemplate();
               dataTemplate.VisualTree = CreateResultGridViewColumn(isIntToEnum, property.BindingPaht);

               var col = new GridViewColumn();
               col.HeaderTemplate = _headerDataTemplate;
               col.CellTemplate = dataTemplate;
               gridView.Columns.Add(col);
            }
            else if (property.BindingPaht.IsPressureValid()) //是否为加压缸相关属性
            {
               Binding binding = new Binding();
               binding.Path = new PropertyPath(property.BindingPaht);
               binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
               binding.Mode = BindingMode.OneWay;

               MultiBinding multiBinding = new MultiBinding { Converter = new PressureDisplayMultiConverter() };
               multiBinding.Bindings.Add(
                  new Binding
                  {
                     Path = new PropertyPath(property.BindingPaht),
                     Mode = BindingMode.OneWay,
                     UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                  }
               );
               multiBinding.Bindings.Add(
                  new Binding
                  {
                     Path = new PropertyPath(nameof(BatTankModel.Func)),
                     Mode = BindingMode.OneWay,
                     UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                  }
               );

               var col = new GridViewColumn { DisplayMemberBinding = multiBinding };
               col.HeaderTemplate = _headerDataTemplate;
               gridView.Columns.Add(col);
            }
            else if (property.BindingPaht is nameof(BatMainModel.Id) or nameof(BatMainModel.Barcode))
            {
               var dataTemplate = new DataTemplate();
               dataTemplate.VisualTree = CreateEditGridViewColumn(property.BindingPaht);

               var col = new GridViewColumn();
               col.HeaderTemplate = _headerDataTemplate;
               col.CellTemplate = dataTemplate;
               gridView.Columns.Add(col);
            }
            else
            {
               Binding binding = new Binding();
               binding.Path = new PropertyPath(property.BindingPaht);
               binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
               binding.Mode = BindingMode.OneWay;

               if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
               {
                  binding.Converter = new DateTimeDisplayConverter();
               }
               else if (property.PropertyType == typeof(ProcessTypeEnum))
               {
                  binding.Converter = new IntToProcessesConverter();
               }
               else if (
                  property.PropertyType == typeof(double)
                  || property.PropertyType == typeof(double?)
                  || property.PropertyType == typeof(float)
                  || property.PropertyType == typeof(float?)
               )
               {
                  binding.StringFormat = property.BindingPaht is nameof(BatVoltageTestModel.TestVoltageValue) ? "F5" : "F3"; //电压值保留5位小数
               }
               else if (property.BindingPaht is nameof(BatTankModel.Func)) //加压缸功能
               {
                  binding.Converter = new PressureFuncDisplayConverter();
               }

               var col = new GridViewColumn { DisplayMemberBinding = binding };
               col.HeaderTemplate = _headerDataTemplate;
               gridView.Columns.Add(col);
            }
         }

         ListView listView = new System.Windows.Controls.ListView();
         listView.SetBinding(ListView.ItemsSourceProperty, new Binding(bindingItemsSource));
         listView.Margin = new Thickness(0, -10, 0, 0);
         listView.Background = new SolidColorBrush(Colors.Transparent);
         Style _dataGridStyle = (Style)Application.Current.FindResource("MainDisplayListViewItemStyle");
         listView.ItemContainerStyle = _dataGridStyle;
         listView.View = gridView;
         listView.BorderThickness = new Thickness(0);
         //开启 UI 虚拟化（默认其实已经是 True，但显式声明最稳妥）
         VirtualizingStackPanel.SetIsVirtualizing(listView, true);

         return listView;
      }
      catch (Exception ex)
      {
         $"创建ListView异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
      return new System.Windows.Controls.ListView();
   }

   /// <summary>
   /// 创建序列
   /// </summary>
   /// <returns></returns>
   private static GridViewColumn CreateNoColumn(bool isAscending)
   {
      GridViewColumn colNo = new GridViewColumn();
      colNo.Header = "No.";

      DataTemplate cellTemplate = new DataTemplate();
      FrameworkElementFactory textBlock = new FrameworkElementFactory(typeof(TextBlock));
      MultiBinding multiBinding = new MultiBinding
      {
         Converter = ReverseIndexMultiConverter.Instance,
         //  StringFormat = "{}{0}." // 例如：1.
      };
      multiBinding.Bindings.Add(new Binding());

      multiBinding.Bindings.Add(new Binding("ItemsSource") { RelativeSource = new RelativeSource { AncestorType = typeof(ListView) } });
      multiBinding.Bindings.Add(new Binding() { Source = isAscending });
      textBlock.SetBinding(TextBlock.TextProperty, multiBinding);
      textBlock.SetValue(TextBlock.WidthProperty, 30.0);
      textBlock.SetValue(TextBox.HorizontalAlignmentProperty, HorizontalAlignment.Center);
      textBlock.SetValue(TextBox.VerticalAlignmentProperty, VerticalAlignment.Center);

      // 设置模板
      cellTemplate.VisualTree = textBlock;
      colNo.CellTemplate = cellTemplate;
      return colNo;
   }

   /// <summary>
   ///
   /// </summary>
   /// <param name="bindingPaht"></param>
   /// <returns></returns>
   private static FrameworkElementFactory CreateEditGridViewColumn(string bindingPaht)
   {
      FrameworkElementFactory text = new FrameworkElementFactory(typeof(TextBox));
      text.SetValue(TextBox.MarginProperty, new Thickness(5, 4, 5, 4));
      text.SetValue(TextBox.IsReadOnlyProperty, true);
      text.SetValue(TextBox.BorderThicknessProperty, new Thickness(0, 0, 0, 0));
      text.SetValue(TextBox.HorizontalAlignmentProperty, HorizontalAlignment.Center);
      text.SetValue(TextBox.VerticalAlignmentProperty, VerticalAlignment.Center);
      text.SetBinding(
         TextBox.TextProperty,
         new Binding
         {
            Path = new System.Windows.PropertyPath(bindingPaht),
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            Mode = BindingMode.OneWay,
         }
      );
      return text;
   }

   /// <summary>
   /// 结果Border
   /// </summary>
   /// <param name="isIntToResult"></param>
   /// <param name="bindingPaht"></param>
   /// <param name="process"></param>
   /// <returns></returns>
   private static FrameworkElementFactory CreateResultGridViewColumn(bool isIntToResult, string bindingPaht, string process = "")
   {
      FrameworkElementFactory border = new FrameworkElementFactory(typeof(Border));
      border.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
      border.SetValue(Border.HorizontalAlignmentProperty, HorizontalAlignment.Center);
      border.SetValue(Border.VerticalAlignmentProperty, VerticalAlignment.Stretch);
      border.SetBinding(
         Border.BackgroundProperty,
         new Binding
         {
            Path = new System.Windows.PropertyPath(bindingPaht),
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            Mode = BindingMode.OneWay,
            Converter = new ResultToBrushConverter(),
         }
      );

      FrameworkElementFactory _text = new FrameworkElementFactory(typeof(TextBlock));
      _text.SetValue(TextBlock.MarginProperty, new Thickness(5, 4, 5, 4));
      _text.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Colors.White));
      _text.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
      _text.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);

      if (string.IsNullOrEmpty(process))
      {
         Binding _bindingTxtConverter = new Binding
         {
            Path = new System.Windows.PropertyPath(bindingPaht),
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            Mode = BindingMode.OneWay,
         };
         if (isIntToResult)
            _bindingTxtConverter.Converter = new IntToResultConverter();
         _text.SetBinding(TextBlock.TextProperty, _bindingTxtConverter);
      }
      else
      {
         MultiBinding multiBinding = new MultiBinding { Converter = new ResultInfoToStringMultiConverter() };
         multiBinding.Bindings.Add(
            new Binding
            {
               Path = new PropertyPath(process),
               Mode = BindingMode.OneWay,
               UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            }
         );
         multiBinding.Bindings.Add(
            new Binding
            {
               Path = new PropertyPath(bindingPaht),
               Mode = BindingMode.OneWay,
               UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            }
         );
         //_multiBinding.Bindings.Add(new Binding
         //{
         //    //RelativeSource = new RelativeSource {Mode =RelativeSourceMode.TemplatedParent}  ,
         //    RelativeSource = new RelativeSource { AncestorType = typeof(Grid) },
         //    Path = new System.Windows.PropertyPath("DataContext.Processes"),
         //    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
         //    Mode = BindingMode.OneWay,
         //});
         _text.SetBinding(TextBlock.TextProperty, multiBinding);
      }
      border.AppendChild(_text);
      return border;
   }
   #endregion

   #region Create DataGrid
   public static DataGrid CreateDataGrid(
      string bindingItemsSource,
      IEnumerable<DisplayPropertyBindingDto> entityProperty,
      bool isResultTxtConverter
   )
   {
      DataGrid _dataGrid = CreateDataGrid(bindingItemsSource);
      _dataGrid.EnableColumnVirtualization = true;
      _dataGrid.EnableRowVirtualization = true;
      foreach (var property in entityProperty)
      {
         if (!property.IsVisible)
            continue;
         var _col = CreateDataGridTextColumn(property.Description, property.BindingPaht, property.PropertyType, isResultTxtConverter);
         _dataGrid.Columns.Add(_col);
      }
      return _dataGrid;
   }

   public static void DataGrid_ColumnDisplayIndexChanged(object sender, DataGridColumnEventArgs e) { }

   public static DataGrid CreateDataGrid(string bindingItemsSource)
   {
      DataGrid _dataGrid = new DataGrid
      {
         IsReadOnly = true,
         Padding = new Thickness(5, 0, 5, 5),
         BorderThickness = new Thickness(0),
         AutoGenerateColumns = false,
         ColumnHeaderHeight = 25d,
         RowHeight = 28d,
         Background = new SolidColorBrush(Colors.Transparent),
         CanUserReorderColumns = true,
      };

      // MarkupExtension action = new Stylet.Xaml.ActionExtension("DataGrid_ColumnDisplayIndexChanged");
      //Stylet.Xaml.ActionExtension action = new Stylet.Xaml.ActionExtension("ColumnDisplayIndexChanged");
      //action.Target =  _dataGrid;
      //_dataGrid.ColumnDisplayIndexChanged += action.;

      Style _dataGridStyle = (Style)Application.Current.FindResource("DataGrid.Small");
      Style _mainDisplayDataGridRowStyle = (Style)Application.Current.FindResource("MainDisplayDataGridRowStyle");
      _dataGrid.Style = _dataGridStyle;
      _dataGrid.RowStyle = _mainDisplayDataGridRowStyle;
      _dataGrid.SetValue(HCC.BorderElement.CornerRadiusProperty, new CornerRadius(0, 0, 0, 0));
      _dataGrid.SetValue(HCC.DataGridAttach.ShowRowNumberProperty, true);
      _dataGrid.SetBinding(
         DataGrid.ItemsSourceProperty,
         new Binding
         {
            Path = new PropertyPath(bindingItemsSource),
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            Mode = BindingMode.TwoWay,
         }
      ); //UpdateSourceTrigger
      return _dataGrid;
   }

   private static DataGridColumn CreateDataGridTextColumn(
      string description,
      string bindingPaht,
      Type propertyType,
      bool isResultTxtConverter
   )
   {
      FrameworkElementFactory _headerText = new FrameworkElementFactory(typeof(TextBlock));
      _headerText.SetResourceReference(TextBlock.TextProperty, description);
      var _headerDataTemplate = new DataTemplate();
      _headerDataTemplate.VisualTree = _headerText;

      if (propertyType.Name == nameof(ResultTypeEnum))
      {
         FrameworkElementFactory _border = new FrameworkElementFactory(typeof(Border));
         _border.SetValue(Border.CornerRadiusProperty, new CornerRadius(3));
         _border.SetValue(Border.VerticalAlignmentProperty, VerticalAlignment.Center);
         _border.SetBinding(
            Border.BackgroundProperty,
            new Binding
            {
               Path = new System.Windows.PropertyPath(bindingPaht),
               UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
               Mode = BindingMode.TwoWay,
               Converter = new ResultToBrushConverter(),
            }
         );

         FrameworkElementFactory _text = new FrameworkElementFactory(typeof(TextBlock));
         _text.SetValue(TextBlock.MarginProperty, new Thickness(0, 4, 0, 4));
         _text.SetValue(TextBlock.ForegroundProperty, new SolidColorBrush(Colors.White));
         _text.SetValue(TextBlock.HorizontalAlignmentProperty, HorizontalAlignment.Center);
         _text.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
         Binding _bindingTxtConverter = new Binding
         {
            Path = new System.Windows.PropertyPath(bindingPaht),
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            Mode = BindingMode.TwoWay,
         };
         if (isResultTxtConverter)
            _bindingTxtConverter.Converter = new IntToResultConverter();
         _text.SetBinding(TextBlock.TextProperty, _bindingTxtConverter);

         _border.AppendChild(_text);
         var dataTemplate = new DataTemplate();
         dataTemplate.VisualTree = _border;

         var _colTemplate = new DataGridTemplateColumn();
         _colTemplate.HeaderTemplate = _headerDataTemplate;
         _colTemplate.CellTemplate = dataTemplate;

         return _colTemplate;
      }
      else
      {
         Binding _binding = new Binding();
         _binding.Path = new PropertyPath(bindingPaht);
         _binding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
         _binding.Mode = BindingMode.TwoWay;
         _binding.StringFormat = propertyType switch
         {
            Type t when t == typeof(DateTime) || t == typeof(DateTime?) => "MM-dd HH:mm:ss",
            Type t when t == typeof(double) || t == typeof(double?) || t == typeof(float) || t == typeof(float?) => "F2",
            _ => default,
         };
         //展示阶段
         if (bindingPaht == "StageResult")
            _binding.Converter = new MultipleResultDisplayConverter();
         else if (bindingPaht == "Stage")
            _binding.Converter = new MultipleProcessDisplayConverter();

         var _col = new DataGridTextColumn { Binding = _binding };
         _col.HeaderTemplate = _headerDataTemplate;
         return _col;
      }
   }
   #endregion

   static Thickness ToThickness(this double[]? margin)
   {
      return margin switch
      {
         var d when d == null || d.Length == 0 => new Thickness(5),
         var d when d.Length == 1 => new Thickness(d[0]),
         var d when d.Length == 2 || d.Length == 3 => new Thickness(d[0], d[1], d[0], d[1]),
         var d when d.Length == 4 => new Thickness(d[0], d[1], d[2], d[3]),
         _ => new Thickness(5),
      };
   }

   private static MultiBinding CreateLevelToBoolMultiConverter(string bindingText, string editlLevelBinding) =>
      new MultiBinding
      {
         Converter = new LevelToBoolMultiConverter(),
         Bindings =
         {
            new Binding()
            {
               RelativeSource = new RelativeSource { AncestorType = typeof(UserControl) },
               Path = new PropertyPath("DataContext.UsersStatus.LocalLoggedinUser.Level"),
            },
            new Binding(editlLevelBinding),
            new Binding()
            {
               RelativeSource = new RelativeSource { AncestorType = typeof(UserControl) },
               Path = new PropertyPath("DataContext.Role.InterlockState"),
            },
            new Binding()
            {
               RelativeSource = new RelativeSource { AncestorType = typeof(UserControl) },
               Path = new PropertyPath("DataContext.Role.InterlockState.Version"),
            },
         },
         ConverterParameter = bindingText,
      };
}
