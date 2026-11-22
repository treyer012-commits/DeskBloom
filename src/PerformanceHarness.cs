using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace DeskBloom {
  [DataContract] internal class PerfResult {
    [DataMember] internal string Scenario;
    [DataMember] internal int Groups,FilesPerGroup,StartupIconLoads,StartupTiles,TotalIconLoads,TotalTiles;
    [DataMember] internal double ReadyMs,OpenCallMs,OpenReadyMs,ReopenReadyMs,RefreshBatchMs,IdleCpuMs;
    [DataMember] internal bool ReusedWindows;
    [DataMember] internal long StartupManagedBytes,FinalManagedBytes;
  }
  internal static class Perf {
    internal static int IconLoads,FileTiles;
    private static void Pump(int ms){var frame=new DispatcherFrame();var timer=new DispatcherTimer {Interval=TimeSpan.FromMilliseconds(ms)};timer.Tick+=delegate {timer.Stop();frame.Continue=false;};timer.Start();Dispatcher.PushFrame(frame);}
    private static void Ready(MainWindow main){var deadline=DateTime.UtcNow.AddSeconds(12);while(DateTime.UtcNow<deadline){if(main.Cards.Count==main.Data.State.Groups.Count&&main.Cards.All(c=>{var property=c.GetType().GetProperty("ContentReady",BindingFlags.NonPublic|BindingFlags.Instance);return property==null||(bool)property.GetValue(c,null);}))break;Pump(10);}foreach(var w in main.GroupWindows)((FrameworkElement)w.Content).UpdateLayout();Application.Current.Dispatcher.Invoke(new Action(delegate {}),DispatcherPriority.Render);}
    private static void Sample(string output,string stage){File.WriteAllText(Path.Combine(output,stage+".ready"),Process.GetCurrentProcess().Id.ToString());var deadline=DateTime.UtcNow.AddSeconds(12);while(!File.Exists(Path.Combine(output,stage+".continue"))&&DateTime.UtcNow<deadline)Pump(25);}
    internal static int Run(string output,string scenario){Directory.CreateDirectory(output);var result=new PerfResult {Scenario=scenario,Groups=4,FilesPerGroup=scenario=="large"?120:12};try{
      var data=new Store(Path.Combine(output,"Data"));data.State.Language="en";L.Language="en";for(int n=0;n<result.Groups;n++){var group=data.CreateGroup("Group "+(n+1));group.Open=false;group.X=30+n*348;group.Y=100;for(int f=0;f<result.FilesPerGroup;f++)File.WriteAllText(Path.Combine(group.FolderPath,"File "+f.ToString("000")+".txt"),"Performance fixture");}data.Save();IconLoads=0;FileTiles=0;
      var app=new Application {ShutdownMode=ShutdownMode.OnExplicitShutdown};var main=new MainWindow(data){Quiet=true};var timer=Stopwatch.StartNew();main.Show();Ready(main);result.ReadyMs=timer.Elapsed.TotalMilliseconds;Pump(250);result.StartupIconLoads=IconLoads;result.StartupTiles=FileTiles;result.StartupManagedBytes=GC.GetTotalMemory(false);Sample(output,"closed");
      var cpu=Process.GetCurrentProcess().TotalProcessorTime;Pump(1500);result.IdleCpuMs=(Process.GetCurrentProcess().TotalProcessorTime-cpu).TotalMilliseconds;
      var first=main.Cards[0];timer.Restart();first.Toggle();result.OpenCallMs=timer.Elapsed.TotalMilliseconds;Ready(main);result.OpenReadyMs=timer.Elapsed.TotalMilliseconds;Pump(260);first.Toggle();Pump(260);timer.Restart();first.Toggle();Ready(main);result.ReopenReadyMs=timer.Elapsed.TotalMilliseconds;Pump(260);first.Toggle();Pump(260);
      var handles=main.GroupWindows.Select(w=>new WindowInteropHelper(w).Handle).ToArray();timer.Restart();for(int n=0;n<3;n++)main.RenderCurrent(false);Ready(main);result.RefreshBatchMs=timer.Elapsed.TotalMilliseconds;result.ReusedWindows=handles.SequenceEqual(main.GroupWindows.Select(w=>new WindowInteropHelper(w).Handle));Pump(300);result.TotalIconLoads=IconLoads;result.TotalTiles=FileTiles;result.FinalManagedBytes=GC.GetTotalMemory(false);Sample(output,"refreshed");main.Close();app.Shutdown();Json.Write(Path.Combine(output,"result.json"),result);return 0;
    }catch(Exception e){File.WriteAllText(Path.Combine(output,"error.txt"),e.ToString());return 1;}}
  }
}
