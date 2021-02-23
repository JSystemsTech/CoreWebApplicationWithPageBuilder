using DbFacade.DataLayer.ConnectionService;
using DbFacade.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.CoreConfiguration
{
    public static class CoreConfigurationExtensions
    {
        public static void ConfigureDbConnection<TDbConnectionConfig>(this IServiceCollection services, string connectionStringSection, Func<string, string, TDbConnectionConfig> constructor)
            where TDbConnectionConfig : DbConnectionConfigBase
        {
            services.AddSingleton(sp =>
            {
                var connectionStingSection = sp.GetRequiredService<IConfiguration>().GetSection("ConnectionStrings").GetSection(connectionStringSection);
                string connectionString = connectionStingSection.GetValue<string>("ConnectionString");
                string providerName = connectionStingSection.GetValue<string>("ProviderName");
                TDbConnectionConfig connection = constructor(connectionString, providerName);
                DbConnectionService.Register(connection);
                return connection;
            }
            );
        }
    }
}
