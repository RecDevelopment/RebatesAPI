using Azure.Storage.Queues;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RebatesAPI.DTO;
using System.Text;

namespace RebatesAPI.Utilities
{
    public class AzureQueueHandler
    {
        public AzureQueueHandler() { }

        public async Task<bool> writeToAzureQueue(ConfirmationRequest request)
        {
            try
            {
                IConfiguration conf = (new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build());

                // Retrieve the connection string for your Azure Storage account
                var connectionString = conf["AZQueue:connectionString"].ToString();

                var queueName = conf["AZQueue:queueName"].ToString();

                // Create a QueueClient instance
                QueueClient queueClient = new QueueClient(connectionString, queueName);

                // Serialize the request object to JSON
                string jsonRequest = JsonConvert.SerializeObject(request);

                byte[] byteArray = Encoding.UTF8.GetBytes(jsonRequest);
                // Encode the byte array to Base64 string
                string base64String = Convert.ToBase64String(byteArray);

                // Enqueue the JSON request to the queue
                await queueClient.SendMessageAsync(base64String);

                return true;

            }
            catch (Exception ex)
            {
            }
            return false;
        }

        public async Task<bool> writeToAzureQueue(JObject message)
        {
            try
            {
                IConfiguration conf = (new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build());

                // Retrieve the connection string for your Azure Storage account
                var connectionString = conf["AZQueue:connectionString"].ToString();

                var queueName = conf["AZQueue:queueName"].ToString();

                // Create a QueueClient instance
                QueueClient queueClient = new QueueClient(connectionString, queueName);

                // Serialize the request object to JSON
                string jsonRequest = JsonConvert.SerializeObject(message);

                byte[] byteArray = Encoding.UTF8.GetBytes(jsonRequest);
                // Encode the byte array to Base64 string
                string base64String = Convert.ToBase64String(byteArray);

                // Enqueue the JSON request to the queue
                await queueClient.SendMessageAsync(base64String);

                return true;

            }
            catch (Exception ex)
            {
            }
            return false;
        }
    }
}
