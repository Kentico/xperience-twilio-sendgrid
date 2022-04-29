namespace Kentico.Xperience.Twilio.SendGrid.Models
{
    /// <summary>
    /// Represents IP pool of a <see cref="SendGridEvent"/>.
    /// </summary>
    public class SendGridEventPool
    {
        /// <summary>
        /// The IP pool's name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// The IP pool's internal SendGrid ID.
        /// </summary>
        public int Id
        {
            get;
            set;
        }
    }
}