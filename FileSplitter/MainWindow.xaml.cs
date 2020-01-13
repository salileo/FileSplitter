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

namespace FileSplitter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public bool SplitFile(string sourceFile, string destinationFolder, int sizeInKB)
        {
            string fileName = System.IO.Path.GetFileName(sourceFile);
            using (Stream inputFileStream = File.OpenRead(sourceFile))
            {
                int fileCount = 1;
                bool streamFinished = false;
                while (!streamFinished)
                {
                    string tmpFileName = destinationFolder + "\\" + fileName + "." + fileCount.ToString() + ".part";
                    fileCount++;

                    using (Stream outputFileStream = File.OpenWrite(tmpFileName))
                    {
                        for (int kbCount = 0; kbCount < sizeInKB; kbCount++)
                        {
                            byte[] oneKBBuffer = new byte[1024];
                            int bytesToRead = 1024;
                            while (bytesToRead > 0)
                            {
                                int bytesRead = inputFileStream.Read(oneKBBuffer, 0, bytesToRead);
                                if (bytesRead > 0)
                                {
                                    outputFileStream.Write(oneKBBuffer, 0, bytesRead);
                                    bytesToRead -= bytesRead;
                                }
                                else if (bytesRead == 0)
                                {
                                    streamFinished = true;
                                    break;
                                }
                            }

                            if (streamFinished)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return true;
        }

        public bool JoinFile(string sourceFile, string destinationFolder)
        {
            string sourceFolder = System.IO.Path.GetDirectoryName(sourceFile);
            string fileName = System.IO.Path.GetFileName(sourceFile);
            fileName = fileName.ToLower();

            string[] parts = fileName.Split(new char[] { '.' });
            if (parts.Length < 3)
            {
                return false;
            }

            if (parts[parts.Length - 1] != "part")
            {
                return false;
            }

            int partNumber = 0;
            if (!int.TryParse(parts[parts.Length - 2], out partNumber))
            {
                return false;
            }

            string suffix = "." + partNumber.ToString() + ".part";
            int index = fileName.LastIndexOf(suffix);
            if (index <= 0)
            {
                return false;
            }

            fileName = fileName.Substring(0, index);
            string destinationFile = destinationFolder + "\\" + fileName;
            using (Stream outputFileStream = File.OpenWrite(destinationFile))
            {
                int fileCount = 1;
                bool mergeFinished = false;
                while (!mergeFinished)
                {
                    string inputFile = sourceFolder + "\\" + fileName + "." + fileCount + ".part";
                    fileCount++;

                    if (File.Exists(inputFile))
                    {
                        using (Stream inputFileStream = File.OpenRead(inputFile))
                        {
                            byte[] oneKBBuffer = new byte[1024];
                            int bytesToRead = 1024;
                            while (true)
                            {
                                int bytesRead = inputFileStream.Read(oneKBBuffer, 0, bytesToRead);
                                if (bytesRead > 0)
                                {
                                    outputFileStream.Write(oneKBBuffer, 0, bytesRead);
                                }
                                else if (bytesRead == 0)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        mergeFinished = true;
                        break;
                    }
                }
            }

            return true;
        }

        private void SplitSourceBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = true;
            openFileDialog.Title = "Select the file to split.";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.SplitSourceFile.Text = openFileDialog.FileName;
            }
        }

        private void SplitDestinationBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = true;
            folderBrowserDialog.Description = "Select the destination directory.";

            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.SplitDestinationFolder.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void JoinSourceBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = true;
            openFileDialog.Filter = "Join File|*.1.part";
            openFileDialog.Title = "Select the first file to join from.";

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.JoinSourceFile.Text = openFileDialog.FileName;
            }
        }

        private void JoinDestinationBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = true;
            folderBrowserDialog.Description = "Select the destination directory.";

            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.JoinDestinationFolder.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void PerformSplit_Click(object sender, RoutedEventArgs e)
        {
            string sourceFile = this.SplitSourceFile.Text;
            string destinationFolder = this.SplitDestinationFolder.Text;
            string sizeText = this.SizeInKB.Text;
            int sizeInKB = 0;
            if (!int.TryParse(sizeText, out sizeInKB))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(sourceFile) || string.IsNullOrWhiteSpace(destinationFolder))
            {
                return;
            }

            SplitFile(sourceFile, destinationFolder, sizeInKB);
            System.Windows.MessageBox.Show("Done");
        }

        private void PerformJoin_Click(object sender, RoutedEventArgs e)
        {
            string sourceFile = this.JoinSourceFile.Text;
            string destinationFolder = this.JoinDestinationFolder.Text;

            if (string.IsNullOrWhiteSpace(sourceFile) || string.IsNullOrWhiteSpace(destinationFolder))
            {
                return;
            }

            JoinFile(sourceFile, destinationFolder);
            System.Windows.MessageBox.Show("Done");
        }
    }
}
