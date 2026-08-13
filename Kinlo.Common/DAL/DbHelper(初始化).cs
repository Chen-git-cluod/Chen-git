namespace Kinlo.Common.DAL;

public partial class DbHelper
{
   IContainer _container;
   ISqlSugarDbFactory _dbFactory;
   private DisplayDataCollection _displayDatas;
   private SnowflakeHelper _snowflakeHelper;
   private ParameterConfig _parameterConfig;

   public DbHelper(IContainer container)
   {
      _container = container;
      _dbFactory = _container.Get<ISqlSugarDbFactory>();
      _displayDatas = container.Get<DisplayDataCollection>();
      _parameterConfig = container.Get<ParameterConfig>();
      _snowflakeHelper = container.Get<SnowflakeHelper>();
   }

   #region 初始化数据库表
   /// <summary>
   /// 初始化数据库
   /// </summary>
   /// <param name="role"></param>
   /// <returns></returns>
   public async Task<bool> Initializer(DatabaseRole role)
   {
      return await _dbFactory.UsingDbAsync(
         role,
         async db =>
         {
            try
            {
               await Task.Run(() =>
               {
                  $"初始化数据库开始！".LogRun();
                  //   var _ret = db.Ado.IsValidConnection();
                  db.DbMaintenance.CreateDatabase();
                  db.Ado.ExecuteCommand("ALTER DATABASE weightdb CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci;");
                  SyncSplitTableFiled(db);
                  $"初始化数据库完成！".LogRun();
               });
               return true;
            }
            catch (Exception ex)
            {
               $"初始化数据库异常：{ex.Message}".LogRun();
               return false;
            }
         }
      );
   }

   /// <summary>
   /// 完整电池字段集合
   /// </summary>
   public HashSet<string> BatteryFieldNames { get; set; }

   //查询时使用的别名
   public string AliasName { get; set; } = "main";

   //完整电池字段的string（查询时用）
   string _fields { get; set; } = string.Empty;

   //加别名的完整电池字段string（查询时用）
   string _aliasFields = string.Empty;

   /// <summary>
   ///
   /// </summary>
   /// <param name="db"></param>
   public void SyncSplitTableFiled(ISqlSugarClient db)
   {
      try
      {
         var types = new List<Type>();
         types.Add(typeof(InjectionDataModel));
         types.Add(typeof(GasConcentrationModel));
         types.Add(typeof(PlcStatusModel));
         types.Add(typeof(PlcAlarmModel));
         types.Add(typeof(MesResendModel));
         types.Add(_displayDatas.CompleteBatteryDatas.RuntimeBatteryType);
         var group = types.GroupBy(t => t).Select(x => x.Key).ToArray();
         db.CodeFirst.SplitTables().InitTables(group); //同步分表

         //取完整电池的字段
         BatteryFieldNames = _displayDatas
            .CompleteBatteryDatas.RuntimeBatteryType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.GetCustomAttribute<SugarColumn>()?.IsIgnore != true)
            .Select(p => p.Name)
            .ToHashSet();

         _fields = string.Join(",", BatteryFieldNames);
         _aliasFields = string.Join(", ", BatteryFieldNames.Select(f => $"{AliasName}.{f.Trim()}"));
      }
      catch (Exception ex)
      {
         $"同步数据库表异常：{ex.Message}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }
   #endregion
}
