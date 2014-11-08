using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.EnterpriseServices;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Web;

namespace PCSMvc.Models
{
    public enum UserRoleEnum : int
    {
        PCSManager = 1,
        DistrictSysAdmin = 2,
        SchoolUser = 3,
        Specialist = 4
    }

    public enum PCSManagerIconsMenu
    {
        Specialist,
        Organizations,
        Students,
        Reports,
        Settings
    }

    public enum SpecialistAreas : int
    {
        Students = 1,
        Office = 2,
        Reports = 3
    }

    public enum TabItemEnum : int
    {
        Current = 1,
        Team_Consulting = 2,
        History = 3
    }

    public enum ReportType
    {
        pdf,
        xls
    }

    #region JSON

    public class JsonTableResult<T>
    {
        public IEnumerable<T> Data { get; set; }
        public long Total { get; set; }
        public JsonExecuteResult Result { get; set; }

        public JsonTableResult()
        {
            Result = new JsonExecuteResult();
        }
    }

    public enum JsonExecuteResultTypes : byte { SUCCESS, SUCCESS_INFORMATION, ERROR } ;

    public class JsonExecuteResult
    {
        public JsonExecuteResultTypes Type { get; set; }
        public string Message { get; set; }
        public object Details { get; set; }
        public string MessageTitle { get; set; }

        public string Status
        {
            get { return Type.ToString(); }
        }

        public JsonExecuteResult()
        {
            Type = JsonExecuteResultTypes.SUCCESS;
        }

        public JsonExecuteResult(JsonExecuteResultTypes type, string message, object details = null, string messagetitle = null)
        {
            Type = type;
            Message = message;
            Details = details;
            MessageTitle = messagetitle;
        }

        public JsonExecuteResult(JsonExecuteResultTypes type)
            : this(type, String.Empty)
        {
        }

        public JsonExecuteResult(JsonExecuteResultTypes type, object details)
            : this(type, String.Empty, details)
        {
        }

        public JsonExecuteResult(JsonExecuteResultTypes type, string message, string messagetitle)
            : this(type, message, null, messagetitle)
        {
        }
    }

    #endregion JSON

    public struct ExcelExportSetting
    {
        public string HeadName;
        public string ColName;
        public bool WrapText;
        public int ColWidth;
    }

    public static class ReportHelper
    {
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    if (Props[i].PropertyType == typeof(DateTime))
                    {
                        values[i] = ((DateTime)Props[i].GetValue(item, null)).Date.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        values[i] = Props[i].GetValue(item, null);
                    }
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }

        public static void GenerateExcelForRecordList(DataTable dt, ExcelExportSetting[] excelExportSettings, string fileName, string fileTitle)
        {
            string fontName = "Calibri";
            int fontSize = 10;
            int unitWd = 1440;
            //InitializeCEUmainHeader();
            //Dimension a Workbook and add a sheet to its Sheets collection
            int sheetNo = 0;
            DataDynamics.SpreadBuilder.Workbook sb = new DataDynamics.SpreadBuilder.Workbook();
            sb.Sheets.AddNew();

            //Set up properties and values for columns, rows and cells as desired
            for (int i = 0; i < excelExportSettings.Length && excelExportSettings[i].ColName != null; i++)
            {
                //if( ds.Tables[0].Columns[i].
                short col = (short)(i);
                sb.Sheets[sheetNo].Columns(col).Width = unitWd * excelExportSettings[i].ColWidth / 100;
            }

            // print title
            int currentRow = 0;
            if (string.IsNullOrWhiteSpace(fileTitle))
            {
                sb.Sheets[sheetNo].Cell(currentRow, 0).Alignment = DataDynamics.SpreadBuilder.Style.HorzAlignments.Center;
                sb.Sheets[sheetNo].Cell(currentRow, 0).VertAlignment = DataDynamics.SpreadBuilder.Style.VertAlignments.Center;
                sb.Sheets[sheetNo].Cell(currentRow, 0).SetValue(fileTitle);
                sb.Sheets[sheetNo].Cell(currentRow, 0).FontSize = 18;
                sb.Sheets[sheetNo].Cell(currentRow, 0).FontName = fontName;
                sb.Sheets[sheetNo].Cell(currentRow, 0).Merge(0, (ushort)excelExportSettings.Length);
                currentRow = 1;
            }

            //report header
            currentRow += PrintHeaderRow(sb, currentRow, fontSize, fontName, excelExportSettings, sheetNo);

            foreach (DataRow row in dt.Rows)
            {
                PrintOneRowInExcel(sb, currentRow, fontSize, fontName, row, excelExportSettings, sheetNo, false);
                currentRow++;
            }
            //Save the Workbook to an Excel file
            sb.Save(fileName);
            sb.Clear();
        }

        private static void PrintOneRowInExcel(DataDynamics.SpreadBuilder.Workbook sb, int currentRow, double cFont, String fontName, DataRow row, ExcelExportSetting[] excelExportSettings, int sheetNo, bool needBold)
        {
            Color clr = Color.Black;
            for (int col = 0; col < excelExportSettings.Length && excelExportSettings[col].ColName != null; col++)
            {
                sb.Sheets[sheetNo].Cell(currentRow, col).Alignment = DataDynamics.SpreadBuilder.Style.HorzAlignments.Left;
                sb.Sheets[sheetNo].Cell(currentRow, col).VertAlignment = DataDynamics.SpreadBuilder.Style.VertAlignments.Top;

                sb.Sheets[sheetNo].Cell(currentRow, col).ForeColor = clr;
                sb.Sheets[sheetNo].Cell(currentRow, col).WrapText = excelExportSettings[col].WrapText;
                sb.Sheets[sheetNo].Cell(currentRow, col).SetValue(row[excelExportSettings[col].ColName].ToString());
                sb.Sheets[sheetNo].Cell(currentRow, col).FontSize = cFont;
                sb.Sheets[sheetNo].Cell(currentRow, col).FontName = fontName;

                sb.Sheets[sheetNo].Cell(currentRow, col).FontBold = needBold;
            }
        }

        private static int PrintHeaderRow(DataDynamics.SpreadBuilder.Workbook sb,
            int currentRow, int fontSize, String fontName, ExcelExportSetting[] excelExportSettings, int sheetNo)
        {
            for (int i = 0; i < excelExportSettings.Length && excelExportSettings[i].ColName != null; i++)
            {
                sb.Sheets[sheetNo].Cell(currentRow, i).FillColor = ColorTranslator.FromHtml("#A5A5A5");
                sb.Sheets[sheetNo].Cell(currentRow, i).FontSize = fontSize;
                sb.Sheets[sheetNo].Cell(currentRow, i).FontName = fontName;
                sb.Sheets[sheetNo].Cell(currentRow, i).SetValue(excelExportSettings[i].HeadName);
            }
            return 1;
        }
    }
}