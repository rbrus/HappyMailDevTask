using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using DynamicData.Binding;
using HappyMail.Domain;
using MailModule;
using Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace HappyMail.ViewModels
{
    /// <summary>
    /// LoginViewModel.
    /// </summary>
    public class MailBoxViewModel : ReactiveObject, IDisposable
    {
        private readonly MailController _mailController;

        [Reactive]
        public MailContent MailContent { get; set; } = new MailContent();

        [Reactive]
        public MailHeader SelectedMailHeader { get; set; }

        private ObservableCollectionExtended<MailHeader> _mailHeaders = new ObservableCollectionExtended<MailHeader>();
        public ObservableCollectionExtended<MailHeader> MailHeaders
        {
            get { return _mailHeaders; }
            set { this.RaiseAndSetIfChanged(ref _mailHeaders, value); }
        }

        public MailBoxViewModel()
        {
            // Get MailController object.
            _mailController = DummyTrivialSingleton.GetMailControllerService();

            _mailController.LoadAllExistingMailHeaders();

            // Subscribe to MailHeader stream.Add published MailHeaders to the MailHeaders list.
            _mailController.MailHeaderStream.ObserveOnDispatcher().Subscribe(content =>
            {
                if(content != null) MailHeaders.Add(content);
            });

            // Subscribe to MailContent stream.
            _mailController.MailContentStream.ObserveOnDispatcher().Subscribe(content =>
            {
                if (SelectedMailHeader == null || SelectedMailHeader.Uid != content.Uid) return;
                MailContent = content;
            });

            // When selection changes, send requested to the mail controller for the email content.
            this.WhenAnyValue(x => x.SelectedMailHeader).Subscribe(selection =>
            {
                if (selection != null)
                {
                    MailContent = null;
                    _mailController.EmailContentRequestByUidStream.OnNext(selection.Uid);
                }
            });
        }

        public void Dispose()
        {
            _mailController?.Dispose();
        }
    }
}
