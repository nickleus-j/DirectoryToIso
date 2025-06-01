using DiscUtils.Iso9660;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DirectoryToIso
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
        private Dictionary<string, string> getFileList(DirectoryInfo folder, DirectoryInfo home)
        {
            Dictionary<string, string> output = new Dictionary<string, string>();
            CDBuilder builder = new CDBuilder();

            foreach (FileInfo file in folder.GetFiles())
            {
                string fileFullPath = file.FullName;
                string addedPath = home.FullName.Equals(folder.FullName) ? "" : "\\";
                string fileOnIso =  $"{folder.FullName.Substring(home.FullName.Length )}{addedPath}{file.Name}";
                output.Add(fileOnIso, file.FullName);
            }

            // Now do it for all subfolders
            foreach (DirectoryInfo directory in folder.GetDirectories())
            {
                getFileList(directory, home).ToList().ForEach(file => output.Add(file.Key, file.Value));
            }

            return output;
        }
        private int BuildIso(DirectoryInfo sourceDirectory, string targetFile)
        {
            CDBuilder builder = new CDBuilder();
            Dictionary<string, string> resultList = new Dictionary<string, string>();

            try
            {
                // Get main folder and put it into results.
                getFileList(sourceDirectory, sourceDirectory).ToList().ForEach(file => resultList.Add(file.Key, file.Value));

                

                // Finally, add all files collected to the ISO.
                foreach (KeyValuePair<string, string> pair in resultList.ToList())
                {
                    builder.AddFile(pair.Key, pair.Value);
                }

                builder.Build(targetFile);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error Writing ISO. Check Permissions and Files. " + e.Message);
                return 1;
            }

            return 0;
        }

        private void DirectoryBtn_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog
            {
                // Set options here
            };
            if (folderDialog.ShowDialog() == true)
            {
                var folderName = folderDialog.FolderName;
                DirectoryBox.Text = folderName;
                IsoLocation.Text = folderName + "\\output.iso";
            }
        }
        private void IsoBtn_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new SaveFileDialog
            {
                Filter = "Iso (*.iso)|*.iso",
                DefaultExt = ".iso",
                Title = "Save text file"
            };
            if (folderDialog.ShowDialog() == true)
            {
                IsoLocation.Text = folderDialog.FileName;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CDBuilder builder = new CDBuilder();
            Dictionary<string, string> resultList = new Dictionary<string, string>();



            if (Directory.Exists(DirectoryBox.Text))
            {
                DirectoryInfo chosenDirectory = new DirectoryInfo(DirectoryBox.Text);
                Dispatcher dispatcher = Application.Current.Dispatcher;
                dispatcher.BeginInvoke(new Action(() =>
                {
                    // Update the UI
                    int buildResult = BuildIso(chosenDirectory, IsoLocation.Text);
                    if (buildResult == 0)
                    {
                        MessageBox.Show("ISO written sucessfully!");
                    }
                }));
                


            }
            else
            {
                MessageBox.Show("Please make sure the directory exists!");
            }
        }
    }
}