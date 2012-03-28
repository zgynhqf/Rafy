using System;
using SimpleCsla;
using SimpleCsla.Serialization;
using OEA.ManagedProperty;

namespace SimpleCsla.Security
{
    /// <summary>
    /// Criteria class for passing a
    /// username/password pair to a
    /// custom identity class.
    /// </summary>
    [Serializable]
    public class UsernameCriteria : ManagedPropertyObject
    {
        /// <summary>
        /// Username property definition.
        /// </summary>
        public static ManagedProperty<string> UsernameProperty = RegisterProperty<UsernameCriteria, string>(e => e.Username, string.Empty);
        /// <summary>
        /// Gets the username.
        /// </summary>
        public string Username
        {
            get { return this.GetProperty(UsernameProperty); }
            private set { LoadProperty(UsernameProperty, value); }
        }

        /// <summary>
        /// Password property definition.
        /// </summary>
        public static ManagedProperty<string> PasswordProperty = RegisterProperty<UsernameCriteria, string>(e => e.Password, string.Empty);
        /// <summary>
        /// Gets the password.
        /// </summary>
        public string Password
        {
            get { return this.GetProperty(PasswordProperty); }
            private set { this.LoadProperty(PasswordProperty, value); }
        }

        /// <summary>
        /// Creates a new instance of the object.
        /// </summary>
        /// <param name="username">
        /// Username value.
        /// </param>
        /// <param name="password">
        /// Password value.
        /// </param>
        public UsernameCriteria(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }

        /// <summary>
        /// Creates a new instance of the object.
        /// </summary>
        protected UsernameCriteria() { }
    }
}