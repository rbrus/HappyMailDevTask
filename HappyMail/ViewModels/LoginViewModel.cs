using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Config;
using HappyMail.Domain;
using MailModule;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HappyMail.ViewModels
{
    /// <summary>
    /// LoginViewModel.
    /// </summary>
    public class LoginViewModel : ReactiveObject, IDisposable
    {
        private readonly MailController _mailController;

        [Reactive]
        public string Login { get; set; }

        [Reactive]
        public string Password { get; set; }

        [Reactive]
        public MailServerType MailServerType { get; set; }

        [Reactive]
        public MailServerEncryption MailServerEncryption { get; set; }

        [Reactive]
        public string ServerHost { get; set; } = "imap.mail.ch";

        [Reactive]
        public int ServerPort { get; set; } = 993;

        [Reactive]
        public ControllerState ConnectionState { get; set; } = ControllerState.Disconnected;

        public ReactiveCommand<Unit, Unit> Connect { get; }
        
        public ReactiveCommand<Unit, Unit> Disconnect { get; }

        public LoginViewModel()
        {
            // Connect command invokes MailController Start method.
            Connect = ReactiveCommand.Create(() =>
            {
                _mailController.Start(MailServerType, MailServerEncryption, ServerHost, Login, Password);
            });

            // Disconnect command invokes MailController Stop method.
            Disconnect = ReactiveCommand.Create(() =>
            {
                _mailController.Stop();
            });

            _mailController = DummyTrivialSingleton.GetMailControllerService();

            // Subscribe for controller state updates.
            _mailController.ControllerStateStream.ObserveOnDispatcher().Subscribe(state => ConnectionState = state);
        }

        public void Dispose()
        {
            _mailController?.Dispose();
            Connect?.Dispose();
        }
    }
}
