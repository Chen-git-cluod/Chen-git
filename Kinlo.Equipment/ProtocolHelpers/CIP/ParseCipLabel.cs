using System.Net;
using System.Text;

namespace Kinlo.Equipment.ProtocolHelpers.CIP;

/// <summary>
/// CIP标签解析
/// </summary>
internal static class ParseCipLabel
{
   /// <summary>
   /// 获取多个标签读取数据报文
   /// </summary>
   /// <param name="lables"></param>
   /// <param name="is_ucmm"></param>
   /// <returns></returns>
   public static byte[] MultipleLableReadRequest(SignalAddressModel[] addresses)
   {
      List<byte> rssBytes = new List<byte>()
      {
         0x0A,
         0x02, //0A-服务代码（多个标签）;02-请求路径大小
         0x20,
         0x02,
         0x24,
         0x01, //请求路径
         (byte)addresses.Length,
         0x00, //标签数量
      };
      var offset = (ushort)(2 + (addresses.Length) * 2); //偏移量（初始值为：2+标签数量*2）
      List<byte> multipleLableBytes = new List<byte>();
      for (int i = 0; i < addresses.Length; i++)
      {
         var oneLableBytes = LabelReadRequest(addresses[i].Lable, addresses[i].Length);
         multipleLableBytes.AddRange(oneLableBytes);
         rssBytes.AddRange(BitConverter.GetBytes(offset));
         offset += (ushort)oneLableBytes.Length; //偏移量 =标签服务长度+初始偏移量
      }
      rssBytes.AddRange(multipleLableBytes);
      return rssBytes.ToArray();
   }

   /// <summary>
   /// 获取单个标签读取数据报文
   /// </summary>
   /// <param name="lable"></param>
   /// <param name="elementCount">读取元素个数</param>
   /// <returns></returns>
   public static byte[] LabelReadRequest(string lable, ushort elementCount)
   {
      var lableBytes = LableToBytes(lable);
      List<byte> rssBytes =
      [
         0x4C, //服务标识读取0x4C 1byte
         (byte)(lableBytes.Length / 2), // 节点长度 2byte （以 word 为单位）
      ];
      rssBytes.AddRange(lableBytes);
      // rssBytes.AddRange([0x01, 0x00]);//服务命令指定数据　默认为0x0001
      rssBytes.AddRange([(byte)elementCount, (byte)(elementCount << 8)]); //读取元素个数
      return rssBytes.ToArray();
   }

   /// <summary>
   /// 获取单个标签写入单个数据报文
   /// </summary>
   /// <param name="lable"></param>
   /// <param name="value"></param>
   /// <param name="is_ucmm"></param>
   /// <returns></returns>
   public static byte[] LableWriteValueRequest(string lable, object value, CommunicationEnum communication, Encoding? encoding = null)
   {
      var lableBytes = LableToBytes(lable);
      List<byte> bytes =
      [
         0x4D, //服务标识写入固定为0x4D 1byte
         (byte)(lableBytes.Length / 2), // 节点长度 2byte
      ];
      bytes.AddRange(lableBytes);

      byte typeCipdata = 0;
      int bytesLength = 0;
      var typeName = value.GetType().Name;
      switch (typeName)
      {
         case "Byte":
            throw new NotImplementedException("未实现");
         //case "Boolean": throw new NotImplementedException("未实现");
         case "String":
            typeCipdata = 0xD0;
            bytesLength = value.ToString().Length;
            bytesLength = bytesLength % 2 != 0 ? bytesLength + 1 : bytesLength;
            break;
         default:
            var info = CIPDataInfoHelper.CIPDataTypeInfos.FirstOrDefault(x => x.DataType == value.GetType());
            if (info == null)
               throw new NotImplementedException("未实现");
            typeCipdata = info.PropertyByre;
            bytesLength = info.Length;
            break;
      }
      bytes.Add(typeCipdata);
      bytes.Add(0x00);
      bytes.Add(0x01);
      bytes.Add(0x00);
      byte[] value_bytes = default;
      if (typeName == "String")
      {
         encoding ??= Encoding.ASCII;
         var codes = encoding.GetBytes(value.ToString());

         bytesLength = codes.Length;
         bytesLength = bytesLength % 2 != 0 ? bytesLength + 1 : bytesLength;

         value_bytes = new byte[bytesLength + 2];
         value_bytes[0] = (byte)(bytesLength);
         value_bytes[1] = (byte)(bytesLength >> 8);

         Array.Copy(codes, 0, value_bytes, 2, codes.Length);
      }
      else
      {
         value_bytes = new byte[bytesLength];
         StructToBytes.GetBytes(value, value_bytes, 0, communication);
      }
      bytes.AddRange(value_bytes);
      return bytes.ToArray();
   }

   static ConcurrentDictionary<string, byte[]> _structHandlerDic = new ConcurrentDictionary<string, byte[]>();

   /// <summary>
   /// 获取单个标签写入类报文
   /// </summary>
   /// <param name="lable"></param>
   /// <param name="value"></param>
   /// <param name="is_ucmm"></param>
   /// <returns></returns>
   public static byte[] LableWriteClassRequest(string lable, object value, CommunicationEnum communication, CipClient plc, string logHeader)
   {
      if (!_structHandlerDic.TryGetValue(lable, out byte[] structHandler))
      {
         var readBytes = plc.Protocol.Serialize(LabelReadRequest(lable, 1));
         var res = plc.Conn.Write(readBytes, lable);
         if (res.State != CommState.Success)
            throw new Exception($"写入{lable}失败:{res.Message}");

         var readRes = plc.Conn.Read(1024, logHeader);
         if (readRes.State != CommState.Success)
            throw new Exception($"读取失败{readRes.Message}！！");
         var buffer = readRes.Data!;
         if (plc.ConnectMode == CipMode.无连接模式)
         {
            if (buffer.Length < 48)
               throw new Exception("取标签句柄报文过短！！！");
            structHandler = [buffer[46], buffer[47]];
         }
         else
         {
            if (buffer.Length < 54)
               throw new Exception("取标签句柄报文过短！！！");
            structHandler = [buffer[52], buffer[53]];
         }
         _structHandlerDic.AddOrUpdate(lable, structHandler, (k, v) => structHandler);
      }

      var lableBytes = LableToBytes(lable);
      List<byte> bytes =
      [
         0x4D, //服务标识写入固定为0x4D 1byte
         (byte)(lableBytes.Length / 2), // 节点长度 2byte
      ];
      bytes.AddRange(lableBytes);
      bytes.Add(0xA0);
      bytes.Add(0x02);
      bytes.AddRange(structHandler);
      bytes.Add(0x01);
      bytes.Add(0);
      int classSize = (int)StructToBytes.GetClassSize(value);
      byte[] array = new byte[classSize];
      int i = 0;
      var bs = StructToBytes.ToBytes(value, array, ref i, 0, communication);

      bytes.AddRange(array);
      return bytes.ToArray();
   }

   /// <summary>
   /// 单个标签转bytes
   /// </summary>
   /// <param name="lable"></param>
   /// <returns></returns>
   /// <exception cref="Exception"></exception>
   private static byte[] LableToBytes(string lable)
   {
      if (string.IsNullOrEmpty(lable))
      {
         throw new Exception("标签为空！！！");
      }
      List<LabelAnalysisModel> labelModels = AnalysisLabel(lable);
      List<byte> bytes = new List<byte>();
      foreach (var item in labelModels)
      {
         bytes.AddRange(LablePrefixToBytes(item.Label));
         if (item.IsArray)
         {
            for (int i = 0; i < item.Index.Length; i++)
            {
               if (item.Index[i] > 255)
               {
                  bytes.Add(0x29);
                  bytes.Add(0);
                  bytes.Add((byte)item.Index[i]);
                  bytes.Add((byte)(item.Index[i] >> 8));
               }
               else
               {
                  bytes.Add(0x28);
                  bytes.Add((byte)item.Index[i]);
               }
            }
         }
      }
      return bytes.ToArray();
   }

   /// <summary>
   /// 标签前缀部分转bytes
   /// </summary>
   /// <param name="lable"></param>
   /// <returns></returns>
   private static byte[] LablePrefixToBytes(string lable)
   {
      var lableAsscii = Encoding.ASCII.GetBytes(lable);
      List<byte> bytes = [0x91, (byte)(lableAsscii.Length)];
      bytes.AddRange(lableAsscii);
      if (lableAsscii.Length % 2 != 0)
         bytes.Add(0x00);

      return bytes.ToArray();
   }

   /// <summary>
   /// 分析标签
   /// </summary>
   /// <param name="lable"></param>
   /// <returns></returns>
   private static List<LabelAnalysisModel> AnalysisLabel(string lable)
   {
      var list = new List<LabelAnalysisModel>();
      string[] _parts = lable.Split('.');
      foreach (var item in _parts)
      {
         string part = Regex.Match(item, @"(?i)(.*)(?=\[)").Groups[0].Value;
         string strIndex = Regex.Match(item, @"(?i)(?<=\[)(.*)(?=\])").Groups[0].Value;
         short[] indexArr = default;
         if (!string.IsNullOrEmpty(strIndex))
         {
            indexArr = strIndex.Split(',').Select(x => Convert.ToInt16(x)).ToArray();
            list.Add(new LabelAnalysisModel(part, true, indexArr));
         }
         else
         {
            list.Add(new LabelAnalysisModel(item, false, null));
         }
      }
      return list;
   }

   public static string GetStatusCode(byte code)
   {
      switch (code)
      {
         case 0x00:
            return "Success";
         case 0x01:
            return "Connection failure";
         case 0x02:
            return "Resource unavailable";
         case 0x03:
            return "Invalid Parameter value";
         case 0x04:
            return "Path segment error";
         case 0x05:
            return "Path destination unknown";
         case 0x06:
            return "Partial transfer";
         case 0x07:
            return "Connection lost";
         case 0x08:
            return "Service not supported";
         case 0x09:
            return "Invalid attribute value";
         case 0x0A:
            return "Attribute List error";
         case 0x0B:
            return "Already in requested mode/state";
         case 0x0C:
            return "Object state conflict";
         case 0x0D:
            return "Object already exists";
         case 0x0E:
            return "Attribute not settable";
         case 0x0F:
            return "Privilege violation";
         case 0x10:
            return "Device state conflict";
         case 0x11:
            return "Reply data too large";
         case 0x12:
            return "Fragmentation of a primitive value";
         case 0x13:
            return "Not enough data";
         case 0x14:
            return "Attribute not supported";
         case 0x15:
            return "Too much data";
         case 0x16:
            return "Object does not exist";
         case 0x17:
            return "Service fragmentation sequence not in progress";
         case 0x18:
            return "No stored attribute data";
         case 0x19:
            return "Store operation failure";
         case 0x1A:
            return "Routing failure, request packet too large";
         case 0x1B:
            return "Routing failure, response packet too large";
         case 0x1C:
            return "Missing attribute list entry data";
         case 0x1D:
            return "Invalid attribute value list";
         case 0x1E:
            return "Embedded service error";
         case 0x1F:
            return "Vendor specific error";
         case 0x20:
            return "Invalid parameter";
         case 0x21:
            return "Write-once value or medium atready written";
         case 0x22:
            return "Invalid Reply Received";
         case 0x23:
            return "Buffer overflow";
         case 0x24:
            return "Message format error";
         case 0x25:
            return "Key failure path";
         case 0x26:
            return "Path size invalid";
         case 0x27:
            return "Unecpected attribute list";
         case 0x28:
            return "Invalid Member ID";
         case 0x29:
            return "Member not settable";
         case 0x2A:
            return "Group 2 only Server failure";
         case 0x2B:
            return "Unknown Modbus Error";
         default:
            return "unknown";
      }
   }
}
