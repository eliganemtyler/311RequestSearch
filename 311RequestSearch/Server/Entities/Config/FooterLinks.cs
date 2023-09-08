namespace _311RequestSearch.Server.Entities.Config
{
    public class FooterLink
    {
        public FooterLink() { }

        public FooterLink(string name, string url)
        {
            this.Name = name;
            this.Url = url;
        }

        public string Name { get; set; }
        public string Url { get; set; }
    }
}
