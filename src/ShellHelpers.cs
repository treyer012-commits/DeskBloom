using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using Drawing = System.Drawing;
using Forms = System.Windows.Forms;
using IOPath = System.IO.Path;

using System.Reflection;
namespace DeskBloom {
  internal static partial class Win {
    internal delegate bool EnumProc(IntPtr h,IntPtr p);
    [StructLayout(LayoutKind.Sequential)] internal struct POINT {public int X,Y;}
    [StructLayout(LayoutKind.Sequential)] internal struct RECT {public int Left,Top,Right,Bottom;}
    [DllImport("user32.dll")] internal static extern bool EnumWindows(EnumProc callback,IntPtr p);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] internal static extern IntPtr FindWindowEx(IntPtr parent,IntPtr after,string cls,string text);
    [DllImport("user32.dll")] internal static extern IntPtr SetParent(IntPtr child,IntPtr parent);
    [DllImport("user32.dll")] internal static extern IntPtr GetParent(IntPtr child);
    [DllImport("user32.dll")] internal static extern bool IsWindow(IntPtr h);
    [DllImport("user32.dll",EntryPoint="GetWindowLongW")] internal static extern int GetWindowLong(IntPtr h,int index);
    [DllImport("user32.dll",EntryPoint="SetWindowLongW")] internal static extern int SetWindowLong(IntPtr h,int index,int value);
    [DllImport("user32.dll")] internal static extern bool SetWindowPos(IntPtr h,IntPtr insert,int x,int y,int width,int height,uint flags);
    [DllImport("user32.dll")] internal static extern bool ScreenToClient(IntPtr h,ref POINT p);
    [DllImport("user32.dll")] internal static extern bool RegisterHotKey(IntPtr h,int id,uint modifiers,uint key);
    [DllImport("user32.dll")] internal static extern bool UnregisterHotKey(IntPtr h,int id);
    [DllImport("user32.dll")] internal static extern bool DestroyIcon(IntPtr icon);
    [DllImport("user32.dll")] internal static extern bool GetCursorPos(out POINT p);
    [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)] internal struct SHFILEINFO {public IntPtr hIcon;public int iIcon;public uint attributes;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=260)]public string displayName;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=80)]public string typeName;}
    [DllImport("shell32.dll",CharSet=CharSet.Unicode)] internal static extern IntPtr SHGetFileInfo(string path,uint attributes,out SHFILEINFO info,uint size,uint flags);
    [DllImport("shell32.dll")] internal static extern void SHChangeNotify(uint eventId,uint flags,IntPtr item1,IntPtr item2);
    internal static IntPtr DesktopHost() {IntPtr host=IntPtr.Zero;EnumWindows(delegate(IntPtr h,IntPtr p){if(FindWindowEx(h,IntPtr.Zero,"SHELLDLL_DefView",null)!=IntPtr.Zero){host=h;return false;}return true;},IntPtr.Zero);return host;}
  }

  internal static partial class Win {
    [DllImport("user32.dll")] internal static extern bool ShowWindow(IntPtr h,int command);
    [DllImport("user32.dll")] internal static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll")] internal static extern bool GetWindowRect(IntPtr h,out RECT rect);
    [DllImport("user32.dll")] internal static extern IntPtr WindowFromPoint(POINT point);
    [DllImport("user32.dll")] internal static extern IntPtr GetAncestor(IntPtr h,uint flags);
    [DllImport("user32.dll")] internal static extern IntPtr GetWindow(IntPtr h,uint command);
    [DllImport("user32.dll")] internal static extern bool EnumChildWindows(IntPtr h,EnumProc proc,IntPtr parameter);
    [DllImport("user32.dll")] internal static extern uint GetWindowThreadProcessId(IntPtr h,out uint process);
    [DllImport("user32.dll",CharSet=CharSet.Unicode)] internal static extern int GetWindowText(IntPtr h,System.Text.StringBuilder title,int count);
    [DllImport("user32.dll")] internal static extern bool PostMessage(IntPtr h,uint msg,IntPtr w,IntPtr l);
    internal static IntPtr IconView() {var host=DesktopHost();var view=FindWindowEx(host,IntPtr.Zero,"SHELLDLL_DefView",null);return view==IntPtr.Zero?IntPtr.Zero:FindWindowEx(view,IntPtr.Zero,"SysListView32",null);}
    internal static bool CloseLegacyWindows(uint process) {
      bool found=false;EnumProc check=delegate(IntPtr h,IntPtr unused){uint p;GetWindowThreadProcessId(h,out p);if(p==process){var title=new System.Text.StringBuilder(256);GetWindowText(h,title,256);if(title.ToString()=="Desk Bloom — группы рабочего стола"){PostMessage(h,0x10,IntPtr.Zero,IntPtr.Zero);found=true;}}return true;};
      EnumWindows(delegate(IntPtr h,IntPtr p){check(h,p);EnumChildWindows(h,check,p);return true;},IntPtr.Zero);return found;
    }
  }

  internal static class FolderPresentation {
    private static object Call(object target,string member,BindingFlags kind,params object[] args){return target.GetType().InvokeMember(member,kind,null,target,args);}
    private static void Release(params object[] objects){foreach(var value in objects)if(value!=null)try {if(Marshal.IsComObject(value))Marshal.ReleaseComObject(value);}catch {}}
    internal static bool Pinned(string path){object shell=null,folder=null,items=null;try {shell=Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));folder=Call(shell,"NameSpace",BindingFlags.InvokeMethod,"shell:::{679f85cb-0220-4080-b29b-5540cc05aab6}");if(folder==null)return false;items=Call(folder,"Items",BindingFlags.InvokeMethod);int count=Convert.ToInt32(Call(items,"Count",BindingFlags.GetProperty));for(int i=0;i<count;i++){object item=null;try {item=Call(items,"Item",BindingFlags.InvokeMethod,i);var candidate=Call(item,"Path",BindingFlags.GetProperty) as string;if(string.Equals(candidate,path,StringComparison.OrdinalIgnoreCase))return true;}finally {Release(item);}}return false;}catch{return false;}finally {Release(items,folder,shell);}}
    internal static bool Pin(string path){object shell=null,folder=null,item=null;try {shell=Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));folder=Call(shell,"NameSpace",BindingFlags.InvokeMethod,path);if(folder==null)return false;item=Call(folder,"Self",BindingFlags.GetProperty);Call(item,"InvokeVerb",BindingFlags.InvokeMethod,"pintohome");return Pinned(path);}catch{return false;}finally {Release(item,folder,shell);}}
    internal static bool Unpin(string path){object shell=null,folder=null,items=null;try {shell=Activator.CreateInstance(Type.GetTypeFromProgID("Shell.Application"));folder=Call(shell,"NameSpace",BindingFlags.InvokeMethod,"shell:::{679f85cb-0220-4080-b29b-5540cc05aab6}");if(folder==null)return true;items=Call(folder,"Items",BindingFlags.InvokeMethod);int count=Convert.ToInt32(Call(items,"Count",BindingFlags.GetProperty));for(int i=0;i<count;i++){object item=null;try {item=Call(items,"Item",BindingFlags.InvokeMethod,i);if(string.Equals(Call(item,"Path",BindingFlags.GetProperty) as string,path,StringComparison.OrdinalIgnoreCase)){Call(item,"InvokeVerb",BindingFlags.InvokeMethod,"unpinfromhome");return true;}}finally {Release(item);}}return true;}catch{return false;}finally {Release(items,folder,shell);}}
  }

  internal class RequestQueue : IDisposable {
    private readonly MainWindow main;private readonly string directory;private readonly EventWaitHandle signal;private readonly RegisteredWaitHandle wait;private readonly Action<string[]> receive;private bool processing,disposed;
    internal RequestQueue(string root,string token,MainWindow window,Action<string[]> handler=null) {main=window;receive=handler??window.HandleDesktopRequest;directory=IOPath.Combine(root,"Requests");Directory.CreateDirectory(directory);signal=new EventWaitHandle(false,EventResetMode.AutoReset,"Local\\DeskBloom.Request."+token);wait=ThreadPool.RegisterWaitForSingleObject(signal,delegate {if(!disposed)try {main.Dispatcher.BeginInvoke(new Action(Scan));}catch {}},null,Timeout.Infinite,false);if(main.IsLoaded)main.Dispatcher.BeginInvoke(new Action(Scan));else main.Loaded+=delegate {Scan();};}
    internal static bool Enqueue(string root,string token,List<string> paths) {string directory=IOPath.Combine(root,"Requests");Directory.CreateDirectory(directory);string path=IOPath.Combine(directory,Guid.NewGuid().ToString("N")+".json");using(var f=File.Create(path+".tmp"))new DataContractJsonSerializer(typeof(List<string>)).WriteObject(f,paths);File.Move(path+".tmp",path);try {using(var e=EventWaitHandle.OpenExisting("Local\\DeskBloom.Request."+token))e.Set();}catch(WaitHandleCannotBeOpenedException){/* The starting receiver will drain the durable queue after Loaded. */}return true;}
    private void Scan() {if(disposed||processing||!main.IsLoaded)return;processing=true;try {while(!disposed){var files=Directory.GetFiles(directory,"*.json");if(files.Length==0)break;foreach(string request in files){List<string> paths;try {using(var f=File.OpenRead(request))paths=(List<string>)new DataContractJsonSerializer(typeof(List<string>)).ReadObject(f);File.Delete(request);}catch {try {File.Move(request,request+".invalid");}catch {return;}continue;}receive(paths.ToArray());}}}finally {processing=false;}}
    public void Dispose() {disposed=true;wait.Unregister(null);signal.Dispose();}
  }

}