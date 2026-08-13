using System.Dynamic;
using Dm.util;
using HandyControl.Controls;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Kinlo.Common.Tools;

public static class ExcelHelper
{
   /// <summary>
   /// 导出电池数据
   /// </summary>
   /// <param name="entitys"></param>
   /// <param name="otherParameter"></param>
   /// <param name="propertyBandings"></param>
   /// <param name="isOpen"></param>
   public static void ExportBattery(
      IEnumerable<ExpandoObject> entitys,
      OtherParameterConfig otherParameter,
      ObservableCollection<DisplayPropertyBindingDto>? propertyBandings = null,
      bool isOpen = true
   )
   {
      SaveFileDialog dlg = new SaveFileDialog();
      dlg.Filter = "Excel 工作簿(*.xlsx)|*.xlsx";
      dlg.FileName = DateTime.Now.ToString("电芯信息yyyy-MM-dd HH点mm分ss秒");
      if (dlg.ShowDialog() != true)
      {
         return;
      }
      entitys.ExportBattery(dlg.FileName, otherParameter, propertyBandings, isOpen);
   }

   /// <summary>
   /// 导出电池数据
   /// </summary>
   /// <param name="entieys"></param>
   /// <param name="savePath"></param>
   /// <param name="otherParameter"></param>
   /// <param name="propertyBandings"></param>
   /// <param name="isOpen"></param>
   public static bool ExportBattery(
      this IEnumerable<ExpandoObject> entieys,
      string savePath,
      OtherParameterConfig otherParameter,
      ObservableCollection<DisplayPropertyBindingDto>? propertyBandings = null,
      bool isOpen = true
   )
   {
      if (entieys.Count() < 1)
      {
         Growl.Warning("导出数据为0！");
         return false;
      }

      try
      {
         #region 语言弃用
         //string requestedCulture = $@"Languages\{otherParameter.OtherParameter.Language.Title}.xaml";
         //ResourceDictionary? _languageDictionary =
         // Application.Current.Resources.MergedDictionaries.FirstOrDefault(d =>
         // d.Source != null && d.Source.OriginalString.Equals(requestedCulture));
         #endregion

         var rows = entieys.ToList();
         string dirPath = Path.GetDirectoryName(savePath);
         if (!Directory.Exists(dirPath))
         {
            Directory.CreateDirectory(dirPath);
         }
         using XSSFWorkbook book = new();
         NPOI.SS.UserModel.ISheet sheet = book.CreateSheet("Sheet1");

         #region 颜色
         var redInnerCellStye = book.CreateCellStyle();
         redInnerCellStye.FillPattern = FillPattern.SolidForeground;
         redInnerCellStye.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Red.Index;

         var greenInnerCellStye = book.CreateCellStyle();
         greenInnerCellStye.FillPattern = FillPattern.SolidForeground;
         greenInnerCellStye.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Green.Index;
         #endregion

         //创建SHEET头
         var headerRow = sheet.CreateRow(0);

         if (propertyBandings != null)
         {
            int colIndex = 0;
            for (int i = 0; i < propertyBandings.Count; i++)
            {
               var _entityPropertyVisible = propertyBandings[i];
               if (_entityPropertyVisible.IsVisible)
               {
                  string description = _entityPropertyVisible.Description;
                  if (otherParameter.CurrentLanguagesDictionary.Contains(description))
                     description = otherParameter.CurrentLanguagesDictionary[_entityPropertyVisible.Description].ToString()!;

                  headerRow.CreateCell(colIndex).SetCellValue(description ?? _entityPropertyVisible.Description);
                  colIndex++;
               }
            }

            for (int i = 0; i < rows.Count; i++)
            {
               colIndex = 0;
               IDictionary<string, object> row = rows[i];
               var bodyRow = sheet.CreateRow(i + 1);
               foreach (var entityPropertyVisible in propertyBandings)
               {
                  var cell = bodyRow.CreateCell(colIndex);
                  string cellString = string.Empty;
                  object? value = null;
                  try
                  {
                     if (!entityPropertyVisible.IsVisible)
                        continue;
                     colIndex++;
                     //var vvv = _row.GetType().GetProperties();
                     //PropertyInfo? _propertyInfo = _row.GetType().GetProperty(entityPropertyVisible.BindingPaht);
                     // if (_propertyInfo == null) continue;
                     row.TryGetValue(entityPropertyVisible.BindingPaht, out value);

                     if (value is float f)
                     {
                        cellString = f.ToString("0.###", CultureInfo.InvariantCulture);
                     }
                     else if (value is double d)
                     {
                        cellString = d.ToString("0.###", CultureInfo.InvariantCulture);
                     }
                     else if (value is DateTime)
                     {
                        cellString = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
                     }
                     else if (entityPropertyVisible.PropertyType.Name == nameof(ResultTypeEnum))
                     {
                        var valueInt = (int)value;
                        if (valueInt > 10 && valueInt < 21)
                        {
                           cell.CellStyle = greenInnerCellStye;
                        }
                        else if (
                           valueInt > 20 /*(int)ResultTypeEnum.出站_未启用MES && valueInt != (int)ResultTypeEnum.进站_未启用MES*/
                        )
                        {
                           cell.CellStyle = redInnerCellStye;
                        }
                        cellString = ((ResultTypeEnum)(int)value).ToString();
                     }
                     else if (entityPropertyVisible.PropertyType.Name == nameof(ProcessTypeEnum))
                     {
                        cellString = ((ProcessTypeEnum)(int)value).ToString();
                     }
                     else if (entityPropertyVisible.BindingPaht.IsPressureValid()) //一注加压缸相关属性
                     {
                        if (
                           row.TryGetValue(nameof(BatTankModel.Func), out var func)
                           && func != null
                           && !string.IsNullOrEmpty(func.ToString())
                        )
                        {
                           cellString = value.ToString().PressursToString(func.ToString()!);
                        }
                        else
                        {
                           cellString = value.ToString();
                        }
                     }
                     else if (entityPropertyVisible.BindingPaht == nameof(BatTankModel.Func)) //一注加压缸功能
                     {
                        cellString = value.ToString().PressureFuncToString();
                     }
                     //else if (entityPropertyVisible.BindingPaht == nameof(BatMainModel.FinalStatus))
                     //{
                     //    //var _rs = _value.ToString().ResultDeserialize();
                     //    //if (_rs.Decide != ResultTypeEnum.合格)
                     //    //{
                     //    //    _cell.CellStyle = _redInnerCellStye;
                     //    //}
                     //    //else
                     //    //{
                     //    //    _cell.CellStyle = _greenInnerCellStye;
                     //    //}
                     //    //_cellString = _rs.ResultSerializeExprot();
                     //}
                     //else if (_value is Enum)
                     //{
                     //    val = ((int)_value).ToString();
                     //}
                     else
                     {
                        cellString = value?.ToString() ?? "";
                     }
                  }
                  catch (Exception e)
                  {
                     cellString = value == null ? "" : value.ToString();
                     // $"【导出Excel】值：[{value}]数据转换异常{e.Message}！".LogRun(Log4NetLevelEnum.错误);
                  }
                  cell.SetCellValue(cellString);
               }
            }
         }
         else
         {
            PropertyInfo[] _excelHeaderPropertyInfos = rows[0].GetType().GetProperties();
            List<int> reserveColumns = new List<int>();
            for (int i = 0; i < _excelHeaderPropertyInfos.Length; i++)
            {
               var excelHeaderPropertyInfo = _excelHeaderPropertyInfos[i];
               var att = excelHeaderPropertyInfo.GetCustomAttribute<SugarColumn>();
               if (att != null)
               {
                  if (propertyBandings != null)
                  {
                     var _EntityPropertyVisible = propertyBandings.FirstOrDefault(x => x.BindingPaht == excelHeaderPropertyInfo.Name);
                  }
               }
               // headerRow.CreateCell(i).SetCellValue(PropertyInfos[i].Description);
            }
         }

         // 写入文件
         using (var fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
         {
            book.Write(fs);
         }

         Growl.Success($"{savePath} 保存成功");
         $"【导出Excel】{savePath}完成！".LogRun();
         if (isOpen)
            Process.Start("explorer.exe", $"/select,\"{savePath}\""); //打开

         return true;
      }
      catch (Exception ex)
      {
         Growl.Warning($"导出Excel异常：{ex.Message}");
         $"【导出Excel】{ex.Message}完成！".LogRun(Log4NetLevelEnum.错误);
         return false;
      }
   }

   /// <summary>
   /// 通用导出Excel
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="entities"></param>
   /// <param name="path"></param>
   /// <exception cref="ArgumentNullException"></exception>
   /// <exception cref="ArgumentException"></exception>
   public static bool ExportExcel<T>(this IEnumerable<T> entities, string path, bool isShowDialog)
      where T : new()
   {
      try
      {
         if (entities == null)
            throw new ArgumentNullException(nameof(entities));
         if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("路径不能为空", nameof(path));

         string? dirPath = Path.GetDirectoryName(path);
         if (!string.IsNullOrEmpty(dirPath))
            Directory.CreateDirectory(dirPath);

         var properties = typeof(T).GetProperties();

         using HSSFWorkbook book = new();
         ISheet sheet = book.CreateSheet("Sheet1");

         //创建SHEET头
         var headerRow = sheet.CreateRow(0);
         for (int i = 0; i < properties.Length; i++)
         {
            var attr = properties[i].GetCustomAttribute<LanguagesAttribute>();
            string name = attr?.Languages.FirstOrDefault() ?? properties[i].Name;
            headerRow.CreateCell(i).SetCellValue(name);
         }

         // 填充数据
         int rowIndex = 1;
         foreach (var entity in entities)
         {
            var excelRow = sheet.CreateRow(rowIndex++);
            for (int colIndex = 0; colIndex < properties.Length; colIndex++)
            {
               var value = properties[colIndex].GetValue(entity)?.ToString();
               excelRow.CreateCell(colIndex).SetCellValue(value ?? string.Empty);
            }
         }

         // 写入文件
         using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
         book.Write(fs);
         if (isShowDialog)
            Growl.Success($"{path} 保存成功");
         $"【导出Excel】{path}完成！".LogRun();
         return true;
      }
      catch (Exception ex)
      {
         Growl.ErrorGlobal(ex.ToString());
         return false;
      }
   }

   /// <summary>
   /// 通用导出Excel
   /// </summary>
   /// <param name="entities"></param>
   /// <param name="path"></param>
   /// <exception cref="ArgumentNullException"></exception>
   /// <exception cref="ArgumentException"></exception>
   public static bool ExportExcel(this IEnumerable<object> entities, string path, bool isShowDialog)
   {
      try
      {
         if (entities == null)
            throw new ArgumentNullException(nameof(entities));
         if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("路径不能为空", nameof(path));

         if (entities.Count() == 0)
         {
            Growl.Warning($"无数据退出！");
            return false;
         }

         string? dirPath = Path.GetDirectoryName(path);
         if (!string.IsNullOrEmpty(dirPath))
            Directory.CreateDirectory(dirPath);

         var obj = entities.First();
         var properties = obj.GetType().GetProperties();

         using HSSFWorkbook book = new();
         ISheet sheet = book.CreateSheet("Sheet1");

         //创建SHEET头
         var headerRow = sheet.CreateRow(0);
         for (int i = 0; i < properties.Length; i++)
         {
            var attr = properties[i].GetCustomAttribute<LanguagesAttribute>();
            string name = attr?.Languages.FirstOrDefault() ?? properties[i].Name;
            headerRow.CreateCell(i).SetCellValue(name);
         }

         List<string> errs = new List<string>();
         // 填充数据
         int rowIndex = 1;
         foreach (var entity in entities)
         {
            var excelRow = sheet.CreateRow(rowIndex++);
            for (int colIndex = 0; colIndex < properties.Length; colIndex++)
            {
               try
               {
                  if (properties[colIndex].Name == "Item")
                     continue;
                  var value = properties[colIndex].GetValue(entity)?.ToString();
                  excelRow.CreateCell(colIndex).SetCellValue(value ?? string.Empty);
               }
               catch (Exception ex)
               {
                  errs.Add(properties[colIndex].Name);
               }
            }
         }

         // 写入文件
         using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
         book.Write(fs);
         if (isShowDialog)
            Growl.Success($"{path} 保存成功");
         $"【导出Excel】{path}完成！".LogRun();
         return true;
      }
      catch (Exception ex)
      {
         Growl.ErrorGlobal(ex.ToString());
         return false;
      }
   }

   /// <summary>
   ///  通用导入EXCEL
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="filePath"></param>
   /// <returns></returns>
   public static IEnumerable<T> ImproExcel<T>(string filePath)
      where T : new()
   {
      List<T> datas = new List<T>();
      try
      {
         using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
         using IWorkbook workbook = Path.GetExtension(filePath).ToLower() switch
         {
            ".xlsx" => new XSSFWorkbook(fs), // 2007+
            ".xls" => new HSSFWorkbook(fs), // 2003
            _ => throw new NotSupportedException("文件格式错误！"),
         };

         var sheet = workbook.GetSheetAt(0);
         if (sheet == null)
            return datas;

         var headerRow = sheet.GetRow(0);
         if (headerRow == null)
            return datas;

         List<int> colIndexMap = new List<int>(); //列对应的位置
         var properties = typeof(T).GetProperties();

         for (int i = 0; i < properties.Length; i++)
         {
            var batAtt = properties[i].GetCustomAttribute<LanguagesAttribute>();
            string name = batAtt switch
            {
               null => properties[i].Name,
               _ => batAtt.Languages[0],
            };
            int colIndex = -1;
            for (int c = 0; c < headerRow.LastCellNum; c++)
            {
               if (headerRow.GetCell(c).StringCellValue == name)
               {
                  colIndex = c;
                  break;
               }
            }
            colIndexMap.Add(colIndex);
         }
         for (int r = 1; r < sheet.PhysicalNumberOfRows; r++)
         {
            var row = sheet.GetRow(r);
            if (row == null)
               continue;

            T entity = new T();
            for (int c = 0; c < colIndexMap.Count; c++)
            {
               if (colIndexMap[c] < 0)
                  continue;
               var cell = row.GetCell(colIndexMap[c]);
               string? val = GetCellValue(cell);
               var prop = properties[c];

               object? safeValue = prop.PropertyType switch
               {
                  Type t when t == typeof(int) => int.TryParse(val, out var v) ? v : 0,

                  Type t when t.IsEnum => Enum.TryParse(t, val, out var enumVal) ? enumVal : Activator.CreateInstance(t),

                  Type t when t == typeof(double) => double.TryParse(val, out var v) ? v : 0d,

                  Type t when t == typeof(float) => float.TryParse(val, out var v) ? v : 0f,

                  Type t when t == typeof(ushort) => ushort.TryParse(val, out var v) ? v : (ushort)0,

                  Type t when t == typeof(bool) => bool.TryParse(val, out var v) ? v : (val == "1" || val?.ToLower() == "true"),

                  Type t when t == typeof(DateTime) => DateTime.TryParse(val, out var v) ? v : DateTime.Now,

                  _ => val,
               };

               prop.SetValue(entity, safeValue);
            }
            datas.Add(entity);
         }
      }
      catch (Exception ex)
      {
         Growl.ErrorGlobal(ex.ToString());
      }
      return datas;
   }

   public static string? GetCellValue(NPOI.SS.UserModel.ICell cell)
   {
      try
      {
         if (cell == null)
            return string.Empty;
         return cell.CellType switch
         {
            var cType when cType == CellType.String => cell.StringCellValue,

            var cType when cType == CellType.Numeric => cell.CellStyle.DataFormat switch //对时间格式（2015.12.5、2015/12/5、2015-12-5等）的处理
            {
               var df when df == 14 || df == 31 || df == 57 || df == 58 => cell.DateCellValue?.ToString(),
               var df when df == 177 || df == 178 || df == 188 => cell.NumericCellValue.ToString("#0.00"),
               _ => cell.NumericCellValue.ToString(),
            },

            var cType when cType == CellType.Formula => cell.CellStyle.DataFormat switch
            {
               var df when df == 177 || df == 178 || df == 188 => cell.NumericCellValue.ToString("#0.00"),
               _ => cell.NumericCellValue.ToString(),
            },

            var cType when cType == CellType.Blank || cType == CellType.Error => string.Empty,

            _ => cell.StringCellValue,
         };
      }
      catch (Exception ex)
      {
         $"[单元格取值]出现异常：{ex}".LogRun(Log4NetLevelEnum.错误);
         return string.Empty;
      }
   }

   /// <summary>
   /// 从文件夹中获取第一个Excel文件并处理，支持".xlsx"及".xls"
   /// </summary>
   /// <param name="directory"></param>
   /// <param name="func"></param>
   /// <returns></returns>
   public static async Task<bool> UseFirstExcelFromDirectoryAsync(this string directory, Func<IWorkbook, Task> func)
   {
      if (!Directory.Exists(directory))
      {
         Directory.CreateDirectory(directory);
         $"无此目录：[{directory}]，已创建;".LogRun(Log4NetLevelEnum.警告, false);
         return false;
      }

      var allowedExtensions = new[] { ".xlsx", ".xls" };
      // 注意 只取第一个找到的文件
      var filePath = Directory
         .EnumerateFiles(directory, "*.*", SearchOption.AllDirectories)
         .FirstOrDefault(f => allowedExtensions.Contains(Path.GetExtension(f).ToLower()));

      if (filePath == null)
      {
         $"目录：[{directory}]下无EXCEL文件;".LogRun(Log4NetLevelEnum.警告, false);
         return false;
      }

      try
      {
         //  FileShare.ReadWrite 保证文件被打开时也能读
         using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

         // 将数据拷贝到内存流，立即释放文件句柄（也可以不拷贝到内存流，主要为了立即释放句柄）
         using var ms = new MemoryStream();
         await fs.CopyToAsync(ms);
         ms.Position = 0;

         // WorkbookFactory会自动识别版本并创建
         using var workbook = WorkbookFactory.Create(ms);

         if (workbook == null)
         {
            $"文件：[{filePath}] 解析失败（Workbook 为空）;".LogRun(Log4NetLevelEnum.警告, false);
            return false;
         }

         if (func != null)
            await func(workbook);

         return true;
      }
      catch (Exception ex)
      {
         $"处理Excel [{filePath}] 时发生异常: {ex.Message}".LogRun(Log4NetLevelEnum.错误);
         return false;
      }
   }
}
