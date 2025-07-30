using Google.Apis.Auth.OAuth2;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using ViewModel.Contact;
using ViewModel;
using View.Util;

namespace ContactsManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ContactsWindowVM viewModel;

        AuthorizationWindow authorizationWindow;

        private AuthorizationVM authorizationVM
        {
            get
            {
                return authorizationWindow.DataContext as AuthorizationVM;
            }
        }

        private ContactsWindowVM contactsWindowVM
        {
            get
            {
                return this.DataContext as ContactsWindowVM;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            contactsWindowVM.ErrorMessage += ShowErrorMessage;
            contactsWindowVM.InformationMessage += ShowInformationMessage;
            contactsWindowVM.ConfirmRequested += ShowConfirmationMesssageBox;
            contactsWindowVM.TextInputRequested += ShowInputWindow;
            contactsWindowVM.TextChangeRequested += ShowTextEditDialogue;

            authorizationWindow = new();
            authorizationVM.OnUserCredentialsReceived += TransferCredentials;

            authorizationWindow.ShowDialog();
            if (!authorizationVM.UserCredentialsReceived)
            {
                authorizationVM.OnUserCredentialsReceived -= TransferCredentials;
                this.Close();
            }

        }

        private string ShowInputWindow(string arg1, string arg2)
        {
            return ShowTextEditDialogue(arg1, arg2);
        }

        private string ShowTextEditDialogue(string header, string message, string preDefinedText = "")
        {
            InputDialog inputDialog = new InputDialog(header,message,preDefinedText);
            inputDialog.ShowDialog();
            return inputDialog.Input;
        }

        private bool ShowConfirmationMesssageBox(string header, string text)
        {
            var result = MessageBox.Show(
                    text,
                    header,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            return result == MessageBoxResult.Yes;
        }

        private void ShowInformationMessage(string message)
        {
            MessageBox.Show(message,
                "Информация.",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show($"Произошла ошибка при работе программы:\n{message}",
                "Ошибка.",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void TransferCredentials(UserCredential credential)
        {
            viewModel = this.DataContext as ContactsWindowVM;
            viewModel.UserCredential = credential;
            authorizationWindow.Close();
        }
    }
}