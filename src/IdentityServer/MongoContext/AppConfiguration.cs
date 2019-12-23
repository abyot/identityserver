using Microsoft.Extensions.Configuration;
using System;

namespace IdentityServer.AppConfig
{
    public class AppConfiguration
    {
        private static IConfiguration Configuration;

        public static void SetConfig(IConfiguration config)
        {
            Configuration = config;
        }

        public static string GetConfiguration(string configKey)
        {
            try
            {
                return Configuration.GetConnectionString(configKey);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }
}
