namespace Kinlo.Common.Interfaces;

/// <summary>
/// db工厂
/// </summary>
public interface ISqlSugarDbFactory
{
   /// <summary>
   /// 在当前方法作用域操作数据库（role决定哪个数据库），自动生命周期
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="role">role决定哪个数据库</param>
   /// <param name="func"></param>
   /// <returns></returns>
   Task<T?> UsingDbAsync<T>(DatabaseRole role, Func<ISqlSugarClient, Task<T>> func);

   /// <summary>
   ///  在当前方法作用域操作数据库（role决定哪个数据库），自动生命周期
   /// </summary>
   /// <param name="role"></param>
   /// <param name="func"></param>
   /// <returns></returns>
   Task UsingDbAsync(DatabaseRole role, Func<ISqlSugarClient, Task> func);

   /// <summary>
   ///  在当前方法作用域操作数据库（role决定哪个数据库），自动生命周期
   /// </summary>
   /// <param name="role"></param>
   /// <param name="action"></param>
   void UsingDb(DatabaseRole role, Action<ISqlSugarClient> action);

   /// <summary>
   /// 在当前方法作用域操作本地数据库1，自动生命周期
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="func"></param>
   /// <returns></returns>
   Task<T?> UsingDbAsync<T>(Func<ISqlSugarClient, Task<T>> func);

   /// <summary>
   /// 带事务自动管理生命周期
   /// </summary>
   /// <typeparam name="T"></typeparam>
   /// <param name="role"></param>
   /// <param name="func"></param>
   /// <returns></returns>
   Task<DbResult<T>> UsingTransactionAsync<T>(DatabaseRole role, Func<ISqlSugarClient, Task<T>> func);

   /// <summary>
   ///  取得DB实例，需手动管理生命周期，切记实例需释放
   /// </summary>
   /// <param name="role"></param>
   /// <returns></returns>
   ISqlSugarClient? CreateClient(DatabaseRole role);
}

public enum DatabaseRole
{
   /// <summary>
   /// 本地数据库1，一般情况本地都使用此数据库，除非一注二注是同一台机时，此数据库为一注
   /// </summary>
   LocalDb1,

   /// <summary>
   /// 本地数据库2，当一注二注是同一台机时，此数据库为二注
   /// </summary>
   LocalDb2,

   /// <summary>
   /// 远程数据库1，二注取一注使用
   /// </summary>
   RemoteDb1,

   /// <summary>
   /// 远程数据库2，二注取一注使用（有时一注有多台）
   /// </summary>
   RemoteDb2,
}
