using System;

namespace RebatesAPI.Utility
{
    public static class EnvironmentDetails
    {
        public static string getUrl(IConfiguration configuration)
        {
            string Url = "", Protocol = "", IP_address = "";

            try
            {
                int APISSLEnabled = Convert.ToInt32(configuration["Environment:APISSLEnabled"].ToString());
                int ProductionEnv = Convert.ToInt32(configuration["Environment:ProductionEnv"].ToString());
                int PortNumber = Convert.ToInt32(configuration["Environment:URLPort"].ToString());
                

                if (APISSLEnabled == 0)
                {
                    Protocol = "http://";
                }
                else if (APISSLEnabled == 1)
                {
                    Protocol = "https://";
                }
                if (ProductionEnv == 0)
                {
                    Url = Protocol + "localhost:" + PortNumber;
                }
                else if (ProductionEnv == 1)
                {
                    Url = Protocol + "10.177.50.7:" + PortNumber;
                }
                else if (ProductionEnv == 2)
                {
                    Url = Protocol + "10.177.50.7:" + PortNumber;
                }
            }
            catch
            {
                Url = "#";
            }


            return Url;
        }

    }
}
