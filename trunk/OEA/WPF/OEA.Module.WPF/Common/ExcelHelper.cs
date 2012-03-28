using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;

namespace OEA.Module.WPF
{
    public static class ExcelHelper
    {
        public static ITableSaver CreateSaver()
        {
            return new DataSetExcelSaver();
        }

        private class DataSetExcelSaver : ISetSaver
        {
            #region ISetSaver Members

            public void SaveToFile(System.Data.DataTable table, string fileName)
            {
                CheckFileName(fileName);

                if (table.DataSet != null)
                {
                    table.DataSet.Tables.Remove(table);
                }
                DataSet ds = new DataSet();
                ds.Tables.Add(table);

                SaveToFile(ds, fileName);
            }

            public void SaveToFile(DataSet dataSet, string fileName)
            {
                CheckFileName(fileName);

                //XlFileFormat format = XlFileFormat.xlCSVWindows;//导出为CSV，出错，不支持
                XlFileFormat format = XlFileFormat.xlWorkbookNormal;//导出为EXCEL
                InnerSave(dataSet, format, fileName, false);
            }

            #endregion

            private static void CheckFileName(string fileName)
            {
                string ext = Path.GetExtension(fileName).ToLower();
                if (ext != ".xlsx" && ext != ".xls")
                {
                    throw new NotSupportedException();
                }
            }
            private static void InnerSave(DataSet dataSet, XlFileFormat format, string outputPath, bool deleteOldFile)
            {
                object missing = Type.Missing;
                ApplicationClass excelApp = null;
                Workbook excelWorkbook = null;

                try
                {
                    if (deleteOldFile && File.Exists(outputPath))
                    {
                        File.Delete(outputPath);
                    }

                    // Create the Excel Application object
                    excelApp = new ApplicationClass();

                    // Create a new Excel Workbook
                    excelWorkbook = excelApp.Workbooks.Add(missing);

                    int sheetIndex = 0;

                    // Copy each DataTable
                    foreach (System.Data.DataTable dt in dataSet.Tables)
                    {
                        // Copy the DataTable to an object array
                        int rowCount = dt.Rows.Count;
                        int colsCount = dt.Columns.Count;
                        string[,] rawData = new string[rowCount + 1, colsCount];

                        // Copy the column names to the first row of the object array
                        for (int col = 0; col < dt.Columns.Count; col++)
                        {
                            rawData[0, col] = dt.Columns[col].ColumnName;
                        }

                        // Copy the values to the object array
                        for (int row = 0; row < rowCount; row++)
                        {
                            var dataRow = dt.Rows[row];
                            for (int col = 0; col < colsCount; col++)
                            {
                                rawData[row + 1, col] = dataRow[col].ToString();
                            }
                        }

                        // Calculate the final column letter
                        string finalColLetter = string.Empty;
                        string colCharset = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                        int colCharsetLen = colCharset.Length;

                        if (dt.Columns.Count > colCharsetLen)
                        {
                            finalColLetter = colCharset.Substring(
                              (dt.Columns.Count - 1) / colCharsetLen - 1, 1);
                        }

                        finalColLetter += colCharset.Substring(
                            (dt.Columns.Count - 1) % colCharsetLen, 1);

                        // Create a new Sheet
                        Worksheet excelSheet = (Worksheet)excelWorkbook.Sheets.Add(
                          excelWorkbook.Sheets.get_Item(++sheetIndex),
                          Type.Missing, 1, XlSheetType.xlWorksheet);

                        excelSheet.Name = dt.TableName;

                        // Fast data export to Excel
                        string excelRange = string.Format("A1:{0}{1}",
                          finalColLetter, dt.Rows.Count + 1);

                        var range = excelSheet.get_Range(excelRange, Type.Missing);
                        range.Value2 = rawData;

                        // Mark the first row as BOLD
                        range = ((Range)excelSheet.Rows[1, Type.Missing]);
                        range.Font.Bold = true;
                    }
                    //excelApp.Application.AlertBeforeOverwriting = false;
                    excelApp.Application.DisplayAlerts = false;
                    // Save and Close the Workbook
                    excelWorkbook.SaveAs(
                        outputPath,
                        format,
                        missing, missing, missing, missing,
                        XlSaveAsAccessMode.xlExclusive,
                        missing, missing, missing, missing, missing
                        );
                }
                finally
                {
                    #region Dispose

                    if (excelWorkbook != null)
                    {
                        excelWorkbook.Close(true, missing, missing);
                        excelWorkbook = null;
                    }

                    // Release the Application object
                    if (excelApp != null)
                    {
                        excelApp.Quit();

                        //释放进程
                        IntPtr t = new IntPtr(excelApp.Hwnd);
                        int k = 0;
                        GetWindowThreadProcessId(t, out k);
                        System.Diagnostics.Process.GetProcessById(k).Kill();

                        excelApp = null;
                    }

                    #endregion
                }
            }

            [DllImport("User32.dll", CharSet = CharSet.Auto)]
            public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int ID);
        }
    }
}
