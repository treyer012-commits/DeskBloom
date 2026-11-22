using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DeskBloom {
  [DataContract] internal class FolderMove {
    [DataMember] internal string GroupId,Source,Destination,Name;
  }
  [DataContract] internal class OldDesktopMove {[DataMember] internal string Source,Destination;[DataMember] internal bool Completed;}
  [DataContract] internal class OldDesktopJournal {[DataMember] internal string From,To;[DataMember] internal List<OldDesktopMove> Moves;}
  internal static class Migration {
    internal static bool Exists(string path){return File.Exists(path)||Directory.Exists(path);}
    private static void Move(string source,string destination){if(Directory.Exists(source))Directory.Move(source,destination);else File.Move(source,destination);}
    private static void Rebase(Store data,string source,string destination){foreach(var group in data.State.Groups)if(Store.Inside(group.FolderPath,source))group.FolderPath=Path.Combine(destination,group.FolderPath.Substring(source.TrimEnd('\\').Length).TrimStart('\\'));}
    private static void FinishMove(Store data,FolderMove item,string journal){if(!Exists(item.Source)&&Exists(item.Destination)){if(!string.IsNullOrEmpty(item.GroupId)){var group=data.State.Groups.Single(g=>g.Id==item.GroupId);group.FolderPath=item.Destination;group.Name=item.Name;}else Rebase(data,item.Source,item.Destination);data.Save();File.Delete(journal);return;}if(Exists(item.Source)&&!Exists(item.Destination)){Move(item.Source,item.Destination);FinishMove(data,item,journal);return;}throw new IOException(L.T("Неоднозначный перенос. Файлы сохранены: ")+item.Source+L.T(" и ")+item.Destination);}
    internal static void Recover(Store data){string groupJournal=Path.Combine(data.Root,"group-folder-move.json");if(File.Exists(groupJournal))FinishMove(data,Json.Read<FolderMove>(groupJournal),groupJournal);string fileJournal=Path.Combine(data.Root,"retired-desktop-file.json");if(File.Exists(fileJournal))FinishMove(data,Json.Read<FolderMove>(fileJournal),fileJournal);}
    internal static void MoveGroup(Store data,GroupData group,string parent,string name){Recover(data);parent=Path.GetFullPath(parent);if(string.Equals(parent.TrimEnd('\\'),data.Desktop.TrimEnd('\\'),StringComparison.OrdinalIgnoreCase))parent=data.GroupsRoot;Directory.CreateDirectory(parent);string source=data.GroupPath(group);if(Store.Inside(parent,source))throw new IOException(L.T("Нельзя переместить группу внутрь самой себя."));string target=Path.Combine(parent,Store.SafeName(name));if(string.Equals(source,target,StringComparison.OrdinalIgnoreCase)){group.Name=Path.GetFileName(target);data.Save();return;}target=Store.Unique(parent,Path.GetFileName(target));if(!string.Equals(Path.GetPathRoot(source),Path.GetPathRoot(target),StringComparison.OrdinalIgnoreCase))throw new IOException(L.T("Выберите папку на том же диске для переноса группы."));var move=new FolderMove {GroupId=group.Id,Source=source,Destination=target,Name=Path.GetFileName(target)};string journal=Path.Combine(data.Root,"group-folder-move.json");Json.Write(journal,move);try {FinishMove(data,move,journal);}catch {if(Directory.Exists(source)&&!Directory.Exists(target))File.Delete(journal);throw;}}
    private static bool Container(string path){try{return File.ReadAllText(Path.Combine(path,".deskbloom-groups")).Trim()=="DeskBloom.Groups/1";}catch{return false;}}
    private static bool PageDocument(string path){if(!string.Equals(Path.GetExtension(path),".desk",StringComparison.OrdinalIgnoreCase)||Directory.Exists(path))return false;try{return File.ReadAllText(path).Contains("DeskBloom.Desktop");}catch{return false;}}
    private static void RecoverOldTransaction(Store data){string path=Path.Combine(data.Root,"desktop-pages-transaction.json");if(!File.Exists(path))return;var journal=Json.Read<OldDesktopJournal>(path);var pages=data.State.LegacyPages;if(pages==null||journal.Moves==null)throw new IOException(L.T("Сначала необходимо восстановить старое переключение страниц."));var active=pages[Math.Max(0,Math.Min(data.State.LegacyPage,pages.Count-1))];if(active.Id!=journal.To||!journal.Moves.All(m=>m.Completed))foreach(var move in journal.Moves.AsEnumerable().Reverse()){
        string a=Path.GetDirectoryName(Path.GetFullPath(move.Source)),b=Path.GetDirectoryName(Path.GetFullPath(move.Destination));if(!new[]{a,b}.Any(p=>string.Equals(p,data.Desktop,StringComparison.OrdinalIgnoreCase))||!pages.Any(p=>new[]{a,b}.Any(q=>string.Equals(q,p.StorageFolder??Path.Combine(data.Root,"DesktopPages",p.Id),StringComparison.OrdinalIgnoreCase)))||Path.GetFileName(move.Source)!=Path.GetFileName(move.Destination))throw new IOException(L.T("Некорректный путь в старом журнале переноса."));
        if(!Exists(move.Source)&&Exists(move.Destination))Move(move.Destination,move.Source);else if(move.Completed&&(!Exists(move.Source)||Exists(move.Destination)))throw new IOException(L.T("Не удалось однозначно восстановить старый перенос."));
      }File.Delete(path);
    }
    internal static List<string> Run(Store data){var pending=new List<string>();RecoverOldTransaction(data);try{Recover(data);}catch(IOException e){pending.Add(e.Message);return pending;}
      foreach(var group in data.State.Groups.ToList()){
        string source=data.GroupPath(group),parent=Path.GetDirectoryName(source);bool managed=Store.Inside(source,data.Files)||Container(parent)||string.Equals(parent,data.Desktop,StringComparison.OrdinalIgnoreCase)||(data.State.LegacyPages!=null&&data.State.LegacyPages.Any(p=>string.Equals(parent,p.StorageFolder,StringComparison.OrdinalIgnoreCase)));
        if(!managed||string.Equals(parent,data.GroupsRoot,StringComparison.OrdinalIgnoreCase)||!Directory.Exists(source))continue;
        try{MoveGroup(data,group,data.GroupsRoot,group.Name);}catch(IOException){pending.Add(group.Name);}catch(UnauthorizedAccessException){pending.Add(group.Name);}
      }
      if(data.State.LegacyPages!=null)foreach(var page in data.State.LegacyPages){string root=page.StorageFolder??Path.Combine(data.Root,"DesktopPages",page.Id);if(!Directory.Exists(root))continue;foreach(string path in Directory.GetFileSystemEntries(root)){
        if(PortableLayouts.IsControlEntry(path)||PageDocument(path)||Container(path)||data.State.Groups.Any(g=>Store.Inside(g.FolderPath,path)))continue;
        try {Directory.CreateDirectory(data.Desktop);var move=new FolderMove {Source=path,Destination=Store.Unique(data.Desktop,Path.GetFileName(path))};string journal=Path.Combine(data.Root,"retired-desktop-file.json");Json.Write(journal,move);FinishMove(data,move,journal);}catch(IOException){pending.Add(Path.GetFileName(path));return pending;}catch(UnauthorizedAccessException){pending.Add(Path.GetFileName(path));return pending;}
      }}
      if(pending.Count==0){data.State.LegacyPages=null;data.State.LegacyPage=0;data.State.Migrated=true;}data.Save();return pending;
    }
  }
}
