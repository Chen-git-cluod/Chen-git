using Kinlo.Common.Models.OhtenModels;

namespace Kinlo.GUI.ViewModel;

public class GasConcentrationViewModel : Screen
{
   /// <summary>
   /// 开始时间
   /// </summary>
   public DateTime StartTime { get; set; }

   /// <summary>
   /// 结束时间
   /// </summary>
   public DateTime EndTime { get; set; }
   public ConcentrationPositionEnum SelectedPosition { get; set; }
   public IEnumerable<ConcentrationPositionEnum> Positions
   {
      get
      {
         foreach (var position in Enum.GetValues(typeof(ConcentrationPositionEnum)))
         {
            yield return (ConcentrationPositionEnum)position;
         }
      }
   }
   public List<GasConcentrationModel> GasConcentratios { get; set; } = new();
   private DbHelper _db;

   public GasConcentrationViewModel(IContainer container)
   {
      EndTime = DateTime.Now;
      StartTime = EndTime.AddDays(-1);
      _db = container.Get<DbHelper>();

      #region 加测试数据
      //Task.Run(async () =>
      //{
      //    Random random = new Random();
      //    for (int k = 0; k < 1000; k++)
      //    {

      //        GasConcentrationModel gasConcentration = new GasConcentrationModel();
      //        gasConcentration.Id = container.Get<SnowflakeHelper>().NextId();
      //        gasConcentration.Concentration = random.Next(188000, 189000) / 100d;
      //        gasConcentration.Position = k switch
      //        {
      //            var ii when ii % 3 == 0 => ConcentrationPositionEnum.注液站,
      //            var ii when ii % 3 == 1 => ConcentrationPositionEnum.储液柜,
      //            _ => ConcentrationPositionEnum.补液站
      //        };
      //        if (!await _db.InsertableAsync(gasConcentration, gasConcentration.Id, "气体浓度"))
      //        {
      //            $"Id:[{gasConcentration.Id}]插入数据失败".LogRun(Log4NetLevelEnum.错误);
      //        }

      //    }
      //});
      #endregion
   }

   public async Task QueryCMD()
   {
      try
      {
         string exp =
            SelectedPosition == ConcentrationPositionEnum.全部
               ? string.Empty
               : $"{nameof(GasConcentrationModel.Position)}={(int)SelectedPosition}";
         GasConcentratios = await _db.GetDatasByInputTimeRangeAsync<GasConcentrationModel>(StartTime, EndTime, exp);
      }
      catch (Exception ex)
      {
         $"异常：{ex}".LogRun(Log4NetLevelEnum.错误);
      }
   }

   public void ExportExcelCMD() { }
}
