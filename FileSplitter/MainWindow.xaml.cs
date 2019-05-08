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
            JoinFile(@"C:\Users\salilk\Desktop\p.1.part", @"C:\Users\salilk\Desktop");
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

        private void SourceBrowse_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DestinationBrowse_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PerformSplit_Click(object sender, RoutedEventArgs e)
        {
            string sourceFile = this.SourceFile.Text;
            string destinationFolder = this.DestinationFolder.Text;
            string sizeText = this.SizeInKB.Text;
            int sizeInKB = 0;
            if (!int.TryParse(sizeText, out sizeInKB))
            {
                return;
            }

            SplitFile(sourceFile, destinationFolder, sizeInKB);
        }
    }
}
