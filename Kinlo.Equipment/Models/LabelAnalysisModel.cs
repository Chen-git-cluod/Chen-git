namespace Kinlo.Equipment.Models;

internal class LabelAnalysisModel
{
   public LabelAnalysisModel(string label, bool isArray, short[] index)
   {
      Label = label;
      IsArray = isArray;
      Index = index;
   }

   /// <summary>
   /// 标签
   /// </summary>
   public string Label { get; set; }

   /// <summary>
   /// 是否是数组
   /// </summary>
   public bool IsArray { get; set; }

   public short[] Index { get; set; }
}
