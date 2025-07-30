using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace View.Controls
{
    /// <summary>
    /// Логика взаимодействия для ContactSelectorControl.xaml
    /// </summary>
    public partial class ContactSelectorControl : UserControl
    {
        public ContactSelectorControl()
        {
            InitializeComponent();
        }

        private void ContactsListBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.HeightChanged && !e.WidthChanged)
            {
                return;
            }
            if (ContactsListBox.MinWidth < ContactsListBox.ActualWidth)
                ContactsListBox.MinWidth = ContactsListBox.ActualWidth;
        }
    }
}
