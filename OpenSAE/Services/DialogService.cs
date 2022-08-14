using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
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
        public string? BrowseOpenDirectory(string title, string? initialDirectory)
        {
            string? result = null;

            _parentWindow.Dispatcher.Invoke(() =>
            {
                var dialog = new CommonOpenFileDialog
                {
                    Title = title,
                    IsFolderPicker = true,
                    InitialDirectory = initialDirectory
                };

                if (dialog.ShowDialog(_parentWindow) == CommonFileDialogResult.Ok)
                {
                    result = dialog.FileName;
                }
            });

            return result;
        }

        /// <inheritdoc />
        public string? BrowseSaveFile(string title, string filter, string? currentFilename)
        {
            SaveFileDialog od = new ()
            {
                Filter = filter,
                Title = title,
            };

            if (currentFilename != null)
            {
                od.InitialDirectory = Path.GetDirectoryName(currentFilename);
                od.FileName = Path.GetFileName(currentFilename);
            }

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

        public bool? ShowYesNoCancel(string title, string message)
        {
            MessageBoxResult result = MessageBoxResult.None;

            _parentWindow.Dispatcher.Invoke(() =>
            {
                result = MessageBox.Show(_parentWindow, message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
            });

            return result switch
            {
                MessageBoxResult.Yes => true,
                MessageBoxResult.No => false,
                _ => null
            };
        }
    }
}
