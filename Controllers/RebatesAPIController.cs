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
//using RebatesRESTAPI.Controllers;
using RebatesAPI.Model;
using System.Reflection.Emit;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Xml;
using RebatesAPI.Utility;
using OfficeOpenXml.Style;
using OfficeOpenXml.Drawing.Style.Effect;

namespace RebatesRESTAPI.Controllers
{
    [Route("api/[controller]")]
    [Route("api/files")]
    [ApiController]   
    public class RebatesAPIController : ControllerBase
    {
        private readonly IConfiguration configuration;

        public RebatesAPIController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        [HttpGet("FetchFileUploadHistory")]
        public async Task<string> FetchFileUploadHistory()
        {
            string jsonData = "";
            SqlParameter[] sqlParameters = { };
            FileUpload Fupload = new FileUpload();
            jsonData = await Fupload.GetJsonDataFromStoredProcedure("Finance", this.configuration, "SPA_FinanceUploadHistoryFetchtest", sqlParameters);
            return jsonData;
        }
        [HttpPost("FetchFileUploadHistoryWithPaging")]
        public async Task<string> FetchFileUploadHistoryWithPaging(Fileuploadhistoryinput input)
            {   
              string jsonData = "";
            string Resultcode = "6000";
            SqlParameter[] sqlParameters = { };
            string xapikey = Request.Headers.Authorization;
            string errorMessage = "";
            bool status;
            string jsonString = "";
            int totalrecords = 0;
            int totalpages = 0;


            try
            {
                //Authorization auth = new Authorization(this.configuration);
                //status = auth.ValidateAuthorization(xapikey, ref jsonData);
                //if (status == false)
               // {
               //     return jsonData;
                //}

                errorMessage = "Records fetched successfully";
                Resultcode = "6000";

                FileUpload Fupload = new FileUpload();
                jsonData = await Fupload.GetJsonDataWithPagingFromStoredProcedureAsync("Finance", this.configuration, "SPA_FinanceUploadHistoryFetchbyPage", input.page, input.pagesize, sqlParameters);

                goto Label;
            Label:
                var jsonObject = new JObject();
                jsonObject.Add("Result", Resultcode);
                jsonObject.Add("Description", errorMessage);
                jsonObject.Add("totalrecords", totalrecords.ToString());
                jsonObject.Add("totalpages", totalpages.ToString());


                var FirstObject = new JObject();
                FirstObject.Add("Output", jsonObject.ToString());

                //string jsonoutputData = "";
                if (jsonData != null)
                    if (jsonData == "")
                    {
                        jsonData = "{}";
                    }
                 FirstObject.Add("GridData", jsonData);
                jsonString = FirstObject.ToString();
                return jsonString;
            }
            catch (Exception ex)
            {
                errorMessage = "Fetching of upload history failed";     //ex.Message  --put this in the AWS queues to capture the actual error;
                Resultcode = "6003";

                var jsonObject = new JObject();
                jsonObject.Add("Result", Resultcode);
                jsonObject.Add("Description", errorMessage);
                jsonObject.Add("totalrecords", totalrecords.ToString());
                jsonObject.Add("totalpages", totalpages.ToString());

                var FirstObject = new JObject();
                FirstObject.Add("Output", jsonObject.ToString());

                
                if (jsonData != null)
                    if (jsonData == "")
                    {
                        jsonData = "{}";
                    }
                FirstObject.Add("GridData", jsonData);
                jsonString = FirstObject.ToString();
                return jsonString;

            }
        }
       
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
                    
                   // ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    ExcelWorkbook workBook = package.Workbook;
                    ExcelWorksheet worksheet = workBook.Worksheets[0];
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

        [HttpPost("UploadRebatesFile")]
        //IFormFile file, int Distributorcode
        //public async Task<String> UploadRebatesFile(RebatesModel Rebates)
        public async Task<String> UploadRebatesFile(IFormFile file, int Distributorcode, string SPAFileUploadedBy)
        {

            //string connectionString = configuration.GetConnectionString("DefaultConnection");
            //IFormFile file;
            //int Distributorcode=0;
            int GroupId = 0;
            int UserRole = 0;
            string Resultcode = "6000";
            string jsonString = "";
            Guid rebatesguid = Guid.NewGuid();
            int SPAUploadStatus = 3;
            string SPAUploadRemarks = "";
            string SPABlobstorageExcelFileLocation = "";

            GroupId = 1;
            UserRole = 1;
            bool status;
            SqlParameter[] sqlParameters = { };
            string xapikey = Request.Headers.Authorization;
            string errorMessage = "";
            string Filename = "";

            if (file != null)
            {
                Filename = file.FileName;
            }

            //file = Rebates.file;
            // Distributorcode = Rebates.distributorcode;

            try
            {
                string jsonData = "";
                Authorization auth = new Authorization(this.configuration);
                status = auth.ValidateAuthorization(xapikey, ref jsonData);
                if (status == false)
                {
                    // return jsonData;
                }


                if (file == null || file.Length == 0)
                {
                    //return BadRequest("File is not selected");
                    errorMessage = "File is not selected";
                    Resultcode = "6001";
                    goto Label;
                }
                string FileExtension = Path.GetExtension(file.FileName);

                if ((FileExtension != ".xlsx"))
                {
                    if (FileExtension != ".csv")
                    {
                        if (FileExtension != ".xls")
                        {
                            errorMessage = "File type is not supported";
                            Resultcode = "6002";
                            goto Label;
                        }
                    }
                }
                if ((FileExtension == ".xlsx"))
                {
                    FileHandler fileHandler = new FileHandler();
                    var files = fileHandler.NewXlsxUploader(file, Distributorcode, SPAFileUploadedBy, configuration);
                }
                else if ((FileExtension == ".xls"))
                {
                    FileHandler fileHandler = new FileHandler();
                    var files = fileHandler.NewXLSUploader(file, Distributorcode, SPAFileUploadedBy, configuration);
                }
                else if ((FileExtension == ".csv"))
                {
                    FileHandler fileHandler = new FileHandler();
                    var files = fileHandler.NewCSVploader(file, Distributorcode, SPAFileUploadedBy, configuration);
                }


            #region

            //    FileUpload spupload = new FileUpload();
            //    //jsonData = await Fupload.SPAAdminFetchAccessrights("Admin", this.configuration, "SPAAdmin_UserGroupAccessRightsFetch", sqlParameters, GroupId, UserRole);
            //    DataTable table;
            //    DataTable SPAcontracttable;
            //    int intnoofcolumnsinExcel = 0;
            //    string DistributorTableName = "";

            //    SPAcontracttable = spupload.SPAFetchSFDCSPAContractandRates("Finance", this.configuration, sqlParameters,Distributorcode);

            //    table = spupload.SPADistributorFetchTemplateFieldNames("Distributor", this.configuration, "SPADistributor_RebatesFieldsFetch", sqlParameters, Distributorcode, ref intnoofcolumnsinExcel, ref DistributorTableName);
            //    //int noofcolumnstoprocess = 0;
            //    //noofcolumnstoprocess = table.Columns.Count;
            //    using (var stream = new MemoryStream())
            //    {
            //        await file.CopyToAsync(stream);
            //        using (var package = new OfficeOpenXml.ExcelPackage(stream))
            //        {
            //            //DataTable table; //= new DataTable();
            //            // DataColumn column = new DataColumn();
            //            //column.DataType = SqlDbType.V


            //            int Rebateclaimid = 1;
            //            int Rebatecolumnnumber = 0;
            //            //Guid rebatesguid = Guid.NewGuid();
            //            int DistributorWatt = 0;
            //            string DisWattnumber = "";
            //            int DistributorQtySold = 0;
            //            decimal DistributorVolume = 0;
            //            decimal DistributorRebateClaim = 0;
            //            decimal DistributorSPAAmount = 0;
            //            string Finalprocessstatus = "PASS";
            //            string Distributorrowprocessstatus = "PASS";
            //            string Distributorprevrowprocessstatus = "PASS";
            //            string CurrentDistributorrowprocessstatus = "PASS";


            //            //ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
            //            if (package.Workbook.Worksheets.Count > 0)
            //            {
            //                //ExcelWorkbook workBook = package.Workbook;
            //                //ExcelWorksheet worksheet = workBook.Worksheets[0];
            //                //var oSheet = package.Workbook.Worksheets["Greentech Rebate Claim Example"];   
            //                ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

            //                //foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
            //                // {
            //                //    table.Columns.Add(firstRowCell.Text);
            //                //  }
            //                Rebateclaimid = 1;

            //                for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
            //                {
            //                    var row = worksheet.Cells[rowNumber, 1, rowNumber, worksheet.Dimension.End.Column];

            //                    //var row = worksheet.Cells[rowNumber, 1, rowNumber, intnoofcolumnsinExcel - 1];

            //                    Rebatecolumnnumber = 0;
            //                    CurrentDistributorrowprocessstatus = "PASS";
            //                    //var row = worksheet.Cells[rowNumber, 1, rowNumber, table.Columns.Count - 1]; //add 25 columns only as per the distributor template.
            //                    var newRow = table.NewRow();
            //                    newRow[Rebatecolumnnumber] = Rebateclaimid;
            //                    Rebatecolumnnumber = Rebatecolumnnumber + 1;
            //                    newRow[Rebatecolumnnumber] = Distributorcode;
            //                    Rebatecolumnnumber = Rebatecolumnnumber + 1;
            //                    newRow[Rebatecolumnnumber] = rebatesguid;
            //                    Rebatecolumnnumber = Rebatecolumnnumber + 1;

            //                    for (var colNumber = 1; colNumber <= intnoofcolumnsinExcel; colNumber++)
            //                    {

            //                        //foreach (var cell in row)
            //                        //{
            //                        //if (table.Columns[Rebatecolumnnumber].ColumnName == "Distributor_Watt")
            //                        //{
            //                        //    DisWattnumber = RebatesUtility.RemoveRECAndGetNumber(cell.Text);
            //                        //    DistributorWatt = int.Parse(DisWattnumber);
            //                        //newRow[Rebatecolumnnumber] = DistributorWatt;
            //                        //    newRow[Rebatecolumnnumber] = cell.Text;
            //                        //}
            //                        //else if (table.Columns[Rebatecolumnnumber].ColumnName == "Distributor_QTYSold")
            //                        //{
            //                        //    DistributorQtySold = int.Parse(cell.Text);
            //                        //newRow[Rebatecolumnnumber] = DistributorQtySold;
            //                        //    newRow[Rebatecolumnnumber] = cell.Text;
            //                        //}

            //                        //else
            //                        //{
            //                        //newRow[cell.Start.Column - 1] = cell.Text;
            //                        newRow[Rebatecolumnnumber] = worksheet.Cells[rowNumber, colNumber].Value;  // cell.Text;
            //                        //}
            //                        if (Rebatecolumnnumber == (intnoofcolumnsinExcel + 3 - 1))
            //                        {
            //                            goto RebatesLabel;
            //                        }
            //                        Rebatecolumnnumber++;

            //                        //}
            //                    }

            //                RebatesLabel:
            //                    DisWattnumber = RebatesUtility.RemoveRECAndGetNumber(newRow["CATALOG_NBR"].ToString());
            //                    if (DisWattnumber != null)
            //                    { if (DisWattnumber.Length > 0)
            //                        {
            //                            DistributorWatt = int.Parse(DisWattnumber);
            //                        }
            //                        else
            //                        {
            //                            DistributorWatt = 0;
            //                        }
            //                    }
            //                    newRow["Distributor_Watt"] = DistributorWatt;

            //                    if (newRow["Distributor_QTYSold"] != null)
            //                    {
            //                        if (newRow["Distributor_QTYSold"].ToString().Length > 0)
            //                        {
            //                            DistributorQtySold = int.Parse(newRow["Distributor_QTYSold"].ToString());
            //                        }
            //                        else
            //                        {
            //                            DistributorQtySold = 0;
            //                        }
            //                    }

            //                    if (newRow["Distributor_RebateClaim"] != null)
            //                    {
            //                        if (newRow["Distributor_RebateClaim"].ToString().Length > 0)
            //                        {
            //                            DistributorRebateClaim = decimal.Parse(newRow["Distributor_RebateClaim"].ToString());
            //                        }
            //                        else
            //                        {
            //                            DistributorRebateClaim = 0;
            //                        }
            //                    }



            //                    DistributorVolume = DistributorWatt * DistributorQtySold;
            //                    newRow["Distributor_Volume"] = DistributorVolume;

            //                    if (DistributorVolume>0)
            //                    {
            //                        DistributorSPAAmount = DistributorRebateClaim / DistributorVolume;
            //                        DistributorSPAAmount = decimal.Parse(DistributorSPAAmount.ToString("0.00"));
            //                    }
            //                    newRow["Distributor_SPAAmount"] = DistributorSPAAmount;
            //                    Console.WriteLine(newRow["SPA_Contract_Number"].ToString());

            //                    newRow["Salesforce_SPAAmount"] = 0;
            //                    newRow["Salesforce_Watt"] = 0;
            //                    newRow["Salesforce_RebateClaim"] = 0;

            //                    DataRow filteredRow = SPAcontracttable.AsEnumerable()
            //                   .Where(singlerow =>singlerow.Field<string>("SPA_Contract_Number") == newRow["SPA_Contract_Number"].ToString()) // Filter based on Id value
            //                   .FirstOrDefault();

            //                    if (filteredRow != null)
            //                    {
            //                        decimal salesforcespaamount = 0;
            //                        salesforcespaamount = decimal.Parse(filteredRow["SPA_Amount"].ToString());
            //                        newRow["Salesforce_SPAAmount"] = salesforcespaamount;    //filteredRow["SPA_Amount"];
            //                        if (RebatesUtility.SearchValueInSemicolonDelimitedString(filteredRow["Watt"].ToString(), newRow["Distributor_Watt"].ToString()) ==true)
            //                        {
            //                            newRow["Salesforce_Watt"] = filteredRow["Distributor_Watt"];

            //                            newRow["Salesforce_RebateClaim"] = salesforcespaamount * DistributorVolume;
            //                        }
            //                        else
            //                        {
            //                            newRow["Distributor_RowProcessRemarks"] = "Cannot find the Watt in SFDC table";
            //                            CurrentDistributorrowprocessstatus = "FAIL";
            //                            //goto Finallabel;
            //                        }
            //                       // newRow["Salesforce_Watt"] = filteredRow["Watt"]; 

            //                    }
            //                    else
            //                    {
            //                        newRow["Salesforce_SPAAmount"] = 0;
            //                        newRow["Salesforce_Watt"] = 0;
            //                        newRow["Salesforce_RebateClaim"] = 0;
            //                        newRow["Distributor_RowProcessRemarks"] = "Cannot find the SPA Contract number in SFDC table";
            //                        CurrentDistributorrowprocessstatus = "FAIL";
            //                    }
            //                    SPAcontracttable.DefaultView.RowFilter = "";

            //                    Distributorprevrowprocessstatus = Distributorrowprocessstatus;

            //                    Distributorrowprocessstatus = CurrentDistributorrowprocessstatus;  //"PASS" currentrowstatus could be fail or pass;


            //                    if (Distributorrowprocessstatus == "PASS")
            //                    {
            //                        newRow["Distributor_RowProcessRemarks"] = "successfully processsed this row";
            //                    }
            //                    else if (Distributorrowprocessstatus == "FAIl")
            //                    {
            //                        newRow["Distributor_RowProcessRemarks"] = "Failed with errors";
            //                    }


            //                Finallabel:
            //                    if ((Distributorrowprocessstatus=="PASS") && (Distributorprevrowprocessstatus=="FAIL"))
            //                    {
            //                        Finalprocessstatus = "FAIL";
            //                    }
            //                    else if ((Distributorrowprocessstatus == "FAIL") && (Distributorprevrowprocessstatus == "PASS"))
            //                    {
            //                        Finalprocessstatus = "FAIL";
            //                    }
            //                    else if ((Distributorrowprocessstatus == "PASS") && (Distributorprevrowprocessstatus == "PASS") && (Finalprocessstatus=="FAIL"))
            //                    {
            //                        Finalprocessstatus = "FAIL";
            //                    }

            //                    newRow["Distributor_RowProcessStatus"] = Distributorrowprocessstatus;


            //                    table.Rows.Add(newRow);
            //                    Rebateclaimid = Rebateclaimid + 1;
            //                }
            //                //InsertData(DataTable dataTable, string strDatabasestore, IConfiguration configuration, string tableName, int Distributorid, Guid uniqueid);
            //                //Bulkinsert is the ideal option primary key will i
            //                FileUpload Fupload = new FileUpload();
            //                //await Fupload.CallSQLBulkUploadAsync(this.configuration, table);
            //               // await Fupload.CallSQLBulkDistributorDataUploadAsync("Distributor", this.configuration, table, DistributorTableName);
            //                Fupload.InsertData(table, "Distributor", this.configuration, DistributorTableName, Distributorcode, rebatesguid);
            //                //using (SqlConnection connection = new SqlConnection(connectionString))
            //                //{
            //                //    connection.Open();
            //                //    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
            //                //    {
            //                //        bulkCopy.DestinationTableName = "SPA_FinanceUploadExcelDataTemp";
            //                //        await bulkCopy.WriteToServerAsync(table);
            //                //    }
            //                //}
            //                if (Finalprocessstatus == "FAIL")
            //                {
            //                    errorMessage = "Upload is successful with data errors";
            //                    Resultcode = "6003";
            //                    goto Label;
            //                }
            //                else if (Finalprocessstatus == "PASS")
            //                {
            //                    errorMessage = "File processed successfully";
            //                    Resultcode = "6000";
            //                    goto Label;
            //                }
            //            }
            //            else 
            //            { 
            //                errorMessage = "There are no sheets in File";
            //                Resultcode = "6003";
            //                goto Label;
            //            }
            //        }
            //    }

            //    errorMessage = "File uploaded successfully";
            //    Resultcode = "6000";
            //    goto Label;
            #endregion

            Label:
                var jsonObject = new JObject();
                jsonObject.Add("Result", Resultcode);
                jsonObject.Add("Description", errorMessage);

                var FirstObject = new JObject();
                FirstObject.Add("Output", jsonObject.ToString());

                string jsonoutputData = "";
                //SqlParameter[] sqlParameters = { };
                //jsonData = await SPAAdminFetchAccessrights("Admin", configuration, "SPAAdmin_UserGroupAccessRightsFetch", sqlParameters, groupid, userroleid);

                if (jsonoutputData != null)
                    if (jsonoutputData == "")
                    {
                        jsonoutputData = "{}";
                    }
                FirstObject.Add("GridData", jsonoutputData);
                jsonString = FirstObject.ToString();




                if (Resultcode == "6000")
                {
                    SPAUploadStatus = 4;
                }
                else
                {
                    SPAUploadStatus = 3;
                }

                SqlParameter[] sqlParameterssp = { };
                FileUpload FuploadSP = new FileUpload();
                SPAUploadRemarks = errorMessage;
                await FuploadSP.SPAFinanceUploadHistoryInsert("Finance", this.configuration, "SPA_FinanceUploadHistoryInsert", sqlParameterssp, Filename, SPAFileUploadedBy, SPAUploadStatus, SPAUploadRemarks, rebatesguid, SPABlobstorageExcelFileLocation);

                return jsonString;


            }
            catch (Exception ex)
            {
                errorMessage = "Upload failed";     //ex.Message  --put this in the AWS queues to capture the actual error;
                Resultcode = "6003";

                var jsonObject = new JObject();
                jsonObject.Add("Result", Resultcode);
                jsonObject.Add("Description", errorMessage);

                var FirstObject = new JObject();
                FirstObject.Add("Output", jsonObject.ToString());

                string jsonoutputData = "";
                //SqlParameter[] sqlParameters = { };
                //jsonData = await SPAAdminFetchAccessrights("Admin", configuration, "SPAAdmin_UserGroupAccessRightsFetch", sqlParameters, groupid, userroleid);

                if (jsonoutputData != null)
                    if (jsonoutputData == "")
                    {
                        jsonoutputData = "{}";
                    }
                FirstObject.Add("GridData", jsonoutputData);
                jsonString = FirstObject.ToString();

                SqlParameter[] sqlParameterssp = { };
                FileUpload FuploadSP = new FileUpload();
                SPAUploadRemarks = errorMessage;
                SPAUploadStatus = 3;
                await FuploadSP.SPAFinanceUploadHistoryInsert("Finance", this.configuration, "SPA_FinanceUploadHistoryInsert", sqlParameterssp, Filename, SPAFileUploadedBy, SPAUploadStatus, SPAUploadRemarks, rebatesguid, SPABlobstorageExcelFileLocation);
                return jsonString;

            }
        }


        //[HttpPost("UploadRebatesFile")]
        ////IFormFile file, int Distributorcode
        ////public async Task<String> UploadRebatesFile(RebatesModel Rebates)
        //public async Task<String> UploadRebatesFile(IFormFile file,int Distributorcode, string SPAFileUploadedBy)
        //{

        //    //string connectionString = configuration.GetConnectionString("DefaultConnection");
        //    //IFormFile file;
        //    //int Distributorcode=0;
        //    int GroupId = 0;
        //    int UserRole = 0;
        //    string Resultcode = "6000";
        //    string jsonString = "";
        //    Guid rebatesguid = Guid.NewGuid();
        //    int SPAUploadStatus = 3;
        //    string SPAUploadRemarks = "";
        //    string SPABlobstorageExcelFileLocation = "";

        //    GroupId = 1;
        //    UserRole = 1;
        //    bool status;
        //    SqlParameter[] sqlParameters = { };
        //    string xapikey = Request.Headers.Authorization;
        //    string errorMessage = "";
        //    string Filename = "";

        //    if (file != null)
        //    {
        //        Filename = file.FileName;
        //    }

        //    //file = Rebates.file;
        //    // Distributorcode = Rebates.distributorcode;

        //    try
        //    {
        //        string jsonData = "";
        //        Authorization auth = new Authorization(this.configuration);
        //        status = auth.ValidateAuthorization(xapikey, ref jsonData);
        //        if (status == false)
        //        {
        //            return jsonData;
        //        }


        //         if (file == null || file.Length == 0)
        //        {
        //            //return BadRequest("File is not selected");
        //            errorMessage = "File is not selected";
        //            Resultcode = "6001";
        //            goto Label;
        //        }
        //        string FileExtension = Path.GetExtension(file.FileName);

        //        if ((FileExtension != ".xlsx"))
        //        {
        //            if (FileExtension != ".csv")
        //            {
        //                if (FileExtension != ".xls")
        //                {
        //                    errorMessage = "File type is not supported";
        //                    Resultcode = "6002";
        //                    goto Label;
        //                }
        //            }
        //        }


        //        FileUpload spupload = new FileUpload();
        //        //jsonData = await Fupload.SPAAdminFetchAccessrights("Admin", this.configuration, "SPAAdmin_UserGroupAccessRightsFetch", sqlParameters, GroupId, UserRole);
        //        DataTable table;
        //        DataTable SPAcontracttable;
        //        int intnoofcolumnsinExcel = 0;
        //        string DistributorTableName = "";

        //        SPAcontracttable = spupload.SPAFetchSFDCSPAContractandRates("Finance", this.configuration, sqlParameters,Distributorcode);

        //        table = spupload.SPADistributorFetchTemplateFieldNames("Distributor", this.configuration, "SPADistributor_RebatesFieldsFetch", sqlParameters, Distributorcode, ref intnoofcolumnsinExcel, ref DistributorTableName);
        //        //int noofcolumnstoprocess = 0;
        //        //noofcolumnstoprocess = table.Columns.Count;
        //        using (var stream = new MemoryStream())
        //        {
        //            await file.CopyToAsync(stream);
        //            using (var package = new OfficeOpenXml.ExcelPackage(stream))
        //            {
        //                //DataTable table; //= new DataTable();
        //                // DataColumn column = new DataColumn();
        //                //column.DataType = SqlDbType.V


        //                int Rebateclaimid = 1;
        //                int Rebatecolumnnumber = 0;
        //                //Guid rebatesguid = Guid.NewGuid();
        //                int DistributorWatt = 0;
        //                string DisWattnumber = "";
        //                int DistributorQtySold = 0;
        //                decimal DistributorVolume = 0;
        //                decimal DistributorRebateClaim = 0;
        //                decimal DistributorSPAAmount = 0;
        //                string Finalprocessstatus = "PASS";
        //                string Distributorrowprocessstatus = "PASS";
        //                string Distributorprevrowprocessstatus = "PASS";
        //                string CurrentDistributorrowprocessstatus = "PASS";


        //                //ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
        //                if (package.Workbook.Worksheets.Count > 0)
        //                {
        //                    //ExcelWorkbook workBook = package.Workbook;
        //                    //ExcelWorksheet worksheet = workBook.Worksheets[0];
        //                    //var oSheet = package.Workbook.Worksheets["Greentech Rebate Claim Example"];   
        //                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

        //                    //foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
        //                    // {
        //                    //    table.Columns.Add(firstRowCell.Text);
        //                    //  }
        //                    Rebateclaimid = 1;

        //                    for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
        //                    {
        //                        var row = worksheet.Cells[rowNumber, 1, rowNumber, worksheet.Dimension.End.Column];

        //                        //var row = worksheet.Cells[rowNumber, 1, rowNumber, intnoofcolumnsinExcel - 1];

        //                        Rebatecolumnnumber = 0;
        //                        CurrentDistributorrowprocessstatus = "PASS";
        //                        //var row = worksheet.Cells[rowNumber, 1, rowNumber, table.Columns.Count - 1]; //add 25 columns only as per the distributor template.
        //                        var newRow = table.NewRow();
        //                        newRow[Rebatecolumnnumber] = Rebateclaimid;
        //                        Rebatecolumnnumber = Rebatecolumnnumber + 1;
        //                        newRow[Rebatecolumnnumber] = Distributorcode;
        //                        Rebatecolumnnumber = Rebatecolumnnumber + 1;
        //                        newRow[Rebatecolumnnumber] = rebatesguid;
        //                        Rebatecolumnnumber = Rebatecolumnnumber + 1;

        //                        for (var colNumber = 1; colNumber <= intnoofcolumnsinExcel; colNumber++)
        //                        {

        //                            //foreach (var cell in row)
        //                            //{
        //                            //if (table.Columns[Rebatecolumnnumber].ColumnName == "Distributor_Watt")
        //                            //{
        //                            //    DisWattnumber = RebatesUtility.RemoveRECAndGetNumber(cell.Text);
        //                            //    DistributorWatt = int.Parse(DisWattnumber);
        //                            //newRow[Rebatecolumnnumber] = DistributorWatt;
        //                            //    newRow[Rebatecolumnnumber] = cell.Text;
        //                            //}
        //                            //else if (table.Columns[Rebatecolumnnumber].ColumnName == "Distributor_QTYSold")
        //                            //{
        //                            //    DistributorQtySold = int.Parse(cell.Text);
        //                            //newRow[Rebatecolumnnumber] = DistributorQtySold;
        //                            //    newRow[Rebatecolumnnumber] = cell.Text;
        //                            //}

        //                            //else
        //                            //{
        //                            //newRow[cell.Start.Column - 1] = cell.Text;
        //                            newRow[Rebatecolumnnumber] = worksheet.Cells[rowNumber, colNumber].Value;  // cell.Text;
        //                            //}
        //                            if (Rebatecolumnnumber == (intnoofcolumnsinExcel + 3 - 1))
        //                            {
        //                                goto RebatesLabel;
        //                            }
        //                            Rebatecolumnnumber++;

        //                            //}
        //                        }

        //                    RebatesLabel:
        //                        DisWattnumber = RebatesUtility.RemoveRECAndGetNumber(newRow["CATALOG_NBR"].ToString());
        //                        if (DisWattnumber != null)
        //                        { if (DisWattnumber.Length > 0)
        //                            {
        //                                DistributorWatt = int.Parse(DisWattnumber);
        //                            }
        //                            else
        //                            {
        //                                DistributorWatt = 0;
        //                            }
        //                        }
        //                        newRow["Distributor_Watt"] = DistributorWatt;

        //                        if (newRow["Distributor_QTYSold"] != null)
        //                        {
        //                            if (newRow["Distributor_QTYSold"].ToString().Length > 0)
        //                            {
        //                                DistributorQtySold = int.Parse(newRow["Distributor_QTYSold"].ToString());
        //                            }
        //                            else
        //                            {
        //                                DistributorQtySold = 0;
        //                            }
        //                        }

        //                        if (newRow["Distributor_RebateClaim"] != null)
        //                        {
        //                            if (newRow["Distributor_RebateClaim"].ToString().Length > 0)
        //                            {
        //                                DistributorRebateClaim = decimal.Parse(newRow["Distributor_RebateClaim"].ToString());
        //                            }
        //                            else
        //                            {
        //                                DistributorRebateClaim = 0;
        //                            }
        //                        }



        //                        DistributorVolume = DistributorWatt * DistributorQtySold;
        //                        newRow["Distributor_Volume"] = DistributorVolume;

        //                        if (DistributorVolume>0)
        //                        {
        //                            DistributorSPAAmount = DistributorRebateClaim / DistributorVolume;
        //                            DistributorSPAAmount = decimal.Parse(DistributorSPAAmount.ToString("0.00"));
        //                        }
        //                        newRow["Distributor_SPAAmount"] = DistributorSPAAmount;
        //                        Console.WriteLine(newRow["SPA_Contract_Number"].ToString());

        //                        newRow["Salesforce_SPAAmount"] = 0;
        //                        newRow["Salesforce_Watt"] = 0;
        //                        newRow["Salesforce_RebateClaim"] = 0;

        //                        DataRow filteredRow = SPAcontracttable.AsEnumerable()
        //                       .Where(singlerow =>singlerow.Field<string>("SPA_Contract_Number") == newRow["SPA_Contract_Number"].ToString()) // Filter based on Id value
        //                       .FirstOrDefault();

        //                        if (filteredRow != null)
        //                        {
        //                            decimal salesforcespaamount = 0;
        //                            salesforcespaamount = decimal.Parse(filteredRow["SPA_Amount"].ToString());
        //                            newRow["Salesforce_SPAAmount"] = salesforcespaamount;    //filteredRow["SPA_Amount"];
        //                            if (RebatesUtility.SearchValueInSemicolonDelimitedString(filteredRow["Watt"].ToString(), newRow["Distributor_Watt"].ToString()) ==true)
        //                            {
        //                                newRow["Salesforce_Watt"] = filteredRow["Distributor_Watt"];

        //                                newRow["Salesforce_RebateClaim"] = salesforcespaamount * DistributorVolume;
        //                            }
        //                            else
        //                            {
        //                                newRow["Distributor_RowProcessRemarks"] = "Cannot find the Watt in SFDC table";
        //                                CurrentDistributorrowprocessstatus = "FAIL";
        //                                //goto Finallabel;
        //                            }
        //                           // newRow["Salesforce_Watt"] = filteredRow["Watt"]; 

        //                        }
        //                        else
        //                        {
        //                            newRow["Salesforce_SPAAmount"] = 0;
        //                            newRow["Salesforce_Watt"] = 0;
        //                            newRow["Salesforce_RebateClaim"] = 0;
        //                            newRow["Distributor_RowProcessRemarks"] = "Cannot find the SPA Contract number in SFDC table";
        //                            CurrentDistributorrowprocessstatus = "FAIL";
        //                        }
        //                        SPAcontracttable.DefaultView.RowFilter = "";

        //                        Distributorprevrowprocessstatus = Distributorrowprocessstatus;

        //                        Distributorrowprocessstatus = CurrentDistributorrowprocessstatus;  //"PASS" currentrowstatus could be fail or pass;


        //                        if (Distributorrowprocessstatus == "PASS")
        //                        {
        //                            newRow["Distributor_RowProcessRemarks"] = "successfully processsed this row";
        //                        }
        //                        else if (Distributorrowprocessstatus == "FAIl")
        //                        {
        //                            newRow["Distributor_RowProcessRemarks"] = "Failed with errors";
        //                        }


        //                    Finallabel:
        //                        if ((Distributorrowprocessstatus=="PASS") && (Distributorprevrowprocessstatus=="FAIL"))
        //                        {
        //                            Finalprocessstatus = "FAIL";
        //                        }
        //                        else if ((Distributorrowprocessstatus == "FAIL") && (Distributorprevrowprocessstatus == "PASS"))
        //                        {
        //                            Finalprocessstatus = "FAIL";
        //                        }
        //                        else if ((Distributorrowprocessstatus == "PASS") && (Distributorprevrowprocessstatus == "PASS") && (Finalprocessstatus=="FAIL"))
        //                        {
        //                            Finalprocessstatus = "FAIL";
        //                        }

        //                        newRow["Distributor_RowProcessStatus"] = Distributorrowprocessstatus;


        //                        table.Rows.Add(newRow);
        //                        Rebateclaimid = Rebateclaimid + 1;
        //                    }
        //                    //InsertData(DataTable dataTable, string strDatabasestore, IConfiguration configuration, string tableName, int Distributorid, Guid uniqueid);
        //                    //Bulkinsert is the ideal option primary key will i
        //                    FileUpload Fupload = new FileUpload();
        //                    //await Fupload.CallSQLBulkUploadAsync(this.configuration, table);
        //                   // await Fupload.CallSQLBulkDistributorDataUploadAsync("Distributor", this.configuration, table, DistributorTableName);
        //                    Fupload.InsertData(table, "Distributor", this.configuration, DistributorTableName, Distributorcode, rebatesguid);
        //                    //using (SqlConnection connection = new SqlConnection(connectionString))
        //                    //{
        //                    //    connection.Open();
        //                    //    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
        //                    //    {
        //                    //        bulkCopy.DestinationTableName = "SPA_FinanceUploadExcelDataTemp";
        //                    //        await bulkCopy.WriteToServerAsync(table);
        //                    //    }
        //                    //}
        //                    if (Finalprocessstatus == "FAIL")
        //                    {
        //                        errorMessage = "Upload is successful with data errors";
        //                        Resultcode = "6003";
        //                        goto Label;
        //                    }
        //                    else if (Finalprocessstatus == "PASS")
        //                    {
        //                        errorMessage = "File processed successfully";
        //                        Resultcode = "6000";
        //                        goto Label;
        //                    }
        //                }
        //                else 
        //                { 
        //                    errorMessage = "There are no sheets in File";
        //                    Resultcode = "6003";
        //                    goto Label;
        //                }
        //            }
        //        }

        //        errorMessage = "File uploaded successfully";
        //        Resultcode = "6000";
        //        goto Label;

        //    Label:
        //        var jsonObject = new JObject();
        //        jsonObject.Add("Result", Resultcode);
        //        jsonObject.Add("Description", errorMessage);

        //        var FirstObject = new JObject();
        //        FirstObject.Add("Output", jsonObject.ToString());

        //        string jsonoutputData = "";
        //        //SqlParameter[] sqlParameters = { };
        //        //jsonData = await SPAAdminFetchAccessrights("Admin", configuration, "SPAAdmin_UserGroupAccessRightsFetch", sqlParameters, groupid, userroleid);

        //        if (jsonoutputData != null)
        //            if (jsonoutputData == "")
        //            {
        //                jsonoutputData = "{}";
        //            }
        //        FirstObject.Add("GridData", jsonoutputData);
        //        jsonString = FirstObject.ToString();




        //        if (Resultcode=="6000")
        //        {
        //            SPAUploadStatus = 4;
        //        }
        //        else
        //        {
        //            SPAUploadStatus = 3;
        //        }

        //        SqlParameter[] sqlParameterssp = { };
        //        FileUpload FuploadSP = new FileUpload();
        //        SPAUploadRemarks = errorMessage;
        //        await FuploadSP.SPAFinanceUploadHistoryInsert("Finance", this.configuration, "SPA_FinanceUploadHistoryInsert", sqlParameterssp, Filename, SPAFileUploadedBy, SPAUploadStatus, SPAUploadRemarks, rebatesguid, SPABlobstorageExcelFileLocation);

        //        return jsonString;
        //    }
        //    catch (Exception ex)
        //    {
        //        errorMessage = "Upload failed";     //ex.Message  --put this in the AWS queues to capture the actual error;
        //        Resultcode = "6003";

        //        var jsonObject = new JObject();
        //        jsonObject.Add("Result", Resultcode);
        //        jsonObject.Add("Description", errorMessage);

        //        var FirstObject = new JObject();
        //        FirstObject.Add("Output", jsonObject.ToString());

        //        string jsonoutputData = "";
        //        //SqlParameter[] sqlParameters = { };
        //        //jsonData = await SPAAdminFetchAccessrights("Admin", configuration, "SPAAdmin_UserGroupAccessRightsFetch", sqlParameters, groupid, userroleid);

        //        if (jsonoutputData != null)
        //            if (jsonoutputData == "")
        //            {
        //                jsonoutputData = "{}";
        //            }
        //        FirstObject.Add("GridData", jsonoutputData);
        //        jsonString = FirstObject.ToString();

        //        SqlParameter[] sqlParameterssp = { };
        //        FileUpload FuploadSP = new FileUpload();
        //        SPAUploadRemarks = errorMessage;
        //        SPAUploadStatus = 3;
        //        await FuploadSP.SPAFinanceUploadHistoryInsert("Finance", this.configuration, "SPA_FinanceUploadHistoryInsert", sqlParameterssp, Filename, SPAFileUploadedBy, SPAUploadStatus, SPAUploadRemarks, rebatesguid, SPABlobstorageExcelFileLocation);
        //        return jsonString;

        //    }
        //}
    }
}
