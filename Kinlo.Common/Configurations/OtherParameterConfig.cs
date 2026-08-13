namespace Kinlo.Common.Configurations;

public class OtherParameterConfig : ConfigurationBase
{
   public OtherParameterConfig(StyletIoC.IContainer container, bool isStartup)
      : base(container, isStartup) { }

   public OtherParameterModel OtherParameter { get; set; }

   /// <summary>
   /// 补液称的一些选项
   /// </summary>
   public ReInjectionElectrolyteModel ReInjectionElectrolyte { get; set; }

   #region 语言相关
   /// <summary>
   /// 加载语言顺序：加载类和属性中特性-->本集合-->Config中的languages.csv
   /// </summary>
   [JsonIgnore]
   List<string[]> DefualtLanguages =
   [
      ["清零时间", "waktu nol", "Reset time"],
      ["累加复制", "salinan kumulatif", "Aggregate copy"],
      ["MES网口IP", "IP Port Jaringan MES", "MES network IP"],
      ["总计", "Total", "Total"],
      ["条", "klausul", "Item"],
      ["MQTT配置", "Konfigurasi MQTT", "MQTT config"],
      ["MES接口", "Antarmuka MES", "MES interface"],
      ["导出当天数据", "Mengekspor data hari ini", "Export daily data"],
      ["类型", "tipologi", "Type"],
      ["MES状态", "Status MES", "MES status"],
      ["工序数据去重", "Proses de-duplikasi data", "Process data deduplication"],
      ["补液站", "stasiun hidrasi", "Electrolyte replenishment station"],
      ["输入条码", "Masukkan kode batang", "Enter barcode"],
      ["输入重量", "Berat Masukan", "Enter weight"],
      ["干重", "berat kering", "net weight"],
      ["输入条码长度", "Masukkan kode batang", "enter barcode length"],
      ["对应PLC权限", "Hak istimewa PLC ", "PLC level"],
      ["对应MES权限", "Hak-hak MES", "MES level"],
      ["修改PLC对应权限", "Memodifikasi hak PLC", "Eidt PLC leve"],
      ["开启MES登陆", "Buka Login MES", "Enable MES login"],
      ["用户登陆", "login pengguna", "log in"],
      ["图表分析", "Analisis grafis", "Charts"],
      ["参数配方", "Formulasi parametrik", "Parameter recipe"],
      ["新建配方", "Resep Baru", "Create recipe"],
      ["编辑配方", "Edit resep", "Modify recipe"],
      ["删除配方", "Menghapus Resep", "Delete recipe"],
      ["配方名称", "Nama formulasi", "Recipe name"],
      ["原材料上卸料", "Bongkar muat bahan", "Loading or unloading of raw materials"],
      ["工单获取", "Akuisisi Perintah Kerja", "Get work order"],
      ["后称重检测", "Pemeriksaan pasca-penimbangan", "after weighting check"],
      ["前称重检测", "Tes pra-penimbangan", "before weighting check"],
      ["物流MES状态", "Status MES Logistik", "Logistics NES status"],
      ["注液量检测", "Deteksi volume injeksi cairan", "enable injection check"],
      ["补液泵范围", "Rentang pompa isi ulang", "Replenish injection range"],
      ["注液泵范围", "Jajaran pompa injeksi cair", "injection range"],
      ["前称重量管控", "前称重量管控", "incoming Weight Control"],
      ["前称重量范围", "前称重量范围", "incoming Weight range"],
      ["后称重量管控", "后称重量管控", "before Weight Control"],
      ["后称重量范围", "后称重量范围", "before Weight range"],
      ["MES进站", "MES进站", "MES inbound"],
      ["MES出站", "MES出站", "MES outbound"],
      ["MES进出站", "MES进出站", "MES inbound and outbound"],
      ["物流线手动组盘出站", "", "Maual tray loading"],
      ["物流线手动组盘出站", "", "Maual tray loading"],
      ["托盘号", "", "Tray code"],
      ["选择时间", "", "select time"],
      ["工序整体", "", "Overall process"],
      ["开始时间", "", "Start time"],
      ["结束时间", "", "End time"],
      ["行", "", "Row"],
      ["列", "", "Column"],
      ["工序", "", "Processes"],
      ["重复保留最后一条", "", "Deduplicated By Latest Id"],
      ["统计注液数据", "", "Statistics Inject data"],
      ["统计测漏数据", "", "Statistics test Leak data"],
      ["统计工序数据", "", "Statistical process data"],
      ["自定义统计", "", "Custom statistics"],
      ["统计", "", "Statistics"],
      ["选择统计通道", "", "Select statistics index"],
      ["选择统计工序", "", "Select statistics process"],
      ["按托盘统计测漏及注液数据", "", "Collect leak detection and liquid injection data by tray"],
      [
         "输入条码（单码：支持模糊查询。 多码：须完整匹配，兼容换行与逗号分隔）",
         "",
         "Barcode (Single code supports fuzzy query; Multiple barcodes must be fully matched and support line breaks, commas, or mixed input)",
      ],
      ["一次注液", "", "Fist Injection"],
      ["二次注液", "", "Two Injection"],
      ["界面数据显示：", "", "Interface data display"],
      ["公共参数", "", "Common parameters"],
      ["心跳时间(ms)：", "", "Heartbeat time(ms)"],
      ["判定", "", ""],
      ["行数", "Row count", "Row count"],
      ["列数", "CoLumn count", "Column count"],
      ["工序数据", "Process data", "Process data"],
      ["注液CPK", "Injection CPK", "Injection CPK"],
      ["PLC报警", "PLC alarm", "PLC alarm"],
      ["统计开始时间", "Record the start time", "Record the start time"],
      ["进站", "", "input count"],
      ["出站", "", "output count"],
      ["上一分钟产量", "", "Last minute count"],
      ["当前分钟累计", "", "Current minute count"],
      ["选择配方", "", ""],
      ["层数", "层数", "Floor count"],
      ["接收报文", "接收报文", "Requested message"],
      ["回复报文", "回复报文", "response message"],
      ["补液称重量", "补液称重量", "replenish weight"],
      ["后称重量", "后称重量", "after weight"],
      ["条码长度", "条码长度", "barcode length"],
      ["转移数据时间(秒)", "转移数据时间(秒)", "clear data time(sec)"],
      ["自定义启动", "自定义启动", "costom boot"],
      ["打开说明书", "打开说明书", "open manual"],
      ["说明书", "说明书", "manual"],
      ["操作", "操作", "handler"],
      ["优先MES登陆", "优先MES登陆", "Log in from MES"],
      ["显示最大数量", "显示最大数量", "Display max count"],
      ["有效范围：", "有效范围：", ""],
      ["语言", "语言", "Language"],
      ["远程助手", "远程助手", "Remote assistant"],
      ["网速标签", "网速标签", "Internet speed tag"],
      ["选择手机类型", "选择手机类型", "Select phone type"],
      ["安卓手机", "安卓手机", "Android phone"],
      ["苹果手机", "苹果手机", "iPhone"],
      ["上一步", "上一步", "Previous step"],
      ["下一步", "下一步", "Next step"],
      ["打开向日葵", "打开向日葵", "Open AweSun"],
      ["扩展", "扩展", "Extension"],
      ["显示Hipot波形图表", "显示Hipot波形图表", "hipot curve show"],
      ["重试次数", "重试次数", "retry count"],
   ];

   /// <summary>
   /// 各种语言字典
   /// </summary>
   [JsonIgnore]
   public Dictionary<string, List<string>> LanguagesDictionary { get; set; } = new Dictionary<string, List<string>>();

   /// <summary>
   /// 当前使用语言字典
   /// </summary>
   [JsonIgnore]
   public ResourceDictionary CurrentLanguagesDictionary { get; set; } = new ResourceDictionary();

   /// <summary>
   /// 语言各类
   /// </summary>
   [JsonIgnore]
   public ObservableCollection<LanguageModel> Languages { get; set; } = new ObservableCollection<LanguageModel>();

   /// <summary>
   /// 当前使用的语言Mode
   /// </summary>
   public LanguageModel CurrentLanguage { get; set; }
   #endregion
   public override void Load()
   {
      try
      {
         var _dic = FileHelper.LoadToDictionary(this.GetType().Name);

         if (_dic != null && _dic.TryGetValue(nameof(CurrentLanguage), out object currentLang) && currentLang != null)
            CurrentLanguage = JsonSerializer.Deserialize<LanguageModel>(currentLang.ToString())!;
         if (CurrentLanguage == null)
            CurrentLanguage = new LanguageModel();

         if (
            _dic != null
            && _dic.TryGetValue(nameof(ReInjectionElectrolyte), out object refillElectrolyteObj)
            && refillElectrolyteObj != null
         )
            ReInjectionElectrolyte = JsonSerializer.Deserialize<ReInjectionElectrolyteModel>(refillElectrolyteObj.ToString())!;
         if (ReInjectionElectrolyte == null)
            ReInjectionElectrolyte = new ReInjectionElectrolyteModel();

         if (_dic != null && _dic.TryGetValue(nameof(OtherParameter), out object value) && value != null)
            OtherParameter = JsonSerializer.Deserialize<OtherParameterModel>(value.ToString())!;
         if (OtherParameter == null)
            OtherParameter = new OtherParameterModel();

         OtherParameter.LoadRecipes(_container);
      }
      catch (Exception ex)
      {
         $"[初始化OtherParameterConfig]异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   /// <summary>
   /// 初始化语言
   /// </summary>
   /// <returns></returns>
   public void InitLanguage(List<Assembly> assemblies)
   {
      try
      {
         Stopwatch _sw = Stopwatch.StartNew();
         _sw.Start();
         //string requestedCulture = $@"Languages\{OtherParameter.OtherParameter.Language.Key}.xaml";

         if (!Directory.Exists(FileHelper.SaveBasePath))
         {
            Directory.CreateDirectory(FileHelper.SaveBasePath);
         }
         string _paht = $@"{FileHelper.SaveBasePath}\languages.csv";

         List<string> _scvLanguages = new List<string>();
         if (!File.Exists(_paht))
         {
            $"[加载语言] 未找到语言文件 [{_paht}];".LogRun(Log4NetLevelEnum.错误, true);
         }
         else
         {
            _scvLanguages = File.ReadAllLines(_paht, Encoding.UTF8).ToList();
            List<string> _titles = new List<string>();
            if (_scvLanguages.Count < 2)
            {
               $"[加载语言] 语言文件少于2行 [{_paht}];".LogRun(Log4NetLevelEnum.错误, true);
               _titles = ["简体中文", "繁體中文", "English"];
            }
            else
            {
               _titles = _scvLanguages[0].Split(',').ToList();
            }
            for (int i = 0; i < _titles.Count; i++)
            {
               Languages.Add(new LanguageModel { Index = i, Title = _titles[i] });
            }
         }

         GetEntityLanguage(assemblies); //加载类中特性的语言
         foreach (var defualtLang in DefualtLanguages) //加载默认集合中语言
         {
            UpdateLanguage(defualtLang);
         }
         for (int i = 1; i < _scvLanguages.Count; i++) //加载Config中的languages.csv中语言
         {
            var item = _scvLanguages[i];
            if (string.IsNullOrEmpty(item) || (item.Length > 1 && item[0] == '/' && item[1] == '/'))
               continue;
            UpdateLanguage(item.Split(','));
         }
         var _switchRS = SwitchLanguage(true);
         _sw.Stop();
         $"[加载语言]{(_switchRS ? "成功" : "失败")}，用时[{_sw.ElapsedMilliseconds}]ms,".LogRun(
            _switchRS ? Log4NetLevelEnum.成功 : Log4NetLevelEnum.错误,
            !_switchRS
         );
      }
      catch (Exception ex)
      {
         $"[加载语言] 出现异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
      }
   }

   /// <summary>
   /// 加载类中特性的语言
   /// </summary>
   private void GetEntityLanguage(List<Assembly> assemblies)
   {
      List<Type> types = new List<Type>();
      foreach (Assembly assembly in assemblies)
      {
         foreach (var type in assembly.GetTypes())
         {
            // var _classLan = type.GetCustomAttribute<LanguagesAttribute>();
            var classLans = type.GetCustomAttributes<LanguagesAttribute>();
            if (classLans != null)
            {
               bool isScanProperty = false; //是否需要扫描类内部属性语言
               bool isScanMethod = false; //是否需要扫描类内部属性语言
               foreach (var classLan in classLans)
               {
                  UpdateLanguage(classLan.Languages);
                  isScanProperty = classLan.IsScanProperty;
                  isScanMethod = classLan.IsScanMethod;
               }
               if (isScanProperty)
               {
                  if (type.IsEnum)
                  {
                     foreach (var obj in Enum.GetValues(type))
                     {
                        var enumName = Enum.GetName(type, obj);
                        var field = type.GetField(enumName);
                        var propertyLans = field?.GetCustomAttributes<LanguagesAttribute>();
                        if (propertyLans != null)
                        {
                           foreach (var propertyLan in propertyLans)
                           {
                              if (propertyLan != null)
                                 UpdateLanguage(propertyLan.Languages);
                           }
                        }
                     }
                  }
                  else if (type.IsClass)
                  {
                     foreach (var propertyInfo in type.GetProperties())
                     {
                        var propertyLans = propertyInfo.GetCustomAttributes<LanguagesAttribute>();
                        if (propertyLans != null)
                        {
                           foreach (var propertyLan in propertyLans)
                           {
                              if (propertyLan != null)
                                 UpdateLanguage(propertyLan.Languages);
                           }
                        }
                     }
                     if (isScanMethod)
                     {
                        foreach (var methodInfo in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                        {
                           var propertyLans = methodInfo.GetCustomAttributes<LanguagesAttribute>();
                           if (propertyLans != null)
                           {
                              foreach (var propertyLan in propertyLans)
                              {
                                 if (propertyLan != null)
                                    UpdateLanguage(propertyLan.Languages);
                              }
                           }
                        }
                     }
                  }
               }
            }
         }
      }
   }

   private void UpdateLanguage(params string[] languages)
   {
      if (languages.Length < 1)
         return;
      if (LanguagesDictionary.TryGetValue(languages[0], out var list))
      {
         list.Clear();
         foreach (var item in languages)
         {
            list.Add(item);
         }
      }
      else
      {
         list = new List<string>();
         foreach (var item in languages)
         {
            list.Add(item);
         }
         LanguagesDictionary.TryAdd(languages[0], list);
      }
   }

   /// <summary>
   /// 切换语言
   /// </summary>
   /// <param name="key"></param>
   /// <param name="isInitialLoad"></param>
   /// <returns></returns>
   public bool SwitchLanguage(bool isInitialLoad = false)
   {
      try
      {
         CurrentLanguagesDictionary.Clear();
         foreach (var language in LanguagesDictionary)
         {
            if (language.Value.Count > CurrentLanguage.Index)
            {
               var _value = language.Value[CurrentLanguage.Index];
               CurrentLanguagesDictionary.Add(language.Key, _value == null || string.IsNullOrEmpty(_value) ? language.Key : _value);
            }
            else
               CurrentLanguagesDictionary.Add(language.Key, language.Key);
         }
         if (isInitialLoad)
            Application.Current.Resources.MergedDictionaries.Add(CurrentLanguagesDictionary);
         else
            this.Save("切换语言", "切换语言", false);

         return true;
      }
      catch (Exception ex)
      {
         $"[切换语言] 出现异常：{ex}".LogRun(Log4NetLevelEnum.错误, true);
         return false;
      }
   }
}
