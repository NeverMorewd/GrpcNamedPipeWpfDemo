using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace DemoClient.Mvvm
{
    /// <summary>
    /// AutoScrollBehavior for ListView
    /// </summary>
    public class AutoScrollBehavior : Behavior<ListBox>
    {
        public bool IsAutoScroll
        {
            get { return (bool)GetValue(IsAutoScrollProperty); }
            set { SetValue(IsAutoScrollProperty, value); }
        }

        public static readonly DependencyProperty IsAutoScrollProperty =
            DependencyProperty.Register("IsAutoScroll", typeof(bool), typeof(AutoScrollBehavior), new PropertyMetadata(false, (d, e) => 
            {
                if (e.NewValue is bool eValue)
                {
                    if (d is AutoScrollBehavior behavior)
                    {
                        if (eValue)
                        {
                            behavior.ScrollToEnd();
                        }
                        else
                        {
                            behavior.ScrollToTop();
                        }
                    }
                   
                }
            }));


        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;           
        }
 
        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;

            if (AssociatedObject.ItemsSource is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged -= DataCollectionChanged;
            }

            base.OnDetaching();
        }

        private void DataCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            ScrollToEnd();
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            if (AssociatedObject.ItemsSource is INotifyCollectionChanged collection)
            {
                collection.CollectionChanged += DataCollectionChanged;
            }
        }

        private void AssociatedObject_LayoutUpdated(object sender, EventArgs e)
        {
            ScrollToEnd();
        }

        private void ScrollToEnd()
        {
            if (IsAutoScroll)
            {
                if (AssociatedObject == null || AssociatedObject.Items == null)
                {
                    return;
                }
                if (AssociatedObject.Items.Count > 0)
                {
                    var lastItem = AssociatedObject.Items[^1];
                    AssociatedObject.ScrollIntoView(lastItem);
                }
            }
        }
        private void ScrollToTop()
        {
            if (AssociatedObject == null || AssociatedObject.Items == null)
            {
                return;
            }
            if (AssociatedObject.Items.Count > 0)
            {
                var firstItem = AssociatedObject.Items[0];
                AssociatedObject.ScrollIntoView(firstItem);
            }
        }
    }
}
