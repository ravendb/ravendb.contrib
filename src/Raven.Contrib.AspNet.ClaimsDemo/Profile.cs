namespace Raven.Contrib.AspNet.ClaimsDemo
{
    /// <summary>
    /// A basic class for demo purposes. It may be good practice
    /// to separate your data from the credentials/roles class.
    /// </summary>
    public class Profile
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string[] Roles { get; set; }
    }
}