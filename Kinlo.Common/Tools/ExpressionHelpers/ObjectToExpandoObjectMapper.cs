namespace Kinlo.Common.Tools;

public class ObjectToExpandoObjectMapper
{
   /// <summary>
   /// </summary>
   /// <typeparam name="TSourc">类</typeparam>
   /// <typeparam name="TTarget">ExpandoObject</typeparam>
   /// <returns></returns>
   public static Action<TSourc, TTarget> ObjectToExpandoObject<TSourc, TTarget>()
   {
      System.Linq.Expressions.ParameterExpression pe_Sourc = System.Linq.Expressions.Expression.Parameter(typeof(TSourc), "p");
      System.Linq.Expressions.ParameterExpression pe_Target = System.Linq.Expressions.Expression.Parameter(typeof(TTarget), "t");
      System.Linq.Expressions.UnaryExpression unaryExpression = System.Linq.Expressions.Expression.TypeAs(
         pe_Target,
         typeof(IDictionary<string, object>)
      );
      List<System.Linq.Expressions.MethodCallExpression> methodCallExpressions = new List<MethodCallExpression>();
      System.Reflection.MethodInfo addMethod = typeof(IDictionary<string, object>).GetMethod(
         "Add",
         new Type[] { typeof(string), typeof(object) }
      );
      var propertie = typeof(TSourc).GetProperties();
      for (int i = 0; i < propertie.Length - 3; i++)
      {
         if (!propertie[i].CanWrite)
            continue;
         System.Linq.Expressions.ConstantExpression parameter = System.Linq.Expressions.Expression.Constant(propertie[i].Name);
         System.Linq.Expressions.MemberExpression property_Sourc = System.Linq.Expressions.Expression.Property(
            pe_Sourc,
            typeof(TSourc).GetProperty(propertie[i].Name)
         );
         System.Linq.Expressions.UnaryExpression data = System.Linq.Expressions.Expression.TypeAs(property_Sourc, typeof(object));
         methodCallExpressions.Add(Expression.Call(unaryExpression, addMethod, parameter, data));
      }

      var _block = Expression.Block(methodCallExpressions);
      var _parameter = new ParameterExpression[] { pe_Sourc, pe_Target };
      var lambda = Expression.Lambda<Action<TSourc, TTarget>>(_block, _parameter);
      return lambda.Compile();
   }
}
