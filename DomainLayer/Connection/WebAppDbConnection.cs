using DbFacade.DataLayer.ConnectionService;
using DbFacadeShared.Factories;
using System.Threading.Tasks;

namespace DomainLayer.Connection
{
    public class WebAppDbConnection: SqlConnectionConfig<WebAppDbConnection>
    {
        public WebAppDbConnection(string connectionString, string providerName) {
            ConnectionString = connectionString;
            ProviderName = providerName;
        }
        private string ConnectionString { get; set; }
        private string ProviderName { get; set; }

        protected override string GetDbConnectionProvider() => ProviderName;
        protected override async Task<string> GetDbConnectionProviderAsync() { await Task.CompletedTask; return ProviderName; }
        protected override string GetDbConnectionString() => ConnectionString;
        protected override async Task<string> GetDbConnectionStringAsync() { await Task.CompletedTask; return ConnectionString; }

        public static IDbCommandConfig GetIdentity = DbCommandConfigFactory<WebAppDbConnection>.CreateFetchCommand("[dbo].[User_GetIdentity]", "Get Identity");
        public static IDbCommandConfig GetIdentities = DbCommandConfigFactory<WebAppDbConnection>.CreateFetchCommand("[dbo].[User_GetIdentities]", "Get Identities");
    }
}
