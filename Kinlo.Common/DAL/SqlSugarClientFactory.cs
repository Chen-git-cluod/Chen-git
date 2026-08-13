namespace Kinlo.Common.DAL;

/// <summary>
/// sqlsugar实例工厂
/// </summary>
public class SqlSugarClientFactory : ISqlSugarDbFactory
{
   protected IContainer _container;
   private ParameterConfig _parameterConfig;

   public SqlSugarClientFactory(IContainer container)
   {
      _container = container;
      _parameterConfig = container.Get<ParameterConfig>();
   }

   public void UsingDb(DatabaseRole role, Action<ISqlSugarClient> action)
   {
      try
      {
         using var db = CreateClient(role);
         if (db == null)
            return;

         action(db);
      }
      catch (Exception ex)
      {
         $"[操作数据库异常] {ex}".LogRun(Log4NetLevelEnum.警告, true);
      }
   }

   public async Task<T?> UsingDbAsync<T>(Func<ISqlSugarClient, Task<T>> func) => await UsingDbAsync(DatabaseRole.LocalDb1, func);

   public async Task UsingDbAsync(DatabaseRole role, Func<ISqlSugarClient, Task> func)
   {
      try
      {
         using var db = CreateClient(role);
         if (db == null)
            return;

         await func(db);
      }
      catch (Exception ex)
      {
         $"[操作数据库异常] {ex}".LogRun(Log4NetLevelEnum.警告, true);
      }
   }

   public async Task<T?> UsingDbAsync<T>(DatabaseRole role, Func<ISqlSugarClient, Task<T>> func)
   {
      try
      {
         using var db = CreateClient(role);
         if (db == null)
            return default;

         return await func(db);
      }
      catch (Exception ex)
      {
         $"[操作数据库异常] {ex}".LogRun(Log4NetLevelEnum.警告, true);
         return default;
      }
   }

   public async Task<DbResult<T>> UsingTransactionAsync<T>(DatabaseRole role, Func<ISqlSugarClient, Task<T>> func)
   {
      using var db = CreateClient(role);
      if (db == null)
         return new DbResult<T> { IsSuccess = false, ErrorMessage = "无法创建数据库连接" };

      return await db.Ado.UseTranAsync(async () => await func(db));
   }

   public ISqlSugarClient? CreateClient(DatabaseRole role)
   {
      try
      {
         string conn = role switch
         {
            DatabaseRole.LocalDb1 => _parameterConfig.AdvancedConfig.LocalConnectionString,
            DatabaseRole.LocalDb2 => _parameterConfig.AdvancedConfig.LocalConnectionString2,
            DatabaseRole.RemoteDb1 => _parameterConfig.AdvancedConfig.OtherConnectionString,
            _ => _parameterConfig.AdvancedConfig.OtherConnectionString2,
         };
         if (conn.IsNullOrEmpty())
         {
            $"[创建数据库连接错误] 未配置数据库连接字符串 Role={role}".LogRun(Log4NetLevelEnum.错误);
            return null;
         }

         conn += " Connection Timeout=10;";

         return Create(conn, role);
      }
      catch (Exception ex)
      {
         $"[创建数据库连接错误] Role={role} Err={ex}".LogRun(Log4NetLevelEnum.警告, true);
         return null;
      }
   }

   /// <summary>
   /// 创建DB
   /// </summary>
   /// <param name="localConnection"></param>
   /// <returns></returns>
   private SqlSugarClient? Create(string localConnection, DatabaseRole databaseRole)
   {
      string header = databaseRole switch
      {
         DatabaseRole.LocalDb1 => "[本地数据库1]",
         DatabaseRole.LocalDb2 => "[本地数据库2]",
         DatabaseRole.RemoteDb1 => "[远程数据库1]",
         _ => "[远程数据库2]",
      };
      // string localConnection = $"{_parameterConfig.AdvancedConfig.LocalConnectionString} Connection Timeout=10;";//SQL Server：Connect Timeout=10;  设置连接超时时间，避免连接不上时长时间等待
      return new SqlSugarClient(
         new ConnectionConfig()
         {
            ConnectionString = localConnection,
            DbType = DbType.MySql,
            IsAutoCloseConnection = true, //自动释放数据库，如果存在事务，在事务结束之后释放。
            ConfigureExternalServices = new ConfigureExternalServices
            {
               EntityNameService = (type, entity) =>
               {
                  entity.IsDisabledDelete = _parameterConfig.AdvancedConfig.IsDisabledDelete; //禁用数据库自动删除列
               },
               EntityService = (c, p) =>
               {
                  var type = p.PropertyInfo.PropertyType;
                  if (
                     type == typeof(string)
                     || type == typeof(DateTime)
                     || (c.PropertyType.IsGenericType && c.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                  ) //为可空类型，如 int? ,老版本写法
                  // new NullabilityInfoContext().Create(c).WriteState is NullabilityState.Nullable  //为可空类型，如 int? ,X#新版本写法
                  {
                     p.IsNullable = true; //所有string类型设置为可空
                  }
               },
            },
         },
         db =>
         {
            db.Ado.CommandTimeOut = _parameterConfig.AdvancedConfig.SqlExecutionTimeout; //执行sql命令超时时间，单位秒

            if (_parameterConfig.AdvancedConfig.SqlLogThreshold <= 0) //打印所有SQL日志
            {
               //5.1.3.24统一了语法和SqlSugarScope一样，老版本AOP可以写外面
               db.Aop.OnLogExecuting = (sql, pars) =>
               {
                  var sqlpars = db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value));
                  $"{header}_{localConnection} \r\n sql语句:\r\n{sql}\r\n参数：{sqlpars}\r\n>>>------------------------------------------------------------------------------------------------------>>>".LogDatabase(); //输出sql,查看执行sql 性能无影响
                  //5.0.8.2 获取无参数化SQL 对性能有影响，特别大的SQL参数多的，调试使用
               };
            }
            else //只打印超过指定时间的SQL日志
            {
               db.Aop.OnLogExecuted = (sql, pars) =>
               {
                  //执行时间超过指定时间（秒）
                  if (db.Ado.SqlExecutionTime.TotalSeconds > _parameterConfig.AdvancedConfig.SqlLogThreshold)
                  {
                     //代码CS文件名
                     // var fileName = db.Ado.SqlStackTrace.FirstFileName;
                     //代码行数
                     // var fileLine = db.Ado.SqlStackTrace.FirstLine;
                     //方法名
                     // var FirstMethodName = db.Ado.SqlStackTrace.FirstMethodName;

                     //参数
                     var sqlpars = db.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value));
                     //db.Ado.SqlStackTrace.MyStackTraceList[1].xxx 获取上层方法的信息
                     $"{header}_{localConnection} \r\n [执行超{_parameterConfig.AdvancedConfig.SqlLogThreshold}秒] 实际用时:[{db.Ado.SqlExecutionTime.TotalSeconds}]秒, sql语句:\r\n{sql}\r\n参数：{sqlpars}\r\n>>>------------------------------------------------------------------------------------------------------>>>\r\n".LogDatabaseTimeout();
                  }
               };
            }
            //注意多租户 有几个设置几个
            //db.GetConnection(i).Aop
         }
      );
   }
}
