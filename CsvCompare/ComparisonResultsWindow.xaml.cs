﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using CsvCompare.Library;
using Microsoft.Win32;

namespace CsvCompare
{
    /// <summary>
    /// Interaction logic for ComparisonWindow.xaml
    /// </summary>
    public partial class ComparisonResultsWindow : Window
    {
        public ComparisonResultsWindow(SettingsWindow settingsWindow)
        {
            try
            {
                InitializeComponent();

                ClearErrors();

                SettingsWindow = settingsWindow;
            }
            catch (Exception ex)
            {
                ErrorLabel.Content = $"Error: {ex.Message}{Environment.NewLine} Stack Trace: {ex.StackTrace}";
                ErrorLabel.Visibility = Visibility.Visible;
            }
        }

        public bool IsClosed { get; set; }
        public ComparisonResults Results { get; set; }
        public SettingsWindow SettingsWindow { get; set; }

        private void StartOver_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearErrors();

                SettingsWindow.Show();

                Hide();
            }
            catch (Exception ex)
            {
                ErrorLabel.Content = $"Error: {ex.Message}{Environment.NewLine} Stack Trace: {ex.StackTrace}";
                ErrorLabel.Visibility = Visibility.Visible;
            }
        }

        private async void ExportResults_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearErrors();
                DisableButtons();

                var saveFileName = GetSaveFileName();

                if (saveFileName == null)
                    return; 

                await CsvWriter.WriteComparisonResultsToFileAsync(saveFileName, Results);

                EnableButtons();
            }
            catch (Exception ex)
            {
                EnableButtons();
                ErrorLabel.Content = $"Error: {ex.Message}{Environment.NewLine} Stack Trace: {ex.StackTrace}";
                ErrorLabel.Visibility = Visibility.Visible;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                ClearErrors();

                IsClosed = true;
                if (!SettingsWindow.IsClosed)
                    SettingsWindow.Close();
            }
            catch (Exception ex)
            {
                ErrorLabel.Content = $"Error: {ex.Message}{Environment.NewLine} Stack Trace: {ex.StackTrace}";
                ErrorLabel.Visibility = Visibility.Visible;
            }
        }

        public async Task SetSettings(string file1Name, string file2Name, List<string> identifierColumns, List<string> exclusionColumns, List<string> inclusionColumns)
        {
            try
            {
                ClearErrors();

                var file1Data = await CsvReader.ReadDataTableFromFileAsync(file1Name);
                var file2Data = await CsvReader.ReadDataTableFromFileAsync(file2Name);

                var comparer = new CsvComparer(file1Data, file2Data, identifierColumns, exclusionColumns, inclusionColumns);

                Results = await comparer.CompareAsync();

                if (Results.Differences.Rows.Count > 0)
                {
                    DifferencesLabel.Content = "Differences:";
                    DifferencesDataGrid.ItemsSource = Results.Differences.DefaultView;
                    DifferencesDataGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    DifferencesLabel.Content = "Differences: None";
                    DifferencesDataGrid.Visibility = Visibility.Collapsed;
                }

                File1OrphansLabel.Content = Results.OrphanColumns1?.Count > 0 
                    ? $"File 1 Extra Columns: {string.Join(", ", Results.OrphanColumns1)}" 
                    : "File 1 Extra Columns: None";

                File2OrphansLabel.Content = Results.OrphanColumns2?.Count > 0 
                    ? $"File 2 Extra Columns: {string.Join(", ", Results.OrphanColumns2)}" 
                    : "File 2 Extra Columns: None";

                if (Results.OrphanRows1?.Count > 0)
                {
                    var table = new DataTable();
                    foreach (DataColumn tableColumn in Results.OrphanRows1[0].Table.Columns)
                        table.Columns.Add(tableColumn.ColumnName);
                    foreach (var dataRow in Results.OrphanRows1)
                        table.ImportRow(dataRow);

                    File1ExtraRowsGrid.ItemsSource = table.DefaultView;

                    File1ExtraRowsLabel.Content = "File 1 Extra Rows:";
                    File1ExtraRowsGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    File1ExtraRowsLabel.Content = "File 1 Extra Rows: None";
                    File1ExtraRowsGrid.Visibility = Visibility.Collapsed;
                }

                if (Results.OrphanRows2?.Count > 0)
                {
                    var table = new DataTable();
                    foreach (DataColumn tableColumn in Results.OrphanRows2[0].Table.Columns)
                        table.Columns.Add(tableColumn.ColumnName);
                    foreach (var dataRow in Results.OrphanRows2)
                        table.ImportRow(dataRow);

                    File2ExtraRowsGrid.ItemsSource = table.DefaultView;

                    File2ExtraRowsLabel.Content = "File 2 Extra Rows:";
                    File2ExtraRowsGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    File2ExtraRowsLabel.Content = "File 2 Extra Rows: None";
                    File2ExtraRowsGrid.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                ErrorLabel.Content = $"Error: {ex.Message}{Environment.NewLine} Stack Trace: {ex.StackTrace}";
                ErrorLabel.Visibility = Visibility.Visible;
            }
        }

        private string GetSaveFileName()
        {
            var dialog = new SaveFileDialog()
            {
                Filter = "CSV Files |*.csv"
            };

            return dialog.ShowDialog(this) == true
                ? dialog.FileName
                : null;
        }

        private void EnableButtons()
        {
            StartOverButton.IsEnabled = true;
            ExportResultButton.IsEnabled = true;
        }

        private void DisableButtons()
        {
            StartOverButton.IsEnabled = false;
            ExportResultButton.IsEnabled = false;
        }

        private void ClearErrors()
        {
            ErrorLabel.Content = "";
            ErrorLabel.Visibility = Visibility.Collapsed;
        }
    }
}
