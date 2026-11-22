using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace DeskBloom {
  internal class FileEntry {
    internal string Path;internal bool Directory;internal long Modified,Length;
    internal string CacheKey {get {string extension=System.IO.Path.GetExtension(Path).ToLowerInvariant();if(IsImage||Directory||extension==".exe"||extension==".lnk"||extension==".url"||extension==".ico"||extension==".appref-ms")return Path.ToLowerInvariant()+"|"+Modified+"|"+Length;return "type:"+extension;}}
    internal bool IsImage {get {string ext=System.IO.Path.GetExtension(Path).ToLowerInvariant();return !Directory&&(ext==".png"||ext==".jpg"||ext==".jpeg"||ext==".bmp"||ext==".gif");}}
    internal bool Same(FileEntry other){return other!=null&&string.Equals(Path,other.Path,StringComparison.OrdinalIgnoreCase)&&Directory==other.Directory&&Modified==other.Modified&&Length==other.Length;}
  }
  // Shell extensions and image codecs never run on the UI thread. Only visible rows request icons.
  internal static class IconCache {
    private class Cached {internal ImageSource Source;internal LinkedListNode<string> Node;}
    private class Job {internal string Key;internal FileEntry Entry;}
    private static readonly object gate=new object();
    private static readonly Dictionary<string,Cached> cache=new Dictionary<string,Cached>(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string,List<WeakReference>> waiting=new Dictionary<string,List<WeakReference>>(StringComparer.OrdinalIgnoreCase);
    private static readonly LinkedList<string> recent=new LinkedList<string>();
    private static readonly BlockingCollection<Job> queue=new BlockingCollection<Job>(128);
    private static Thread worker;
    internal static int Count {get{lock(gate)return cache.Count;}}
    internal static int Pending {get{lock(gate)return waiting.Count;}}
    [DllImport("ole32.dll")]private static extern int CoInitializeEx(IntPtr reserved,uint flags);
    [DllImport("ole32.dll")]private static extern void CoUninitialize();
    internal static void Request(FileEntry entry,Image target){string key=entry.CacheKey;target.Tag=key;lock(gate){Cached found;if(cache.TryGetValue(key,out found)){recent.Remove(found.Node);recent.AddFirst(found.Node);target.Source=found.Source;return;}List<WeakReference> listeners;if(waiting.TryGetValue(key,out listeners)){listeners.Add(new WeakReference(target));return;}waiting[key]=new List<WeakReference>{new WeakReference(target)};
        if(worker==null){worker=new Thread(Work){IsBackground=true,Name="DeskBloom icons"};worker.SetApartmentState(ApartmentState.STA);worker.Start();}
        var job=new Job {Key=key,Entry=entry};if(!queue.TryAdd(job)){Job stale;if(queue.TryTake(out stale))waiting.Remove(stale.Key);if(!queue.TryAdd(job))waiting.Remove(key);}
      }}
    private static void Work(){int com=CoInitializeEx(IntPtr.Zero,2);try {foreach(var job in queue.GetConsumingEnumerable()){
        ImageSource source=null;try {Interlocked.Increment(ref Perf.IconLoads);source=job.Entry.IsImage?Ui.LoadImage(job.Entry.Path,64):Ui.FileIcon(job.Entry.Path);if(source!=null&&source.CanFreeze&&!source.IsFrozen)source.Freeze();}catch {}
        List<WeakReference> listeners;lock(gate){if(!waiting.TryGetValue(job.Key,out listeners))continue;waiting.Remove(job.Key);var node=recent.AddFirst(job.Key);cache[job.Key]=new Cached {Source=source,Node=node};while(cache.Count>128){string oldest=recent.Last.Value;recent.RemoveLast();cache.Remove(oldest);}}
        var app=Application.Current;if(app==null||app.Dispatcher.HasShutdownStarted)continue;ImageSource ready=source;string key=job.Key;var targets=listeners;try {app.Dispatcher.BeginInvoke(DispatcherPriority.Background,new Action(delegate {foreach(var reference in targets){var image=reference.Target as Image;if(image!=null&&(image.Tag as string)==key)image.Source=ready;}}));}catch(InvalidOperationException){}
      }}finally{if(com>=0)CoUninitialize();}}
  }
}
