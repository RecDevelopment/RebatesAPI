using RebatesAPI.Databases;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using Spire.Xls;
using Microsoft.VisualBasic.FileIO;
using System.IO.Compression;
using RebatesAPI.Utilities;

namespace RebatesAPI.Utility
{
    public class FileHandler
    {
        public FileHandler()
        {

        }

        public async Task<string> NewXlsxUploader(IFormFile file, int Distributorcode, string SPAFileUploadedBy, IConfiguration configuration, Guid rebatesguid)
        {

            FileUpload spupload = new FileUpload();
            //jsonData = await Fupload.SPAAdminFetchAccessrights("Admin", this.configuration, "SPAAdmin_UserGroupAccessRightsFetch", sqlParameters, GroupId, UserRole);
            DataTable table;
            DataTable SPAcontracttable;
            int intnoofcolumnsinExcel = 0;
            string DistributorTableName = "";
            SqlParameter[] sqlParameters = { };
            string errorMessage = "";
            string Filename = "";
            string Resultcode = "6000";
            string jsonString = "";
            int SPAUploadStatus = 3;
            string SPAUploadRemarks = "";

            SPAcontracttable = spupload.SPAFetchSFDCSPAContractandRates("Finance", configuration, sqlParameters, Distributorcode);

            table = spupload.SPADistributorFetchTemplateFieldNames("Distributor", configuration, "SPADistributor_RebatesFieldsFetch", sqlParameters, Distributorcode, ref intnoofcolumnsinExcel, ref DistributorTableName);

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new OfficeOpenXml.ExcelPackage(stream))
                {
                    //DataTable table; //= new DataTable();
                    // DataColumn column = new DataColumn();
                    //column.DataType = SqlDbType.V


                    int Rebateclaimid = 1;
                    int Rebatecolumnnumber = 0;
                    //Guid rebatesguid = Guid.NewGuid();
                    int DistributorWatt = 0;
                    string DisWattnumber = "";
                    int DistributorQtySold = 0;
                    decimal DistributorVolume = 0;
                    decimal DistributorRebateClaim = 0;
                    decimal DistributorSPAAmount = 0;
                    string Finalprocessstatus = "PASS";
                    string Distributorrowprocessstatus = "PASS";
                    string Distributorprevrowprocessstatus = "PASS";
                    string CurrentDistributorrowprocessstatus = "PASS";


                    //ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    if (package.Workbook.Worksheets.Count > 0)
                    {
                        //ExcelWorkbook workBook = package.Workbook;
                        //ExcelWorksheet worksheet = workBook.Worksheets[0];
                        //var oSheet = package.Workbook.Worksheets["Greentech Rebate Claim Example"];   
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                        //foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                        // {
                        //    table.Columns.Add(firstRowCell.Text);
                        //  }
                        Rebateclaimid = 1;

                        for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
                        {
                            var row = worksheet.Cells[rowNumber, 1, rowNumber, worksheet.Dimension.End.Column];

                            //var row = worksheet.Cells[rowNumber, 1, rowNumber, intnoofcolumnsinExcel - 1];

                            Rebatecolumnnumber = 0;
                            CurrentDistributorrowprocessstatus = "PASS";
                            //var row = worksheet.Cells[rowNumber, 1, rowNumber, table.Columns.Count - 1]; //add 25 columns only as per the distributor template.
                            var newRow = table.NewRow();
                            newRow[Rebatecolumnnumber] = Rebateclaimid;
                            Rebatecolumnnumber = Rebatecolumnnumber + 1;
                            newRow[Rebatecolumnnumber] = Distributorcode;
                            Rebatecolumnnumber = Rebatecolumnnumber + 1;
                            newRow[Rebatecolumnnumber] = rebatesguid;
                            Rebatecolumnnumber = Rebatecolumnnumber + 1;

                            for (var colNumber = 1; colNumber <= intnoofcolumnsinExcel; colNumber++)
                            {

                                //foreach (var cell in row)
                                //{
                                //if (table.Columns[Rebatecolumnnumber].ColumnName == "Distributor_Watt")
                                //{
                                //    DisWattnumber = RebatesUtility.RemoveRECAndGetNumber(cell.Text);
                                //    DistributorWatt = int.Parse(DisWattnumber);
                                //newRow[Rebatecolumnnumber] = DistributorWatt;
                                //    newRow[Rebatecolumnnumber] = cell.Text;
                                //}
                                //else if (table.Columns[Rebatecolumnnumber].ColumnName == "Distributor_QTYSold")
                                //{
                                //    DistributorQtySold = int.Parse(cell.Text);
                                //newRow[Rebatecolumnnumber] = DistributorQtySold;
                                //    newRow[Rebatecolumnnumber] = cell.Text;
                                //}

                                //else
                                //{
                                //newRow[cell.Start.Column - 1] = cell.Text;
                                newRow[Rebatecolumnnumber] = worksheet.Cells[rowNumber, colNumber].Value;  // cell.Text;
                                                                                                           //}
                                if (Rebatecolumnnumber == (intnoofcolumnsinExcel + 3 - 1))
                                {
                                    goto RebatesLabel;
                                }
                                Rebatecolumnnumber++;

                                //}
                            }

                        RebatesLabel:
                            DisWattnumber = RebatesUtility.RemoveRECAndGetNumber(newRow["CATALOG_NBR"].ToString());
                            if (DisWattnumber != null)
                            {
                                if (DisWattnumber.Length > 0)
                                {
                                    DistributorWatt = int.Parse(DisWattnumber);
                                }
                                else
                                {
                                    DistributorWatt = 0;
                                }
                            }
                            newRow["Distributor_Watt"] = DistributorWatt;

                            if (newRow["Distributor_QTYSold"] != null)
                            {
                                if (newRow["Distributor_QTYSold"].ToString().Length > 0)
                                {
                                    DistributorQtySold = int.Parse(newRow["Distributor_QTYSold"].ToString());
                                }
                                else
                                {
                                    DistributorQtySold = 0;
                                }
                            }

                            if (newRow["Distributor_RebateClaim"] != null)
                            {
                                if (newRow["Distributor_RebateClaim"].ToString().Length > 0)
                                {
                                    DistributorRebateClaim = decimal.Parse(newRow["Distributor_RebateClaim"].ToString());
                                }
                                else
                                {
                                    DistributorRebateClaim = 0;
                                }
                            }



                            DistributorVolume = DistributorWatt * DistributorQtySold;
                            newRow["Distributor_Volume"] = DistributorVolume;

                            if (DistributorVolume > 0)
                            {
                                DistributorSPAAmount = DistributorRebateClaim / DistributorVolume;
                                DistributorSPAAmount = decimal.Parse(DistributorSPAAmount.ToString("0.00"));
                            }
                            newRow["Distributor_SPAAmount"] = DistributorSPAAmount;
                            Console.WriteLine(newRow["SPA_Contract_Number"].ToString());

                            newRow["Salesforce_SPAAmount"] = 0;
                            newRow["Salesforce_Watt"] = 0;
                            newRow["Salesforce_RebateClaim"] = 0;

                            DataRow filteredRow = SPAcontracttable.AsEnumerable()
                           .Where(singlerow => singlerow.Field<string>("SPA_Contract_Number") == newRow["SPA_Contract_Number"].ToString()) // Filter based on Id value
                           .FirstOrDefault();

                            if (filteredRow != null)
                            {
                                decimal salesforcespaamount = 0;
                                salesforcespaamount = decimal.Parse(filteredRow["SPA_Amount"].ToString());
                                newRow["Salesforce_SPAAmount"] = salesforcespaamount;    //filteredRow["SPA_Amount"];
                                if (RebatesUtility.SearchValueInSemicolonDelimitedString(filteredRow["Watt"].ToString(), newRow["Distributor_Watt"].ToString()) == true)
                                {
                                    newRow["Salesforce_Watt"] = filteredRow["Distributor_Watt"];

                                    newRow["Salesforce_RebateClaim"] = salesforcespaamount * DistributorVolume;
                                }
                                else
                                {
                                    newRow["Distributor_RowProcessRemarks"] = "Cannot find the Watt in SFDC table";
                                    CurrentDistributorrowprocessstatus = "FAIL";
                                    //goto Finallabel;
                                }
                                // newRow["Salesforce_Watt"] = filteredRow["Watt"]; 

                            }
                            else
                            {
                                newRow["Salesforce_SPAAmount"] = 0;
                                newRow["Salesforce_Watt"] = 0;
                                newRow["Salesforce_RebateClaim"] = 0;
                                newRow["Distributor_RowProcessRemarks"] = "Cannot find the SPA Contract number in SFDC table";
                                CurrentDistributorrowprocessstatus = "FAIL";
                            }
                            SPAcontracttable.DefaultView.RowFilter = "";

                            Distributorprevrowprocessstatus = Distributorrowprocessstatus;

                            Distributorrowprocessstatus = CurrentDistributorrowprocessstatus;  //"PASS" currentrowstatus could be fail or pass;


                            if (Distributorrowprocessstatus == "PASS")
                            {
                                newRow["Distributor_RowProcessRemarks"] = "successfully processsed this row";
                            }
                            else if (Distributorrowprocessstatus == "FAIl")
                            {
                                newRow["Distributor_RowProcessRemarks"] = "Failed with errors";
                            }


                        Finallabel:
                            if ((Distributorrowprocessstatus == "PASS") && (Distributorprevrowprocessstatus == "FAIL"))
                            {
                                Finalprocessstatus = "FAIL";
                            }
                            else if ((Distributorrowprocessstatus == "FAIL") && (Distributorprevrowprocessstatus == "PASS"))
                            {
                                Finalprocessstatus = "FAIL";
                            }
                            else if ((Distributorrowprocessstatus == "PASS") && (Distributorprevrowprocessstatus == "PASS") && (Finalprocessstatus == "FAIL"))
                            {
                                Finalprocessstatus = "FAIL";
                            }

                            newRow["Distributor_RowProcessStatus"] = Distributorrowprocessstatus;


                            table.Rows.Add(newRow);
                            Rebateclaimid = Rebateclaimid + 1;
                        }
                        //InsertData(DataTable dataTable, string strDatabasestore, IConfiguration configuration, string tableName, int Distributorid, Guid uniqueid);
                        //Bulkinsert is the ideal option primary key will i
                        FileUpload Fupload = new FileUpload();
                        //await Fupload.CallSQLBulkUploadAsync(this.configuration, table);
                        // await Fupload.CallSQLBulkDistributorDataUploadAsync("Distributor", this.configuration, table, DistributorTableName);
                        Fupload.InsertData(table, "Distributor", configuration, DistributorTableName, Distributorcode, rebatesguid);
                        //using (SqlConnection connection = new SqlConnection(connectionString))
                        //{
                        //    connection.Open();
                        //    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                        //    {
                        //        bulkCopy.DestinationTableName = "SPA_FinanceUploadExcelDataTemp";
                        //        await bulkCopy.WriteToServerAsync(table);
                        //    }
                        //}

                        var uploaded = await this.CompressFileAndUpload(file, "SPA-RebatesAPI", rebatesguid, Distributorcode, "Claims");
                        if (Finalprocessstatus == "FAIL")
                        {
                            errorMessage = "Upload is successful with data errors";
                            Resultcode = "6003";
                            goto Label;
                        }
                        else if (Finalprocessstatus == "PASS")
                        {
                            errorMessage = "File processed successfully";
                            Resultcode = "6000";
                            goto Label;
                        }
                    }
                    else
                    {
                        errorMessage = "There are no sheets in File";
                        Resultcode = "6003";
                        goto Label;
                    }
                }

            }

            errorMessage = "File uploaded successfully";
            Resultcode = "6000";
            goto Label;


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




            //if (Resultcode == "6000")
            //{
            //    SPAUploadStatus = 4;
            //}
            //else
            //{
            //    SPAUploadStatus = 3;
            //}

            //SqlParameter[] sqlParameterssp = { };
            //FileUpload FuploadSP = new FileUpload();
            //SPAUploadRemarks = errorMessage;

            ////await FuploadSP.SPAFinanceUploadHistoryInsert("Finance", configuration, "SPA_FinanceUploadHistoryInsert", sqlParameterssp, Filename, SPAFileUploadedBy, SPAUploadStatus, SPAUploadRemarks, rebatesguid, SPABlobstorageExcelFileLocation);

            return jsonString;
        }

        public async Task<string> NewXLSUploader(IFormFile file, int Distributorcode, string SPAFileUploadedBy, IConfiguration configuration, Guid rebatesguid)
        {

            FileUpload spupload = new FileUpload();
            //jsonData = await Fupload.SPAAdminFetchAccessrights("Admin", this.configuration, "SPAAdmin_UserGroupAccessRightsFetch", sqlParameters, GroupId, UserRole);
            DataTable table;
            DataTable SPAcontracttable;
            int intnoofcolumnsinExcel = 0;
            string DistributorTableName = "";
            SqlParameter[] sqlParameters = { };
            string errorMessage = "";
            string Filename = "";
            string Resultcode = "6000";
            string jsonString = "";
            int SPAUploadStatus = 3;
            string SPAUploadRemarks = "";

            SPAcontracttable = spupload.SPAFetchSFDCSPAContractandRates("Finance", configuration, sqlParameters, Distributorcode);

            table = spupload.SPADistributorFetchTemplateFieldNames("Distributor", configuration, "SPADistributor_RebatesFieldsFetch", sqlParameters, Distributorcode, ref intnoofcolumnsinExcel, ref DistributorTableName);

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                //using (var package = new OfficeOpenXml.ExcelPackage(stream))
                //{
                //DataTable table; //= new DataTable();
                // DataColumn column = new DataColumn();
                //column.DataType = SqlDbType.V


                int Rebateclaimid = 1;
                int Rebatecolumnnumber = 0;
                //Guid rebatesguid = Guid.NewGuid();
                int DistributorWatt = 0;
                string DisWattnumber = "";
                int DistributorQtySold = 0;
                decimal DistributorVolume = 0;
                decimal DistributorRebateClaim = 0;
                decimal DistributorSPAAmount = 0;
                string Finalprocessstatus = "PASS";
                string Distributorrowprocessstatus = "PASS";
                string Distributorprevrowprocessstatus = "PASS";
                string CurrentDistributorrowprocessstatus = "PASS";


                //ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                Workbook workbook = new Workbook();
                workbook.LoadFromStream(stream);
                if (workbook.Worksheets.Count > 0)
                {
                    //ExcelWorkbook workBook = package.Workbook;
                    //ExcelWorksheet worksheet = workBook.Worksheets[0];
                    //var oSheet = package.Workbook.Worksheets["Greentech Rebate Claim Example"];   
                    var worksheet = workbook.Worksheets[0];

                    //foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                    // {
                    //    table.Columns.Add(firstRowCell.Text);
                    //  }
                    Rebateclaimid = 1;

                    for (var rowNumber = 2; rowNumber <= worksheet.Range.LastRow; rowNumber++)
                    {
                        //var row = worksheet.Cells[rowNumber, 1, rowNumber, worksheet.Range.LastColumn];

                        //var row = worksheet.Cells[rowNumber, 1, rowNumber, intnoofcolumnsinExcel - 1];

                        Rebatecolumnnumber = 0;
                        CurrentDistributorrowprocessstatus = "PASS";
                        //var row = worksheet.Cells[rowNumber, 1, rowNumber, table.Columns.Count - 1]; //add 25 columns only as per the distributor template.
                        var newRow = table.NewRow();
                        newRow[Rebatecolumnnumber] = Rebateclaimid;
                        Rebatecolumnnumber = Rebatecolumnnumber + 1;
                        newRow[Rebatecolumnnumber] = Distributorcode;
                        Rebatecolumnnumber = Rebatecolumnnumber + 1;
                        newRow[Rebatecolumnnumber] = rebatesguid;
                        Rebatecolumnnumber = Rebatecolumnnumber + 1;

                        for (var colNumber = 1; colNumber <= intnoofcolumnsinExcel; colNumber++)
                        {

                            //foreach (var cell in row)
                            //{
                            //if (table.Columns[Rebatecolumnnumber].ColumnName == "Distributor_Watt")
                            //{
                            //    DisWattnumber = RebatesUtility.RemoveRECAndGetNumber(cell.Text);
                            //    DistributorWatt = int.Parse(DisWattnumber);
                            //newRow[Rebatecolumnnumber] = DistributorWatt;
                            //    newRow[Rebatecolumnnumber] = cell.Text;
                            //}
                            //else if (table.Columns[Rebatecolumnnumber].ColumnName == "Distributor_QTYSold")
                            //{
                            //    DistributorQtySold = int.Parse(cell.Text);
                            //newRow[Rebatecolumnnumber] = DistributorQtySold;
                            //    newRow[Rebatecolumnnumber] = cell.Text;
                            //}

                            //else
                            //{
                            //newRow[cell.Start.Column - 1] = cell.Text;
                            newRow[Rebatecolumnnumber] = worksheet.GetStringValue(rowNumber, colNumber); //  [rowNumber, colNumber].Value;  // cell.Text;
                                                                                                         //}
                            if (Rebatecolumnnumber == (intnoofcolumnsinExcel + 3 - 1))
                            {
                                goto RebatesLabel;
                            }
                            Rebatecolumnnumber++;

                            //}
                        }

                    RebatesLabel:
                        DisWattnumber = RebatesUtility.RemoveRECAndGetNumber(newRow["CATALOG_NBR"].ToString());
                        if (DisWattnumber != null)
                        {
                            if (DisWattnumber.Length > 0)
                            {
                                DistributorWatt = int.Parse(DisWattnumber);
                            }
                            else
                            {
                                DistributorWatt = 0;
                            }
                        }
                        newRow["Distributor_Watt"] = DistributorWatt;

                        if (newRow["Distributor_QTYSold"] != null)
                        {
                            if (newRow["Distributor_QTYSold"].ToString().Length > 0)
                            {
                                DistributorQtySold = int.Parse(newRow["Distributor_QTYSold"].ToString());
                            }
                            else
                            {
                                DistributorQtySold = 0;
                            }
                        }

                        if (newRow["Distributor_RebateClaim"] != null)
                        {
                            if (newRow["Distributor_RebateClaim"].ToString().Length > 0)
                            {
                                DistributorRebateClaim = decimal.Parse(newRow["Distributor_RebateClaim"].ToString());
                            }
                            else
                            {
                                DistributorRebateClaim = 0;
                            }
                        }



                        DistributorVolume = DistributorWatt * DistributorQtySold;
                        newRow["Distributor_Volume"] = DistributorVolume;

                        if (DistributorVolume > 0)
                        {
                            DistributorSPAAmount = DistributorRebateClaim / DistributorVolume;
                            DistributorSPAAmount = decimal.Parse(DistributorSPAAmount.ToString("0.00"));
                        }
                        newRow["Distributor_SPAAmount"] = DistributorSPAAmount;
                        Console.WriteLine(newRow["SPA_Contract_Number"].ToString());

                        newRow["Salesforce_SPAAmount"] = 0;
                        newRow["Salesforce_Watt"] = 0;
                        newRow["Salesforce_RebateClaim"] = 0;

                        DataRow filteredRow = SPAcontracttable.AsEnumerable()
                       .Where(singlerow => singlerow.Field<string>("SPA_Contract_Number") == newRow["SPA_Contract_Number"].ToString()) // Filter based on Id value
                       .FirstOrDefault();

                        if (filteredRow != null)
                        {
                            decimal salesforcespaamount = 0;
                            salesforcespaamount = decimal.Parse(filteredRow["SPA_Amount"].ToString());
                            newRow["Salesforce_SPAAmount"] = salesforcespaamount;    //filteredRow["SPA_Amount"];
                            if (RebatesUtility.SearchValueInSemicolonDelimitedString(filteredRow["Watt"].ToString(), newRow["Distributor_Watt"].ToString()) == true)
                            {
                                newRow["Salesforce_Watt"] = filteredRow["Distributor_Watt"];

                                newRow["Salesforce_RebateClaim"] = salesforcespaamount * DistributorVolume;
                            }
                            else
                            {
                                newRow["Distributor_RowProcessRemarks"] = "Cannot find the Watt in SFDC table";
                                CurrentDistributorrowprocessstatus = "FAIL";
                                //goto Finallabel;
                            }
                            // newRow["Salesforce_Watt"] = filteredRow["Watt"]; 

                        }
                        else
                        {
                            newRow["Salesforce_SPAAmount"] = 0;
                            newRow["Salesforce_Watt"] = 0;
                            newRow["Salesforce_RebateClaim"] = 0;
                            newRow["Distributor_RowProcessRemarks"] = "Cannot find the SPA Contract number in SFDC table";
                            CurrentDistributorrowprocessstatus = "FAIL";
                        }
                        SPAcontracttable.DefaultView.RowFilter = "";

                        Distributorprevrowprocessstatus = Distributorrowprocessstatus;

                        Distributorrowprocessstatus = CurrentDistributorrowprocessstatus;  //"PASS" currentrowstatus could be fail or pass;


                        if (Distributorrowprocessstatus == "PASS")
                        {
                            newRow["Distributor_RowProcessRemarks"] = "successfully processsed this row";
                        }
                        else if (Distributorrowprocessstatus == "FAIl")
                        {
                            newRow["Distributor_RowProcessRemarks"] = "Failed with errors";
                        }


                    Finallabel:
                        if ((Distributorrowprocessstatus == "PASS") && (Distributorprevrowprocessstatus == "FAIL"))
                        {
                            Finalprocessstatus = "FAIL";
                        }
                        else if ((Distributorrowprocessstatus == "FAIL") && (Distributorprevrowprocessstatus == "PASS"))
                        {
                            Finalprocessstatus = "FAIL";
                        }
                        else if ((Distributorrowprocessstatus == "PASS") && (Distributorprevrowprocessstatus == "PASS") && (Finalprocessstatus == "FAIL"))
                        {
                            Finalprocessstatus = "FAIL";
                        }

                        newRow["Distributor_RowProcessStatus"] = Distributorrowprocessstatus;


                        table.Rows.Add(newRow);
                        Rebateclaimid = Rebateclaimid + 1;
                    }
                    //InsertData(DataTable dataTable, string strDatabasestore, IConfiguration configuration, string tableName, int Distributorid, Guid uniqueid);
                    //Bulkinsert is the ideal option primary key will i
                    FileUpload Fupload = new FileUpload();
                    //await Fupload.CallSQLBulkUploadAsync(this.configuration, table);
                    // await Fupload.CallSQLBulkDistributorDataUploadAsync("Distributor", this.configuration, table, DistributorTableName);
                    Fupload.InsertData(table, "Distributor", configuration, DistributorTableName, Distributorcode, rebatesguid);
                    //using (SqlConnection connection = new SqlConnection(connectionString))
                    //{
                    //    connection.Open();
                    //    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    //    {
                    //        bulkCopy.DestinationTableName = "SPA_FinanceUploadExcelDataTemp";
                    //        await bulkCopy.WriteToServerAsync(table);
                    //    }
                    //}

                    var uploaded = await this.CompressFileAndUpload(file, "SPA-RebatesAPI", rebatesguid, Distributorcode, "Claims");
                    if (Finalprocessstatus == "FAIL")
                    {
                        errorMessage = "Upload is successful with data errors";
                        Resultcode = "6003";
                        goto Label;
                    }
                    else if (Finalprocessstatus == "PASS")
                    {
                        errorMessage = "File processed successfully";
                        Resultcode = "6000";
                        goto Label;
                    }
                   
                }
                else
                {
                    errorMessage = "There are no sheets in File";
                    Resultcode = "6003";
                    goto Label;
                }
                //}  end of using

            }

            errorMessage = "File uploaded successfully";
            Resultcode = "6000";
            goto Label;


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




            //if (Resultcode == "6000")
            //{
            //    SPAUploadStatus = 4;
            //}
            //else
            //{
            //    SPAUploadStatus = 3;
            //}

            //SqlParameter[] sqlParameterssp = { };
            //FileUpload FuploadSP = new FileUpload();
            //SPAUploadRemarks = errorMessage;

            ////await FuploadSP.SPAFinanceUploadHistoryInsert("Finance", configuration, "SPA_FinanceUploadHistoryInsert", sqlParameterssp, Filename, SPAFileUploadedBy, SPAUploadStatus, SPAUploadRemarks, rebatesguid, SPABlobstorageExcelFileLocation);

            return jsonString;
        }


        public async Task<string> NewCSVploader(IFormFile file, int Distributorcode, string SPAFileUploadedBy, IConfiguration configuration, Guid rebatesguid)
        {
            FileUpload spupload = new FileUpload();
            //jsonData = await Fupload.SPAAdminFetchAccessrights("Admin", this.configuration, "SPAAdmin_UserGroupAccessRightsFetch", sqlParameters, GroupId, UserRole);
            DataTable table;
            DataTable SPAcontracttable;
            int intnoofcolumnsinExcel = 0;
            string DistributorTableName = "";
            SqlParameter[] sqlParameters = { };
            string errorMessage = "";
            string Filename = "";
            string Resultcode = "6000";
            string jsonString = "";
            int SPAUploadStatus = 3;
            string SPAUploadRemarks = "";

            SPAcontracttable = spupload.SPAFetchSFDCSPAContractandRates("Finance", configuration, sqlParameters, Distributorcode);

            table = spupload.SPADistributorFetchTemplateFieldNames("Distributor", configuration, "SPADistributor_RebatesFieldsFetch", sqlParameters, Distributorcode, ref intnoofcolumnsinExcel, ref DistributorTableName);

            using (var stream = new MemoryStream())
            {
                try { 
                    await file.CopyToAsync(stream);
                   
                    
                }
                catch (Exception ex) { }

                int Rebateclaimid = 1;
                int Rebatecolumnnumber = 0;
                //Guid rebatesguid = Guid.NewGuid();
                int DistributorWatt = 0;
                string DisWattnumber = "";
                int DistributorQtySold = 0;
                decimal DistributorVolume = 0;
                decimal DistributorRebateClaim = 0;
                decimal DistributorSPAAmount = 0;
                string Finalprocessstatus = "PASS";
                string Distributorrowprocessstatus = "PASS";
                string Distributorprevrowprocessstatus = "PASS";
                string CurrentDistributorrowprocessstatus = "PASS";


                using (TextFieldParser parser = new TextFieldParser(stream))
                {
                    stream.Position = 0;
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    int currentRow = 0;
                    Rebateclaimid = 1;
                    while (!parser.EndOfData)
                    {
                        currentRow++;
                        string[] fields = parser.ReadFields();

                        if (currentRow > 1)
                        {
                            Rebatecolumnnumber = 0;
                            CurrentDistributorrowprocessstatus = "PASS";
                            //var row = worksheet.Cells[rowNumber, 1, rowNumber, table.Columns.Count - 1]; //add 25 columns only as per the distributor template.
                            var newRow = table.NewRow();
                            newRow[Rebatecolumnnumber] = Rebateclaimid;
                            Rebatecolumnnumber = Rebatecolumnnumber + 1;
                            newRow[Rebatecolumnnumber] = Distributorcode;
                            Rebatecolumnnumber = Rebatecolumnnumber + 1;
                            newRow[Rebatecolumnnumber] = rebatesguid;
                            Rebatecolumnnumber = Rebatecolumnnumber + 1;

                            for (var colNumber = 0; colNumber <= intnoofcolumnsinExcel; colNumber++)
                            {


                                newRow[Rebatecolumnnumber] = fields[colNumber]; //  [rowNumber, colNumber].Value;  // cell.Text;
                                                                                //}
                                if (Rebatecolumnnumber == (intnoofcolumnsinExcel + 2 - 1))
                                {
                                    goto RebatesLabel;
                                }
                                Rebatecolumnnumber++;

                                //}
                            }

                        RebatesLabel:
                            DisWattnumber = RebatesUtility.RemoveRECAndGetNumber(newRow["CATALOG_NBR"].ToString());
                            //DisWattnumber = RebatesUtility.RemoveRECAndGetNumber(newRow[14].ToString());
                            if (DisWattnumber != null)
                            {
                                if (DisWattnumber.Length > 0)
                                {
                                    DistributorWatt = int.Parse(DisWattnumber);
                                }
                                else
                                {
                                    DistributorWatt = 0;
                                }
                            }
                            newRow["Distributor_Watt"] = DistributorWatt;

                            if (newRow["Distributor_QTYSold"] != null)
                            {
                                if (newRow["Distributor_QTYSold"].ToString().Length > 0)
                                {
                                    var testing = newRow["Distributor_QTYSold"].ToString();
                                    var testing2 = newRow[0].ToString();
                                    DistributorQtySold = int.Parse(newRow["Distributor_QTYSold"].ToString());
                                }
                                else
                                {
                                    DistributorQtySold = 0;
                                }
                            }

                            if (newRow["Distributor_RebateClaim"] != null)
                            {
                                if (newRow["Distributor_RebateClaim"].ToString().Length > 0)
                                {
                                    DistributorRebateClaim = decimal.Parse(newRow["Distributor_RebateClaim"].ToString());
                                }
                                else
                                {
                                    DistributorRebateClaim = 0;
                                }
                            }

                            DistributorVolume = DistributorWatt * DistributorQtySold;
                            newRow["Distributor_Volume"] = DistributorVolume;

                            if (DistributorVolume > 0)
                            {
                                DistributorSPAAmount = DistributorRebateClaim / DistributorVolume;
                                DistributorSPAAmount = decimal.Parse(DistributorSPAAmount.ToString("0.00"));
                            }
                            newRow["Distributor_SPAAmount"] = DistributorSPAAmount;
                            Console.WriteLine(newRow["SPA_Contract_Number"].ToString());

                            newRow["Salesforce_SPAAmount"] = 0;
                            newRow["Salesforce_Watt"] = 0;
                            newRow["Salesforce_RebateClaim"] = 0;

                            DataRow filteredRow = SPAcontracttable.AsEnumerable()
                           .Where(singlerow => singlerow.Field<string>("SPA_Contract_Number") == newRow["SPA_Contract_Number"].ToString()) // Filter based on Id value
                           .FirstOrDefault();

                            if (filteredRow != null)
                            {
                                decimal salesforcespaamount = 0;
                                salesforcespaamount = decimal.Parse(filteredRow["SPA_Amount"].ToString());
                                newRow["Salesforce_SPAAmount"] = salesforcespaamount;    //filteredRow["SPA_Amount"];
                                if (RebatesUtility.SearchValueInSemicolonDelimitedString(filteredRow["Watt"].ToString(), newRow["Distributor_Watt"].ToString()) == true)
                                {
                                    newRow["Salesforce_Watt"] = filteredRow["Distributor_Watt"];

                                    newRow["Salesforce_RebateClaim"] = salesforcespaamount * DistributorVolume;
                                }
                                else
                                {
                                    newRow["Distributor_RowProcessRemarks"] = "Cannot find the Watt in SFDC table";
                                    CurrentDistributorrowprocessstatus = "FAIL";
                                    //goto Finallabel;
                                }
                                // newRow["Salesforce_Watt"] = filteredRow["Watt"]; 

                            }
                            else
                            {
                                newRow["Salesforce_SPAAmount"] = 0;
                                newRow["Salesforce_Watt"] = 0;
                                newRow["Salesforce_RebateClaim"] = 0;
                                newRow["Distributor_RowProcessRemarks"] = "Cannot find the SPA Contract number in SFDC table";
                                CurrentDistributorrowprocessstatus = "FAIL";
                            }
                            SPAcontracttable.DefaultView.RowFilter = "";

                            Distributorprevrowprocessstatus = Distributorrowprocessstatus;

                            Distributorrowprocessstatus = CurrentDistributorrowprocessstatus;  //"PASS" currentrowstatus could be fail or pass;


                            if (Distributorrowprocessstatus == "PASS")
                            {
                                newRow["Distributor_RowProcessRemarks"] = "successfully processsed this row";
                            }
                            else if (Distributorrowprocessstatus == "FAIl")
                            {
                                newRow["Distributor_RowProcessRemarks"] = "Failed with errors";
                            }

                        Finallabel:
                            if ((Distributorrowprocessstatus == "PASS") && (Distributorprevrowprocessstatus == "FAIL"))
                            {
                                Finalprocessstatus = "FAIL";
                            }
                            else if ((Distributorrowprocessstatus == "FAIL") && (Distributorprevrowprocessstatus == "PASS"))
                            {
                                Finalprocessstatus = "FAIL";
                            }
                            else if ((Distributorrowprocessstatus == "PASS") && (Distributorprevrowprocessstatus == "PASS") && (Finalprocessstatus == "FAIL"))
                            {
                                Finalprocessstatus = "FAIL";
                            }

                            newRow["Distributor_RowProcessStatus"] = Distributorrowprocessstatus;


                            table.Rows.Add(newRow);
                            Rebateclaimid = Rebateclaimid + 1;

                        }

                    }

                    FileUpload Fupload = new FileUpload();
                    //await Fupload.CallSQLBulkUploadAsync(this.configuration, table);
                    // await Fupload.CallSQLBulkDistributorDataUploadAsync("Distributor", this.configuration, table, DistributorTableName);
                    Fupload.InsertData(table, "Distributor", configuration, DistributorTableName, Distributorcode, rebatesguid);
                    //using (SqlConnection connection = new SqlConnection(connectionString))
                    //{
                    //    connection.Open();
                    //    using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                    //    {
                    //        bulkCopy.DestinationTableName = "SPA_FinanceUploadExcelDataTemp";
                    //        await bulkCopy.WriteToServerAsync(table);
                    //    }
                    //}

                    var uploaded = await this.CompressFileAndUpload(file, "SPA-RebatesAPI", rebatesguid, Distributorcode, "Claims");
                    if (Finalprocessstatus == "FAIL")
                    {
                        errorMessage = "Upload is successful with data errors";
                        Resultcode = "6003";
                        goto Label;
                    }
                    else if (Finalprocessstatus == "PASS")
                    {
                        errorMessage = "File processed successfully";
                        Resultcode = "6000";
                        goto Label;
                    }

                }
            }

            errorMessage = "File uploaded successfully";
            Resultcode = "6000";
            goto Label;

            //////

            

            //////

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

            return jsonString;

        }

        public async Task<bool> CompressFileAndUpload(IFormFile formFile, string source, Guid ID, int distributorCode, string FileType)
        {
            try
            {
                byte[] fileBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await formFile.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }

                byte[] compressedBytes;
                using (var compressedStream = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress))
                    {
                        await gzipStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                    }
                    compressedBytes = compressedStream.ToArray();
                }

                string compressedBase64String = Convert.ToBase64String(compressedBytes);

                var json = new JObject();
                json["FileName"] = formFile.FileName;
                json["FileData"] = compressedBase64String;
                json["ProcessName"] = FileType;
                json["DataType"] = "File";
                json["Source"] = source;
                json["DistributorCode"] = distributorCode;
                json["StorageLocation"] = "";
                json["Id"] = ID;

                AzureQueueHandler azure = new AzureQueueHandler();
                var result = await azure.writeToAzureQueue(json);

                return result;
            }
            catch (Exception ex) { 
            }
            return false;
        }

    }
}
