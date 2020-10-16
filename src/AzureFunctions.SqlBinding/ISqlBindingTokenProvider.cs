using System.Threading.Tasks;

namespace AzureFunctions.SqlBinding
{
    public interface ISqlBindingTokenProvider
    {
        Task<string> GetToken();
    }
}
