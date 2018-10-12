using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FlatFileReader3._0.FileUploadClass;
using FlatFileReader3._0.Db;

namespace FlatFileReader3._0.Controllers
{
    public class HomeController : Controller
    {
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
                    FileProcess fileProcess = new FileProcess();
                    DataTable dt = fileProcess.csv(path, delimiter, fileName);
                    SQLBulkCopy sQLBulkCopy = new SQLBulkCopy();
                    string str = sQLBulkCopy.ProcessBulkCopy(dt, fileName);
                    TempData["Success"] = fileName +" --- "+ str;
                    System.IO.File.Delete(path);
                }
                catch (Exception ex)
                {
                    TempData["Fail"] = ex.Message;
                }
            }

            return View();
        }

        
        public ActionResult sap()
        {
           return View();
        }
        [HttpPost]
        public ActionResult sap(HttpPostedFileBase FileUpload, string tabletext, Char delimiter)
        {
            if (FileUpload.ContentLength > 0)
            {


                try
                {
                    string fileName = Path.GetFileName(FileUpload.FileName);
                    string path = Path.Combine(Server.MapPath("~/App_Data"), fileName);
                    FileUpload.SaveAs(path);
                    SplitSourceTargetCsv dataProcess = new SplitSourceTargetCsv();
                    int fileExtPos = fileName.LastIndexOf(".");
                    if (fileExtPos >= 0)
                    {
                        fileName = fileName.Substring(0, fileExtPos);
                    }

                    TempData["Success"] = dataProcess.Split_Source_Target_fromCSV(path, fileName, tabletext, delimiter);
                    System.IO.File.Delete(path);

                }
                catch (Exception ex)
                {
                    TempData["Fail"] = ex.Message;
                }
            }

            return View();
        }

        //This method will validate the starcite sheet as per test cases
        public ActionResult validateStarCiteSheet()
        {


            return View();
        }

        [HttpPost]
        public ActionResult validateStarCiteSheet(HttpPostedFileBase FileUpload)
        {

            return View();
        }


        public string ValidateSheet()
        {
            string Result = String.Empty;
            try
            {
                List<string> FailedArr = new List<string>();

                DataTable dt = new DataTable();

                StarciteDb StarciteDb = new StarciteDb();
                var StarCiteConfig = StarciteDb.getStarCiteConfig();
                dt = StarciteDb.getStarCiteTargetData("STARCITE_20180313_Global_target");

                if (dt.Columns.Count == StarCiteConfig.Count) {
                    if (dt.Rows.Count > 0) {

                        for (int j = 0; j < dt.Rows.Count; j++) {
                            if (StarCiteConfig.Count > 0)
                            {
                                for (int i = 0; i < StarCiteConfig.Count; i++)
                                {
                                    string fieldName = StarCiteConfig[i].fieldName;
                                    int isRequired = Convert.ToInt32(StarCiteConfig[i].isRequired);
                                    string actualValue = StarCiteConfig[i].actualValue;

                                    string value = dt.Rows[j][fieldName].ToString();
                                    if (isRequired == 1) {
                                        if (value == "" || value == null)
                                        {
                                            int num = j + 1;
                                            string failedText = "Row Number " + num + " " + fieldName + " failed, Value Required.";
                                            FailedArr.Add(failedText);
                                            break;
                                        }
                                    }
                                    if (actualValue != "")
                                    {
                                        if (value != actualValue)
                                        {
                                            int num = j + 1;
                                            string failedText = "Row Number " + num + " " + fieldName + " failed, Value incorrect. Given-" + value + ", Correct is " + actualValue;
                                            FailedArr.Add(failedText);
                                            break;
                                        }
                                    }

                                }
                            }
                        }
                        Console.Write(FailedArr);
                        if (FailedArr.Count == 0)
                        {
                            //Everything is fine.
                            Result = "Success.";
                        }
                    }
                }
                else
                {
                    Result = "Configuration columns and Target columns are not matched.";
                }

            }catch(Exception ex)
            {

            }
            return Result;
        }

    }
}