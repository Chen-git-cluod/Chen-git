namespace Kinlo.GUI.ViewModel;

[AddINotifyPropertyChangedInterface]
public class FileNode
{
   public string Name { get; set; }
   public string FullPath { get; set; }
   public string RootPath { get; set; }
   public string DisplayPaht { get; set; }
   public bool IsDirectory { get; set; }

   // 这个属性用来绑定到界面上的 Image 控件
   public ImageSource IconSource { get; set; }

   public FileNode(string fullPath, bool isDir, string tootPaht)
   {
      FullPath = fullPath;
      Name = isDir ? Path.GetFileName(fullPath) : Path.GetFileName(fullPath);
      IsDirectory = isDir;
      RootPath = tootPaht;
      DisplayPaht = Path.GetRelativePath(RootPath, fullPath);
      // 加载图标
      // 如果是文件夹，传 path；如果是文件，也可以传 path
      // 注意：SHGetFileInfo 对某些特殊文件可能需要完整路径，普通扩展名文件直接传路径即可
      this.IconSource = SystemIconHelper.GetFileIcon(fullPath);
   }

   public (bool state, FileNode? node) GetParentSafe()
   {
      DirectoryInfo? parent = Directory.GetParent(FullPath);

      if (parent == null)
      {
         return (false, null);
      }

      if (!IsSubPath(RootPath, parent.FullName))
      {
         return (false, null);
      }

      return (true, new FileNode(parent.FullName, true, RootPath));
   }

   public static bool IsSubPath(string rootPath, string fullPath)
   {
      rootPath = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

      fullPath = Path.GetFullPath(fullPath);

      return fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase);
   }
}
