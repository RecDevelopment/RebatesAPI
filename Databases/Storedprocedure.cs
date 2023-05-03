namespace RebatesAPI.Databases
{
    using Microsoft.Extensions.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using RebatesAPI.Model;
    using System.Diagnostics.Eventing.Reader;
    using Newtonsoft.Json.Linq;
    using System.Text.RegularExpressions;
    using RebatesAPI.Utilities;
    using System.Net.Mail;
    using RebatesAPI.Utility;

    public class FileUpload
    {
        private const DbType @string = DbType.String;
        private string connectionString="";
        private static void OnRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            Console.WriteLine("Copied {0} rows.", e.RowsCopied);
            //Later write the no. of rows copied to database, along with excel file name etc.
        }

        public async Task<string> GetJsonDataWithPagingFromStoredProcedureAsync(string strDatabasestore, IConfiguration configuration, string storedProcedureName, int page, int pageSize, SqlParameter[] parameters)
        {
            int totalrecords = 0;
            int totalpages = 0;
            //totalrecords = 0;
            //totalpages = 0;
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
            List<FinanceUploadHistory> results = new List<FinanceUploadHistory>();
            if ((strDatabasestore == "Finance") || ((strDatabasestore == "Default")))
                this.connectionString = configuration.GetConnectionString("DefaultConnection");
            else if ((strDatabasestore == "Admin"))
                this.connectionString = configuration.GetConnectionString("AdminConnection");
            else if ((strDatabasestore == "Auth"))
                this.connectionString = configuration.GetConnectionString("AuthConnection");
            else if ((strDatabasestore == "Distributor"))
                this.connectionString = configuration.GetConnectionString("DistributorConnection");
            else if ((strDatabasestore == "Log"))
                this.connectionString = configuration.GetConnectionString("LogConnection");

            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }
                    //Add any input parameters to the stored procedure if required
                    command.Parameters.AddWithValue("@page", page);
                    command.Parameters.AddWithValue("@pagesize", pageSize);
                    SqlParameter totalCountParam = new SqlParameter("@TotalCount", SqlDbType.Int);
                    totalCountParam.Direction = ParameterDirection.Output;
                    command.Parameters.Add(totalCountParam);
                  

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        

                        if (reader.HasRows)
                        {
                            while (await reader.ReadAsync())
                            {
                                FinanceUploadHistory data = new FinanceUploadHistory()
                                {
                                    SPAUploadId = Convert.ToInt32(reader["SPAUploadId"]),
                                    UniqueId = reader["UniqueId"].ToString(),

                                    SPAExcelFileName = reader["SPAExcelFileName"].ToString()
                                       ,
                                    SPAFileUploadedBy = reader["SPAFileuploadedby"].ToString(),
                                    SPAUploadFileStatus = reader["SPAUploadFileStatus"].ToString(),
                                    SPAFileProcessStatus = reader["SPAUploadFileStatus"].ToString(),
                                    SPAuploadprocessedFile = reader["SPAuploadprocessedFile"].ToString(),
                                    SPAFileApprovalStatus = reader["SPAFileApprovalStatus"].ToString(),
                                    SPAProcessedFilelocation = reader["SPAProcessedFilelocation"].ToString(),
                                    SPAUploadstatus = Convert.ToInt32(reader["SPAUploadstatus"]),
                                    SPAfileUploadedby = reader["SPAProcessedFilelocation"].ToString(),
                                    SPAUploadRemarks = reader["SPAUploadRemarks"].ToString(),
                                    SPAUploadCreatedate = reader["SPAUploadcreateddate"].ToString(),
                                    SPAUploadHHMMSS = reader["SPAUploadHHMMSS"].ToString(),
                                    SPAUploadprocessedstatus = Convert.ToInt32(reader["SPAUploadprocessedstatus"].ToString()),
                                    SPAUploadprocessedBy = reader["SPAUploadprocessedBy"].ToString(),
                                    SPAUploadprocessedDate =reader["SPAUploadprocessedDate"].ToString(),
                                    SPAUploadApprovalStatus = Convert.ToInt32(reader["SPAUploadApprovalStatus"]),
                                    SPAUploadApprovedBy = reader["SPAUploadApprovedBy"].ToString(),
                                    SPAUploadApprovedDate = reader["SPAUploadApprovedDate"].ToString(),
     
    };

                                results.Add(data);
                            };
                         

                            // Apply paging
                            //results = results.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                        }
                        else
                        {
                            totalrecords = 0;
                            totalpages = 0;
                        }
                       
                    };
                    totalrecords = int.Parse(totalCountParam.Value.ToString());
                    totalpages = (int)Math.Ceiling((double)totalrecords / pageSize);
                };
            };

            return JsonConvert.SerializeObject(new {totalrecords=totalrecords, totalpages=totalpages,Data = results });

        }

        public async Task<string> GetJsonDataFromStoredProcedure(string strDatabasestore, IConfiguration configuration, string storedProcedureName, SqlParameter[] parameters)
        {
            string jsonData = null;
            if ((strDatabasestore == "Finance") || ((strDatabasestore == "Default")))
                this.connectionString = configuration.GetConnectionString("DefaultConnection");
            else if ((strDatabasestore == "Admin"))
                this.connectionString = configuration.GetConnectionString("AdminConnection");
            else if ((strDatabasestore == "Auth"))
                this.connectionString = configuration.GetConnectionString("AuthConnection");
            else if ((strDatabasestore == "Distributor"))
                this.connectionString = configuration.GetConnectionString("DistributorConnection");
            else if ((strDatabasestore == "Log"))
                this.connectionString = configuration.GetConnectionString("LogConnection");



            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            DataTable dataTable = new DataTable();
                            dataTable.Load(reader);
                            jsonData = JsonConvert.SerializeObject(dataTable);
                        }
                    }
                }
            }

            return jsonData;
        }

        public async Task<string> SPAAuthLogin(string strDatabasestore, IConfiguration configuration, string storedProcedureName, SqlParameter[] parameters,string Emailid, string password)
        {
            //Replace the connection string with your MSSQL Server connection string
            if ((strDatabasestore == "Finance") || ((strDatabasestore == "Default")))
                this.connectionString = configuration.GetConnectionString("DefaultConnection");
            else if ((strDatabasestore == "Admin"))
                this.connectionString = configuration.GetConnectionString("AdminConnection");
            else if ((strDatabasestore == "Auth"))
                this.connectionString = configuration.GetConnectionString("AuthConnection");
            else if ((strDatabasestore == "Distributor"))
                this.connectionString = configuration.GetConnectionString("DistributorConnection");
            else if ((strDatabasestore == "Log"))
                this.connectionString = configuration.GetConnectionString("LogConnection");

            int groupid = 0;
            int userroleid = 0;

            //string storedProcedureName = "YourStoredProcedureName";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                {
                    //Set the command type to stored procedure
                    command.CommandType = CommandType.StoredProcedure;

                    //Add any input parameters to the stored procedure if required
                    command.Parameters.AddWithValue("@username", Emailid);
                    command.Parameters.AddWithValue("@password", password);

                    //Add the output parameter to the stored procedure
                    SqlParameter outputParameter = new SqlParameter("@SPAResult", SqlDbType.Int);
                    outputParameter.Direction = ParameterDirection.Output;
                    command.Parameters.Add(outputParameter);

                    SqlParameter varoutputParameter = new SqlParameter("@SPAResultDesc", SqlDbType.VarChar, 500);
                    varoutputParameter.Direction = ParameterDirection.Output;
                    command.Parameters.Add(varoutputParameter);


                    SqlParameter vargroupoutputParameter = new SqlParameter("@SPAGroupId", SqlDbType.Int);
                    vargroupoutputParameter.Direction = ParameterDirection.Output;
                    command.Parameters.Add(vargroupoutputParameter);

                    SqlParameter varuserroleoutputParameter = new SqlParameter("@SPAUserRole", SqlDbType.TinyInt);
                    varuserroleoutputParameter.Direction = ParameterDirection.Output;
                    command.Parameters.Add(varuserroleoutputParameter);

                    //Execute the stored procedure
                    command.ExecuteNonQuery();
                    //await command.ExecuteNonQueryAsync();

                    int outputValue = int.Parse(outputParameter.Value.ToString());
                    string outputvalue1 = varoutputParameter.Value.ToString();
                    groupid = int.Parse(vargroupoutputParameter.Value.ToString());
                    userroleid= int.Parse(varuserroleoutputParameter.Value.ToString());


                    var jsonObject = new JObject();
                    jsonObject.Add("Result", outputValue);
                    jsonObject.Add("Description", outputvalue1);

                    var FirstObject = new JObject();
                    FirstObject.Add("Output",jsonObject.ToString());
                    
                    string jsonData = "";
                    SqlParameter[] sqlParameters = { };
                    jsonData = await SPAAdminFetchAccessrights("Admin", configuration, "SPAAdmin_UserGroupAccessRightsFetch", sqlParameters, groupid, userroleid);

                    if (jsonData != null)
                        if (jsonData == "")
                        {
                            jsonData = "{}";
                        }
                    FirstObject.Add("AccessRigths", jsonData);
                    string jsongString = FirstObject.ToString();


                    return jsongString;

                }
            }
        }
        // Helper method to map SqlDbType to Type
        static Type GetDataTypeFromSqlDbType(SqlDbType sqlDbType)
        {
            switch (sqlDbType)
            {
                case SqlDbType.VarChar:
                case SqlDbType.NVarChar:
                    return typeof(string);
                case SqlDbType.Int:
                    return typeof(int);
                case SqlDbType.Float:
                    return typeof(double);
                case SqlDbType.DateTime:
                    return typeof(DateTime);
                // Add more cases for other SqlDbType values as needed
                default:
                    throw new ArgumentException($"Unknown SqlDbType: {sqlDbType}");
            }
        }

        public void InsertData(DataTable dataTable, string strDatabasestore, IConfiguration configuration, string tableName, int Distributorid, Guid uniqueid)
        {
            if ((strDatabasestore == "Finance") || ((strDatabasestore == "Default")))
                this.connectionString = configuration.GetConnectionString("DefaultConnection");
            else if ((strDatabasestore == "Admin"))
                this.connectionString = configuration.GetConnectionString("AdminConnection");
            else if ((strDatabasestore == "Auth"))
                this.connectionString = configuration.GetConnectionString("AuthConnection");
            else if ((strDatabasestore == "Distributor"))
                this.connectionString = configuration.GetConnectionString("DistributorConnection");
            else if ((strDatabasestore == "Log"))
                this.connectionString = configuration.GetConnectionString("LogConnection");

            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                connection.Open();

                foreach (DataRow row in dataTable.Rows)
                {
                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = GenerateSqlInsertStatement(row, tableName, Distributorid, uniqueid);
                        command.ExecuteNonQuery();
                    }
                }

                connection.Close();
            }
        }

        private static string GenerateSqlInsertStatement(DataRow row, string tableName, int Distributorid, Guid uniqueid)
        {
            //string insertStatement = $"INSERT INTO {tableName} ('Distributorid,UniqueId' + {GetColumnNames(row)}) VALUES ({Distributorid},{uniqueid},{GetValues(row)});";
            string insertStatement = $"INSERT INTO {tableName} ({GetColumnNames(row)}) VALUES ({GetValues(row)});";
            return insertStatement;
        }

        private static string GetColumnNames(DataRow row)
        {
            string columnNames = string.Join(", ", row.Table.Columns.Cast<DataColumn>().Select(c => c.ColumnName));
            return columnNames;
        }

        private static string GetValues(DataRow row)
        {
            string values = string.Join(", ", row.ItemArray.Select(v => $"'{v}'"));
            return values;
        }

        public DataTable SPADistributorFetchTemplateFieldNames(string strDatabasestore, IConfiguration configuration, string storedProcedureName, SqlParameter[] parameters, int Distributorid, ref int NoofcolumnsinExcel,ref string DistributorTableName)
        {
            string jsonData = "";
            if ((strDatabasestore == "Finance") || ((strDatabasestore == "Default")))
                this.connectionString = configuration.GetConnectionString("DefaultConnection");
            else if ((strDatabasestore == "Admin"))
                this.connectionString = configuration.GetConnectionString("AdminConnection");
            else if ((strDatabasestore == "Auth"))
                this.connectionString = configuration.GetConnectionString("AuthConnection");
            else if ((strDatabasestore == "Distributor"))
                this.connectionString = configuration.GetConnectionString("DistributorConnection");
            else if ((strDatabasestore == "Log"))
                this.connectionString = configuration.GetConnectionString("LogConnection");

            // Create SqlConnection
            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                // Create a new DataTable
                DataTable dataTable = new DataTable();
                int rowcount = 0;
                

                // Create SqlCommand for the stored procedure
                using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                {
                        command.CommandType = CommandType.StoredProcedure;

                        if (parameters != null)
                        {
                            command.Parameters.AddRange(parameters);
                        }

                        //Add any input parameters to the stored procedure if required
                        command.Parameters.AddWithValue("@DistributoridId", Distributorid);

                        connection.Open();
                    // Execute the stored procedure and read the data reader
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            string columnName1Value = "";
                            DataColumn dataclaimid = new DataColumn("RebateclaimRowId", typeof(Int64));
                            dataTable.Columns.Add(dataclaimid);
                            DataColumn datadistributorid = new DataColumn("Distributorid", typeof(Int16));
                            dataTable.Columns.Add(datadistributorid);
                            DataColumn dataUniqueId = new DataColumn("UniqueId", typeof(Guid));
                            dataTable.Columns.Add(dataUniqueId);

                            while (reader.Read())
                            {
                                // Access values in each column by column name or index
                                columnName1Value = reader["TemplateFieldName"].ToString();
                                //string columnName = reader.GetName(i);
                                //SqlDbType dataType = reader.GetFieldType(i) == typeof(string) ? SqlDbType.VarChar : SqlDbType.Int;
                                DbType dataType = DbType.String;
                                DataColumn column = new DataColumn(columnName1Value, typeof(string));
                                dataTable.Columns.Add(column);
                                // ... Access values for other columns as needed

                                // Process the row data as needed
                                //Console.WriteLine("ColumnName1: {0}, ColumnName2: {1}", columnName1Value, columnName2Value);
                                // Use the dataTable as needed
                                if (rowcount == 1)
                                {
                                    if (reader["NoofcolumnsinExcel"] == null)
                                    {
                                        NoofcolumnsinExcel = 0;
                                    }
                                    else
                                    {
                                        NoofcolumnsinExcel = int.Parse(reader["NoofcolumnsinExcel"].ToString());
                                    }

                                    if (reader["DistributorTableName"] == null)
                                    {
                                        DistributorTableName = "";
                                    }
                                    else
                                    {
                                        DistributorTableName = reader["DistributorTableName"].ToString();
                                    }
                                }
                                rowcount = 1;
                            }

                            DataColumn dataDisWatt = new DataColumn("Distributor_Watt", typeof(int));
                            //dataDisVolume.MaxLength = 10;                            
                            dataTable.Columns.Add(dataDisWatt);

                            DataColumn dataDisVolume = new DataColumn("Distributor_Volume", typeof(decimal));
                            //dataDisVolume.MaxLength = 10;                            
                            dataTable.Columns.Add(dataDisVolume);


                            DataColumn dataDisSPAAmount = new DataColumn("Distributor_SPAAmount", typeof(decimal));
                            //dataDisSPAAmount.MaxLength = 10;
                            dataTable.Columns.Add(dataDisSPAAmount);

                            DataColumn dataSFDCSPAAmount = new DataColumn("Salesforce_SPAAmount", typeof(decimal));
                            //dataDisSPAAmount.MaxLength = 10;
                            dataTable.Columns.Add(dataSFDCSPAAmount);

                            DataColumn dataSFDCWatt = new DataColumn("Salesforce_Watt", typeof(decimal));
                            //dataSFDCWatt.MaxLength = 10;
                            dataTable.Columns.Add(dataSFDCWatt);


                            DataColumn dataSFDCRebateClaim = new DataColumn("Salesforce_RebateClaim", typeof(decimal));
                            //dataSFDCRebateClaim.MaxLength = 10;
                            dataTable.Columns.Add(dataSFDCRebateClaim);

                            DataColumn dataDisRowProcessStatus = new DataColumn("Distributor_RowProcessStatus", typeof(string));
                            dataTable.Columns.Add(dataDisRowProcessStatus);

                            DataColumn dataDisRowProcessRemarks = new DataColumn("Distributor_RowProcessRemarks", typeof(string));
                            dataTable.Columns.Add(dataDisRowProcessRemarks);
                            // Loop through the data reader to get field names and create DataColumns
                            //for (int i = 0; i < reader.rowcount; i++)
                            //{
                            //   string columnName = reader.GetName(i);
                            //  DataColumn column = new DataColumn(columnName);
                            // dataTable.Columns.Add(column);
                            // }

                            // Load data from the data reader into the DataTable
                            //dataTable.Load(reader);

                            
                                
                            Console.WriteLine("DataTable created successfully with {0} rows and {1} columns.", dataTable.Rows.Count, dataTable.Columns.Count);
                        }
                        else
                        {
                            Console.WriteLine("No data returned by the stored procedure.");
                        }
                    }
                }
                return dataTable;
            }
        }


        public DataTable SPAFetchSFDCSPAContractandRates(string strDatabasestore, IConfiguration configuration,  SqlParameter[] parameters, int Distributorid)
        {
            string jsonData = "";
            string storedProcedureName = "SPA_SFDCContractandratesFetch";
            if ((strDatabasestore == "Finance") || ((strDatabasestore == "Default")))
                this.connectionString = configuration.GetConnectionString("DefaultConnection");
            else if ((strDatabasestore == "Admin"))
                this.connectionString = configuration.GetConnectionString("AdminConnection");
            else if ((strDatabasestore == "Auth"))
                this.connectionString = configuration.GetConnectionString("AuthConnection");
            else if ((strDatabasestore == "Distributor"))
                this.connectionString = configuration.GetConnectionString("DistributorConnection");
            else if ((strDatabasestore == "Log"))
                this.connectionString = configuration.GetConnectionString("LogConnection");

            // Create SqlConnection
            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                // Create a new DataTable
                DataTable dataTable = new DataTable();
                int rowcount = 0;

                
                DataColumn SPAContractnumbercolumn = new DataColumn("SPA_Contract_Number", typeof(string));
                dataTable.Columns.Add(SPAContractnumbercolumn);

               
                DataColumn Accountnamecolumn = new DataColumn("Account_Name", typeof(string));
                dataTable.Columns.Add(Accountnamecolumn);


                DataColumn Channelnamecolumn = new DataColumn("Channel_Name", typeof(string));
                dataTable.Columns.Add(Channelnamecolumn);

                DataColumn Installernamecolumn = new DataColumn("Installer_Name", typeof(string));
                dataTable.Columns.Add(Installernamecolumn);


                DataColumn PrimarySPARecipientcolumn = new DataColumn("Primary_SPA_Recipient", typeof(string));
                dataTable.Columns.Add(PrimarySPARecipientcolumn);


                DataColumn StartDatecolumn = new DataColumn("Start_Date", typeof(string));
                dataTable.Columns.Add(StartDatecolumn);

                DataColumn EndDatecolumn = new DataColumn("End_Date", typeof(string));
                dataTable.Columns.Add(EndDatecolumn);

                DataColumn ProductSeriescolumn = new DataColumn("Product_Series", typeof(string));
                dataTable.Columns.Add(ProductSeriescolumn);

                DataColumn Wattcolumn = new DataColumn("Watt", typeof(string));
                dataTable.Columns.Add(Wattcolumn);

                DataColumn Statuscolumn = new DataColumn("Status", typeof(string));
                dataTable.Columns.Add(Statuscolumn);

                DataColumn spaamtcurrencycolumn = new DataColumn("SPA_Amount_currency", typeof(string));
                dataTable.Columns.Add(spaamtcurrencycolumn);

                DataColumn spaamountcolumn = new DataColumn("SPA_Amount", typeof(decimal));
                dataTable.Columns.Add(spaamountcolumn);


                DataColumn volumeforecastcolumn = new DataColumn("Initial_Volume_Forecast_KW", typeof(int));
                dataTable.Columns.Add(volumeforecastcolumn);


                DataColumn volumecapcolumn = new DataColumn("Volume_cap", typeof(string));
                dataTable.Columns.Add(volumecapcolumn);


                DataColumn volumecapKWcolumn = new DataColumn("Volume_cap_KW", typeof(int));
                dataTable.Columns.Add(volumecapKWcolumn);
                // Create SqlCommand for the stored procedure
                using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    //Add any input parameters to the stored procedure if required
                    command.Parameters.AddWithValue("@Distributorid", Distributorid);

                    connection.Open();
                    // Execute the stored procedure and read the data reader
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var newRow = dataTable.NewRow();
                                newRow["SPA_Contract_Number"] = reader["SPA_Contract_Number"];
                                newRow["Account_Name"] = reader["Account_Name"];
                                newRow["Channel_Name"] = reader["Channel_Name"];
                                newRow["Installer_Name"] = reader["Installer_Name"];
                                newRow["Primary_SPA_Recipient"] = reader["Primary_SPA_Recipient"];
                                newRow["Start_Date"] = reader["Start_Date"];
                                newRow["End_Date"] = reader["End_Date"];
                                newRow["Product_Series"] = reader["Product_Series"];
                                newRow["Watt"] = reader["Watt"];
                                newRow["status"] = reader["status"];
                                newRow["SPA_Amount_Currency"] = reader["SPA_Amount_Currency"];
                                newRow["SPA_Amount"] = reader["SPA_Amount"];
                                newRow["Initial_Volume_Forecast_KW"] = reader["Initial_Volume_Forecast_KW"];
                                newRow["Volume_Cap"] = reader["Volume_Cap"];
                                newRow["Volume_Cap_KW"] = reader["Volume_Cap_KW"];


                                dataTable.Rows.Add(newRow);
                            }
                                //Console.WriteLine("DataTable created successfully with {0} rows and {1} columns.", dataTable.Rows.Count, dataTable.Columns.Count);
                        }
                        else
                        {
                            Console.WriteLine("No data returned by the stored procedure.");
                        }
                    }
                }
                return dataTable;
            }
        }


        public async Task<string> SPAAdminFetchAccessrights(string strDatabasestore, IConfiguration configuration, string storedProcedureName, SqlParameter[] parameters,int GroupId, int UserRole)
        {
            string jsonData = "";
            if ((strDatabasestore == "Finance") || ((strDatabasestore == "Default")))
                this.connectionString = configuration.GetConnectionString("DefaultConnection");
            else if ((strDatabasestore == "Admin"))
                this.connectionString = configuration.GetConnectionString("AdminConnection");
            else if ((strDatabasestore == "Auth"))
                this.connectionString = configuration.GetConnectionString("AuthConnection");
            else if ((strDatabasestore == "Distributor"))
                this.connectionString = configuration.GetConnectionString("DistributorConnection");
            else if ((strDatabasestore == "Log"))
                this.connectionString = configuration.GetConnectionString("LogConnection");



            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    //Add any input parameters to the stored procedure if required
                    command.Parameters.AddWithValue("@GroupId", GroupId);
                    command.Parameters.AddWithValue("@UserRole", UserRole);

                    //Add the output parameter to the stored procedure
                    //SqlParameter outputParameter = new SqlParameter("@OutputParamName", SqlDbType.NVarChar, 50);
                    //outputParameter.Direction = ParameterDirection.Output;
                    //command.Parameters.Add(outputParameter);

                    await connection.OpenAsync();

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (reader.HasRows)
                        {
                            DataTable dataTable = new DataTable();
                            dataTable.Load(reader);
                            jsonData = JsonConvert.SerializeObject(dataTable);
                        }
                    }
                }
            }

            return jsonData;
        }



        public async Task<string> SPAPWDReset(string strDatabasestore, IConfiguration configuration, string storedProcedureName, SqlParameter[] parameters, string Emailid)
        {
            int rertunCode = 0;
            string rertunrDesc = "";
            string jsongString = "";

            try
            {
                if ((strDatabasestore == "Finance") || ((strDatabasestore == "Default")))
                    this.connectionString = configuration.GetConnectionString("DefaultConnection");
                else if ((strDatabasestore == "Admin"))
                    this.connectionString = configuration.GetConnectionString("AdminConnection");
                else if ((strDatabasestore == "Auth"))
                    this.connectionString = configuration.GetConnectionString("AuthConnection");
                else if ((strDatabasestore == "Distributor"))
                    this.connectionString = configuration.GetConnectionString("DistributorConnection");
                else if ((strDatabasestore == "Log"))
                    this.connectionString = configuration.GetConnectionString("LogConnection");

                string emailAddress = "";
                int userroleid = 0;

                Random rand = new Random(818);
                int randVal = rand.Next(100000, 999999);

                string token = EncryptionOption.getHash(Emailid + DateTime.Now.ToString("hhmmssffffff") + randVal.ToString());
                // Has the token  

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                    {
                        //Set the command type to stored procedure
                        command.CommandType = CommandType.StoredProcedure;

                        //Add any input parameters to the stored procedure if required
                        command.Parameters.AddWithValue("@username", Emailid);
                        command.Parameters.AddWithValue("@token", token);

                        SqlParameter outParaSPAResult = new SqlParameter("@SPAResult", SqlDbType.Int);
                        outParaSPAResult.Direction = ParameterDirection.Output;
                        command.Parameters.Add(outParaSPAResult);

                        SqlParameter outParaSPAResultDesc = new SqlParameter("@SPAResultDesc", SqlDbType.VarChar, 500);
                        outParaSPAResultDesc.Direction = ParameterDirection.Output;
                        command.Parameters.Add(outParaSPAResultDesc);

                        SqlParameter outParaSPAemail = new SqlParameter("@SPAemail", SqlDbType.VarChar, 255);
                        outParaSPAemail.Direction = ParameterDirection.Output;
                        command.Parameters.Add(outParaSPAemail);

                        command.ExecuteNonQuery();

                        rertunCode = int.Parse(outParaSPAResult.Value.ToString());
                        rertunrDesc = outParaSPAResultDesc.Value.ToString();
                        emailAddress = outParaSPAemail.Value.ToString();

                        var jsonObject = new JObject();
                        jsonObject.Add("Result", rertunCode);
                        jsonObject.Add("Description", rertunrDesc);
                        jsonObject.Add("Email", emailAddress);

                        jsongString = jsonObject.ToString();

                        if (rertunCode == 50010)
                        {



                            var emailContents = new SMTPEmailSender.EmailDetails();
                            emailContents.Subject = configuration["EmailSettings:PasswordRecovery:EmailSubject"].ToString();
                            emailContents.Body = @"<html> 
<head></head> 
<body>  
<table> 
<tr> 
<td width=10%>&nbsp;</td> 
<td width=80%>
<table style=""text-align:left; background-color:#E0E0E0 "">
<tr>
	<td style=""background-color:#606060;padding:20px;vertical-align:middle; color:#FFFFFF; font-size:15px;text-align:center;"">
	REC Distribution Portal- <strong> PASSWORD RESET </strong> 
	</td>
<tr>
<tr>
	<td style=""text-align:left; padding-left:50px;padding-right:50px;padding-top:20px; padding-bottom:20px; background-color:#E0E0E0 "">
<p> Dear User,<p/> 
<p> We have received a request to<strong> reset your password</strong> to the<strong> REC Distribution Portal.</strong><p/> 
<p><a href = '"+EnvironmentDetails.getUrl(configuration)+"/PasswordEntryScr?token=" + token + @"' > Click this link to set your new password.</a></p> 
<p> 
This link is valid for the next 24 hours.</br> 
</p>
<p>  Sunny Regards,</br></br> 
</p>
<p>  REC Group.</br> 
</p>

</td>
<tr>
</table>
<td width=10%>&nbsp;</td> 
</tr></table> 
</body>  
</html>
";

                            emailContents.FromAddress = configuration["EmailSettings:PasswordRecovery:EmailAddress"].ToString();
                            var toList = new List<string>();

                            toList.Add(emailAddress);


                            emailContents.ToAddresses = toList;


                            Emails emailSender = new Utilities.Emails();
                            emailSender.SendEmails(emailContents, "SMPTSettings", configuration);

                        }

                        return jsongString;

                    }
                }
            }
            catch (Exception ex)
            {
                //return null;
            }

            rertunCode = 50019;
            rertunrDesc = "Something Went Wrong in Sending the Password Reset Email";

            var jsonErrObject = new JObject();
            jsonErrObject.Add("Result", rertunCode);
            jsonErrObject.Add("Description", rertunrDesc);
            jsonErrObject.Add("Email", "");

            jsongString = jsonErrObject.ToString();

            return jsongString;

        }


        public async Task<string> SPAPWDResetApply(string strDatabasestore, IConfiguration configuration, string storedProcedureName, SqlParameter[] parameters, string token, string password, string passwordComfirm)
        {
            int rertunCode = 0;
            string rertunrDesc = "";
            string jsongString = "";
            try
            {

                //if (password == passwordComfirm)
                //{

                if ((strDatabasestore == "Finance") || ((strDatabasestore == "Default")))
                    this.connectionString = configuration.GetConnectionString("DefaultConnection");
                else if ((strDatabasestore == "Admin"))
                    this.connectionString = configuration.GetConnectionString("AdminConnection");
                else if ((strDatabasestore == "Auth"))
                    this.connectionString = configuration.GetConnectionString("AuthConnection");
                else if ((strDatabasestore == "Distributor"))
                    this.connectionString = configuration.GetConnectionString("DistributorConnection");
                else if ((strDatabasestore == "Log"))
                    this.connectionString = configuration.GetConnectionString("LogConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                    {
                        //Set the command type to stored procedure
                        command.CommandType = CommandType.StoredProcedure;

                        //Add any input parameters to the stored procedure if required
                        command.Parameters.AddWithValue("@token", token);
                        command.Parameters.AddWithValue("@newPassword", password);


                        SqlParameter outParaSPAResult = new SqlParameter("@SPAResult", SqlDbType.Int);
                        outParaSPAResult.Direction = ParameterDirection.Output;
                        command.Parameters.Add(outParaSPAResult);

                        SqlParameter outParaSPAResultDesc = new SqlParameter("@SPAResultDesc", SqlDbType.VarChar, 500);
                        outParaSPAResultDesc.Direction = ParameterDirection.Output;
                        command.Parameters.Add(outParaSPAResultDesc);

                        command.ExecuteNonQuery();

                        rertunCode = int.Parse(outParaSPAResult.Value.ToString());
                        rertunrDesc = outParaSPAResultDesc.Value.ToString();

                        //if (rertunCode == 50015)
                        //{

                        var jsonObject = new JObject();
                        jsonObject.Add("Result", rertunCode);
                        jsonObject.Add("Description", rertunrDesc);

                        jsongString = jsonObject.ToString();
                        return jsongString;
                        //}
                        //else
                        //{
                        //    return null;
                        //}

                    }
                }
                //}
                //else
                //{
                //    var jsonObject = new JObject();
                //    jsonObject.Add("Result", 50018);
                //    jsonObject.Add("Description", "Passwords Do Not Match");

                //    string jsongString = jsonObject.ToString();
                //    return jsongString;
                //}
            }
            catch (Exception ex)
            { }
            rertunCode = 50020;
            rertunrDesc = "Something Went Wrong in Updating the New Password";

            var jsonErrObject = new JObject();
            jsonErrObject.Add("Result", rertunCode);
            jsonErrObject.Add("Description", rertunrDesc);
            jsonErrObject.Add("Email", "");

            jsongString = jsonErrObject.ToString();

            return jsongString;
        }

        public async Task CallSQLBulkUploadAsync(IConfiguration configuration,DataTable table)
        {
            //Replace the connection string with your MSSQL Server connection string
            //string connectionString = "Data Source=YOUR_SERVER_NAME;Initial Catalog=YOUR_DATABASE_NAME;Integrated Security=True";
            this.connectionString = configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "SPA_FinanceUploadExcelDataTemp";
                    bulkCopy.SqlRowsCopied += new SqlRowsCopiedEventHandler(OnRowsCopied);
                    bulkCopy.NotifyAfter = table.Rows.Count;
                    await bulkCopy.WriteToServerAsync(table);

                    
                    //noofrowsprocessed = bulkCopy.SqlRowsCopied;
                }
                
            }
        }


        public async Task CallSQLBulkDistributorDataUploadAsync(string strDatabasestore, IConfiguration configuration, DataTable table, string strTableName)
        {
            //Replace the connection string with your MSSQL Server connection string
            //string connectionString = "Data Source=YOUR_SERVER_NAME;Initial Catalog=YOUR_DATABASE_NAME;Integrated Security=True";
           ;

            if ((strDatabasestore == "Finance") || ((strDatabasestore == "Default")))
                this.connectionString = configuration.GetConnectionString("DefaultConnection");
            else if ((strDatabasestore == "Admin"))
                this.connectionString = configuration.GetConnectionString("AdminConnection");
            else if ((strDatabasestore == "Auth"))
                this.connectionString = configuration.GetConnectionString("AuthConnection");
            else if ((strDatabasestore == "Distributor"))
                this.connectionString = configuration.GetConnectionString("DistributorConnection");
            else if ((strDatabasestore == "Log"))
                this.connectionString = configuration.GetConnectionString("LogConnection");


            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                
                connection.Open();
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = strTableName;    // "SPA_RebatesTemplate1WESCODist1";
                    bulkCopy.SqlRowsCopied += new SqlRowsCopiedEventHandler(OnRowsCopied);
                    bulkCopy.NotifyAfter = table.Rows.Count;
                    await bulkCopy.WriteToServerAsync(table);


                    //noofrowsprocessed = bulkCopy.SqlRowsCopied;
                }

            }
        }


        public async Task<string> SPAFinanceUploadHistoryInsert(string strDatabasestore, IConfiguration configuration, string storedProcedureName, SqlParameter[] parameters, string SPAExcelFileName, string SPAFileUploadedBy,int SPAUploadStatus, string SPAUploadRemarks, Guid SPAUploadGUID, string SPABlobstorageExcelFileLocation)
        {
            //Replace the connection string with your MSSQL Server connection string
            if ((strDatabasestore == "Finance") || ((strDatabasestore == "Default")))
                this.connectionString = configuration.GetConnectionString("DefaultConnection");
            else if ((strDatabasestore == "Admin"))
                this.connectionString = configuration.GetConnectionString("AdminConnection");
            else if ((strDatabasestore == "Auth"))
                this.connectionString = configuration.GetConnectionString("AuthConnection");
            else if ((strDatabasestore == "Distributor"))
                this.connectionString = configuration.GetConnectionString("DistributorConnection");
            else if ((strDatabasestore == "Log"))
                this.connectionString = configuration.GetConnectionString("LogConnection");

            int groupid = 0;
            int userroleid = 0;

            //string storedProcedureName = "YourStoredProcedureName";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                {
                    //Set the command type to stored procedure
                    command.CommandType = CommandType.StoredProcedure;

                    //Add any input parameters to the stored procedure if required
                    command.Parameters.AddWithValue("@SPAExcelFileName", SPAExcelFileName);
                    command.Parameters.AddWithValue("@SPAFileUploadedBy", SPAFileUploadedBy);
                    command.Parameters.AddWithValue("@SPAUploadStatus", SPAUploadStatus);
                    command.Parameters.AddWithValue("@SPAUploadRemarks", SPAUploadRemarks);
                    command.Parameters.AddWithValue("@SPAUploadGUID", SPAUploadGUID);
                    command.Parameters.AddWithValue("@SPAExcelFileLocation", SPABlobstorageExcelFileLocation);

                    //Add the output parameter to the stored procedure
                    SqlParameter outputParameter = new SqlParameter("@SPAResult", SqlDbType.Int);
                    outputParameter.Direction = ParameterDirection.Output;
                    command.Parameters.Add(outputParameter);

                    SqlParameter varoutputParameter = new SqlParameter("@SPAResultDesc", SqlDbType.VarChar, 500);
                    varoutputParameter.Direction = ParameterDirection.Output;
                    command.Parameters.Add(varoutputParameter);

                    //Execute the stored procedure
                    command.ExecuteNonQuery();
                    //await command.ExecuteNonQueryAsync();

                    int outputValue = int.Parse(outputParameter.Value.ToString());
                    string outputvalue1 = varoutputParameter.Value.ToString();
                   
                    var jsonObject = new JObject();
                    jsonObject.Add("Result", outputValue);
                    jsonObject.Add("Description", outputvalue1);

                    var FirstObject = new JObject();
                    FirstObject.Add("Output", jsonObject.ToString());
                                        
                    string jsongString = FirstObject.ToString();

                    return jsongString;

                }
            }
        }
        public void CallStoredProcedure(IConfiguration configuration)
        {
            //Replace the connection string with your MSSQL Server connection string
            //string connectionString = "Data Source=YOUR_SERVER_NAME;Initial Catalog=YOUR_DATABASE_NAME;Integrated Security=True";
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
            //Replace "YourStoredProcedureName" with the name of your stored procedure
            string storedProcedureName = "YourStoredProcedureName";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(storedProcedureName, connection))
                {
                    //Set the command type to stored procedure
                    command.CommandType = CommandType.StoredProcedure;

                    //Add any input parameters to the stored procedure if required
                    command.Parameters.AddWithValue("@InputParamName", "InputParamValue");

                    //Add the output parameter to the stored procedure
                    SqlParameter outputParameter = new SqlParameter("@OutputParamName", SqlDbType.NVarChar, 50);
                    outputParameter.Direction = ParameterDirection.Output;
                    command.Parameters.Add(outputParameter);

                    //Execute the stored procedure
                    command.ExecuteNonQuery();

                    //Get the value of the output parameter
                    string outputValue = outputParameter.Value.ToString();
                }
            }
        }

    }
}
