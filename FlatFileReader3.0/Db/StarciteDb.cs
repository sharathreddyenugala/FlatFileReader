using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using FlatFileReader3._0.Models;

namespace FlatFileReader3._0.Db
{
    public class StarciteDb
    {
        MedispendEntities MedispendEntities = new MedispendEntities();
        starcite_global_conf Starcite = new starcite_global_conf();

        public virtual IList<starcite_global_conf> getStarCiteConfig()
        {
            var row = from starcite_global_conf in MedispendEntities.starcite_global_conf
                      .Where(r => r.isActive == 1)
                      select starcite_global_conf;
            return row.ToList();

        }


        public DataTable getStarCiteTargetData(string tableName)
        {
            DataTable dt = new DataTable();
            try
            {
                string cs = ConfigurationManager.ConnectionStrings["DataBaseConnectionString"].ConnectionString;
                string query = "select * from " + tableName;
                SqlDataAdapter cmd = new SqlDataAdapter(query, cs);
                cmd.Fill(dt);
                   
            }
            catch(Exception ex)
            {
                
            }

            return dt;
        }

    }
}