using CommunityToolkit.Mvvm.Input;
using Google.Apis.Auth.OAuth2;
using Google.Apis.PeopleService.v1;
using Google.Apis.Util.Store;
using Serilog;
using System.IO;

namespace ViewModel
{
    public class AuthorizationVM
    {
        public delegate void LoginHandler(UserCredential credential);
        public event LoginHandler? OnUserCredentialsReceived;

        private bool _userCredentialsReceived = false;
        public bool UserCredentialsReceived { get => _userCredentialsReceived; }

        public RelayCommand LoginCommand { get; set; }

        public AuthorizationVM()
        {
            LoginCommand = new RelayCommand(Login);
        }

        public void Login()
        {
            UserCredential credential;

            string credPath = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                "ContactsManger");

            if (Directory.Exists(credPath))
            {
                Directory.Delete(credPath, true);
            }

            try
            {

                using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
                {
                    //Используется GoogleWebAuthorizationBroker из за простоты
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.FromStream(stream).Secrets,
                        [PeopleServiceService.Scope.Contacts],
                        "user",
                        CancellationToken.None,
                        new FileDataStore("ContactsManger")).Result;
                }

                Log.Information("Login Succesfull");
                _userCredentialsReceived = true;
                OnUserCredentialsReceived?.Invoke(credential);
            }
            catch (AggregateException exceptions)
            {
                foreach (var ex in exceptions.InnerExceptions)
                {
                    Log.Error(ex.Message);
                }
            }
        }
    }

}
