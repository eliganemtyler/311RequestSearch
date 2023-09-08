namespace _311RequestSearch.Server.Configuration
{
    // Properties defined in here will only be available on the server
    public class ServerAppConfig
    {
       public string AppRegistrationId { get; set; }
       public bool RequireAuthentication { get; set; }
    }
}
