using System.Windows;
using System.Windows.Controls;
using Google.Apis.PeopleService.v1.Data;

namespace View.Controls
{
    /// <summary>
    /// Логика взаимодействия для SingleContactControl.xaml
    /// </summary>
    public partial class SingleContactControl : UserControl
    {
        public SingleContactControl()
        {
            InitializeComponent();
        }

        private void TogglePopupButton_Click(object sender, RoutedEventArgs e)
        {
            ContactGroupPopup.IsOpen = !ContactGroupPopup.IsOpen;
        }
    }
}
