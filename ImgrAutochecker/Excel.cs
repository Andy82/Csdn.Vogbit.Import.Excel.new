using System;
using System.Collections.Generic;
using ImageRight.Client.Workflow;
using MsExcel = Microsoft.Office.Interop.Excel;

namespace ImgrAutochecker
{
    class Excel
    {
        private static MsExcel.Application ObjExcel;
        private static MsExcel.Workbook ObjWorkBook;
        private static MsExcel.Worksheet ObjWorkSheet;

        public static void SaveToExcel(List<WorkflowStepData> steps)
        {
            try
            {
                //Приложение самого Excel
                ObjExcel = new MsExcel.Application();
                ObjExcel.Visible = false;
                //Книга.
                ObjWorkBook = ObjExcel.Workbooks.Add();
                //Таблица.
                ObjWorkSheet = ObjWorkBook.Sheets[1] as MsExcel.Worksheet;

                ObjWorkSheet.Cells[1, 1] = "Workflow";
                ObjWorkSheet.Cells[1, 2] = "From Step";
                ObjWorkSheet.Cells[1, 3] = "To Step";
                ObjWorkSheet.Cells[1, 4] = "Has Error";
                ObjWorkSheet.Cells[1, 5] = "Error Message";


                int row = 2;

                foreach (WorkflowStepData step in steps)
                {
                    ObjWorkSheet.Cells[row, 1] = step.Workflow;
                    ObjWorkSheet.Cells[row, 2] = step.FromStep;
                    ObjWorkSheet.Cells[row, 3] = step.ToStep;
                    ObjWorkSheet.Cells[row, 4] = step.Error;
                    ObjWorkSheet.Cells[row, 5] = step.ErrorMessage;
                    int attrIndex = 6;
                    foreach (KeyValuePair<string, string> attributeVal in step.Attribute)
                    {
                        if (row == 2)
                        {
                            ObjWorkSheet.Cells[1, attrIndex] = attributeVal.Key;
                        }
                        ObjWorkSheet.Cells[row, attrIndex] = attributeVal.Value;
                        attrIndex++;
                    }
                    row++;                    
                }
                ObjExcel.Visible = true;
            }
            catch (System.Exception ex) { Console.WriteLine("Ошибка: " + ex.Message, "Ошибка при считывании excel файла"); }
        }
    }
}
