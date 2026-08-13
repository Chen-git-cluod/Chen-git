namespace Kinlo.Common.Tools;

public static class ObservableCollectionConverter
{
   public static ObservableCollection<TSource> ToObservableCollection<TSource>(this IEnumerable<TSource> source)
   {
      if (source == null)
      {
         throw new Exception("不能为空！");
      }

      return source is ObservableCollection<TSource> observableCollectionProvider
         ? observableCollectionProvider
         : new ObservableCollection<TSource>(source);
   }
}
