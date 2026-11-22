using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DeskBloom {
  [DataContract] public class TitlePart {
    [DataMember] public string Text;
    [DataMember(EmitDefaultValue=false)] public int? Color;
    [DataMember(EmitDefaultValue=false)] public bool Bold,Italic,Underline;
    internal bool SameStyle(TitlePart other){return other!=null&&Color==other.Color&&Bold==other.Bold&&Italic==other.Italic&&Underline==other.Underline;}
  }
  internal static class Appearance {
    internal static readonly string[] Colors={"#9F8CFF","#6AD1CE","#FFB782","#EC91B7","#EF6B73","#E99068","#E9B85A","#DED570","#A8C76D","#74C58A","#53BAA1","#51BED4","#69A9E8","#7D92E9","#A493E5","#C38CDD","#DF8DCE","#D284A5","#B99682","#A6ADB9","#F3F4F8","#CED6E3","#8798AE","#526782","#27384F","#1B2435","#F4D8C5","#D4E9D4","#CFE6F0","#E5DCF6"};
    internal static readonly string[] Backgrounds={"#152236","#192A45","#20344A","#173746","#173A38","#243E32","#31412D","#43402B","#433326","#432C2C","#3C2938","#342B45","#29263F","#23364B","#17434C","#1C493E","#35462F","#4B412B","#4D3430","#492F42","#3B3351","#263D56","#28323E","#20262F","#353B43","#E7EDF7","#FFFFFF","#DDE8E4","#F1E5DC","#E7DFF1"};
    private static readonly Dictionary<string,ImageSource> icons=new Dictionary<string,ImageSource>();
    private static readonly Queue<string> iconOrder=new Queue<string>();
    private static readonly Dictionary<string,ImageSource> custom=new Dictionary<string,ImageSource>();
    private static readonly Queue<string> customOrder=new Queue<string>();
    internal static Color Color(int index){return (Color)ColorConverter.ConvertFromString(Colors[Math.Max(0,Math.Min(29,index))]);}
    internal static Brush Accent(GroupData group){return Ui.Brush(Colors[Math.Max(0,Math.Min(29,group.Color))]);}
    internal static Brush Background(GroupData group){return Ui.Brush(group.BackgroundColor.HasValue?Backgrounds[Math.Max(0,Math.Min(29,group.BackgroundColor.Value))]:Backgrounds[0]);}
    internal static Brush BodyInk(GroupData group){var color=((SolidColorBrush)Background(group)).Color;return Ui.Brush((color.R*.2126+color.G*.7152+color.B*.0722)>170?"#203046":"#EFF3FF");}
    internal static Brush BodyHover(GroupData group){return Ui.Brush(((SolidColorBrush)BodyInk(group)).Color.R<80?"#CCD8E7":"#34425B");}
    private static Color Blend(Color color,Color baseColor,double factor){return System.Windows.Media.Color.FromRgb((byte)(baseColor.R+(color.R-baseColor.R)*factor),(byte)(baseColor.G+(color.G-baseColor.G)*factor),(byte)(baseColor.B+(color.B-baseColor.B)*factor));}
    internal static Brush Header(GroupData group){Color color=Color(group.Color);var brush=new LinearGradientBrush(Blend(color,System.Windows.Media.Color.FromRgb(29,40,61),.35),Blend(color,System.Windows.Media.Color.FromRgb(22,33,51),.19),35);brush.Freeze();return brush;}
    internal static string Key(GroupData group){return group.Color+"|"+group.BackgroundColor+"|"+group.Icon+"|"+group.IconImage+"|"+group.IconRevision+"|"+string.Join(";",(group.TitleParts??new List<TitlePart>()).Select(p=>(p.Text??"")+":"+p.Color+":"+p.Bold+":"+p.Italic+":"+p.Underline));}
    internal static string Plain(IEnumerable<TitlePart> parts){return parts==null?"":string.Concat(parts.Select(p=>p.Text??""));}
    internal static void ApplyTitle(TextBlock target,GroupData group){if(group.TitleParts==null||group.TitleParts.Count==0||Plain(group.TitleParts)!=group.Name){target.Text=group.Name;return;}target.Inlines.Clear();foreach(var part in group.TitleParts){var run=new Run(part.Text??"");if(part.Color.HasValue)run.Foreground=Ui.Brush(Colors[Math.Max(0,Math.Min(29,part.Color.Value))]);if(part.Bold)run.FontWeight=FontWeights.Bold;if(part.Italic)run.FontStyle=FontStyles.Italic;if(part.Underline)run.TextDecorations=TextDecorations.Underline;target.Inlines.Add(run);}}
    internal static string[] IconNamesRu={"Папка","Книга","Портфель","Звезда","Сердце","Глобус","Компьютер","Игровой контроллер","Календарь","Нота","Лампочка","Шестерёнка","Человек","Изображение","Часы"};
    internal static string[] IconNamesEn={"Folder","Book","Briefcase","Star","Heart","Globe","Computer","Game controller","Calendar","Music note","Light bulb","Settings","Person","Image","Clock"};
    private static readonly string[] paths={
      "M3,7 L9,7 L11,10 L21,10 L21,20 L3,20 Z M3,7 L3,4 L10,4 L12,7 L21,7 L21,10",
      "M12,5 C9,3 5,3 3,4 L3,19 C6,18 9,18 12,20 C15,18 18,18 21,19 L21,4 C18,3 15,3 12,5 L12,20",
      "M8,7 L8,4 L16,4 L16,7 M3,8 L21,8 L21,20 L3,20 Z M3,12 C8,16 16,16 21,12 M10,14 L14,14",
      "M12,2 L15,8.5 L22,9.5 L17,14.5 L18.2,21.5 L12,18 L5.8,21.5 L7,14.5 L2,9.5 L9,8.5 Z",
      "M12,21 C8,17 2,13 2,8 C2,2 10,1 12,7 C14,1 22,2 22,8 C22,13 16,17 12,21 Z",
      "M22,12 A10,10 0 1 1 2,12 A10,10 0 1 1 22,12 M2,12 L22,12 M12,2 C6,7 6,17 12,22 C18,17 18,7 12,2",
      "M3,3 L21,3 L21,16 L3,16 Z M12,16 L12,21 M7,21 L17,21",
      "M8,7 L16,7 C21,7 22,15 21,18 C20,21 17,16 15,16 L9,16 C7,16 4,21 3,18 C2,15 3,7 8,7 Z M6,10 L6,14 M4,12 L8,12 M16,11 L16.1,11 M19,13 L19.1,13",
      "M3,5 L21,5 L21,21 L3,21 Z M7,2 L7,8 M17,2 L17,8 M3,10 L21,10 M7,14 L8,14 M12,14 L13,14 M17,14 L18,14 M7,18 L8,18 M12,18 L13,18",
      "M9,18 L9,5 L20,3 L20,16 M9,9 L20,7 M9,18 C9,22 2,23 2,19 C2,16 9,15 9,18 M20,16 C20,20 13,21 13,17 C13,14 20,13 20,16",
      "M9,18 L15,18 M9,21 L15,21 M8,15 C8,12 5,11 5,7 C5,0 19,0 19,7 C19,11 16,12 16,15 L8,15 M12,7 L12,15",
      "M9,3 L15,3 L16,6 L19,6 L22,11 L20,14 L20,17 L15,21 L12,20 L9,21 L4,17 L4,14 L2,11 L5,6 L8,6 Z M16,12 A4,4 0 1 1 8,12 A4,4 0 1 1 16,12",
      "M16,7 A4,4 0 1 1 8,7 A4,4 0 1 1 16,7 M4,21 L4,19 C4,12 20,12 20,19 L20,21",
      "M3,3 L21,3 L21,21 L3,21 Z M3,18 L9,11 L13,15 L16,12 L21,18 M17,7 A1,1 0 1 1 15,7 A1,1 0 1 1 17,7",
      "M22,12 A10,10 0 1 1 2,12 A10,10 0 1 1 22,12 M12,6 L12,12 L16,15"
    };
    internal static ImageSource Glyph(int index,Brush color){index=Math.Max(0,Math.Min(14,index));string key=index+"|"+color.ToString();ImageSource image;if(icons.TryGetValue(key,out image))return image;var pen=new Pen(color,1.65){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round,LineJoin=PenLineJoin.Round};pen.Freeze();var drawing=new DrawingGroup();drawing.Children.Add(new GeometryDrawing(Brushes.Transparent,null,new RectangleGeometry(new Rect(0,0,24,24))));drawing.Children.Add(new GeometryDrawing(null,pen,Geometry.Parse(paths[index])));drawing.Freeze();var result=new DrawingImage(drawing);result.Freeze();if(icons.Count>=128)icons.Remove(iconOrder.Dequeue());icons[key]=result;iconOrder.Enqueue(key);return result;}
    internal static ImageSource ActionGlyph(string name,Brush color){string path=name=="save"?"M4,3 L17,3 L21,7 L21,21 L3,21 L3,3 Z M7,3 L7,10 L16,10 L16,3 M7,21 L7,14 L17,14 L17,21":name=="file"?"M5,2 L14,2 L19,7 L19,22 L5,22 Z M14,2 L14,7 L19,7 M9,14 L15,14 M12,11 L12,17":name=="cancel"?"M6,6 L18,18 M18,6 L6,18":name=="reset"?"M4,8 C8,0 21,4 21,12 C21,21 9,24 4,17 M4,3 L4,8 L9,8":null;if(path==null)return Glyph(name=="folder"?0:13,color);string key=name+"|"+color;ImageSource cached;if(icons.TryGetValue(key,out cached))return cached;var pen=new Pen(color,1.7){StartLineCap=PenLineCap.Round,EndLineCap=PenLineCap.Round,LineJoin=PenLineJoin.Round};var drawing=new DrawingGroup();drawing.Children.Add(new GeometryDrawing(Brushes.Transparent,null,new RectangleGeometry(new Rect(0,0,24,24))));drawing.Children.Add(new GeometryDrawing(null,pen,Geometry.Parse(path)));drawing.Freeze();var image=new DrawingImage(drawing);image.Freeze();if(icons.Count>=128)icons.Remove(iconOrder.Dequeue());icons[key]=image;iconOrder.Enqueue(key);return image;}
    internal static ImageSource Icon(Store store,GroupData group){string path=GroupAssets.IconPath(store,group);if(path!=null&&File.Exists(path)){string key=path+"|"+group.IconRevision;ImageSource found;if(custom.TryGetValue(key,out found))return found;try{found=Ui.LoadImage(path,96);if(custom.Count>=24)custom.Remove(customOrder.Dequeue());custom[key]=found;customOrder.Enqueue(key);return found;}catch{}}return Glyph(group.Icon,Accent(group));}
  }
  internal static class GroupAssets {
    internal const string Folder=".deskbloom";
    internal static bool IsOwnedFolder(string path){if(!string.Equals(System.IO.Path.GetFileName(path),Folder,StringComparison.OrdinalIgnoreCase))return false;try{return File.ReadAllText(System.IO.Path.Combine(path,".owner")).Trim()=="DeskBloom.Metadata/1";}catch{return false;}}
    internal static string IconPath(Store store,GroupData group){return group.IconImage==Folder+"/icon.png"?System.IO.Path.Combine(store.GroupPath(group),Folder,"icon.png"):null;}
    internal static void SetIcon(Store store,GroupData group,string source){var bitmap=Ui.LoadImage(source,96);string root=System.IO.Path.Combine(store.GroupPath(group),Folder);if(Directory.Exists(root)&&!IsOwnedFolder(root))throw new IOException(L.T("Папка служебных данных занята."));Directory.CreateDirectory(root);File.WriteAllText(System.IO.Path.Combine(root,".owner"),"DeskBloom.Metadata/1");File.SetAttributes(root,File.GetAttributes(root)|FileAttributes.Hidden);string target=System.IO.Path.Combine(root,"icon.png"),temp=target+".tmp";var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bitmap));using(var stream=File.Create(temp))encoder.Save(stream);if(File.Exists(target))File.Replace(temp,target,null);else File.Move(temp,target);group.IconImage=Folder+"/icon.png";group.IconRevision=DateTime.UtcNow.Ticks;}
  }
}
