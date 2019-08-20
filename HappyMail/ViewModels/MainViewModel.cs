using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Config;
using DynamicData;
using DynamicData.Annotations;
using DynamicData.Binding;
using HappyMail.Domain;
using HappyMail.Views;
using MailModule;
using MaterialDesignColors;
using MaterialDesignColors.ColorManipulation;
using MaterialDesignThemes.Wpf;
using Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HappyMail.ViewModels
{
    public class MainViewModel : ReactiveObject, IDisposable
    {
        private readonly MailController _mailController;
        public ICommand ThemeColorToggleBaseCommand { get; set; }

        private ObservableCollectionExtended<MailMenuItem> _testBurgerMenuItems = new ObservableCollectionExtended<MailMenuItem>();
        public ObservableCollectionExtended<MailMenuItem> TestBurgerMenuItems
        {
            get { return _testBurgerMenuItems; }
            set { this.RaiseAndSetIfChanged(ref _testBurgerMenuItems, value); }
        }

        [Reactive]
        public ControllerState ConnectionState { get; set; } = ControllerState.Disconnected;

        public MainViewModel()
        {
            _mailController = DummyTrivialSingleton.GetMailControllerService();
            ThemeColorToggleBaseCommand = new AnotherCommandImplementation(o => SetAppColor((bool) o));

            SetAppColor(true);

            // ToDo: Move menu creation to better place or add it in xaml or generate base on some other data.
            _testBurgerMenuItems.Add(new MailMenuItem() { Name = "Inbox   ", NumberOfEmails = 3, Content = new MailBoxView("Inbox"), IconContent = new PackIcon { Kind = PackIconKind.Inbox } });
            _testBurgerMenuItems.Add(new MailMenuItem() { Name = "Drafts", IconContent = new PackIcon { Kind = PackIconKind.Draft } });
            _testBurgerMenuItems.Add(new MailMenuItem() { Name = "Sent", IconContent = new PackIcon { Kind = PackIconKind.Send } });
            _testBurgerMenuItems.Add(new MailMenuItem() { Name = "Archived", IconContent = new PackIcon { Kind = PackIconKind.Archive } });
            _testBurgerMenuItems.Add(new MailMenuItem() { Name = "Important", IconContent = new PackIcon { Kind = PackIconKind.ImportantDevices } });
            _testBurgerMenuItems.Add(new MailMenuItem() { Name = "Spam   ", NumberOfEmails = 99, IconContent = new PackIcon { Kind = PackIconKind.Adb } });
            _testBurgerMenuItems.Add(new MailMenuItem() { Name = "Trash", IconContent = new PackIcon { Kind = PackIconKind.Trash } });

            // Subscribe for controller state updates.
            _mailController.ControllerStateStream.ObserveOnDispatcher().Subscribe(state => ConnectionState = state);
        }

        // ToDo: Make this functionality more user friendly. E.,g. give more colors, palettes or/and themes to choose from.
        private void SetAppColor(bool isChecked)
        {
            ITheme theme = Application.Current.Resources.GetTheme();

            var color = (Color)ColorConverter.ConvertFromString(isChecked == true ? "#FF009688" : "#FF039BE5");

            theme.PrimaryLight = new ColorPair(color.Lighten(), theme.PrimaryLight.ForegroundColor);
            theme.PrimaryMid = new ColorPair(color, theme.PrimaryMid.ForegroundColor);
            theme.PrimaryDark = new ColorPair(color.Darken(), theme.PrimaryDark.ForegroundColor);

            Application.Current.Resources.SetTheme(theme);
        }

        public void Dispose()
        {
            _mailController?.Dispose();
        }
    }
}
