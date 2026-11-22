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

namespace DeskBloom {
  internal static class Ui {
    internal static readonly Color[] Accents={Color.FromRgb(159,140,255),Color.FromRgb(106,209,206),Color.FromRgb(255,183,130),Color.FromRgb(236,145,183)};
    internal static Brush Ink=new SolidColorBrush(Color.FromRgb(239,243,255));
    internal static Brush Muted=new SolidColorBrush(Color.FromRgb(158,173,198));
    private static readonly Dictionary<string,Brush> brushCache=new Dictionary<string,Brush>(); private static readonly FontFamily font=new FontFamily("Segoe UI"); private static ControlTemplate buttonTemplate;
    static Ui(){Ink.Freeze();Muted.Freeze();}
    internal static Brush Brush(string hex) {Brush value;if(brushCache.TryGetValue(hex,out value))return value;var brush=new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));brush.Freeze();brushCache[hex]=brush;return brush;}
    internal static TextBlock Text(string text,double size,Brush color) {return new TextBlock {Text=text,FontFamily=font,FontSize=size,Foreground=color,VerticalAlignment=VerticalAlignment.Center};}
    internal static Border Rounded(UIElement child,string color,double radius,Thickness padding) {return new Border {Child=child,Background=Brush(color),CornerRadius=new CornerRadius(radius),Padding=padding};}
    internal static Button Button(string text,Action click,bool accent) {
      var b=new Button {Content=text,Foreground=accent?Brush("#111626"):Ink,Background=accent?Brush("#C4B5FD"):Brush("#26344B"),BorderBrush=Brushes.Transparent,BorderThickness=new Thickness(0),FontFamily=font,FontSize=13,Padding=new Thickness(15,9,15,9),Cursor=Cursors.Hand,Margin=new Thickness(4,0,4,0),Focusable=true};
      if(buttonTemplate==null){var template=new ControlTemplate(typeof(Button));var border=new FrameworkElementFactory(typeof(Border));border.SetValue(Border.CornerRadiusProperty,new CornerRadius(10));border.SetValue(Border.BackgroundProperty,new TemplateBindingExtension(Control.BackgroundProperty));border.SetValue(Border.BorderBrushProperty,new TemplateBindingExtension(Control.BorderBrushProperty));border.SetValue(Border.BorderThicknessProperty,new TemplateBindingExtension(Control.BorderThicknessProperty));border.SetValue(Border.PaddingProperty,new TemplateBindingExtension(Control.PaddingProperty));var presenter=new FrameworkElementFactory(typeof(ContentPresenter));presenter.SetValue(ContentPresenter.HorizontalAlignmentProperty,HorizontalAlignment.Center);presenter.SetValue(ContentPresenter.VerticalAlignmentProperty,VerticalAlignment.Center);border.AppendChild(presenter);template.VisualTree=border;buttonTemplate=template;}b.Template=buttonTemplate;
      string glyph=text==L.T("+ Папка")?"folder":text==L.T("+ Файлы")?"file":text==L.T("Сохранить")?"save":text==L.T("Загрузить изображение…")?"image":text==L.T("Отмена")?"cancel":null;if(glyph!=null){var content=new StackPanel {Orientation=Orientation.Horizontal};content.Children.Add(new Image {Source=Appearance.ActionGlyph(glyph,b.Foreground),Width=15,Height=15,Margin=new Thickness(0,0,7,0)});content.Children.Add(Text(text.TrimStart('+',' '),13,b.Foreground));b.Content=content;}
      var scale=new ScaleTransform(1,1);b.RenderTransform=scale;b.RenderTransformOrigin=new Point(.5,.5);Action<double> zoom=delegate(double value){Animate(scale,ScaleTransform.ScaleXProperty,scale.ScaleX,value,110);Animate(scale,ScaleTransform.ScaleYProperty,scale.ScaleY,value,110);};b.MouseEnter+=delegate {Animate(b,UIElement.OpacityProperty,b.Opacity,.86,130);};b.MouseLeave+=delegate {Animate(b,UIElement.OpacityProperty,b.Opacity,1,130);zoom(1);};b.PreviewMouseLeftButtonDown+=delegate {zoom(.96);};b.PreviewMouseLeftButtonUp+=delegate {zoom(1);};b.LostMouseCapture+=delegate {zoom(1);};b.IsEnabledChanged+=delegate {b.Opacity=b.IsEnabled?1:.45;};b.Click+=delegate {click();};AutomationProperties.SetName(b,text);return b;
    }
    internal static MenuItem Menu(string text,Action click) {var m=new MenuItem {Header=text};m.Click+=delegate {click();};return m;}
    internal static bool IsButtonSource(DependencyObject source,DependencyObject stop){while(source!=null&&source!=stop){if(source is Button)return true;var content=source as FrameworkContentElement;source=content!=null?content.Parent:(source is Visual?VisualTreeHelper.GetParent(source):null);}return false;}
    internal static BitmapImage LoadImage(string path,int width) {path=IOPath.GetFullPath(path);int pixelsWide=0,pixelsHigh=0;if(width>0)using(var header=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete)){var decoder=BitmapDecoder.Create(header,BitmapCreateOptions.DelayCreation,BitmapCacheOption.None);pixelsWide=decoder.Frames[0].PixelWidth;pixelsHigh=decoder.Frames[0].PixelHeight;}var image=new BitmapImage();image.BeginInit();image.CreateOptions=BitmapCreateOptions.IgnoreImageCache;image.CacheOption=BitmapCacheOption.OnLoad;if(width>0){int size=Math.Min(width,Math.Max(pixelsWide,pixelsHigh));if(pixelsHigh>pixelsWide)image.DecodePixelHeight=size;else image.DecodePixelWidth=size;}image.UriSource=new Uri(path);image.EndInit();image.Freeze();return image;}
    internal static ImageSource FileIcon(string path) {Win.SHFILEINFO info;if(Win.SHGetFileInfo(path,0,out info,(uint)Marshal.SizeOf(typeof(Win.SHFILEINFO)),0x100|0x1)==IntPtr.Zero&&Win.SHGetFileInfo(path,0x80,out info,(uint)Marshal.SizeOf(typeof(Win.SHFILEINFO)),0x100|0x1|0x10)==IntPtr.Zero)return null;try {var image=Imaging.CreateBitmapSourceFromHIcon(info.hIcon,Int32Rect.Empty,BitmapSizeOptions.FromWidthAndHeight(36,36));image.Freeze();return image;}finally {if(info.hIcon!=IntPtr.Zero)Win.DestroyIcon(info.hIcon);}}
    private static DoubleAnimation Motion(double from,double to,int ms){var animation=new DoubleAnimation(from,to,TimeSpan.FromMilliseconds(ms)){FillBehavior=FillBehavior.Stop,EasingFunction=new CubicEase {EasingMode=EasingMode.EaseOut}};Timeline.SetDesiredFrameRate(animation,45);return animation;}
    internal static void Animate(UIElement element,DependencyProperty property,double from,double to,int ms) {element.BeginAnimation(property,null);element.SetValue(property,to);if(Math.Abs(from-to)>.001)element.BeginAnimation(property,Motion(from,to,ms));}
    internal static void Animate(Animatable element,DependencyProperty property,double from,double to,int ms) {element.BeginAnimation(property,null);element.SetValue(property,to);if(Math.Abs(from-to)>.001)element.BeginAnimation(property,Motion(from,to,ms));}
    internal static void Open(string path) {Process.Start(new ProcessStartInfo(path){UseShellExecute=true});}
    internal static void Error(Exception e) {MessageBox.Show(e.Message,"Desk Bloom",MessageBoxButton.OK,MessageBoxImage.Information);}
    internal static string Prompt(Window owner,string title,string value) {
      var w=new Window {Title=title,Width=410,Height=190,ResizeMode=ResizeMode.NoResize,WindowStartupLocation=WindowStartupLocation.CenterScreen,Background=Brush("#142136"),Foreground=Ink,FontFamily=font,ShowInTaskbar=false};
      var panel=new StackPanel {Margin=new Thickness(22)};w.Content=panel;panel.Children.Add(Text(title,15,Ink));var box=new TextBox {Text=value,Margin=new Thickness(0,12,0,14),Padding=new Thickness(8),FontSize=14};panel.Children.Add(box);string answer=null;var ok=Button(L.T("Сохранить"),delegate {if(!string.IsNullOrWhiteSpace(box.Text)){answer=box.Text.Trim();w.DialogResult=true;}},true);panel.Children.Add(ok);box.KeyDown+=delegate(object s,KeyEventArgs e){if(e.Key==Key.Enter&&!string.IsNullOrWhiteSpace(box.Text)){answer=box.Text.Trim();w.DialogResult=true;}if(e.Key==Key.Escape)w.Close();};w.Loaded+=delegate {box.Focus();box.SelectAll();};w.ShowDialog();return answer;
    }
    internal static void Render(FrameworkElement element,string path) {element.UpdateLayout();int width=(int)Math.Ceiling(element.ActualWidth),height=(int)Math.Ceiling(element.ActualHeight);var bitmap=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);bitmap.Render(element);var e=new PngBitmapEncoder();e.Frames.Add(BitmapFrame.Create(bitmap));using(var f=File.Create(path))e.Save(f);}
  }

}
