using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;

namespace FlatFileReader3._0.FileUploadClass
{
    public class SplitSourceDBTable
    {
        public DataTable ReadCsv(string filename, string delimiter = "", string table = "")
        {
            DataTable dt = new DataTable();
            long totalLines = 0;
            try
            {
                string Feedback = string.Empty;
                string[] strArray;
                DataRow row;
                FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                using (StreamReader streamReader = new StreamReader(fileStream))
                {
                    string charsRead = streamReader.ReadLine();
                    if (delimiter.ToLower().Contains("\\t"))
                        delimiter = "\t";
                    strArray = charsRead.Split(delimiter.ToCharArray());

                    for (int i = 0; i <= strArray.Length - 1; i++)
                    {
                        dt.Columns.Add(strArray[i].ToString(), typeof(string));
                    }
                    CreateTableShema createTableShema = new CreateTableShema();
                    bool result = createTableShema.GetTableMetadata(table +"_Original", dt);
                    if (result == false)
                    {
                        
                        dt.Clear();
                        string connString = ConfigurationManager.ConnectionStrings["DataBaseConnectionString"].ConnectionString;
                        DataTable TempDt = new DataTable();
                        //make our connection and dispose at the end
                        SqlDataAdapter adt = new SqlDataAdapter("select * from "+ table +"", connString);
                        adt.Fill(TempDt);
                        long linesProcessed = 0;
                        for (int i=0;i<= TempDt.Rows.Count-1;i++)
                        {
                            if(i < 100000)
                            {
                                row = dt.NewRow();
                                string chararray = TempDt.Rows[i][1].ToString();
                                row.ItemArray = chararray.Split(delimiter.ToCharArray());
                                dt.Rows.Add(row);
                                linesProcessed += 1;
                                totalLines += 1;
                            }
                            else
                            {
                                SQLBulkCopy sql = new SQLBulkCopy();
                                string rees = sql.ProcessBulkCopy(dt, table + "_Original");
                                dt.Clear();
                                linesProcessed = 0;
                            }
                            
                        }
                        
                    }
                }


            }
            catch (Exception err) { }
            return dt;

        }
    }
}