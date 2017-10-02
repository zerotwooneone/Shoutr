using System;
using System.Windows;

namespace WpfPractice
{
    public interface IResizeService
    {
        event EventHandler<SizeChangedEventArgs> SliverPanelResized;
        void OnSliverPanelResized(SizeChangedEventArgs e);
    }
}