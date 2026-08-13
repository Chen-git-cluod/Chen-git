namespace Kinlo.Common.Converters;

/// <summary>
/// 在UI显示中反转Index
/// </summary>
public class ReverseIndexMultiConverter : IMultiValueConverter
{
   public static readonly ReverseIndexMultiConverter Instance = new ReverseIndexMultiConverter();

   private ReverseIndexMultiConverter() { }

   public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
   {
      if (values is null || values.Length < 3)
         return "?";

      if (values[0] is object item && values[1] is ListCollectionView listCollectionView && values[2] is bool isAscending)
      {
         int reverseIndex = listCollectionView.Count - listCollectionView.IndexOf(item);
         // int reverseIndex = listCollectionView.IndexOf(item) + 1;
         return isAscending
            ? (listCollectionView.IndexOf(item) + 1).ToString() //正序
            : reverseIndex.ToString(); // 倒序
      }
      return "0";
   }

   public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
   {
      throw new NotImplementedException();
   }
}
