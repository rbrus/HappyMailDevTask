using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HappyMail.Dialogs;
using HappyMail.ViewModels;
using MaterialDesignColors;
using MaterialDesignColors.ColorManipulation;
using MaterialDesignThemes.Wpf;
using ReactiveUI;

namespace HappyMail.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dependencyObject = Mouse.Captured as DependencyObject;
            while (dependencyObject != null)
            {
                if (dependencyObject is ScrollBar) return;
                dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
            }

            LeftMenuToggleButton.IsChecked = false;
        }

        private async void MenuPopupButton_OnClick(object sender, RoutedEventArgs e)
        {
            // ToDo: Temporally string comparison.
            if (((ButtonBase) sender).Content.ToString() == "Exit")
            {
                Application.Current.Shutdown();
            }
            else
            {
                var sampleMessageDialog = new SampleMessageDialog
                {
                    Message = { Text = ((ButtonBase)sender).Content.ToString() }
                };

                await DialogHost.Show(sampleMessageDialog, "RootDialog");
            }
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            (this.DataContext as MainViewModel)?.Dispose();
        }
    }
}
