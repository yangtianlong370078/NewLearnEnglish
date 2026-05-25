using System.Data;
using System.Data.OleDb;

namespace LearnEnglish.Models
{
    public class ExcelHandle
    {
        /// <summary>
        /// 数据读取
        /// </summary>
        /// <returns></returns>
        public static async Task<DataTable> DataRead(string filePath)
        {
            string strConn = "Provider=Microsoft.ACE.OLEDB.12.0; Persist Security Info=False;" + "Data Source=" + filePath + ";" + "Extended Properties='Excel 8.0;HDR=Yes;IMEX=1';";
            OleDbConnection conn = new OleDbConnection(strConn);
           await conn.OpenAsync();
            string strExcel = "";
            OleDbDataAdapter myCommand = null;
            DataSet ds = null;

            string sheetname = "";
            System.Data.DataTable dt = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

            if (dt != null && dt.Rows.Count > 0)
            {
                sheetname = dt.Rows[0]["TABLE_NAME"].ToString();
            }
            else
            {
                throw new Exception("导入文件无任何内容，请确认");
            }

            strExcel = "select * from [" + sheetname + "]";
            myCommand = new OleDbDataAdapter(strExcel, strConn);
            ds = new DataSet();
            myCommand.Fill(ds, "table1");

            return ds.Tables[0];
        }
    }
}
