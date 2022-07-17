using OpenSAE.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSAE
{
    public static class DialogServiceExtensions
    {
        public static void ShowInfoMessage(this IDialogService service, string title, string message)
        {
            service.ShowMessage(title, message, false);
        }

        public static void ShowErrorMessage(this IDialogService service, string title, string message, Exception? exception = null)
        {
            if (exception != null)
            {
                message += Environment.NewLine + Environment.NewLine + exception.Message;
            }

            service.ShowMessage(title, message, true);
        }
    }
}
