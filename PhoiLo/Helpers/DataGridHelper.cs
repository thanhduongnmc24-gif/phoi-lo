using System;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PhoiLo.Helpers
{
    public static class DataGridHelper
    {
        // Thêm dấu ? vào Action? để cho phép null
        public static void HandleExcelActions(DataGrid grid, KeyEventArgs e, Action? onDataChanged = null)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                string clipboardText = Clipboard.GetText();
                if (string.IsNullOrEmpty(clipboardText)) return;
                if (grid.SelectedCells.Count == 0) return;

                var startCell = grid.SelectedCells[0];
                int startRowIndex = grid.Items.IndexOf(startCell.Item);
                int startColIndex = startCell.Column.DisplayIndex;

                string[] lines = clipboardText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < lines.Length && (startRowIndex + i) < grid.Items.Count; i++)
                {
                    string[] cells = lines[i].Split('\t');
                    object item = grid.Items[startRowIndex + i];

                    for (int j = 0; j < cells.Length; j++)
                    {
                        int targetColIndex = startColIndex + j;
                        if (targetColIndex < grid.Columns.Count)
                        {
                            SetCellValue(item, grid.Columns[targetColIndex], cells[j]);
                        }
                    }
                }
                onDataChanged?.Invoke();
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                if (grid.SelectedCells.Count > 0)
                {
                    foreach (var cell in grid.SelectedCells)
                    {
                        SetCellValue(cell.Item, cell.Column, "");
                    }
                    onDataChanged?.Invoke();
                    e.Handled = true;
                }
            }
        }

        private static void SetCellValue(object item, DataGridColumn column, string value)
        {
            if (item == null || column == null) return;

            if (item is DataRowView rowView)
            {
                string colName = "";
                if (column is DataGridTextColumn textCol && textCol.Binding is System.Windows.Data.Binding binding)
                {
                    colName = binding.Path.Path.Trim('[', ']');
                }

                if (!string.IsNullOrEmpty(colName) && rowView.Row.Table.Columns.Contains(colName))
                {
                    rowView.Row[colName] = value;
                }
            }
            else
            {
                if (column is DataGridTextColumn textCol && textCol.Binding is System.Windows.Data.Binding binding)
                {
                    // Thêm dấu ? vào PropertyInfo? và object?
                    PropertyInfo? prop = item.GetType().GetProperty(binding.Path.Path);
                    if (prop != null && prop.CanWrite)
                    {
                        try {
                            object? convertedValue = Convert.ChangeType(value, prop.PropertyType);
                            prop.SetValue(item, convertedValue);
                        } catch { prop.SetValue(item, value); }
                    }
                }
            }
        }
    }
}