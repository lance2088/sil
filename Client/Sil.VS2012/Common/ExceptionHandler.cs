using System;
using System.Windows.Forms;

namespace Sil.VSCommon
{
    /// <summary>
    /// A basic exception handler for the plugin.
    /// </summary>
    public static class ExceptionHandler
    {
        /// <summary>
        /// Handles the exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="exception">The exception.</param>
        public static void HandleException(string message, Exception exception)
        {
            //  Currently, for an exception we just show the message box.
            MessageBox.Show(message + Environment.NewLine +
                exception, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}