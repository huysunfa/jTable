using jTable.AdoNet.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace jTable.Controllers
{
    public class DefaultController : Controller
    {

        // GET: Default
        public ActionResult Index()
        {
            const string listUrl = "/default/List?tablename={0}";
            const string addUrl = "/default/add?tablename={0}";
            const string updateUrl = "/default/update?tablename={0}";
            const string deleteUrl = "/default/delete?tablename={0}";
            string tableName = "mstr_Country";

            ViewBag.primaryKeyName = MasterTableDAL.GetPrimaryKey(tableName);
            ViewBag.FieldData = MasterTableDAL.GetJsonFields(tableName);
            ViewBag.ListUrl = String.Format(listUrl, tableName);
            ViewBag.AddUrl = String.Format(addUrl, tableName);
            ViewBag.UpdateUrl = String.Format(updateUrl, tableName);
            ViewBag.DeleteUrl = String.Format(deleteUrl, tableName);
            //
            return View();
        }
        public string ToJson(DataTable dt)
        {
            List<Dictionary<string, object>> lst = new List<Dictionary<string, object>>();
            Dictionary<string, object> item;
            foreach (DataRow row in dt.Rows)
            {
                item = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    item.Add(col.ColumnName, (Convert.IsDBNull(row[col]) ? null : row[col]));
                }
                lst.Add(item);
            }
            return JsonConvert.SerializeObject(lst);
        }

        public ActionResult List(string tableName)
        {

            var records = MasterTableDAL.GetListOfRecords(tableName);
            var json = ToJson(records);
            return JsonMax(json);
        }

        public JsonResult JsonMax(object data)
        {
            JsonResult jsonResult = Json(data, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = 2147483647;
            return jsonResult;
        }
        [HttpPost]
        public ActionResult Add()
        {
            string tableName = "";
            if (!String.IsNullOrEmpty(Request.QueryString["tableName"]))
            {
                tableName = Request.QueryString["tableName"];
            }
            //we will be receiving all values in form variables.
            Dictionary<string, string> ColumnList = MasterTableDAL.GetColumns(tableName);
            List<ColumnFieldValue> columnFieldValueList = new List<ColumnFieldValue>();

            foreach (var item in ColumnList)
            {
                if (item.Key == ColumnList.FirstOrDefault().Key)
                {
                    continue;
                }
                foreach (string key in Request.Form.AllKeys)
                {
                    if (key.Replace("values", "").Replace("[", "").Replace("]", "").ToLower() == item.Key.ToLower())
                    {
                        var columnFieldValue = new ColumnFieldValue();
                        columnFieldValue.ColumnName = item.Key;
                        columnFieldValue.ColumnValue = Request.Form[key];
                        columnFieldValue.ColumnType = item.Value;
                        columnFieldValueList.Add(columnFieldValue);
                    }
                }
            }

            //create column string
            string strColumns = String.Join(",", columnFieldValueList.Select(x => x.ColumnName));
            string strValues = "'" + String.Join("',N'", columnFieldValueList.Select(V => V.ColumnValue)) + "'";

            string jsonResult = MasterTableDAL.AddRecord(tableName, strColumns, strValues);
            return JsonMax("OK");

        }
        [HttpPost]
        public ActionResult Update(string tableName)
        {
           
            Dictionary<string, string> ColumnList = MasterTableDAL.GetColumns(tableName);
            List<ColumnFieldValue> columnFieldValueList = new List<ColumnFieldValue>();
            var primaryKeyName = MasterTableDAL.GetPrimaryKey(tableName);

            foreach (var item in ColumnList)
            {
                foreach (string key in Request.Form.AllKeys)
                {
                    var newkey = key.Replace("values", "").Replace("[", "").Replace("]", "").ToLower();
                    if (newkey == item.Key.ToLower() && newkey != primaryKeyName.ToLower())
                    {
                        var columnFieldValue = new ColumnFieldValue();
                        columnFieldValue.ColumnName = item.Key;
                        columnFieldValue.ColumnValue = Request.Form[key];
                        columnFieldValue.ColumnType = item.Value;
                        columnFieldValueList.Add(columnFieldValue);
                    }
                }
            }

            //create column string
            string strColumnUpdate = "";

            foreach (var column in columnFieldValueList)
            {
                switch (column.ColumnType)
                {
                    case "System.Int64":
                    case "System.Int32":
                    case "System.Boolean":
                        strColumnUpdate = strColumnUpdate + column.ColumnName + "=" + column.ColumnValue + ",";
                        break;
                    case "System.String":
                    case "System.DateTime":
                        strColumnUpdate = strColumnUpdate + column.ColumnName + "='" + column.ColumnValue + "',";
                        break;
                }
            }
            strColumnUpdate = strColumnUpdate.TrimEnd(',');

            //audit fields
            strColumnUpdate = strColumnUpdate + " WHERE " + primaryKeyName + " = " + Request.Form["key"];
            MasterTableDAL.UpdateRecord(tableName, strColumnUpdate);
            return Content("{\"Result\":\"OK\"}", "application/json");
        }

        [HttpPost]
        public ActionResult Delete(string tableName)
        {
            string primaryKey = MasterTableDAL.GetPrimaryKey(tableName);

            MasterTableDAL.DeleteRecord(tableName, primaryKey, Request.Form["key"]);

            return Content("{\"Result\":\"OK\"}", "application/json");

        }

    }

    public class ColumnFieldValue
    {
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
        public string ColumnValue { get; set; }
    }

}