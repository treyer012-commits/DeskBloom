using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace DeskBloom {
  internal static class MemoryProbe {
    private static readonly List<WeakReference> dialogs=new List<WeakReference>();
    private static void Pump(int ms){var frame=new DispatcherFrame();var timer=new DispatcherTimer {Interval=TimeSpan.FromMilliseconds(ms)};timer.Tick+=delegate {timer.Stop();frame.Continue=false;};timer.Start();Dispatcher.PushFrame(frame);}
    private static IEnumerable<T> Children<T>(DependencyObject root)where T:DependencyObject{for(int n=0;n<VisualTreeHelper.GetChildrenCount(root);n++){var child=VisualTreeHelper.GetChild(root,n);if(child is T)yield return (T)child;foreach(var next in Children<T>(child))yield return next;}}
    private static void Click(Button button){button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));}
    private static void Dialog(Window window,Action action){dialogs.Add(new WeakReference(window));window.ShowActivated=false;window.Loaded+=delegate {window.Dispatcher.BeginInvoke(DispatcherPriority.Background,new Action(delegate{action();}));};window.ShowDialog();Pump(40);}
    private static void AppearanceDialog(MainWindow main,int round){var group=main.Data.State.Groups[round%main.Data.State.Groups.Count];var window=new AppearanceWindow(main,group);Dialog(window,delegate {var swatches=Children<Button>(window).Where(b=>AutomationProperties.GetName(b).StartsWith(L.T("Цвет")+" ")).ToArray();for(int n=0;n<10;n++)Click(swatches[(n+round)%30]);Click(Children<Button>(window).Single(b=>AutomationProperties.GetName(b)==L.T("Сохранить")));});}
    private static void RenameDialog(MainWindow main,int round){var group=main.Data.State.Groups[round%main.Data.State.Groups.Count];var window=new RenameWindow(main,group);Dialog(window,delegate {window.Editor.SelectAll();Click(Children<Button>(window).Single(b=>AutomationProperties.GetName(b)==L.T("Жирный")));Click(Children<Button>(window).Where(b=>AutomationProperties.GetName(b).StartsWith(L.T("Цвет")+" ")).ElementAt(round%30));Click(Children<Button>(window).Single(b=>AutomationProperties.GetName(b)==L.T("Сохранить")));});}
    private static void Stage(string output,string phase){Pump(400);var process=Process.GetCurrentProcess();Json.Write(Path.Combine(output,phase+"-internal.json"),new MemorySnapshot {ManagedBytes=GC.GetTotalMemory(false),PrivateBytes=process.PrivateMemorySize64,WorkingSet=process.WorkingSet64,LiveDialogReferences=dialogs.Count(w=>w.IsAlive),RenderMode=RenderOptions.ProcessRenderMode.ToString(),GraphicsModules=process.Modules.Cast<ProcessModule>().Where(m=>m.ModuleName.StartsWith("amd",StringComparison.OrdinalIgnoreCase)||m.ModuleName.StartsWith("nv",StringComparison.OrdinalIgnoreCase)||m.ModuleName.StartsWith("d3d",StringComparison.OrdinalIgnoreCase)).Select(m=>m.ModuleName).ToArray()});File.WriteAllText(Path.Combine(output,phase+".ready"),process.Id.ToString());var deadline=DateTime.UtcNow.AddSeconds(20);while(!File.Exists(Path.Combine(output,phase+".continue"))&&DateTime.UtcNow<deadline)Pump(20);}
    internal static int Run(string output,string mode){Directory.CreateDirectory(output);try {RenderOptions.ProcessRenderMode=mode=="software"?RenderMode.SoftwareOnly:RenderMode.Default;var data=new Store(Path.Combine(output,"Data"));data.State.Language="ru";L.Language="ru";for(int n=0;n<5;n++){var group=data.CreateGroup("Memory fixture "+n);group.Open=false;group.X=20+n*90;group.Y=35+n*85;for(int f=0;f<18;f++)File.WriteAllText(Path.Combine(group.FolderPath,"Document "+f+".txt"),"Fixture");}data.Save();var app=new Application {ShutdownMode=ShutdownMode.OnExplicitShutdown};var main=new MainWindow(data);main.Show();Pump(800);Stage(output,"cold");AppearanceDialog(main,0);Stage(output,"appearance");RenameDialog(main,0);Stage(output,"rename");for(int n=1;n<=10;n++){AppearanceDialog(main,n);RenameDialog(main,n);}Stage(output,"dialogs");var cpu=Process.GetCurrentProcess().TotalProcessorTime;for(int n=0;n<10;n++){foreach(var card in main.Cards)card.Toggle();Pump(260);foreach(var card in main.Cards)card.Toggle();Pump(260);}File.WriteAllText(Path.Combine(output,"toggle-cpu-ms.txt"),(Process.GetCurrentProcess().TotalProcessorTime-cpu).TotalMilliseconds.ToString(System.Globalization.CultureInfo.InvariantCulture));Stage(output,"toggles");Pump(2500);Stage(output,"idle");main.Close();app.Shutdown();return 0;}catch(Exception error){File.WriteAllText(Path.Combine(output,"error.txt"),error.ToString());return 1;}}
  }
  [System.Runtime.Serialization.DataContract] internal class MemorySnapshot {
    [System.Runtime.Serialization.DataMember] internal long ManagedBytes,PrivateBytes,WorkingSet;
    [System.Runtime.Serialization.DataMember] internal int LiveDialogReferences;
    [System.Runtime.Serialization.DataMember] internal string RenderMode;
    [System.Runtime.Serialization.DataMember] internal string[] GraphicsModules;
  }
}
