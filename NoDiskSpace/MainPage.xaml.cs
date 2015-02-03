using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace NoDiskSpace
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            
        }

        static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        static string SizeSuffix(Int64 value)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }

            int i = 0;
            var dValue = (decimal)value;
            while (Math.Round(dValue / 1024) >= 1)
            {
                dValue /= 1024;
                i++;
            }

            return string.Format("{0:n1} {1}", dValue, SizeSuffixes[i]);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Windows.Storage.ApplicationData.Current.ClearAsync()
            //IsolatedStorageFile.GetUserStoreForApplication().AvailableFreeSpace;
            // http://msdn.microsoft.com/en-us/library/windowsphone/develop/system.io.isolatedstorage.isolatedstoragefile.quota(v=vs.105).aspx

            //Debug.WriteLine(Windows.Storage.ApplicationData.Current.LocalFolder.Properties.RetrievePropertiesAsync());
            //Debug.WriteLine("Available: " + IsolatedStorageFile.GetUserStoreForApplication().AvailableFreeSpace);


            long total = 0;

            using (var isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Debug.WriteLine("Available: " + isolatedStorage.AvailableFreeSpace + " (" + SizeSuffix(isolatedStorage.AvailableFreeSpace) + ")");

                string folder = @"\Gen*";

                foreach (var folderName in isolatedStorage.GetDirectoryNames(folder))
                {
                    foreach (var fileName in isolatedStorage.GetFileNames(folderName + "\\*"))
                    {
                        using (var file = isolatedStorage.OpenFile(folderName + "\\" + fileName, FileMode.Open))
                        {
                            // 1024 * 1024 = 1048576
                            // but most of the files created have a length of 1048578 - either UTF8 identifier prefix or CRLF at end
                            // account for this difference
                            total += file.Length;
                        }
                    }
                }
            }

            Debug.WriteLine("Used: " + total + " bytes " + SizeSuffix(total));
            MessageBox.Show(total + " bytes " + SizeSuffix(total));
        }

        StringBuilder sb = null;

        private void ApplicationBarIconButton_OnClick(object sender, EventArgs e)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                var x = store.GetDirectoryNames(@"\Gen*").Count();

                store.CreateDirectory("Gen" + x);

                if (sb == null)
                {
                    sb = new StringBuilder();

                    for (int j = 0; j < 1024; j++)
                    {
                        for (int k = 0; k < (j < 1023 ? 1024 : 1022); k++)
                        {
                            sb.Append(k.ToString().Substring(k.ToString().Length - 1));
                        }
                    }
                }

                for (int i = 0; i < 100; i++)
                {
                    using (var writeFile = new StreamWriter(new IsolatedStorageFileStream(string.Format("Gen{0}/{1}", x, i), FileMode.Create, FileAccess.Write, store)))
                    {
                        /// string someTextData = "This is some text data to be saved in a new text file in the IsolatedStorage!";
                        writeFile.WriteLine(sb.ToString());
                        writeFile.Close();
                    }
                }

                Debug.WriteLine("Still available " + store.AvailableFreeSpace + " (" + SizeSuffix(store.AvailableFreeSpace) + ")");
                //Debug.WriteLine(store.Quota.ToString());

               // MessageBox.Show(store.AvailableFreeSpace.ToString());
            }
        }
    }
}