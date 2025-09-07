using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

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
            Border border = sender as Border;
            if (border == null) return;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                string file = files[0];

                if (Path.GetExtension(file)?.ToLower() == ".csv")
                {
                    e.Effects = DragDropEffects.Copy;
                    border.BorderBrush = System.Windows.Media.Brushes.DodgerBlue;
                    border.Background = System.Windows.Media.Brushes.AliceBlue;
                }
                else
                {
                    e.Effects = DragDropEffects.None;
                    border.BorderBrush = System.Windows.Media.Brushes.Gray;
                    border.Background = System.Windows.Media.Brushes.WhiteSmoke;
                }
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void NewCsv_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string file = files[0];

            if (Path.GetExtension(file)?.ToLower() != ".csv")
            {
                return;
            }

            if (sender is Border border)
            {
                border.BorderBrush = System.Windows.Media.Brushes.DodgerBlue;
                border.Background = System.Windows.Media.Brushes.AliceBlue;
            }

            newCsvPath = file;
            TxtNewCsv.Text = $"Новий: {Path.GetFileName(newCsvPath)}";
            UpdateMergeButton();
        }

        private void OldCsv_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string file = files[0];

            if (Path.GetExtension(file)?.ToLower() != ".csv")
            {
                return;
            }

            if (sender is Border border)
            {
                border.BorderBrush = System.Windows.Media.Brushes.DodgerBlue;
                border.Background = System.Windows.Media.Brushes.AliceBlue;
            }

            oldCsvPath = file;
            TxtOldCsv.Text = $"Старий: {Path.GetFileName(oldCsvPath)}";
            UpdateMergeButton();
        }

        private void Border_DragEnter(object sender, DragEventArgs e) => HandleDrag(sender, e);
        private void Border_DragOver(object sender, DragEventArgs e) => HandleDrag(sender, e);

        private void Border_DragLeave(object sender, DragEventArgs e)
        {
            if (sender is Border b)
            {
                if ((b == borderNew && string.IsNullOrEmpty(newCsvPath)) ||
                    (b == borderOld && string.IsNullOrEmpty(oldCsvPath)))
                {
                    b.BorderBrush = System.Windows.Media.Brushes.Gray;
                    b.Background = System.Windows.Media.Brushes.WhiteSmoke;
                }
            }
        }

        private void BtnNew_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "CSV Files (*.csv)|*.csv" };
            if (dlg.ShowDialog() == true)
            {
                newCsvPath = dlg.FileName;
                TxtNewCsv.Text = $"Новий: {Path.GetFileName(newCsvPath)}";
                borderNew.BorderBrush = System.Windows.Media.Brushes.DodgerBlue;
                borderNew.Background = System.Windows.Media.Brushes.AliceBlue;
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
                borderOld.BorderBrush = System.Windows.Media.Brushes.DodgerBlue;
                borderOld.Background = System.Windows.Media.Brushes.AliceBlue;
                UpdateMergeButton();
            }
        }

        private void BtnClearNew_Click(object sender, RoutedEventArgs e)
        {
            newCsvPath = null;
            TxtNewCsv.Text = "Перетягніть новий CSV";
            borderNew.BorderBrush = System.Windows.Media.Brushes.Gray;
            borderNew.Background = System.Windows.Media.Brushes.WhiteSmoke;
            UpdateMergeButton();
        }

        private void BtnClearOld_Click(object sender, RoutedEventArgs e)
        {
            oldCsvPath = null;
            TxtOldCsv.Text = "Перетягніть старий CSV";
            borderOld.BorderBrush = System.Windows.Media.Brushes.Gray;
            borderOld.Background = System.Windows.Media.Brushes.WhiteSmoke;
            UpdateMergeButton();
        }

        private void BtnMerge_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Encoding = Encoding.UTF8,
                    PrepareHeaderForMatch = args => args.Header.Trim(),
                    ShouldQuote = _ => true
                };

                var oldData = new Dictionary<string, string>();
                using (var reader = new StreamReader(oldCsvPath, Encoding.UTF8))
                using (var csv = new CsvReader(reader, config))
                {
                    csv.Read();
                    csv.ReadHeader();
                    while (csv.Read())
                    {
                        var record = csv.GetRecord<dynamic>() as IDictionary<string, object>;
                        string key = record["key"]?.ToString().Trim();
                        string source = record["source"]?.ToString();
                        if (key != null) oldData[key] = source;
                    }
                }

                SaveFileDialog saveDlg = new SaveFileDialog { Filter = "CSV Files (*.csv)|*.csv" };
                if (saveDlg.ShowDialog() != true) return;

                using (var reader = new StreamReader(newCsvPath, Encoding.UTF8))
                using (var csvReader = new CsvReader(reader, config))
                using (var writer = new StreamWriter(saveDlg.FileName, false, new UTF8Encoding(false)))
                using (var csvWriter = new CsvWriter(writer, config))
                {
                    csvReader.Read();
                    csvReader.ReadHeader();
                    var headers = csvReader.Context.Reader.HeaderRecord;

                    foreach (var h in headers)
                        csvWriter.WriteField(h);
                    csvWriter.NextRecord();

                    while (csvReader.Read())
                    {
                        var record = csvReader.GetRecord<dynamic>() as IDictionary<string, object>;
                        string key = record["key"]?.ToString().Trim();
                        string sourceNew = record["source"]?.ToString();

                        if (key != null && oldData.ContainsKey(key))
                        {
                            string sourceOld = oldData[key];
                            if (sourceNew != sourceOld)
                                record["Translation"] = sourceOld;
                        }

                        foreach (var h in headers)
                            csvWriter.WriteField(record.ContainsKey(h) ? record[h] : "");
                        csvWriter.NextRecord();
                    }
                }

                MessageBox.Show("Переклади зі старого CSV успішно перенесені у новий файл!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
