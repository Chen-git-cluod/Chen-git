using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinlo.Common.Validations;

public abstract class ValidationBase : INotifyDataErrorInfo
{
   private readonly Dictionary<string, List<string>> _errors = new();
   public bool HasErrors => _errors.Any();

   public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

   public IEnumerable GetErrors(string? propertyName)
   {
      if (string.IsNullOrEmpty(propertyName))
         return null;
      return _errors.TryGetValue(propertyName, out var errors) ? errors : null;
   }

   protected void OnErrorsChanged(string propertyName) => ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));

   protected void AddError(string propertyName, string error)
   {
      if (!_errors.ContainsKey(propertyName))
         _errors[propertyName] = new List<string>();

      if (!_errors[propertyName].Contains(error))
      {
         _errors[propertyName].Add(error);
         OnErrorsChanged(propertyName);
      }
   }

   protected void ClearErrors(string propertyName)
   {
      if (_errors.ContainsKey(propertyName))
      {
         _errors.Remove(propertyName);
         OnErrorsChanged(propertyName);
      }
   }

   /// <summary>
   /// 可选：统一触发验证的方法，由派生类实现具体字段验证逻辑
   /// </summary>
   public virtual void ValidateAll() { }

   /// <summary>
   /// 检查是否所有字段都有效（自动调用 ValidateAll）
   /// </summary>
   public bool IsValid()
   {
      ValidateAll();
      return !HasErrors;
   }
}
