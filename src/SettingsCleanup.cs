using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DeskBloom {
  // User group folders, .groups documents and custom icon assets are never cleanup targets.
  internal static class SettingsCleanup {
    private static readonly string[] files={"state.json","state.json.bak","state.json.tmp","layout-locations.json","layout-locations.json.bak","layout-locations.json.tmp","group-folder-move.json","group-folder-move.json.tmp","retired-desktop-file.json","retired-desktop-file.json.tmp","desktop-pages-transaction.json","desktop-pages-transaction.json.tmp"};
    private static readonly Regex requestName=new Regex("^[a-f0-9]{32}\\.json(?:\\.tmp|\\.invalid)?$",RegexOptions.IgnoreCase|RegexOptions.CultureInvariant);
    internal static bool ProductionRoot(string root){return string.Equals(Path.GetFullPath(root).TrimEnd('\\'),Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"DeskBloom").TrimEnd('\\'),StringComparison.OrdinalIgnoreCase);}
    private static bool Recognized(string root){foreach(string name in new[]{"state.json","state.json.bak"})try{var state=Json.Read<StateData>(Path.Combine(root,name));if(state!=null&&state.Version>=1&&state.Version<=3&&state.Groups!=null)return true;}catch{}return false;}
    internal static bool Remove(string root){root=Path.GetFullPath(root).TrimEnd('\\');if(root.Length==0||string.Equals(root,Path.GetPathRoot(root).TrimEnd('\\'),StringComparison.OrdinalIgnoreCase))return false;bool production=ProductionRoot(root);if(!Directory.Exists(root)){if(production)ShellIntegration.Remove();return true;}if((File.GetAttributes(root)&FileAttributes.ReparsePoint)!=0||(!production&&!Recognized(root)))return false;
      bool success=true;foreach(string name in files)try {string file=Path.Combine(root,name);if(File.Exists(file)){File.SetAttributes(file,FileAttributes.Normal);File.Delete(file);}}catch(IOException){success=false;}catch(UnauthorizedAccessException){success=false;}
      string requests=Path.Combine(root,"Requests");try {if(Directory.Exists(requests)&&(File.GetAttributes(requests)&FileAttributes.ReparsePoint)==0){foreach(string file in Directory.GetFiles(requests))if(requestName.IsMatch(Path.GetFileName(file))){File.SetAttributes(file,FileAttributes.Normal);File.Delete(file);}if(Directory.GetFileSystemEntries(requests).Length==0)Directory.Delete(requests,false);}}catch(IOException){success=false;}catch(UnauthorizedAccessException){success=false;}
      foreach(string name in new[]{"Groups","Files","DesktopPages","Wallpapers","Screenshots","Архив прежних страниц"})try {string folder=Path.Combine(root,name);if(Directory.Exists(folder)&&(File.GetAttributes(folder)&FileAttributes.ReparsePoint)==0&&Directory.GetFileSystemEntries(folder).Length==0)Directory.Delete(folder,false);}catch(IOException){success=false;}catch(UnauthorizedAccessException){success=false;}
      try {if(Directory.GetFileSystemEntries(root).Length==0)Directory.Delete(root,false);}catch(IOException){success=false;}catch(UnauthorizedAccessException){success=false;}
      if(production)try {ShellIntegration.Remove();}catch{success=false;}return success;
    }
  }
}
