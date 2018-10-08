using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace FlatFileReader3._0.FileUploadClass
{
    public class SplitSourceTargetCsv
    {
        public  DataTable ProcessCSV(string fileName, string delimiter="")
        {
            //Set up our variables
            string Feedback = string.Empty;
            string line = string.Empty;
            string[] strArray;
            DataTable dt = new DataTable();
            DataRow row;
            StreamReader sr = new StreamReader(fileName);
            if (delimiter.ToLower().Contains("\\t"))
                delimiter = "\t";
            //char _delimeter = Convert.ToChar(delimiter);
            //Read the first line and split the string at , with our regular expression in to an array
            line = sr.ReadLine();
            strArray = line.Split(delimiter.ToCharArray());

            for(int i=0;i<= strArray.Length-1;i++)
            {
                dt.Columns.Add(strArray[i].ToString(), typeof(string));
            }

            //Read each line in the CVS file until it’s empty
            while ((line = sr.ReadLine()) != null)
            {
                row = dt.NewRow();

                //add our current value to our data row
                row.ItemArray = line.Split(delimiter.ToCharArray());
                dt.Rows.Add(row);
            }

            //Tidy Streameader up
            sr.Dispose();

            //return a the new DataTable
            return dt;

        }

        //for Source and Target combined CSV files

        public DataTable ProcessSource_Target_CSV(string fileName)
        {
            //Set up our variables
            string Feedback = string.Empty;
            string line = string.Empty;
            string[] strArray;
            DataTable dt = new DataTable();
            DataRow row;
            StreamReader sr = new StreamReader(fileName);

            //Read the first line and split the string at , with our regular expression in to an array
            line = sr.ReadLine();
            strArray = line.Split('|');
            //strArray = Regex.Split(line, '|');

            for (int i = 0; i <= strArray.Length - 1; i++)
            {
                
                if (strArray[i].ToString().Contains(","))
                {
                    strArray[i] = strArray[i].Replace(",", string.Empty);
                }

                dt.Columns.Add(strArray[i].ToString(), typeof(string));
            }

            //Read each line in the CVS file until it’s empty
            while ((line = sr.ReadLine()) != null)
            {
                row = dt.NewRow();
                string[] lineArray;
                lineArray = line.Split('|');

                for (int i = 0; i <= lineArray.Length - 1; i++)
                {
                    if (lineArray[i].ToString().Contains(","))
                    {
                        lineArray[i] = lineArray[i].Replace(",", string.Empty);
                    }
                }

                row.ItemArray = lineArray;
                dt.Rows.Add(row);
            }

            //Tidy Streameader up
            sr.Dispose();

            //return a the new DataTable
            return dt;

        }

        //for Source and Target combined CSV files

        public string Split_Source_Target_fromCSV(string file, string name, string tabletext, Char delimeter)
        {
            //Set up our variables
            string Feedback = string.Empty;
            string line = string.Empty;
            string[] strArray;
            
            
            DataTable source_dt = new DataTable();
            DataRow source_row;
            DataTable target_dt = new DataTable();
            DataRow target_row;
            StreamReader sr = new StreamReader(file);

            //Read the first line and split the string at , with our regular expression in to an array
            line = sr.ReadLine();
            strArray = line.Split('|');
            //strArray = Regex.Split(line, '|');

            for (int i = 0; i <= strArray.Length - 1; i++)
            {
                if (strArray[i].ToString().Contains(","))
                {
                    strArray[i] = strArray[i].Replace(",", string.Empty);
                }
                if(i==1)
                {
                    source_dt.Columns.Add(strArray[i].ToString(), typeof(string));
                }
                else
                {
                    target_dt.Columns.Add(strArray[i].ToString(), typeof(string));
                }
            }
            string[] _source = new string[source_dt.Columns.Count ];
            string[] _target = new string[target_dt.Columns.Count ];

            //Read each line in the CVS file until it’s empty
            while ((line = sr.ReadLine()) != null)
            {
                source_row = source_dt.NewRow();
                target_row = target_dt.NewRow();

                string[] lineArray;
                
                lineArray = line.Split('|');

                for (int i = 0; i <= lineArray.Length - 1; i++)
                {
                    if (lineArray[i].ToString().Contains(","))
                    {
                        lineArray[i] = lineArray[i].Replace(",,", string.Empty);
                    }
                    /*if(i<2)
                    {
                        _source[i] = lineArray[i];
                    }
                    else
                    {
                        _target[i-2] = lineArray[i];
                    }*/


                    if (i == 1)
                    {
                        _source[i-1] = lineArray[i];
                    }
                    else
                    {   
                        if (i == 0)
                        {
                            _target[i] = lineArray[i];
                        }
                        else
                        {
                            _target[i - 1] = lineArray[i];
                        }
                        
                    }
                }
                
                source_row.ItemArray = _source;
                target_row.ItemArray = _target;
                source_dt.Rows.Add(source_row);
                target_dt.Rows.Add(target_row);
            }

          
            //Tidy Streameader up
            sr.Dispose();
            try
            {
                //Feedback = ProcessBulkCopy(source_dt, name + "_source");
                createSourceTableAndData(source_dt, name + "_source", tabletext, delimeter);
            }
            catch(Exception err) { Feedback = err.Message; }
            try
            {
                Feedback = ProcessBulkCopy(target_dt, name + "_target");
            }
            catch (Exception err) { Feedback = err.Message; }

            //return a the new DataTable
            return Feedback;

        }



        public string ProcessBulkCopy(DataTable dt, string filename)
        {
            string Feedback = string.Empty;
            string connString = ConfigurationManager.ConnectionStrings["DataBaseConnectionString"].ConnectionString;

            //make our connection and dispose at the end
            using (SqlConnection conn = new SqlConnection(connString))
            {
                //make our command and dispose at the end
                using (var copy = new SqlBulkCopy(conn))
                {

                    //Open our connection
                    conn.Open();
                    //drop table if alreday exists in database
                    SqlCommand droptable = new SqlCommand("IF  EXISTS (SELECT * FROM dbo.sysobjects where id = object_id('dbo.[" + filename + "]'))drop table dbo.[" + filename + "]", conn);
                    droptable.ExecuteNonQuery();
                    copy.DestinationTableName = "["+ filename +"]" ;

                    //get create table script to create talbe
                    string query= CreateTABLE(filename, dt);
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.ExecuteNonQuery();
                    ///Set target table and tell the number of rows

                    copy.BatchSize = 1000;//dt.Rows.Count;
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

        public string SplitDataTable(string table)
        {
            string Feedback = string.Empty;
            try
            {

                string connString = ConfigurationManager.ConnectionStrings["DataBaseConnectionString"].ConnectionString;

                //make our connection and dispose at the end
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    //Open our connection
                    conn.Open();

                }
            }
            catch (Exception err) { }
            return Feedback;
        }


        public static string CreateTABLE(string tableName, DataTable table)
        {
            string sqlsc;
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
            return sqlsc.Substring(0, sqlsc.Length - 1) + "\n)";
        }

        public string createSourceTableAndData(DataTable dt, string filename, string tabletext, Char delimeter)
        {
            string Feedback = string.Empty;

            try
            {
                List<int> MismatchedSrcArr = new List<int>();

                DataTable source_dt = new DataTable();
                DataRow source_row;

                string[] lineArray;
                lineArray = tabletext.Split(delimeter);

                for(int i=0; i<lineArray.Length; i++)
                {
                    source_dt.Columns.Add(lineArray[i].ToString(), typeof(string));
                }

                string[] _source = new string[source_dt.Columns.Count];

                for(int j=0; j< dt.Rows.Count; j++)
                {
                    source_row = source_dt.NewRow();
                    string sourcerow = dt.Rows[j].ItemArray[0].ToString();

                    lineArray = sourcerow.Split(',');

                    _source = lineArray;
                    try
                    {
                        source_row.ItemArray = _source;
                        source_dt.Rows.Add(source_row);
                    }catch(Exception ex)
                    {
                        Console.Write(ex.Message);
                        MismatchedSrcArr.Add(j);
                    }
                }
                Console.Write(MismatchedSrcArr.Count);
                Feedback = ProcessBulkCopy(source_dt, filename);

            }
            catch(Exception ex)
            {
                Feedback = ex.Message;
            }
            return Feedback;

        }

    }

    
    
}