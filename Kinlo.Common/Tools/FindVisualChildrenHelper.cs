namespace Kinlo.Common.Tools;

public class FindVisualChildrenHelper
{
   /// <summary>
   /// 返回集合内ItemTemplate内控件，例如ItemsControl内
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="depObj"></param>
   /// <returns></returns>
   public static List<T> FindVisualChildren<T>(DependencyObject depObj)
      where T : DependencyObject
   {
      List<T> list = new List<T>();
      if (depObj != null)
      {
         for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
         {
            DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
            if (child != null && child is T)
            {
               list.Add((T)child);
            }

            List<T> childItems = FindVisualChildren<T>(child);
            if (childItems != null && childItems.Count() > 0)
            {
               foreach (var item in childItems)
               {
                  list.Add(item);
               }
            }
         }
      }
      return list;
   }
}
