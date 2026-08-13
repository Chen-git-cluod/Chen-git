using Kinlo.Equipment.Devices.Fingerprints.Live20R;

namespace Kinlo.Common.Configurations;

/// <summary>
/// 指纹验证器
/// </summary>
public partial class UsersStatusConfig
{
   /// <summary>
   /// 指纹库
   /// </summary>
   public Dictionary<int, byte[]> FingerpringDatas { get; set; } = new Dictionary<int, byte[]>();

   /// <summary>
   /// 实时指纹委托
   /// </summary>
   [JsonIgnore]
   public Action<byte[], Live20R>? CurrentFingerpringAction { get; set; } = null;

   /// <summary>
   /// 指纹读取器
   /// </summary>
   [JsonIgnore]
   public Live20R? Live20RFingerprints { get; set; }

   /// <summary>
   /// 指纹登陆
   /// </summary>
   /// <param name="fingerprint"></param>
   /// <param name="fingerprintStr"></param>
   public void FingerprintLogin(byte[] fingerprint, string fingerprintStr)
   {
      if (IsLogin)
      {
         int _fingerKey = Live20RFingerprints.DBIdentify(fingerprint);
         _ = LoginAsync(string.Empty, fingerprintStr, string.Empty, _fingerKey, LoginAccountTypeEnum.指纹登陆);
      }
      else
      {
         CurrentFingerpringAction?.Invoke(fingerprint, Live20RFingerprints);
      }
   }

   /// <summary>
   /// 初始化live20R指纹器
   /// </summary>
   /// <param name="devicesConfig"></param>
   private void InitializeLive20RFingerprintDevice(DevicesConfig devicesConfig)
   {
      if (devicesConfig.DeviceList.FirstOrDefault(x => x.Communication == CommunicationEnum.Live20R指纹器 && x.IsEnable) == null)
         return;

      Live20RFingerprints = CreateLive20R(GlobalStaticTemporary.GlobalToken, out var removedFingerprintIds);
      if (Live20RFingerprints != null)
      {
         Live20RFingerprints.FingerpringAction = FingerprintLogin;
      }
      if (removedFingerprintIds.Count > 0)
      {
         this.Save("系统初始化", $"[初始化UsersStatusConfig]清除本地无用户指纹ID：{string.Join(",", removedFingerprintIds)}");
      }
   }

   /// <summary>
   /// Live20R指纹器
   /// </summary>
   /// <param name="cancellationToken"></param>
   /// <returns></returns>
   /// <exception cref="Exception"></exception>
   public Live20R? CreateLive20R(CancellationToken cancellationToken, out List<int> removedFingerprintIds)
   {
      removedFingerprintIds = new List<int>();
      try
      {
         var _ive20R = new Kinlo.Equipment.Devices.Fingerprints.Live20R.Live20R(cancellationToken);
         _ive20R.Open();
         if (FingerpringDatas != null)
         {
            for (int i = FingerpringDatas.Count - 1; i > -1; i--)
            {
               int _fingerpringId = FingerpringDatas.ElementAt(i).Key;
               if (!LocalUsers.Any(x => x.FingerprintID == _fingerpringId))
               {
                  FingerpringDatas.Remove(_fingerpringId);
                  removedFingerprintIds.Add(_fingerpringId);
               }
            }
            if (removedFingerprintIds.Count > 0)
            {
               $"[初始化UsersStatusConfig]清除本地无用户指纹ID：{string.Join(",", removedFingerprintIds)}".LogRun(Log4NetLevelEnum.信息);
            }
            FingerpringDatas
               ?.ToList()
               .ForEach(item =>
               {
                  _ive20R.DBAdd(item.Key, item.Value);
               });
         }
         return _ive20R;
      }
      catch (Exception ex)
      {
         $"[初始化UsersStatusConfig] 异常：{ex}".LogRun(Log4NetLevelEnum.警告);
      }
      return null;
   }
}
