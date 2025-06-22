namespace AttendanceTrackerApi.Models
{
    /// <summary>
    /// Reprezentuje záznam docházky jednoho uživatele za jeden den.
    /// </summary>
    public class AttendanceRecord
    {
        /// <summary>
        /// Primární klíč záznamu.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Id uživatele, ke kterému záznam patří (cizí klíč).
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Datum záznamu docházky.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Čas příchodu do práce.
        /// </summary>
        public TimeSpan ArrivalTime { get; set; }

        /// <summary>
        /// Čas odchodu z práce.
        /// </summary>
        public TimeSpan DepartureTime { get; set; }

        /// <summary>
        /// Volitelná poznámka (například dovolená, práce z domu…).
        /// </summary>
        public string? Note { get; set; }
    }
}
