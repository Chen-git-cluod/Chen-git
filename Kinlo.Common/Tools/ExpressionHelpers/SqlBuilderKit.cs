namespace Kinlo.Common.Tools.ExpressionHelpers;

/// <summary>
/// 表达树生成sql
/// </summary>
public static class SqlBuilderKit
{
   public static Func<IEnumerable<object>, string> BuildUpdateSqlFunc(Type type, string tableName, string keyPropertyName = "Id")
   {
      var param = Expression.Parameter(typeof(IEnumerable<object>), "items");

      var toListCall = Expression.Call(typeof(Enumerable), "Cast", new[] { type }, param);

      var listVar = Expression.Variable(typeof(IEnumerable<>).MakeGenericType(type), "typedItems");
      var assignList = Expression.Assign(listVar, toListCall);

      var sb = new StringBuilder();
      var props = type.GetProperties().Where(p => p.CanRead && p.Name != keyPropertyName).ToList();
      var keyProp = type.GetProperty(keyPropertyName);
      if (keyProp == null)
         throw new ArgumentException($"找不到主键属性 {keyPropertyName}");

      // 构建 body: 迭代每个 item 生成 UPDATE SQL
      var blockExpressions = new List<Expression> { assignList };

      var foreachVar = Expression.Variable(type, "item");
      var loopBody = Expression.Block(
         props.Select(p =>
         {
            var value = Expression.Property(foreachVar, p);
            var nullCheck = Expression.NotEqual(Expression.Convert(value, typeof(object)), Expression.Constant(null));
            var formatUpdate = Expression.Call(
               typeof(SqlBuilderKit).GetMethod(
                  nameof(AppendUpdateSql),
                  System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static
               ),
               Expression.Constant(sb),
               Expression.Constant(tableName),
               Expression.Constant(p.Name),
               Expression.Convert(value, typeof(object)),
               Expression.Convert(Expression.Property(foreachVar, keyProp), typeof(object))
            );
            return Expression.IfThen(nullCheck, formatUpdate);
         })
      );

      var loop = ExpressionHelpers.ForEach(listVar, foreachVar, loopBody);

      blockExpressions.Add(loop);
      var block = Expression.Block(new[] { listVar }, blockExpressions);
      var lambda = Expression.Lambda<Func<IEnumerable<object>, string>>(
         Expression.Block(block, Expression.Call(typeof(SqlBuilderKit).GetMethod(nameof(GetSql)), Expression.Constant(sb))),
         param
      );

      return lambda.Compile();
   }

   private static void AppendUpdateSql(StringBuilder sb, string table, string col, object val, object key)
   {
      string valueStr = val switch
      {
         string s => $"'{s.Replace("'", "''")}'",
         DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss.fff}'",
         bool b => b ? "1" : "0",
         null => "NULL",
         _ => val.ToString(),
      };

      sb.AppendLine($"UPDATE {table} SET {col} = {valueStr} WHERE Id = {key};");
   }

   private static string GetSql(StringBuilder sb) => sb.ToString();

   public static class ExpressionHelpers
   {
      public static Expression ForEach(Expression collection, ParameterExpression loopVar, Expression loopContent)
      {
         var enumeratorVar = Expression.Variable(typeof(IEnumerator<>).MakeGenericType(loopVar.Type), "enumerator");
         var getEnumerator = Expression.Assign(enumeratorVar, Expression.Call(collection, "GetEnumerator", Type.EmptyTypes));

         var moveNext = Expression.Call(enumeratorVar, typeof(System.Collections.IEnumerator).GetMethod("MoveNext")!);
         var breakLabel = Expression.Label("LoopBreak");

         return Expression.Block(
            new[] { enumeratorVar },
            getEnumerator,
            Expression.Loop(
               Expression.IfThenElse(
                  Expression.IsFalse(moveNext),
                  Expression.Break(breakLabel),
                  Expression.Block(
                     new[] { loopVar },
                     Expression.Assign(loopVar, Expression.Property(enumeratorVar, "Current")),
                     loopContent
                  )
               ),
               breakLabel
            )
         );
      }
   }
}
