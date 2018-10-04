using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.IO;
using System.Text;
using System.Data.SqlClient;
using System.Configuration;


namespace FlatFileReader3._0.FileUploadClass
{
    public class FileProcess
    {
        public DataTable csv(string filename, string delimiter = "",string table="")
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
                    bool result = createTableShema.GetTableMetadata(table, dt);
                   if(result == false )
                    {
                        long linesProcessed = 0;
                        while (!String.IsNullOrEmpty(charsRead))
                        {
                           if(linesProcessed < 100000)
                            {
                                charsRead = streamReader.ReadLine();

                                row = dt.NewRow();
                                UTF8Encoding utf8 = new UTF8Encoding();


                                var preamble = utf8.GetPreamble();

                                row.ItemArray = charsRead.Split(delimiter.ToCharArray());
                                

                              

                                dt.Rows.Add(row);
                                linesProcessed += 1;
                                totalLines += 1;
                            }
                            else
                            {
                                SQLBulkCopy sql = new SQLBulkCopy();
                                string rees = sql.ProcessBulkCopy(dt, table);
                                dt.Clear();
                                linesProcessed = 0;
                            }
                           
                        }//while loop endpoint
                    }//result loop endpoint
                }
                

            }
            catch (Exception err) { }
            return dt;
            
        }


    }
}