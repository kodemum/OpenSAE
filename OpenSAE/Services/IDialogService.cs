using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE.Services
{
    /// <summary>
    /// Service for showing interaction dialogs
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Opens an open file dialog with the specified arguments.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="filter"></param>
        /// <returns>Full path to any file selected - null if canceled.</returns>
        string? BrowseOpenFile(string title, string filter);

        /// <summary>
        /// Opens an save file dialog with the specified arguments.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="filter"></param>
        /// <returns>Full path to any file selected - null if canceled.</returns>
        string? BrowseSaveFile(string title, string filter, string? currentFilename);

        /// <summary>
        /// Shows a confirmation dialog with the specified message.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <returns>Value indicating if the confirmation was accepted.</returns>
        bool ShowConfirmation(string title, string message);

        /// <summary>
        /// Shows a dialog with the specified message.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="isError"></param>
        void ShowMessage(string title, string message, bool isError);
    }
}
