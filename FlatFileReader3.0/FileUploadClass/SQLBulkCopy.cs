using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FlatFileReader3._0.FileUploadClass
{
    public class SQLBulkCopy
    {
        public string ProcessBulkCopy(DataTable dt, string tablenmae)
        {
            string Feedback = string.Empty;
            string connString = ConfigurationManager.ConnectionStrings["DataBaseConnectionString"].ConnectionString;

            //make our connection and dispose at the end
            using (SqlConnection conn = new SqlConnection(connString))
            {
                //make our command and dispose at the end
                using (var copy = new SqlBulkCopy(conn))
                {
                    conn.Open();
                    copy.DestinationTableName = "[" + tablenmae + "]";
                    copy.BatchSize = 50000;//dt.Rows.Count;
                    try
                    {
                        //Send it to the server
                        copy.WriteToServer(dt);
                        Feedback = "upload succesfully";
                        if (conn.State == ConnectionState.Open) { conn.Close(); }
                    }
                    catch (Exception ex)
                    {
                        Feedback = ex.Message;
                        if (conn.State == ConnectionState.Open) { conn.Close(); }
                    }
                }
            }

            return Feedback;
        }
    }
}