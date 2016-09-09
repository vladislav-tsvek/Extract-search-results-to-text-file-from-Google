using DotNetBrowser;
using DotNetBrowser.DOM;
using DotNetBrowser.Events;
using DotNetBrowser.WinForms;
using DotNetBrowser.WPF;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Extract_search_results_to_text_file_from_Google
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            browserView.Browser.LoadURL("http://www.google.com");

            ComplexPageLoad();

            this.Text = browserView.Browser.Title;
            toolStripAddress.Text = browserView.Browser.URL.ToString();
        }

        private void toolStripAddress_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ComplexPageLoad();
                browserView.Browser.LoadURL(toolStripAddress.Text.ToString());
                this.Text = browserView.Browser.Title;
                toolStripAddress.Text = browserView.Browser.URL.ToString();
            }
        }

        private void ComplexPageLoad()
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            FinishLoadingFrameHandler listener = new FinishLoadingFrameHandler((object sender, FinishLoadingEventArgs e) =>
            {
                if (e.IsMainFrame)
                {
                    resetEvent.Set();
                }
            });
            browserView.Browser.FinishLoadingFrameEvent += listener;
            try
            {
                browserView.Browser.LoadURL(toolStripAddress.Text.ToString());
                resetEvent.WaitOne(new TimeSpan(0, 0, 45));
            }
            finally
            {
                browserView.Browser.FinishLoadingFrameEvent -= listener;
            }
        }

        private void toolStripGoogleSearchToFile_Click(object sender, EventArgs e)
        {
            //Only for Google search results

            DOMDocument document = browserView.Browser.GetDocument();
            string allTextToFile = "";
            string searchWord = "";

            try
            {
                searchWord = document.GetElementByClassName("q qs").GetAttribute("href").Split('&')[0].ToString();
            }
            catch
            {
                MessageBox.Show("There is no info to parse", "Error!");
            }


            if (searchWord.Contains("/search?q="))
            {
                ManualResetEvent resetEvent = new ManualResetEvent(false);
                FinishLoadingFrameHandler listener = new FinishLoadingFrameHandler((object sender1, FinishLoadingEventArgs e1) =>
                {
                    if (e1.IsMainFrame)
                    {
                        resetEvent.Set();
                    }
                });
                browserView.Browser.FinishLoadingFrameEvent += listener;

                try
                {
                    browserView.Browser.LoadURL("https://www.google.com.ua" + searchWord);
                    resetEvent.WaitOne(new TimeSpan(0, 0, 45));
                }
                finally
                {
                    browserView.Browser.FinishLoadingFrameEvent -= listener;
                }

                // Return address of Web page
                toolStripAddress.Text = browserView.Browser.URL.ToString();
                this.Text = browserView.Browser.Title;

                int countDiv = document.GetElementsByTagName("div").Count;
                int countH3 = 0;

                //Quantity of <h3> tags in <div> tags
                for (int i = 0; i < countDiv; i++)
                {
                    int tmpH3 = document.GetElementsByTagName("div")[i].GetElementsByTagName("h3").Count;
                    countH3 += tmpH3;
                }

                //Search for <a> tags in <h3>
                for (int i = 0; i < countH3; i++)
                {
                    string text = "";
                    try
                    {
                        text = document.GetElementsByTagName("h3")[i].GetElementByTagName("a").InnerText.ToString();
                    }
                    catch
                    {
                        break;
                    }
                                                           
                    //Make string for add info to file
                    string textToFile = "# " + (i + 1) + Environment.NewLine +
                       "name =  " + text + Environment.NewLine +
                       "hyperlink = " + document.GetElementsByTagName("h3")[i].GetElementByTagName("a").GetAttribute("href") + Environment.NewLine;

                    allTextToFile += textToFile + Environment.NewLine;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "TXT files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                //Choose path and name to save the file
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Format name of file
                    string txtFileName = saveFileDialog.FileName.ToString();
                    //string weq = saveFileDialog.
                    File.WriteAllText(txtFileName, allTextToFile);
                }
            }
        }
    }
}