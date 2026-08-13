using System.Windows.Input;
using HandyControl.Controls;
using static Kinlo.Common.GlobalStaticTemporary;

namespace Kinlo.GUI.ViewModel;

//[UIDisplay(true)]
public class FileDisplayViewModel : Screen
{
   public ObservableCollection<FileNode> Files { get; set; } = new ObservableCollection<FileNode>();
   public FileNode? CurrentFileNode { get; set; }
   Permission _permission = new Permission(false, (ulong)DefaultRoleEnum.工艺 | (ulong)DefaultRoleEnum.设备);
   GlobalStaticTemporary _globalTemporary;

   public FileDisplayViewModel(IContainer container)
   {
      _globalTemporary = container.Get<GlobalStaticTemporary>();
   }

   /// <summary>
   /// 加载文件夹内容
   /// </summary>
   /// <param name="node"></param>
   public void LoadDirectory(FileNode node)
   {
      Files.Clear();
      //Title = path; // 显示当前路径
      CurrentFileNode = node;
      try
      {
         if (!Directory.Exists(node.FullPath))
            Directory.CreateDirectory(node.FullPath);

         var dir = new DirectoryInfo(node.FullPath);
         //加载文件夹
         foreach (var subDir in dir.GetDirectories())
         {
            Files.Add(new FileNode(subDir.FullName, true, node.RootPath));
         }

         //  加载文件
         foreach (var file in dir.GetFiles())
         {
            Files.Add(new FileNode(file.FullName, false, node.RootPath));
         }
      }
      catch (Exception ex)
      {
         Growl.Warning(ex.Message);
      }
   }

   /// <summary>
   /// 返回上一级
   /// </summary>
   public void BackCmd()
   {
      if (CurrentFileNode == null)
      {
         Growl.Warning("请先选中设备！");
         return;
      }
      var nodeRes = CurrentFileNode.GetParentSafe();
      if (nodeRes.state)
      {
         LoadDirectory(nodeRes.node!);
      }
      else
      {
         Growl.Warning("已到达目录最上层！");
      }
   }

   /// <summary>
   /// 刷新
   /// </summary>
   public void RefreshCmd()
   {
      if (CurrentFileNode == null)
      {
         Growl.Warning("请先选中设备！");
         return;
      }
      LoadDirectory(CurrentFileNode);
   }

   /// <summary>
   /// 上传文件
   /// </summary>
   public async Task UploadCmd()
   {
      if (!_globalTemporary.PermissionVerification(_permission, out var msg))
      {
         Growl.Warning(msg);
         return;
      }
      var dialog = new Microsoft.Win32.OpenFileDialog { Multiselect = true };
      if (dialog.ShowDialog() == true)
      {
         foreach (var file in dialog.FileNames)
         {
            await ExecuteInternalUploadAsync(file);
         }
      }
   }

   /// <summary>
   /// 拖拽松开鼠标
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   public void FileButton_Drop(object sender, DragEventArgs e)
   {
      if (!_globalTemporary.PermissionVerification(_permission, out var msg))
      {
         Growl.Warning(msg);
         return;
      }
      if (!e.Data.GetDataPresent(DataFormats.FileDrop))
         return;
      // 获取所有拖入的路径（可能是文件和文件夹的混合）
      string[] droppedPaths = (string[])e.Data.GetData(DataFormats.FileDrop);
      if (droppedPaths == null || !droppedPaths.Any())
         return;

      try
      {
         foreach (var path in droppedPaths)
         {
            _ = ExecuteInternalUploadAsync(path);
         }
      }
      catch (Exception ex)
      {
         // 使用 HandyControl 的 Growl 或 MessageBox 报错
         Growl.Error($"上传过程中发生错误: {ex.Message}");
      }
      finally
      {
         // this.IsBusy = false;
      }
   }

   /// <summary>
   /// 拖拽悬停视觉效果
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   public void FileButton_DragOver(object sender, DragEventArgs e)
   {
      // 必须告诉系统，允许在此处执行“复制”操作
      if (e.Data.GetDataPresent(DataFormats.FileDrop))
      {
         e.Effects = DragDropEffects.Copy; // 鼠标变成带 + 的图标
      }
      else
      {
         e.Effects = DragDropEffects.None;
      }

      // 标记事件已处理，防止被父级拦截
      e.Handled = true;
   }

   /// <summary>
   /// 内部异步处理逻辑：自动识别并分流
   /// </summary>
   private async Task ExecuteInternalUploadAsync(string sourcePath)
   {
      string name = Path.GetFileName(sourcePath);
      // 兼容根目录情况
      if (string.IsNullOrEmpty(name))
         name = sourcePath.Replace(":", "").Replace("\\", "");

      string destPath = Path.Combine(CurrentFileNode.FullPath, name);

      // 1. 判断是文件夹还是文件
      if (Directory.Exists(sourcePath))
      {
         // 异步执行文件夹递归复制
         await Task.Run(() => CopyDirectoryRecursive(sourcePath, destPath));
      }
      else if (File.Exists(sourcePath))
      {
         // 异步执行单文件复制
         await Task.Run(() => File.Copy(sourcePath, destPath, true));
      }

      LoadDirectory(CurrentFileNode);
   }

   /// <summary>
   /// 递归复制文件夹工具方法
   /// </summary>
   private void CopyDirectoryRecursive(string sourceDir, string destDir)
   {
      // 创建目标文件夹
      Directory.CreateDirectory(destDir);

      // 复制当前层级的所有文件
      foreach (string file in Directory.GetFiles(sourceDir))
      {
         string destFile = Path.Combine(destDir, Path.GetFileName(file));
         File.Copy(file, destFile, true);
      }

      // 递归处理子文件夹
      foreach (string subDir in Directory.GetDirectories(sourceDir))
      {
         string newDestDir = Path.Combine(destDir, Path.GetFileName(subDir));
         CopyDirectoryRecursive(subDir, newDestDir);
      }
   }

   /// <summary>
   /// 点击文件
   /// </summary>
   /// <param name="sender"></param>
   /// <param name="e"></param>
   public void FileItem_Click(object sender, MouseButtonEventArgs e)
   {
      if (sender is Border border)
      {
         if (border.DataContext is FileNode node)
         {
            if (node.IsDirectory)
            {
               if (!Directory.Exists(node.FullPath))
               {
                  Growl.Warning("相关文件夹已被删除！");
                  LoadDirectory(CurrentFileNode);
                  return;
               }
               LoadDirectory(node);
            }
            else
            {
               // 如果是文件 -> 打开
               try
               {
                  if (!File.Exists(node.FullPath))
                  {
                     Growl.Warning("相关文件已被删除！");
                     LoadDirectory(CurrentFileNode);
                     return;
                  }
                  Process.Start(new ProcessStartInfo(node.FullPath) { UseShellExecute = true });
               }
               catch { }
            }
         }
      }
   }
}
