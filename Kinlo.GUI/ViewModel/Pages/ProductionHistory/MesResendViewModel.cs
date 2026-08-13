using HandyControl.Controls;
using Kinlo.Services.PeriodicTasks;

namespace Kinlo.GUI.ViewModel;

[UIDisplayAttribute(true)]
public class MesResendViewModel : Screen
{
   /// <summary>
   /// 数据总量
   /// </summary>
   public int TotalCount { get; set; }

   /// <summary>
   /// 总页数
   /// </summary>
   public int TotalPage { get; set; }

   /// <summary>
   /// 选中的页面索引
   /// </summary>
   public int PageIndex { get; set; } = 1;

   private int _dataCountPerPage = 25;

   /// <summary>
   /// 每页数量
   /// </summary>
   public int DataCountPerPage
   {
      get { return _dataCountPerPage; }
      set
      {
         if (_dataCountPerPage != value)
         {
            _dataCountPerPage = value;
            //if (DataList != null)//如果有数据 即重新查询
            //{
            //    QueryCMD();
            //}
         }
      }
   }

   /// <summary>
   /// 开始时间
   /// </summary>
   public DateTime StartTime { get; set; }

   /// <summary>
   /// 结束时间
   /// </summary>
   public DateTime EndTime { get; set; }

   private string _barcode;

   /// <summary>
   /// 查询条码
   /// </summary>
   public string Barcode
   {
      get { return _barcode; }
      set
      {
         if (_barcode != value)
         {
            List<string> lines = new List<string>();

            using (StringReader reader = new StringReader(value))
            {
               string? line;
               while ((line = reader.ReadLine()) != null)
               {
                  if (!string.IsNullOrWhiteSpace(line)) // 跳过空行和只含空白的行
                  {
                     lines.Add(line.Trim());
                  }
               }
            }
            _barcode = string.Join(',', lines);
         }
      }
   }

   /// <summary>
   /// 上传MES间隔时间(ms)
   /// </summary>
   public int IntervalTime { get; set; } = 10;

   /// <summary>
   /// 一次最多上传多少条
   /// </summary>
   public int SendCount { get; set; } = 80000;
   public IEnumerable<QueryMesResendTypeEnum> QueryMesResendTypeEnums
   {
      get { return Enum.GetValues<QueryMesResendTypeEnum>(); }
   }
   public QueryMesResendTypeEnum SelectStatus { get; set; }
   private Dialog _dialog;
   private DisplayDataCollection _displayData;
   private IContainer _container;
   private DbHelper _sugarDB;
   private ParameterConfig _parameterConfig;
   private MesInterfaceParameterConfig _mesInterfaceParameterConfig;
   private IBatteryCache _batteryCache;
   private MesService _mesService;

   public MesResendViewModel(IContainer container)
   {
      _container = container;
      EndTime = DateTime.Now;
      StartTime = EndTime.AddDays(-1);
      _mesInterfaceParameterConfig = container.Get<MesInterfaceParameterConfig>();
      _sugarDB = container.Get<DbHelper>();
      _displayData = container.Get<DisplayDataCollection>();
      _parameterConfig = container.Get<ParameterConfig>();
      _batteryCache = container.Get<IBatteryCache>();
      _mesService = container.Get<MesService>();
   }

   public void QueryCMD() { }

   /// <summary>
   /// 手动启用MES补传
   /// </summary>
   public void MaualStartResendCmd()
   {
      _ = PeriodicTasksHelper.PollingResendMes(_container, DateTime.Now, false, sendCount: SendCount, intervalTime: IntervalTime);
   }

   /// <summary>
   /// 手动停止MES补传
   /// </summary>
   public void StopResendCmd()
   {
      PeriodicTasksHelper.IsStopResend = true;
   }
}
