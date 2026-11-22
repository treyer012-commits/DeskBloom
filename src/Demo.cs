using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DeskBloom {
  // Isolated, deterministic examples of the real GroupCard controls for documentation.
  internal static class Demo {
    private static void Wait(Window window,Canvas canvas){var deadline=DateTime.UtcNow.AddSeconds(8);do{window.UpdateLayout();var frame=new DispatcherFrame();var timer=new DispatcherTimer {Interval=TimeSpan.FromMilliseconds(25)};timer.Tick+=delegate {timer.Stop();frame.Continue=false;};timer.Start();Dispatcher.PushFrame(frame);}while((canvas.Children.OfType<GroupCard>().Any(c=>!c.ContentReady)||IconCache.Pending>0)&&DateTime.UtcNow<deadline);window.UpdateLayout();}
    private static TextBlock Label(string text,double size,string color){return Ui.Text(text,size,Ui.Brush(color));}
    private static void At(Canvas canvas,UIElement element,double x,double y){Canvas.SetLeft(element,x);Canvas.SetTop(element,y);canvas.Children.Add(element);}
    private static void SampleImage(string path){var drawing=new DrawingVisual();using(var dc=drawing.RenderOpen()){dc.DrawRectangle(new LinearGradientBrush(Color.FromRgb(77,69,140),Color.FromRgb(43,139,158),35),null,new Rect(0,0,160,100));dc.DrawRoundedRectangle(new SolidColorBrush(Color.FromRgb(198,181,255)),null,new Rect(20,20,120,18),8,8);for(int i=0;i<3;i++)dc.DrawRoundedRectangle(Brushes.White,null,new Rect(22+i*42,52,32,30),6,6);}var bitmap=new RenderTargetBitmap(160,100,96,96,PixelFormats.Pbgra32);bitmap.Render(drawing);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using(var file=File.Create(path))encoder.Save(file);}
    internal static int Render(string output,string language){output=Path.GetFullPath(output);Directory.CreateDirectory(Path.GetDirectoryName(output));string demoRoot=Path.Combine(Path.GetDirectoryName(output),"demo-data-"+Guid.NewGuid().ToString("N"));var data=new Store(demoRoot);data.State.Language=language=="ru"?"ru":"en";L.Language=data.State.Language;bool ru=L.Language=="ru";
      var work=data.CreateGroup(ru?"Работа":"Work");work.Open=true;work.Color=0;work.Icon=2;foreach(string name in ru?new[]{"План.txt","Задачи.md","Бюджет.csv","Заметки.txt"}:new[]{"Brief.txt","Roadmap.md","Budget.csv","Notes.txt"})File.WriteAllText(Path.Combine(work.FolderPath,name),"DeskBloom demo content");Directory.CreateDirectory(Path.Combine(work.FolderPath,ru?"Материалы":"Assets"));SampleImage(Path.Combine(work.FolderPath,ru?"Палитра.png":"Palette.png"));
      var ideas=data.CreateGroup(ru?"Идеи":"Ideas");ideas.Open=true;ideas.Color=1;ideas.Icon=10;ideas.BackgroundColor=4;Directory.CreateDirectory(Path.Combine(ideas.FolderPath,ru?"Референсы":"References"));File.WriteAllText(Path.Combine(ideas.FolderPath,ru?"Прочитать.txt":"Read later.txt"),"DeskBloom demo content");File.WriteAllText(Path.Combine(ideas.FolderPath,ru?"Наброски.md":"Sketches.md"),"DeskBloom demo content");
      var personal=data.CreateGroup(ru?"Личное":"Personal");personal.Open=false;personal.Color=3;personal.Icon=4;foreach(string name in ru?new[]{"Поездки.txt","Покупки.txt","Книги.md"}:new[]{"Trips.txt","Shopping.txt","Books.md"})File.WriteAllText(Path.Combine(personal.FolderPath,name),"DeskBloom demo content");data.Save();
      var app=new Application {ShutdownMode=ShutdownMode.OnExplicitShutdown};var controller=new MainWindow(data){Quiet=true};var canvas=new Canvas {Width=1000,Height=660,Background=new LinearGradientBrush(Color.FromRgb(17,27,45),Color.FromRgb(33,49,70),35)};
      var brand=Label("DeskBloom",32,"#F1F4FF");brand.FontWeight=FontWeights.SemiBold;At(canvas,brand,42,27);At(canvas,Label(ru?"Группы на обычном рабочем столе Windows":"Groups on your Windows desktop",14,"#AABBD0"),44,74);
      At(canvas,new GroupCard(controller,work),44,130);At(canvas,new GroupCard(controller,ideas),602,174);At(canvas,new GroupCard(controller,personal),44,510);
      At(canvas,Label(ru?"Нажми на заголовок — раскроется сетка файлов.":"Click a group header to reveal its files.",13,"#C5D4E8"),604,424);At(canvas,Label(ru?"В Проводнике каждая группа — обычная папка.":"Each group is an ordinary folder in File Explorer.",12,"#93A9C2"),604,452);
      At(canvas,Label("4.1.2 · "+(ru?"Русский":"English"),12,"#93A9C2"),44,620);At(canvas,Label(ru?"Демонстрационные файлы · настоящий интерфейс":"Demo files · actual app controls",11,"#849CB8"),724,622);
      var window=new Window {Content=canvas,WindowStyle=WindowStyle.None,ResizeMode=ResizeMode.NoResize,SizeToContent=SizeToContent.WidthAndHeight,ShowInTaskbar=false,ShowActivated=false,Opacity=0};window.Show();Wait(window,canvas);app.Dispatcher.Invoke(new Action(delegate {}),DispatcherPriority.Render);Ui.Render(canvas,output);
      // A second view shows the same real controls with every group collapsed.
      foreach(var card in canvas.Children.OfType<GroupCard>().ToList()){card.Dispose();canvas.Children.Remove(card);}work.Open=false;ideas.Open=false;At(canvas,new GroupCard(controller,work),44,130);At(canvas,new GroupCard(controller,ideas),44,232);At(canvas,new GroupCard(controller,personal),44,334);Wait(window,canvas);Ui.Render(canvas,Path.Combine(Path.GetDirectoryName(output),Path.GetFileNameWithoutExtension(output)+"-collapsed.png"));window.Close();app.Shutdown();return 0;
    }
  }
}
