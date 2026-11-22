using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace DeskBloom {
  internal static class ToggleTests {
    private static void Assert(bool condition,string message){if(!condition)throw new Exception(message);}
    private static void Pump(int ms){var frame=new DispatcherFrame();var timer=new DispatcherTimer {Interval=TimeSpan.FromMilliseconds(ms)};timer.Tick+=delegate {timer.Stop();frame.Continue=false;};timer.Start();Dispatcher.PushFrame(frame);}
    private static IEnumerable<T> Children<T>(DependencyObject root)where T:DependencyObject {for(int n=0;n<VisualTreeHelper.GetChildrenCount(root);n++){var child=VisualTreeHelper.GetChild(root,n);if(child is T)yield return (T)child;foreach(var next in Children<T>(child))yield return next;}}
    private static Border Body(GroupCard card){return (Border)((StackPanel)card.Child).Children[1];}
    private static void Check(GroupCard card,string stage,string directory){var body=Body(card);card.UpdateLayout();var rows=Children<FileRowView>(card).ToArray();File.AppendAllText(Path.Combine(directory,"toggle-trace.txt"),stage+" Open="+card.Data.Open+" Height="+body.ActualHeight+" Specified="+body.Height+" Opacity="+body.Opacity+" Visible="+body.Visibility+" Rows="+rows.Length+" Labels="+Children<TextBlock>(body).Count()+Environment.NewLine);Ui.Render(card,Path.Combine(directory,"toggle-"+stage+".png"));if(card.Data.Open){Assert(body.Visibility==Visibility.Visible&&body.Opacity>.99&&body.ActualHeight>100,stage+": expanded body must be visible");Assert(rows.Length>0&&Children<TextBlock>(body).Any(t=>t.Text=="File 0.txt"),stage+": file grid must actually be visible");Assert(Children<FileViewport>(body).Single().ActualHeight>0&&Children<FileViewport>(body).Single().IsHitTestVisible,stage+": viewport clickable");}else Assert(body.Visibility==Visibility.Collapsed&&body.ActualHeight<1&&body.Child==null,stage+": collapsed body must release content");}
    internal static void Run(MainWindow main,string directory,List<string> log){var group=main.Data.CreateGroup("Toggle regression");group.Open=false;for(int n=0;n<12;n++)File.WriteAllText(Path.Combine(group.FolderPath,"File "+n+".txt"),"Fixture");main.RenderCurrent(false);var card=main.Cards.Single(c=>c.Data==group);var deadline=DateTime.UtcNow.AddSeconds(8);while(!card.ContentReady&&DateTime.UtcNow<deadline)Pump(20);Assert(card.ContentReady,"toggle fixture metadata");
      card.Toggle();Pump(320);Check(card,"first-open",directory);card.Toggle();Pump(320);Check(card,"first-close",directory);card.Toggle();Pump(320);Check(card,"reopen",directory);
      for(int i=0;i<8;i++){card.Toggle();Pump(35);}Pump(320);Check(card,"rapid-even",directory);for(int i=0;i<7;i++){card.Toggle();Pump(25);}Pump(320);Check(card,"rapid-odd",directory);
      if(!group.Open)card.Toggle();Pump(320);File.WriteAllText(Path.Combine(group.FolderPath,"Changed.txt"),"Changed");card.LoadItems(false);Pump(400);Check(card,"refresh-open",directory);card.Toggle();Pump(40);card.LoadItems(false);Pump(320);Check(card,"refresh-closing",directory);card.Toggle();Pump(320);Check(card,"open-after-refresh",directory);
      PortableLayouts.Detach(main.Data,group);main.RenderCurrent(false);log.Add("PASS: first animated expansion, collapse/reopen, interrupted rapid toggles and asynchronous refresh render visible clickable file rows and release collapsed content.");}
  }
}
