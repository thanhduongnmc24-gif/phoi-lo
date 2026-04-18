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

        // [Suy luận] Hàm thông minh lưu độ rộng đã được bọc giáp chống ghi đè
        public static void EnableWidthAutoSave(DataGrid grid, string tableName)
        {
            // Cờ khóa: true = đang nạp dữ liệu, cấm lưu bậy bạ.
            bool isRestoring = true;

            Action applyWidths = () => {
                isRestoring = true; // Bật khóa an toàn
                if (App.Config.ColumnWidths != null) {
                    for (int i = 0; i < grid.Columns.Count; i++) {
                        string key = $"{tableName}_Col{i}";
                        if (App.Config.ColumnWidths.TryGetValue(key, out double width) && width > 5) {
                            // Ép cứng WPF dùng Pixel, không cho dùng Auto để khỏi tự nhảy
                            grid.Columns[i].Width = new DataGridLength(width, DataGridLengthUnitType.Pixel);
                        }
                    }
                }
                // Chờ WPF vẽ xong toàn bộ giao diện rồi mới mở khóa
                grid.Dispatcher.BeginInvoke(new Action(() => isRestoring = false), DispatcherPriority.Render);
            };

            // Khi bảng vừa được tải lên màn hình
            grid.Loaded += (s, e) => applyWidths();

            // Bắt sự kiện khi bảng có bất kỳ thay đổi đồ họa nào (kéo cột)
            grid.LayoutUpdated += (s, e) => {
                // Kẻ thù đây rồi: Nếu đang khôi phục, hoặc bảng đang bị ẩn đi, thì cấm lưu để tránh ghi đè sai
                if (isRestoring || !grid.IsLoaded || !grid.IsVisible) return;

                if (App.Config.ColumnWidths == null) App.Config.ColumnWidths = new Dictionary<string, double>();
                bool changed = false;
                for (int i = 0; i < grid.Columns.Count; i++) {
                    string key = $"{tableName}_Col{i}";
                    double currentActual = grid.Columns[i].ActualWidth;
                    
                    App.Config.ColumnWidths.TryGetValue(key, out double savedValue);
                    
                    // Chỉ lưu khi người dùng thực sự kéo cột (lệch > 2 pixel)
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