using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FlatFileReader3._0.FileUploadClass
{
    public class CreateTableShema
    {
        public bool GetTableMetadata(string tableName, DataTable table)
        {
            bool res = true;
            string sqlsc;
            string query = "";
            sqlsc = "CREATE TABLE [" + tableName + "] (";
            for (int i = 0; i < table.Columns.Count; i++)
            {
                sqlsc += "\n [" + table.Columns[i].ColumnName + "] ";
                string columnType = table.Columns[i].DataType.ToString();
                switch (columnType)
                {
                    case "System.Int32":
                        sqlsc += " int ";
                        break;
                    case "System.Int64":
                        sqlsc += " bigint ";
                        break;
                    case "System.Int16":
                        sqlsc += " smallint";
                        break;
                    case "System.Byte":
                        sqlsc += " tinyint";
                        break;
                    case "System.Decimal":
                        sqlsc += " decimal ";
                        break;
                    case "System.DateTime":
                        sqlsc += " datetime ";
                        break;
                    case "System.String":
                    default:
                        sqlsc += string.Format(" nvarchar({0}) ", table.Columns[i].MaxLength == -1 ? "max" : table.Columns[i].MaxLength.ToString());
                        break;
                }
                if (table.Columns[i].AutoIncrement)
                    sqlsc += " IDENTITY(" + table.Columns[i].AutoIncrementSeed.ToString() + "," + table.Columns[i].AutoIncrementStep.ToString() + ") ";
                if (!table.Columns[i].AllowDBNull)
                    sqlsc += " NOT NULL ";
                sqlsc += ",";
            }
             query= sqlsc.Substring(0, sqlsc.Length - 1) + "\n)";
            string connString = ConfigurationManager.ConnectionStrings["DataBaseConnectionString"].ConnectionString;

            //make our connection and dispose at the end
            SqlConnection conn = new SqlConnection(connString);
            //conn.ConnectionString = connString;
            conn.Open();
            string drop = "IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[" + tableName + "]') AND type in (N'U')) DROP TABLE[dbo].[" + tableName + "] ";
            SqlCommand Dropcommand = new SqlCommand(drop, conn);
            Dropcommand.ExecuteNonQuery();


            string str = "IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].["+ tableName + "]') AND type in (N'U')) BEGIN   "+ query +" END";

            SqlCommand command = new SqlCommand(str, conn);
            var result =command.ExecuteScalar();
            conn.Close();
            return res =  result != null ? (int)result > 0 : false;

            
            
        }
    }
}