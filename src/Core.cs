using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using Microsoft.VisualBasic.FileIO;
using IOPath=System.IO.Path;

[assembly: System.Reflection.AssemblyVersion("4.1.2.0")]
[assembly: System.Reflection.AssemblyFileVersion("4.1.2.0")]

namespace DeskBloom {
  [DataContract] public class GroupData {
    [DataMember] public string Id=Guid.NewGuid().ToString("N");
    [DataMember] public string Name=L.T("Новая группа");
    [DataMember] public double X=42,Y=36;
    [DataMember] public bool Open=true;
    [DataMember] public int Color;
    [DataMember(EmitDefaultValue=false)] public int? BackgroundColor;
    [DataMember(EmitDefaultValue=false)] public int Icon;
    [DataMember(EmitDefaultValue=false)] public string IconImage;
    [DataMember(EmitDefaultValue=false)] public long IconRevision;
    [DataMember(EmitDefaultValue=false)] public List<TitlePart> TitleParts;
    [DataMember] public string FolderPath;
  }
  // Read-only compatibility fields, retired after the one-time migration.
  [DataContract] internal class LegacyPage {
    [DataMember] internal string Id,StorageFolder,GroupStorageFolder;
    [DataMember] internal List<GroupData> Groups=new List<GroupData>();
  }
  [DataContract] internal class StateData {
    [DataMember] internal int Version=3;
    [DataMember] internal List<GroupData> Groups=new List<GroupData>();
    [DataMember] internal List<string> KnownGroupFolders=new List<string>();
    [DataMember(Name="Pages",EmitDefaultValue=false)] internal List<LegacyPage> LegacyPages;
    [DataMember(Name="Page",EmitDefaultValue=false)] internal int LegacyPage;
    [DataMember] internal bool Migrated;
    [DataMember] internal string Language;
  }
  internal static class Json {
    internal static T Read<T>(string path){using(var file=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete))return (T)new DataContractJsonSerializer(typeof(T)).ReadObject(file);}
    internal static void Write<T>(string path,T value,bool backup=false){byte[] bytes;using(var buffer=new MemoryStream()){new DataContractJsonSerializer(typeof(T)).WriteObject(buffer,value);bytes=buffer.ToArray();}if(File.Exists(path)){try {if(File.ReadAllBytes(path).SequenceEqual(bytes))return;}catch(IOException){}}
      string temp=path+".tmp";using(var stream=new FileStream(temp,FileMode.Create,FileAccess.Write,FileShare.None)){stream.Write(bytes,0,bytes.Length);stream.Flush(true);}for(int attempt=0;;attempt++)try {if(File.Exists(path))File.Replace(temp,path,backup?path+".bak":null,true);else File.Move(temp,path);break;}catch(IOException){if(attempt==4)throw;Thread.Sleep(25);}
    }
  }
  internal class Store {
    internal readonly string Root,Files,StatePath,GroupsRoot,Desktop;
    internal StateData State;
    internal bool Production;
    internal Store(string root,string groupsRoot=null,string desktop=null){Root=Path.GetFullPath(root);Files=Path.Combine(Root,"Files");StatePath=Path.Combine(Root,"state.json");Production=string.Equals(Root,Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),"DeskBloom"),StringComparison.OrdinalIgnoreCase);GroupsRoot=groupsRoot??(Production?DefaultGroupsRoot():Path.Combine(Root,"Groups"));Desktop=desktop??(Production?Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory):Path.Combine(Root,"Desktop"));Directory.CreateDirectory(Root);Directory.CreateDirectory(GroupsRoot);Load();}
    private static string DefaultGroupsRoot(){string docs=Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),ru=Path.Combine(docs,"Группы Desk Bloom"),en=Path.Combine(docs,"DeskBloom Groups");if(Directory.Exists(ru))return ru;if(Directory.Exists(en))return en;return L.SystemLanguage=="ru"?ru:en;}
    private void Load(){Exception failure=null;try {if(File.Exists(StatePath))State=Json.Read<StateData>(StatePath);}catch(Exception e){failure=e;try {State=Json.Read<StateData>(StatePath+".bak");}catch {}}
      if(State==null)State=new StateData();if(State.Language!="en"&&State.Language!="ru")State.Language=File.Exists(StatePath)?"ru":L.SystemLanguage;L.Language=State.Language;if(State.Groups==null)State.Groups=new List<GroupData>();if(State.KnownGroupFolders==null)State.KnownGroupFolders=new List<string>();
      if(State.LegacyPages!=null)foreach(var page in State.LegacyPages)foreach(var group in page.Groups??new List<GroupData>())if(!State.Groups.Any(g=>g.Id==group.Id))State.Groups.Add(group);
      foreach(var group in State.Groups)if(string.IsNullOrEmpty(group.FolderPath))group.FolderPath=Path.Combine(Files,group.Id);
      PortableLayouts.Discover(this);if(failure!=null&&State.Groups.Count==0)throw new IOException(L.T("Не удалось прочитать настройки. Существующие папки и файлы сохранены."),failure);
      var ids=new HashSet<string>(StringComparer.OrdinalIgnoreCase);foreach(var group in State.Groups){Guid id;if(!Guid.TryParseExact(group.Id,"N",out id)||!ids.Add(group.Id))throw new IOException(L.T("Некорректный идентификатор группы."));if(double.IsNaN(group.X)||double.IsInfinity(group.X))group.X=42;if(double.IsNaN(group.Y)||double.IsInfinity(group.Y))group.Y=36;}
    }
    internal void Save(){State.Version=3;State.KnownGroupFolders=State.Groups.Select(GroupPath).Distinct(StringComparer.OrdinalIgnoreCase).ToList();Json.Write(StatePath,State,true);PortableLayouts.Save(this);}
    internal string GroupPath(GroupData group){return Path.GetFullPath(group.FolderPath??Path.Combine(Files,group.Id));}
    internal static string[] ProtectedRoots {get{return new[]{Environment.GetFolderPath(Environment.SpecialFolder.Windows),Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),Environment.GetEnvironmentVariable("ProgramW6432")};}}
    internal static bool Inside(string path,string parent){return (Path.GetFullPath(path).TrimEnd('\\')+"\\").StartsWith(Path.GetFullPath(parent).TrimEnd('\\')+"\\",StringComparison.OrdinalIgnoreCase);}
    internal static string Unique(string parent,string name){string path=Path.Combine(parent,name);if(!File.Exists(path)&&!Directory.Exists(path))return path;string stem=Path.GetFileNameWithoutExtension(name),ext=Path.GetExtension(name);for(int n=2;;n++){path=Path.Combine(parent,stem+" ("+n+")"+ext);if(!File.Exists(path)&&!Directory.Exists(path))return path;}}
    internal static string SafeName(string name){var bad=Path.GetInvalidFileNameChars();string result=new string((name??L.T("Группа")).Select(c=>bad.Contains(c)?'_':c).ToArray()).Trim().TrimEnd('.');if(result.Length>100)result=result.Substring(0,100);if(string.IsNullOrWhiteSpace(result))result=L.T("Группа");string stem=Path.GetFileNameWithoutExtension(result).ToUpperInvariant();if(new[]{"CON","PRN","AUX","NUL","COM1","COM2","COM3","COM4","COM5","COM6","COM7","COM8","COM9","LPT1","LPT2","LPT3","LPT4","LPT5","LPT6","LPT7","LPT8","LPT9"}.Contains(stem))result="_"+result;return result;}
    internal GroupData CreateGroup(string name,string parent=null){if(string.IsNullOrEmpty(parent)||string.Equals(Path.GetFullPath(parent).TrimEnd('\\'),Desktop.TrimEnd('\\'),StringComparison.OrdinalIgnoreCase))parent=GroupsRoot;Directory.CreateDirectory(parent);var group=new GroupData {Name=SafeName(name),Color=State.Groups.Count%4};group.FolderPath=Unique(parent,group.Name);group.Name=Path.GetFileName(group.FolderPath);Directory.CreateDirectory(group.FolderPath);State.Groups.Add(group);Save();return group;}
    internal string MoveIn(string source,GroupData group){source=Path.GetFullPath(source).TrimEnd('\\');string target=GroupPath(group);if(!Directory.Exists(target))throw new IOException(L.T("Папка группы не найдена: ")+target);if(!File.Exists(source)&&!Directory.Exists(source))throw new IOException(L.T("Не найдено: ")+source);foreach(var root in ProtectedRoots)if(!string.IsNullOrEmpty(root)&&Inside(source,root))throw new IOException(L.T("Добавьте существующий ярлык установленной программы; её системные файлы должны оставаться на месте."));
      if(string.Equals(source,System.Reflection.Assembly.GetExecutingAssembly().Location,StringComparison.OrdinalIgnoreCase)||Inside(target,source)||string.Equals(source,Root,StringComparison.OrdinalIgnoreCase)||State.Groups.Any(g=>string.Equals(GroupPath(g),source,StringComparison.OrdinalIgnoreCase)))throw new IOException(L.T("Нельзя перенести эту папку или сам органайзер в группу."));if(string.Equals(Path.GetDirectoryName(source),target,StringComparison.OrdinalIgnoreCase))return source;if(PortableLayouts.IsControlEntry(source))throw new IOException(L.T("Откройте файл .groups двойным щелчком, чтобы добавить саму группу."));string destination=Unique(target,Path.GetFileName(source));if(Directory.Exists(source))FileSystem.MoveDirectory(source,destination,UIOption.OnlyErrorDialogs,UICancelOption.ThrowException);else FileSystem.MoveFile(source,destination,UIOption.OnlyErrorDialogs,UICancelOption.ThrowException);return destination;
    }
  }
  [DataContract] internal class GroupDocument {
    [DataMember] internal string Format="DeskBloom.Group";
    [DataMember] internal int Version=1;
    [DataMember] internal bool Registered=true;
    [DataMember] internal GroupData Group;
  }
  [DataContract] internal class LayoutLocations {[DataMember] internal List<string> Folders=new List<string>();}
  internal static class PortableLayouts {
    internal static GroupDocument Read(string file){try {if(new FileInfo(file).Length>1024*1024)return null;var doc=Json.Read<GroupDocument>(file);Guid id;return doc!=null&&doc.Format=="DeskBloom.Group"&&doc.Version==1&&doc.Group!=null&&Guid.TryParseExact(doc.Group.Id,"N",out id)?doc:null;}catch{return null;}}
    internal static bool IsControlEntry(string path){if(Directory.Exists(path))return GroupAssets.IsOwnedFolder(path);string name=Path.GetFileName(path);return name==".deskbloom-groups"||name==".deskbloom-page"||string.Equals(name,"desktop.ini",StringComparison.OrdinalIgnoreCase)||(string.Equals(Path.GetExtension(path),".groups",StringComparison.OrdinalIgnoreCase)&&Read(path)!=null);}
    internal static string DocumentPath(GroupData group){foreach(var path in Directory.GetFiles(group.FolderPath,"*.groups")){var doc=Read(path);if(doc!=null&&doc.Group.Id==group.Id)return path;}return Store.Unique(group.FolderPath,Store.SafeName(group.Name)+".groups");}
    internal static void Save(Store data){foreach(var group in data.State.Groups){if(!Directory.Exists(data.GroupPath(group)))continue;string path=DocumentPath(group);Json.Write(path,new GroupDocument {Group=group});string desired=Path.Combine(group.FolderPath,Store.SafeName(group.Name)+".groups");if(!string.Equals(path,desired,StringComparison.OrdinalIgnoreCase)&&!File.Exists(desired)&&!Directory.Exists(desired))File.Move(path,desired);}Json.Write(Path.Combine(data.Root,"layout-locations.json"),new LayoutLocations {Folders=data.State.KnownGroupFolders});}
    private static string[] Children(string path){try{return Directory.GetDirectories(path).Where(p=>(File.GetAttributes(p)&FileAttributes.ReparsePoint)==0).ToArray();}catch{return new string[0];}}
    internal static void Discover(Store data){var folders=new HashSet<string>(data.State.KnownGroupFolders,StringComparer.OrdinalIgnoreCase);foreach(var group in data.State.Groups)folders.Add(data.GroupPath(group));foreach(string child in Children(data.GroupsRoot))folders.Add(child);try {foreach(string folder in Json.Read<LayoutLocations>(Path.Combine(data.Root,"layout-locations.json")).Folders)folders.Add(folder);}catch{}
      if(data.Production)foreach(string child in Children(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"Downloads")))folders.Add(child);
      foreach(string folder in folders){if(!Directory.Exists(folder))continue;string[] files;try{files=Directory.GetFiles(folder,"*.groups");}catch{continue;}foreach(string file in files){var doc=Read(file);if(doc==null||!doc.Registered)continue;var prior=data.State.Groups.FirstOrDefault(g=>g.Id==doc.Group.Id);if(prior==null){doc.Group.FolderPath=folder;data.State.Groups.Add(doc.Group);}else if(!Directory.Exists(data.GroupPath(prior)))prior.FolderPath=folder;}}
    }
    internal static GroupData Import(Store data,string path){var doc=Read(path);if(doc==null)throw new IOException(L.T("Это не файл группы Desk Bloom."));var group=data.State.Groups.FirstOrDefault(g=>g.Id==doc.Group.Id);if(group==null){group=doc.Group;group.FolderPath=Path.GetDirectoryName(Path.GetFullPath(path));data.State.Groups.Add(group);}group.Open=true;data.Save();return group;}
    internal static void Detach(Store data,GroupData group){if(Directory.Exists(group.FolderPath))Json.Write(DocumentPath(group),new GroupDocument {Registered=false,Group=group});data.State.Groups.Remove(group);data.Save();}
  }
}
