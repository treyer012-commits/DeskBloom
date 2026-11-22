using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DeskBloom {
  // A small fixed-row viewport: no list selection, hidden containers or theme templates.
  internal class FileViewport : Grid {
    private readonly Canvas content=new Canvas {ClipToBounds=true};
    private readonly Canvas rail=new Canvas {Width=12,Background=Brushes.Transparent};
    private readonly Border thumb=new Border {Width=8,CornerRadius=new CornerRadius(4),Background=Ui.Brush("#526982")};
    private readonly Dictionary<int,FileRowView> visible=new Dictionary<int,FileRowView>();
    private readonly Stack<FileRowView> pool=new Stack<FileRowView>();
    private FileRow[] rows=new FileRow[0];private double offset,dragStart,startOffset;private bool dragging;
    internal int RealizedRows {get{return visible.Count;}}
    internal FileViewport(){Focusable=true;ClipToBounds=true;ColumnDefinitions.Add(new ColumnDefinition());ColumnDefinitions.Add(new ColumnDefinition {Width=GridLength.Auto});Children.Add(content);Grid.SetColumn(rail,1);Children.Add(rail);rail.Children.Add(thumb);Canvas.SetLeft(thumb,2);AutomationProperties.SetName(this,L.T("Прокрутить файлы"));SizeChanged+=delegate {UpdateRows();};content.SizeChanged+=delegate {UpdateRows();};
      PreviewMouseWheel+=delegate(object sender,MouseWheelEventArgs e){Scroll(offset-e.Delta/120.0*(SystemParameters.WheelScrollLines<0?ActualHeight:94*Math.Max(1,SystemParameters.WheelScrollLines)));e.Handled=true;};MouseLeftButtonDown+=delegate {Focus();};KeyDown+=delegate(object sender,KeyEventArgs e){double next=offset;switch(e.Key){case Key.Up:next-=94;break;case Key.Down:next+=94;break;case Key.PageUp:next-=ActualHeight;break;case Key.PageDown:next+=ActualHeight;break;case Key.Home:next=0;break;case Key.End:next=double.MaxValue;break;default:return;}Scroll(next);e.Handled=true;};
      thumb.MouseLeftButtonDown+=delegate(object sender,MouseButtonEventArgs e){Focus();dragging=true;dragStart=e.GetPosition(rail).Y;startOffset=offset;thumb.CaptureMouse();e.Handled=true;};thumb.MouseMove+=delegate(object sender,MouseEventArgs e){if(dragging&&e.LeftButton==MouseButtonState.Pressed){double travel=Math.Max(1,ActualHeight-thumb.Height);Scroll(startOffset+(e.GetPosition(rail).Y-dragStart)*Maximum/travel);}};thumb.MouseLeftButtonUp+=delegate(object sender,MouseButtonEventArgs e){dragging=false;thumb.ReleaseMouseCapture();e.Handled=true;};rail.MouseLeftButtonDown+=delegate(object sender,MouseButtonEventArgs e){Focus();Scroll(offset+(e.GetPosition(rail).Y<Canvas.GetTop(thumb)?-ActualHeight:ActualHeight));e.Handled=true;};
    }
    private double RowHeight {get{return rows.Length==1&&rows[0].Entries.Length==0?70:94;}}
    private double Maximum {get{return Math.Max(0,rows.Length*RowHeight-ActualHeight);}}
    private void Scroll(double value){offset=Math.Max(0,Math.Min(value,Maximum));UpdateRows();}
    internal void SetRows(IEnumerable<FileRow> value){foreach(var view in visible.Values){view.DataContext=null;content.Children.Remove(view);}visible.Clear();pool.Clear();rows=value.ToArray();UpdateRows();}
    internal void ClearRows(){SetRows(new FileRow[0]);offset=0;}
    internal void ScrollLast(){Scroll(double.MaxValue);}
    private void UpdateRows(){if(ActualHeight<1||content.ActualWidth<1)return;offset=Math.Max(0,Math.Min(offset,Maximum));bool overflow=Maximum>0;rail.Visibility=overflow?Visibility.Visible:Visibility.Collapsed;thumb.Height=Math.Min(ActualHeight,Math.Max(22,ActualHeight*ActualHeight/Math.Max(1,rows.Length*RowHeight)));Canvas.SetTop(thumb,Maximum==0?0:offset/Maximum*(ActualHeight-thumb.Height));int first=(int)(offset/RowHeight),last=Math.Min(rows.Length-1,(int)Math.Ceiling((offset+ActualHeight)/RowHeight));foreach(int index in visible.Keys.Where(i=>i<first||i>last).ToList()){var view=visible[index];visible.Remove(index);content.Children.Remove(view);view.DataContext=null;if(pool.Count<2)pool.Push(view);}for(int index=first;index<=last;index++){FileRowView view;if(!visible.TryGetValue(index,out view)){view=pool.Count>0?pool.Pop():new FileRowView();view.DataContext=rows[index];visible[index]=view;content.Children.Add(view);}view.Width=content.ActualWidth;Canvas.SetLeft(view,0);Canvas.SetTop(view,index*RowHeight-offset);}}
  }
}
