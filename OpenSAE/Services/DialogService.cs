using Microsoft.Win32;
using System.Windows;

namespace OpenSAE.Services
{
    public class DialogService : IDialogService
    {
        private readonly Window _parentWindow;

        public DialogService(Window parentWindow)
        {
            _parentWindow = parentWindow;
        }

        /// <inheritdoc />
        public string? BrowseOpenFile(string title, string filter)
        {
            OpenFileDialog od = new OpenFileDialog()
            {
                Filter = filter,
                Title = title,
            };

            string? result = null;

            _parentWindow.Dispatcher.Invoke(() =>
            {
                if (od.ShowDialog(_parentWindow) == true)
                {
                    result = od.FileName;
                }
            });

            return result;
        }

        /// <inheritdoc />
        public bool ShowConfirmation(string title, string message)
        {
            MessageBoxResult result = MessageBoxResult.None;

            _parentWindow.Dispatcher.Invoke(() =>
            {
                result = MessageBox.Show(_parentWindow, message, title, MessageBoxButton.OKCancel, MessageBoxImage.Information);
            });

            return result == MessageBoxResult.OK;
        }

        /// <inheritdoc />
        public void ShowMessage(string title, string message, bool isError)
        {
            _parentWindow.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(_parentWindow, message, title, MessageBoxButton.OK, isError ? MessageBoxImage.Error : MessageBoxImage.Information);
            });
        }
    }
}
