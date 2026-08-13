namespace Kinlo.Common.Dto;

[AddINotifyPropertyChangedInterface]
public class AlarmDialogDto
{
   public AlarmDialogDto(string msg, string title = "")
   {
      Message = msg;
      Title = title;
   }

   public string Title { get; set; } = string.Empty;
   public string Message { get; set; } = string.Empty;
}
