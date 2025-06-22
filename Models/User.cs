namespace AttendanceTrackerApi.Models
{
    /// <summary>
    /// Reprezentuje uživatele systému.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Primární klíč uživatele.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Uživatelské jméno (unikátní).
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Zahashované heslo uživatele.
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// E-mail uživatele.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Role uživatele (např. admin, user).
        /// </summary>
        public string Role { get; set; }
    }
}
