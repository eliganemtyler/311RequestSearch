namespace _311RequestSearch.Server.Entities.Config
{
  public class DatadogRum
  {
    public DatadogRumConfig Config { get; set; }
  }

  public class DatadogRumConfig
  {
    public string ApplicationId { get; set; }
    public string ClientToken { get; set; }
    public string Site { get; set; }
    public string Service { get; set; }
    public string Env { get; set; }
    public string Version { get; set; }
    public bool TrackInteractions { get; set; }
    public int ResourceSampleRate { get; set; }
    public int SampleRate { get; set; }
    public bool SilentMultipleInit { get; set; }
    public string ProxyHost { get; set; }
    public bool TrackSessionAcrossSubdomains { get; set; }
    public bool UseSecureSessionCookie { get; set; }
    public bool UseCrossSiteSessionCookie { get; set; }
  }
}