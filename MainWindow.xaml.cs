using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CSV_Merger_Tool
{
    public partial class MainWindow : Window
    {
        private string newCsvPath;
        private string oldCsvPath;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void UpdateMergeButton()
        {
            BtnClearNew.IsEnabled = !string.IsNullOrEmpty(newCsvPath);
            BtnClearOld.IsEnabled = !string.IsNullOrEmpty(oldCsvPath);
            BtnMerge.IsEnabled = !string.IsNullOrEmpty(newCsvPath) && !string.IsNullOrEmpty(oldCsvPath);
        }

        private void HandleDrag(object sender, DragEventArgs e)
        {
            if (!(sender is Border border)) return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files == null || files.Length == 0) return;

                string file = files[0];

                if (Path.GetExtension(file)?.ToLower() == ".csv")
                {
                    e.Effects = DragDropEffects.Copy;
                    border.BorderBrush = Brushes.DodgerBlue;
                    border.Background = Brushes.AliceBlue;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                    border.BorderBrush = Brushes.Gray;
                    border.Background = Brushes.WhiteSmoke;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private void NewCsv_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;

            string file = files[0];

            if (Path.GetExtension(file)?.ToLower() != ".csv")
            {
                ResetBorderStyle(borderNew, newCsvPath);
                return;
            }

            if (sender is Border border)
            {
                border.BorderBrush = Brushes.DodgerBlue;
                border.Background = Brushes.AliceBlue;
            }

            newCsvPath = file;
            TxtNewCsv.Text = $"Новий: {Path.GetFileName(newCsvPath)}";
            UpdateMergeButton();
        }

        private void OldCsv_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files == null || files.Length == 0) return;

            string file = files[0];

            if (Path.GetExtension(file)?.ToLower() != ".csv")
            {
                ResetBorderStyle(borderOld, oldCsvPath);
                return;
            }

            if (sender is Border border)
            {
                border.BorderBrush = Brushes.DodgerBlue;
                border.Background = Brushes.AliceBlue;
            }

            oldCsvPath = file;
            TxtOldCsv.Text = $"Старий: {Path.GetFileName(oldCsvPath)}";
            UpdateMergeButton();
        }

        private void Border_DragEnter(object sender, DragEventArgs e) => HandleDrag(sender, e);
        private void Border_DragOver(object sender, DragEventArgs e) => HandleDrag(sender, e);

        private void Border_DragLeave(object sender, DragEventArgs e)
        {
            if (sender is Border border)
            {
                if (border == borderNew)
                    ResetBorderStyle(borderNew, newCsvPath);
                else if (border == borderOld)
                    ResetBorderStyle(borderOld, oldCsvPath);
            }
        }

        private void ResetBorderStyle(Border border, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                border.BorderBrush = Brushes.Gray;
                border.Background = Brushes.WhiteSmoke;
            }
            else
            {
                border.BorderBrush = Brushes.DodgerBlue;
                border.Background = Brushes.AliceBlue;
            }
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "CSV Files (*.csv)|*.csv" };
            if (dlg.ShowDialog() == true)
            {
                newCsvPath = dlg.FileName;
                TxtNewCsv.Text = $"Новий: {Path.GetFileName(newCsvPath)}";
                borderNew.BorderBrush = Brushes.DodgerBlue;
                borderNew.Background = Brushes.AliceBlue;
                UpdateMergeButton();
            }
        }

        private void BtnOld_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "CSV Files (*.csv)|*.csv" };
            if (dlg.ShowDialog() == true)
            {
                oldCsvPath = dlg.FileName;
                TxtOldCsv.Text = $"Старий: {Path.GetFileName(oldCsvPath)}";
                borderOld.BorderBrush = Brushes.DodgerBlue;
                borderOld.Background = Brushes.AliceBlue;
                UpdateMergeButton();
            }
        }

        private void BtnClearNew_Click(object sender, RoutedEventArgs e)
        {
            newCsvPath = null;
            TxtNewCsv.Text = "Перетягніть новий CSV";
            borderNew.BorderBrush = Brushes.Gray;
            borderNew.Background = Brushes.WhiteSmoke;
            UpdateMergeButton();
        }

        private void BtnClearOld_Click(object sender, RoutedEventArgs e)
        {
            oldCsvPath = null;
            TxtOldCsv.Text = "Перетягніть старий CSV";
            borderOld.BorderBrush = Brushes.Gray;
            borderOld.Background = Brushes.WhiteSmoke;
            UpdateMergeButton();
        }

        private void BtnMerge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists(newCsvPath))
                {
                    MessageBox.Show("Новий CSV файл не знайдено!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!File.Exists(oldCsvPath))
                {
                    MessageBox.Show("Старий CSV файл не знайдено!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Encoding = Encoding.UTF8,
                    PrepareHeaderForMatch = args => args.Header.Trim(),
                    ShouldQuote = _ => true,
                    MissingFieldFound = null,
                    BadDataFound = null
                };

                var oldData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                using (var reader = new StreamReader(oldCsvPath, Encoding.UTF8))
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Read();
                    csv.ReadHeader();
                    var headerRecord = csv.HeaderRecord;

                    if (!headerRecord.Contains("key", StringComparer.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Старий CSV не містить колонку 'key'!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    while (csv.Read())
                    {
                        string key = csv.GetField("key")?.Trim();
                        string source = csv.GetField("source");

                        if (!string.IsNullOrWhiteSpace(key))
                        {
                            oldData[key] = source ?? string.Empty;
                        }
                    }
                }

                SaveFileDialog saveDlg = new SaveFileDialog
                {
                    Filter = "CSV Files (*.csv)|*.csv",
                    FileName = "merged.csv"
                };

                if (saveDlg.ShowDialog() != true) return;

                using (var reader = new StreamReader(newCsvPath, Encoding.UTF8))
                using (var csvReader = new CsvReader(reader, config))
                using (var writer = new StreamWriter(saveDlg.FileName, false, new UTF8Encoding(false)))
                using (var csvWriter = new CsvWriter(writer, config))
                {
                    csvReader.Read();
                    csvReader.ReadHeader();
                    var headers = csvReader.HeaderRecord;

                    if (!headers.Contains("key", StringComparer.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Новий CSV не містить колонку 'key'!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    foreach (var h in headers)
                        csvWriter.WriteField(h);
                    csvWriter.NextRecord();

                    int updatedCount = 0;

                    while (csvReader.Read())
                    {
                        var record = new Dictionary<string, string>();

                        foreach (var header in headers)
                        {
                            record[header] = csvReader.GetField(header) ?? string.Empty;
                        }

                        string key = record.ContainsKey("key") ? record["key"]?.Trim() : null;
                        string sourceNew = record.ContainsKey("source") ? record["source"] : null;

                        if (!string.IsNullOrWhiteSpace(key) && oldData.ContainsKey(key))
                        {
                            string sourceOld = oldData[key];
                            if (sourceNew != sourceOld && record.ContainsKey("Translation"))
                            {
                                record["Translation"] = sourceOld;
                                updatedCount++;
                            }
                        }

                        foreach (var h in headers)
                        {
                            csvWriter.WriteField(record.ContainsKey(h) ? record[h] : string.Empty);
                        }
                        csvWriter.NextRecord();
                    }

                    MessageBox.Show(
                        $"Перенесено рядків: {updatedCount}\nФайл збережено.",
                        "Успіх",
                        MessageBoxButton.OK);
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Помилка читання/запису файлу:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (CsvHelperException ex)
            {
                MessageBox.Show($"Помилка обробки CSV:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Несподівана помилка:\n{ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}