using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using PhoiLo.Models;

namespace PhoiLo.Helpers
{
    public static class DataGridHelper
    {
        private static DispatcherTimer? _saveTimer;

        // [Suy luận] Hàm này giúp lưu cấu hình có độ trễ (Debounce) để tránh ghi file liên tục gây lag
        public static void SaveConfig() => RequestSave();

        public static void EnableWidthAutoSave(DataGrid grid, string tableName)
        {
            bool isRestoring = true;

            Action applyWidths = () => {
                isRestoring = true;
                if (App.Config.ColumnWidths != null) {
                    for (int i = 0; i < grid.Columns.Count; i++) {
                        string key = $"{tableName}_Col{i}";
                        if (App.Config.ColumnWidths.TryGetValue(key, out double width) && width > 5) {
                            grid.Columns[i].Width = new DataGridLength(width, DataGridLengthUnitType.Pixel);
                        }
                    }
                }
                grid.Dispatcher.BeginInvoke(new Action(() => isRestoring = false), DispatcherPriority.Render);
            };

            grid.Loaded += (s, e) => applyWidths();

            grid.LayoutUpdated += (s, e) => {
                if (isRestoring || !grid.IsLoaded || !grid.IsVisible) return;

                if (App.Config.ColumnWidths == null) App.Config.ColumnWidths = new Dictionary<string, double>();
                bool changed = false;
                for (int i = 0; i < grid.Columns.Count; i++) {
                    string key = $"{tableName}_Col{i}";
                    double currentActual = grid.Columns[i].ActualWidth;
                    App.Config.ColumnWidths.TryGetValue(key, out double savedValue);
                    
                    if (Math.Abs(currentActual - savedValue) > 2.0 && currentActual > 5) {
                        App.Config.ColumnWidths[key] = currentActual;
                        changed = true;
                    }
                }
                if (changed) RequestSave();
            };
        }

        private static void RequestSave() {
            if (_saveTimer == null) {
                _saveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                _saveTimer.Tick += (s, e) => {
                    _saveTimer.Stop();
                    App.Config.SaveToFile();
                };
            }
            _saveTimer.Stop();
            _saveTimer.Start();
        }

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
                if (startRowIndex < 0 || startColIndex < 0) return;

                string[] lines = clipboardText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < lines.Length && (startRowIndex + i) < grid.Items.Count; i++) {
                    string[] cells = lines[i].Split('\t');
                    object item = grid.Items[startRowIndex + i];
                    for (int j = 0; j < cells.Length; j++) {
                        int targetColIndex = startColIndex + j;
                        if (targetColIndex < grid.Columns.Count) SetCellValue(item, grid.Columns[targetColIndex], cells[j]);
                    }
                }
                onDataChanged?.Invoke();
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                if (grid.SelectedCells.Count > 0) {
                    foreach (var cell in grid.SelectedCells) SetCellValue(cell.Item, cell.Column, "");
                    onDataChanged?.Invoke();
                    e.Handled = true;
                }
            }
        }

        private static void SetCellValue(object item, DataGridColumn column, string value)
        {
            if (item == null || column == null) return;
            if (item is DataRowView rowView) {
                string colName = "";
                if (column is DataGridTextColumn textCol && textCol.Binding is System.Windows.Data.Binding binding)
                    colName = binding.Path.Path.Trim('[', ']');
                if (!string.IsNullOrEmpty(colName) && rowView.Row.Table.Columns.Contains(colName))
                    rowView.Row[colName] = value;
            } else {
                if (column is DataGridTextColumn textCol && textCol.Binding is System.Windows.Data.Binding binding) {
                    PropertyInfo? prop = item.GetType().GetProperty(binding.Path.Path);
                    if (prop != null && prop.CanWrite) {
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