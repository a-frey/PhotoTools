using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PhotoBox
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int PREVIEW_WINDOW_AMOUNT = 100;
        PhotoTools tools;

        public MainWindow()
        {
            InitializeComponent();
            tools = new PhotoTools();

            /*
             * TODO: Datei Gleichheit nach größe und Name, dann automatisch _00x anhängen
             */
        }

        private void BtnSelectFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (System.Windows.Forms.DialogResult.OK.Equals(result))
                {
                    tools.Init(dialog.SelectedPath);
                    txtBoxSelectedPath.Text = tools.Path;
                    txtBoxEntries.Text = "";
                    txtBoxLog.Text = "";
                    txtBoxPreview.Text = "";
                }
                WriteLog(tools.GetFiles().ToList().Count + " files found.");
                ShowFiles();
                ShowPreview();
            }
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            tools = new PhotoTools();
            txtBoxEntries.Text = "";
            txtBoxPreview.Text = "";
            txtBoxSelectedPath.Text = "";
            txtBoxLog.Text = "";

            pgBar.Minimum = 0;
            pgBar.Maximum = 1;

            UpdateProgressBar(true);
        }

        private void BtnRename_Click(object sender, RoutedEventArgs e)
        {
            if (tools.GetFiles().Any())
            {

                WriteLog("=================");
                WriteLog("Starting Renaming");
                WriteLog("=================");

                RenameImages();
            }
            else
            {
                WriteLog("Please select a folder with Images");
            }
        }

        private void RenameImages()
        {
            int amount = tools.GetFiles().Count();

            pgBar.Minimum = 0;
            pgBar.Maximum = amount;

            tools.GetFiles().ToList().ForEach(
                elem =>
                {
                    RenameImage(elem);
                    UpdateProgressBar(false);
                    
                }
            );

            WriteLog("==================");
            WriteLog("Rename Images DONE");
            WriteLog("==================");
        }

        private void UpdateProgressBar(bool reset)
        {
            if (reset)
            {
                pgBar.Value = 0;       // direkte Änderung der ProgressBar-Eigenschaft
                pgBar.DataContext = 0;
            }
            else
            {
                ++pgBar.Value;       // direkte Änderung der ProgressBar-Eigenschaft

                var percentage = pgBar.Value + "/" + pgBar.Maximum + "  " + Math.Round(pgBar.Value / pgBar.Maximum * 100, 2);
                pgBar.DataContext = percentage;
            }
            
            // Aktualisierung der ProgressBar ("refresh")
            pgBar.Dispatcher.Invoke(
                EmptyDelegate,
                System.Windows.Threading.DispatcherPriority.Background
                );
        }

        Action EmptyDelegate = delegate () { }; // Zuweisung einer anonymen Methode ohne ausführbaren Code

        private void RenameImage(FileInfo file)
        {
            if (tools.IsJpegImage(file))
            {
                string oldPath = file.FullName;
                string newPath = tools.GetNewFileName(file);

                if (!File.Exists(newPath))
                {
                    File.Move(oldPath, newPath);
                    WriteLog("Moved '" + file.Name + "' to '" + newPath + "'");
                }
                else if (IsFileNameCorrect(oldPath, newPath))
                {
                    WriteLog("File '" + file.Name + "' has already the correct name");
                }
                else
                {
                    var newPathPostFix = GetNewFileNamePostFix(newPath);
                    File.Move(oldPath, newPathPostFix);
                    WriteLog(file.Name + "' => '" + newPathPostFix + "'");

                }
            }
            else
            {
                WriteLog(file.Name + " is not an image. SKIPPING");
            }
        }

        private static bool IsFileNameCorrect(string originalPath, string newPath)
        {
            if (originalPath.Equals(newPath, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            string newtmp = System.IO.Path.GetFileNameWithoutExtension(newPath) + "_";

            // check if file is already renamed
            return System.IO.Path.GetFileNameWithoutExtension(originalPath).StartsWith(newtmp);
        }

        private string GetNewFileNamePostFix(string newPath)
        {
            FileInfo file = new FileInfo(newPath);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(newPath);
            string resultpath = "";

            for (int i = 1; i < 1000; i++)
            {
                string PaddedResult = i.ToString().PadLeft(4, '0');
                var tmpPath = file.DirectoryName + "\\" + fileName + "_" + PaddedResult + file.Extension;
                if (!File.Exists(tmpPath))
                {
                    resultpath = tmpPath;
                    break;
                }
            }
            return resultpath;
        }

        private void WriteLog(string content)
        {
            txtBoxLog.Text = content + "\n" + txtBoxLog.Text;

        }

        private void ShowFiles()
        {
            tools.GetFiles().Take(PREVIEW_WINDOW_AMOUNT).ToList().ForEach(elem => ShowInTextBox(elem.Name));
        }

        private void ShowPreview()
        {
            tools.GetFiles().Take(PREVIEW_WINDOW_AMOUNT).ToList().ForEach(elem => ShowInPreviewBox(tools.GetNewFileName(elem)));
        }

        private void ShowInTextBox(string elem)
        {
            txtBoxEntries.Text += elem;
            txtBoxEntries.Text += "\n";
        }

        private void ShowInPreviewBox(string elem)
        {
            txtBoxPreview.Text += elem;
            txtBoxPreview.Text += "\n";
        }

        
    }
}
