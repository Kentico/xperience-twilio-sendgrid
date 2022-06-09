using System.Collections.Generic;

namespace Kentico.Xperience.Twilio.SendGrid.Models
{
    /// <summary>
    /// Parameters that are stored in session and retrieved when performing a mass action.
    /// </summary>
    public class SuppressionManagementActionParameters
    {
        /// <summary>
        /// The name of the action being performed.
        /// </summary>
        public string ActionName
        {
            get;
            set;
        }


        /// <summary>
        /// The title to be displayed in the modal window.
        /// </summary>
        public string Title
        {
            get;
            set;
        }


        /// <summary>
        /// The Xperience contacts to perform the action on.
        /// </summary>
        public List<int> ContactIDs
        {
            get;
            set;
        }


        /// <summary>
        /// The javascript to call when the modal window is closed.
        /// </summary>
        public string ReloadScript
        {
            get;
            set;
        }
    }
}