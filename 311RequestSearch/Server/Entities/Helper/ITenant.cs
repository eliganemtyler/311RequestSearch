using System.Threading.Tasks;

namespace _311RequestSearch.Server.Entities.Helper
{
    /// <summary>
    /// Resolves Tenant information when running as a Multi-Tenant solution
    /// </summary>
    public interface ITenant
    {
        /// <summary>
        /// Returns the Tenant Identifer string
        /// </summary>
        Task<string> ResolveAsync();
    }
}
