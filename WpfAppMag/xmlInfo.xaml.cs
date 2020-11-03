using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
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
using System.Windows.Shapes;
using System.Xml;

namespace WpfAppMag
{
    /// <summary>
    /// Logika interakcji dla klasy xmlInfo.xaml
    /// </summary>
    public partial class xmlInfo : Window
    {
        public xmlInfo(DataRowView row)
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
            xmlInside.Text = PrintXML(row["struktura_xml"].ToString());
            //XmlDocument testowka = new XmlDocument();
            //testowka.LoadXml(row["struktura_xml"].ToString());
            //load(testowka);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public static string PrintXML(string xml)
        {
            string result = "";

            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
            XmlDocument document = new XmlDocument();

            try
            {
                document.LoadXml(xml);

                writer.Formatting = Formatting.Indented;

                document.WriteContentTo(writer);
                writer.Flush();
                mStream.Flush();

                mStream.Position = 0;

                StreamReader sReader = new StreamReader(mStream);

                string formattedXml = sReader.ReadToEnd();

                result = formattedXml;
            }
            catch (XmlException)
            {
                result = null;
            }

            mStream.Close();
            writer.Close();

            return result;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(xmlInside.Text);
        }

        void load(XmlDocument xmldoc)
        {
            try
            {
                int i = 0;
                string nodesX = "";
                string attrVal = "";
                string result = string.Empty;
                List<int> Value1 = new List<int>();
                List<int> Value2 = new List<int>();
                List<string> Operator = new List<string>();
                OpenFileDialog openFileDialog = new OpenFileDialog();

                    foreach (XmlNode node in xmldoc.DocumentElement.ChildNodes)
                    {
                        if (node.LocalName == "BPMNDiagram")
                        {
                            continue;
                        }

                        attrVal = attrVal + "\n-" + node.LocalName;

                        for (i = 0; i < node.ChildNodes.Count; i++)
                        {
                            string check = test(node.ChildNodes[i], nodesX, 2);
                            attrVal = attrVal + " " + check/*node.ChildNodes[i].LocalName*/ ;
                        }

                    }

                    //xmlnode = xmldoc.GetElementsByTagName(prefixNode + "process");
                    //for (i = 0; i < xmlnode.Count; i++)
                    //{
                    //    attrVal = attrVal + xmlnode[i].InnerXml;//xmlnode[i].Attributes["id"].Value;
                    //}
                    xmlInside.Text = attrVal;

                    //for (i = 0; i <= xmlnode.Count - 1; i++)
                    //{
                    //    xmlnode[i].ChildNodes.Item(0).InnerText.Trim();
                    //    str = str + xmlnode[i].ChildNodes.Item(0).InnerText.Trim() + "  " + xmlnode[i].ChildNodes.Item(1).InnerText.Trim() + "  " + xmlnode[i].ChildNodes.Item(2).InnerText.Trim() + System.Environment.NewLine;
                    //    Value1.Add(int.Parse(xmlnode[i].ChildNodes.Item(0).InnerText.Trim()));
                    //    Value2.Add(int.Parse(xmlnode[i].ChildNodes.Item(2).InnerText.Trim()));
                    //    Operator.Add(xmlnode[i].ChildNodes.Item(1).InnerText.Trim());
                    //}
                    //loadStatusText.Text = str;
                }
                //for (int j = 0; j < Value1.Count; j++)
                //{
                //    if (Operator[j] == "+")
                //    {
                //        result = result + Value1[j] + Value2[j] + System.Environment.NewLine;
                //    }

                //    //add if else block or switch cases for all the operators.
                //    //e.g if (Operator[j] == "-")

                //}

                //loadStatusText.Text = result;
           
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        public string test(XmlNode node, string nodesX, int x)
        {
            bool ch = false;
            if (node.ChildNodes.Count != 0)
            {
                string result = new String('-', x);
                nodesX = nodesX + "\n" + result + node.LocalName;
                for (int i = 0; i < node.Attributes.Count; i++)
                {
                    nodesX = nodesX + "\n" + node.Attributes[i].Name + " = " + node.Attributes[i].Value;
                }
                for (int i = 0; i < node.ChildNodes.Count; i++)
                {
                    x++;
                    nodesX = nodesX + test(node.ChildNodes[i], null, x);
                    ch = true;
                    x--;
                }
            }
            else
            {
                try
                {
                    for (int i = 0; i < node.Attributes.Count; i++)
                    {
                        nodesX = nodesX + "\n" + node.Attributes[i].Name + " = " + node.Attributes[i].Value;
                    }
                } catch{}
            }

            if (node.LocalName == "#text")
            {
                return " - " + node.Value;
            }
            else
            {
                if (!ch)
                {
                    string result = new String('-', x);
                    nodesX = "\n" + result + node.LocalName + nodesX;
                    return nodesX;
                }
                else
                {
                    return nodesX;
                }
            }
        }
    }
}
