using System;
using System.Windows;

namespace WpfPractice
{
    public class ResizeService : IResizeService
    {
        public event EventHandler<SizeChangedEventArgs> SliverPanelResized;

        public void OnSliverPanelResized(SizeChangedEventArgs e)
        {
            SliverPanelResized?.Invoke(this, e);
        }
    }
}