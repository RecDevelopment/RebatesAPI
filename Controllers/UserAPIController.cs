using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.IO.Packaging;
using OfficeOpenXml;
using RebatesAPI.Databases;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Drawing.Printing;
using Newtonsoft.Json.Linq;
using RebatesAPI.Model;
using RebatesAPI.Utilities;

namespace RebatesRESTAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAPIController : ControllerBase
    {
        private readonly IConfiguration configuration;
      
        public UserAPIController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        [HttpPost("SPAUserLoginTest")]
        public async Task<string> SPAUserLoginTest(string userid, string password)
        {
            string jsonData = "";
            int GroupId = 0;
            int UserRole = 0;

            GroupId = 1;
            UserRole = 1;
            SqlParameter[] sqlParameters = {};
            
            FileUpload Fupload = new FileUpload();
            jsonData = await Fupload.SPAAdminFetchAccessrights("Admin", this.configuration, "SPAAdmin_UserGroupAccessRightsFetch", sqlParameters, GroupId, UserRole);
            return jsonData;

            //var jsonObject = new JObject();
            //jsonObject.Add("Result", outputValue);
            //jsonObject.Add("Description", outputvalue1);

            //var FirstObject = new JObject();
            //FirstObject.Add("Output", jsonObject.ToString());

            //string jsongString = FirstObject.ToString();
            //return jsongString;
        }

        private bool ValidateAuthorization(string inputXAPIkey,ref string jsonstringoutput)
        {
            string SPAAPIkey = "SPA79c6i2e6rBDgLkTE42VmUIQJXTNTe3RKsuFkXRBI8QBZNVMMxNvMLhkyxECd2";
            const string APIKEY = "x-api-key";
            
            var apiKey=configuration.GetValue<string>(APIKEY);
            

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

        [HttpPost("PasswordRecoveryApply")]
        public async Task<string> PasswordRecoveryApply(PasswordRest PasswordReset)
        {
            string jsonData = "";
            SqlParameter[] sqlParameters = { };
            bool status;
            string xapikey = Request.Headers.Authorization;
            status = ValidateAuthorization(xapikey, ref jsonData);
            if (status == false)
            {
                return jsonData;
            }
            FileUpload Fupload = new FileUpload();

            jsonData = await Fupload.SPAPWDResetApply("Auth", this.configuration, "SPA_Password_Reset_Apply", sqlParameters, PasswordReset.token, PasswordReset.password, PasswordReset.passwordConfirm);
            return jsonData;
        }

        [HttpPost("PasswordRecovery")]
        public async Task<string> PasswordRecovery(PasswrdReqeust PwdResetRequest)
        {
            string jsonData = "";
            SqlParameter[] sqlParameters = { };
            bool status;
            string xapikey = Request.Headers.Authorization;
            status = ValidateAuthorization(xapikey, ref jsonData);
            if (status == false)
            {
                return jsonData;
            }
            FileUpload Fupload = new FileUpload();

            jsonData = await Fupload.SPAPWDReset("Auth", this.configuration, "SPA_Password_Reset_Request", sqlParameters, PwdResetRequest.Email);
            return jsonData;
        }


        [HttpPost("SPAUserLogin")]
        public async Task<string> SPAUserLogin(Login data)
        {
            string jsonData = "";
            int GroupId = 0;
            int UserRole = 0;

            GroupId = 1;
            UserRole = 1;
            bool status;
            SqlParameter[] sqlParameters = { };
            string xapikey = Request.Headers.Authorization;
            status = ValidateAuthorization(xapikey, ref jsonData);
            if (status == false)
            {
                return jsonData;
            }

            FileUpload Fupload = new FileUpload();
            jsonData = await Fupload.SPAAuthLogin("Auth", this.configuration, "SPA_UserLogin", sqlParameters, data.userid, data.password);
            return jsonData;
        }
       // [HttpGet("FetchFileUploadHistoryWithPaging")]
       // public async Task<string> FetchFileUploadHistoryWithPaging(int page, int pagesize)
       // {
        //    string jsonData = "";
       //     SqlParameter[] sqlParameters = { };
       //     FileUpload Fupload = new FileUpload();
       //     jsonData = await Fupload.GetJsonDataWithPagingFromStoredProcedureAsync("Finance", this.configuration, "SPA_FinanceUploadHistoryFetchbyPage", page, pagesize, sqlParameters);
       //     return jsonData;
        //}

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {

            //string connectionString = configuration.GetConnectionString("DefaultConnection");


            if (file == null || file.Length == 0)
            {
                return BadRequest("File is not selected");
            }

            if (Path.GetExtension(file.FileName) != ".xlsx")
            {
                return BadRequest("File type is not supported");
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new OfficeOpenXml.ExcelPackage(stream))
                {
                    DataTable table = new DataTable();

                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                    {
                        table.Columns.Add(firstRowCell.Text);
                    }

                    for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                    {
                        var row = worksheet.Cells[rowNumber, 1, rowNumber, worksheet.Dimension.End.Column];
                        var newRow = table.NewRow();
                        foreach (var cell in row)
                        {
                            newRow[cell.Start.Column - 1] = cell.Text;
                        }
                        table.Rows.Add(newRow);
                    }
                    FileUpload Fupload = new FileUpload();
                    await Fupload.CallSQLBulkUploadAsync(this.configuration, table);
                    //using (SqlConnection connection = new SqlConnection(connectionString))
                    //{
                    //    connection.Open();
                    //    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    //    {
                    //        bulkCopy.DestinationTableName = "SPA_FinanceUploadExcelDataTemp";
                    //        await bulkCopy.WriteToServerAsync(table);
                    //    }
                    //}
                }
            }
            return Ok("File uploaded successfully");
        }
    }
}
