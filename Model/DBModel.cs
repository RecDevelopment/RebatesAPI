using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace RebatesAPI.Model
{
    public class DBModel
    {
    }
    public class FinanceUploadHistory
    {
        public int SPAUploadId { get; set; }
        public string UniqueId { get; set; }
        public string SPAExcelFileName { get; set; }
        public string SPAFileUploadedBy { get; set; }
        public string SPAUploadFileStatus { get; set; }
        public string SPAFileProcessStatus { get; set; }
        public string SPAuploadprocessedFile { get; set; }
        public string SPAFileApprovalStatus { get; set; }
        public string SPAProcessedFilelocation { get; set; }
        public int SPAUploadstatus { get; set; }
        public string SPAfileUploadedby { get; set; }
        public string SPAUploadRemarks { get; set; }
        public string SPAUploadCreatedate { get; set; }
        public string SPAUploadHHMMSS { get; set; }
        public int SPAUploadprocessedstatus { get; set; }
        public string SPAUploadprocessedBy { get; set; }
        public string SPAUploadprocessedDate { get; set; }
        public int SPAUploadApprovalStatus { get; set; }
        public string SPAUploadApprovedBy { get; set; }
        public string SPAUploadApprovedDate { get; set; }


    }

    public class Authorization
    {
        private readonly IConfiguration configuration;
        public Authorization(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public bool ValidateAuthorization(string inputXAPIkey, ref string jsonstringoutput)
        {
            string SPAAPIkey = "SPA79c6i2e6rBDgLkTE42VmUIQJXTNTe3RKsuFkXRBI8QBZNVMMxNvMLhkyxECd2";
            const string APIKEY = "x-api-key";

            var apiKey = configuration.GetValue<string>(APIKEY);


            var jsonObject = new JObject();
            bool status = false;

            if (inputXAPIkey != SPAAPIkey)
            {

                jsonObject.Add("Result", -3000);
                jsonObject.Add("Description", "Unauthorized request");
                status = false;
            }
            else
            {
                jsonObject.Add("Result", 3000);
                jsonObject.Add("Description", "Authorized");
                status = true;
            }

            var FirstObject = new JObject();
            FirstObject.Add("Output", jsonObject.ToString());

            string jsonData = "";

            if (jsonData != null)
                if (jsonData == "")
                {
                    jsonData = "{}";
                }
            FirstObject.Add("AccessRigths", jsonData);
            string jsongString = FirstObject.ToString();


            jsonstringoutput = jsongString;
            return status;

        }
    }
	public class Login
	{
		public string userid { get; set; }
		public string password { get; set; }


    }
    public class Fileuploadhistoryinput
    {
        public int page { get; set; }
        public int pagesize { get; set; }


    }
    public class RebatesModel
    {
        public IFormFile file { get; set; }
        public int distributorcode { get; set; }


    }

    public class PasswrdReqeust
    {
        public string Email { get; set; }
    }
    public class PasswordRest
    {
        public string token { get; set; }
        public string password { get; set; }

        public string passwordConfirm { get; set; }
    }

    public class FileLocation
    {
        public string fileLocation { get; set; }
        public string Id { get; set; }
    }
}
