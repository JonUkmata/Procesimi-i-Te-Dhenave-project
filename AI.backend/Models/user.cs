namespace AI.backend.Models
{
    public class User
    {
        public int Id { get; set; }                 // Identifikues unik
        public string Username { get; set; }        // Emri i përdoruesit
        public string Email { get; set; }           // Email-i
        public string PasswordHash { get; set; }    // Hash i fjalëkalimit
        public string Role { get; set; }            // Roli (p.sh. Admin, User)
        public System.DateTime CreatedAt { get; set; } // Data e krijimit
        public bool IsActive { get; set; }          // Nëse llogaria është aktive
    }
}

