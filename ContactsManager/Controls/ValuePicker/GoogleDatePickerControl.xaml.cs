using Google.Apis.PeopleService.v1.Data;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace View.Controls.ValuePicker;

public partial class GoogleDatePickerControl : UserControl
{
    public GoogleDatePickerControl()
    {
        InitializeComponent();
    }

    private void NumberTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        e.Handled = !e.Text.All(char.IsDigit);
    }

    private void YearTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
    {
        e.Handled = !e.Text.All(char.IsDigit);
    }

    private void NumberTextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Command == ApplicationCommands.Paste)
        {
            if (Clipboard.ContainsText() && !Clipboard.GetText().All(char.IsDigit))
            {
                e.Handled = true;
            }
        }
    }

    private void YearTextBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Command == ApplicationCommands.Paste)
        {
            if (Clipboard.ContainsText() && !Clipboard.GetText().All(char.IsDigit))
            {
                e.Handled = true;
            }
        }
    }
}
