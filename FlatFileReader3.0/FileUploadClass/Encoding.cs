using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace FlatFileReader3._0.FileUploadClass
{
    public class Encoding
    {
        public string UTF8_Encode(string str)
        {
            try
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                //Encoding csvEncoding = Encoding.UTF8;
                //Encoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(mystring));
            }
            catch (Exception err) { }
            return str;
        }
    }
}