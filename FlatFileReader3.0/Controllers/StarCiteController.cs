using FlatFileReader3._0.FileUploadClass;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FlatFileReader3._0.Controllers
{
    public class StarCiteController : Controller
    {
        // GET: StarCite
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Index(HttpPostedFileBase FileUpload, string delimiter = "")
        {
            if (FileUpload.ContentLength > 0)
            {
                try
                {
                    string fileName = Path.GetFileName(FileUpload.FileName);
                    string path = Path.Combine(Server.MapPath("~/App_Data"), fileName);
                    FileUpload.SaveAs(path);

                    int fileExtPos = fileName.LastIndexOf(".");
                    if (fileExtPos >= 0)
                        fileName = fileName.Substring(0, fileExtPos);
                    SplitSourceDBTable fileProcess = new SplitSourceDBTable();
                    DataTable dt = fileProcess.ReadCsv(path, delimiter, fileName);
                    SQLBulkCopy sQLBulkCopy = new SQLBulkCopy();
                    string str = sQLBulkCopy.ProcessBulkCopy(dt, fileName);
                    TempData["Success"] = fileName + " --- " + str;
                    System.IO.File.Delete(path);
                }
                catch (Exception ex)
                {
                    TempData["Fail"] = ex.Message;
                }
            }

            return View();
        }
    }
}