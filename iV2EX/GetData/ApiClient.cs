using WebApiClient;

namespace iV2EX.GetData
{
    internal static class ApiClient
    {
        public static IV2exApi Client { get; } = HttpApiClient.Create<IV2exApi>();
    }
}