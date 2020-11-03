using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
using System.Xml;
using System.Xml.Linq;

namespace WpfAppMag
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string cnString = Properties.Settings.Default.xmlDBConnectionString1;
        int returnId = -1;
        int returnboundaryEventId = -1;

        public MainWindow()
        {
            WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            InitializeComponent();
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
                    if (node.LocalName == "BPMNDiagram" || node.LocalName == "message" || node.LocalName == "resource")
                    {
                        continue;
                    }

                    attrVal = attrVal + "\n-" + node.LocalName;

                    SqlConnection cnConnection = new SqlConnection(cnString);
                    DataRowView row = (DataRowView)tableDataGrid1.SelectedItem;

                    for (i = 0; i < node.Attributes.Count; i++)
                    {
                        nodesX = nodesX + "\n" + node.Attributes[i].Name + " = " + node.Attributes[i].Value;

                        try
                        {
                            if (node.LocalName != "#text")
                            {
                                if (i == 0)
                                {
                                    using (SqlCommand cmd = new SqlCommand("INSERT INTO [TAGS](IdProcess, IdTag, tagType) VALUES(@IdProcess, ISNULL(@IdTag,'null'), ISNULL(@tagType,'null'));SELECT SCOPE_IDENTITY();", cnConnection))
                                    {
                                        // define parameters
                                        cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = row["Id"];
                                        if (node.Attributes[i].Name == "id")
                                            cmd.Parameters.Add("@IdTag", SqlDbType.VarChar).Value = node.Attributes[i].Value;
                                        cmd.Parameters.Add("@tagType", SqlDbType.VarChar).Value = node.LocalName;
                                        // open connection, execute query, close connection
                                        cnConnection.Open();
                                        object returnObj = cmd.ExecuteScalar();

                                        if (returnObj != null)
                                        {
                                            int.TryParse(returnObj.ToString(), out returnId);
                                        }

                                        cnConnection.Close();
                                    }
                                }
                                else
                                {
                                    using (SqlCommand cmd = new SqlCommand("UPDATE [TAGS] set IdProcess = @IdProcess, name = CASE WHEN name is null THEN @name ELSE name end, tagType = @tagType WHERE Id = @rowid", cnConnection))
                                    {
                                        // define parameters
                                        cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = row["Id"];
                                        cmd.Parameters.Add("@rowid", SqlDbType.Int).Value = returnId;
                                        if (node.Attributes[i].Name == "name")
                                            cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = node.Attributes[i].Value;
                                        else
                                            cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = DBNull.Value;
                                        cmd.Parameters.Add("@tagType", SqlDbType.VarChar).Value = node.LocalName;
                                        // open connection, execute query, close connection
                                        cnConnection.Open();
                                        cmd.ExecuteScalar();
                                        cnConnection.Close();
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
                        }
                    }

                    for (i = 0; i < node.ChildNodes.Count; i++)
                    {
                        string check = test(node.ChildNodes[i], nodesX, 2);
                        attrVal = attrVal + " " + check;
                    }

                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public string test(XmlNode node, string nodesX, int x)
        {
            SqlConnection cnConnection = new SqlConnection(cnString);
            DataRowView row = (DataRowView)tableDataGrid1.SelectedItem;
            bool ch = false;
            bool isPotentialOwner = false;
            try
            {
                if (node.ChildNodes.Count != 0)
                {
                    string result = new String('-', x);
                    nodesX = nodesX + "\n" + result + node.LocalName;

                    for (int i = 0; i < node.Attributes.Count; i++)
                    {
                        nodesX = nodesX + "\n" + node.Attributes[i].Name + " = " + node.Attributes[i].Value;

                        try
                        {
                            if (node.LocalName != "#text" && node.ParentNode.LocalName != "boundaryEvent" && node.ParentNode.LocalName != "startEvent" && node.ParentNode.LocalName != "endEvent" && node.ParentNode.LocalName != "intermediateThrowEvent" && node.ParentNode.LocalName != "intermediateCatchEvent")
                            {
                                if (i == 0)
                                {
                                    using (SqlCommand cmd = new SqlCommand("INSERT INTO [TAGS](IdProcess, IdTag, tagType) VALUES(@IdProcess, ISNULL(@IdTag,'null'), ISNULL(@tagType,'null'));SELECT SCOPE_IDENTITY();", cnConnection))
                                    {
                                        // define parameters
                                        cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = row["Id"];
                                        if (node.Attributes[i].Name == "id")
                                            cmd.Parameters.Add("@IdTag", SqlDbType.VarChar).Value = node.Attributes[i].Value;
                                        cmd.Parameters.Add("@tagType", SqlDbType.VarChar).Value = node.LocalName;
                                        // open connection, execute query, close connection
                                        cnConnection.Open();
                                        object returnObj = cmd.ExecuteScalar();

                                        if (returnObj != null)
                                        {
                                            if (node.LocalName == "boundaryEvent" || node.LocalName == "startEvent" || node.LocalName == "endEvent" || node.LocalName == "intermediateThrowEvent" || node.LocalName == "intermediateCatchEvent")
                                                int.TryParse(returnObj.ToString(), out returnboundaryEventId);
                                            else
                                                int.TryParse(returnObj.ToString(), out returnId);
                                        }

                                        cnConnection.Close();
                                    }
                                }
                                else
                                {
                                    using (SqlCommand cmd = new SqlCommand("UPDATE [TAGS] set IdProcess = @IdProcess, name = CASE WHEN name is null THEN @name ELSE name end, attachedToRef = CASE WHEN attachedToRef is null THEN @attachedToRef ELSE attachedToRef end,tagType = @tagType WHERE Id = @rowid", cnConnection))
                                    {
                                        // define parameters
                                        cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = row["Id"];
                                        if (node.LocalName == "boundaryEvent" || node.LocalName == "startEvent" || node.LocalName == "endEvent" || node.LocalName == "intermediateThrowEvent" || node.LocalName == "intermediateCatchEvent")
                                            cmd.Parameters.Add("@rowid", SqlDbType.Int).Value = returnboundaryEventId;
                                        else
                                            cmd.Parameters.Add("@rowid", SqlDbType.Int).Value = returnId;
                                        if (node.Attributes[i].Name == "name")
                                            cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = node.Attributes[i].Value;
                                        else
                                            cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = DBNull.Value;
                                        if (node.Attributes[i].Name == "attachedToRef")
                                            cmd.Parameters.Add("@attachedToRef", SqlDbType.VarChar).Value = node.Attributes[i].Value;
                                        else
                                            cmd.Parameters.Add("@attachedToRef", SqlDbType.VarChar).Value = DBNull.Value;
                                        cmd.Parameters.Add("@tagType", SqlDbType.VarChar).Value = node.LocalName;
                                        // open connection, execute query, close connection
                                        cnConnection.Open();
                                        cmd.ExecuteScalar();
                                        cnConnection.Close();
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            //MessageBox.Show(e.ToString());
                        }
                    }

                    if ((node.ParentNode.LocalName != "boundaryEvent" && node.ParentNode.LocalName != "startEvent" && node.ParentNode.LocalName != "endEvent" && node.ParentNode.LocalName != "intermediateThrowEvent" && node.ParentNode.LocalName != "intermediateCatchEvent") || (node.LocalName == "outgoing" || node.LocalName == "incoming"))
                    {
                        bool inLoop = false;
                        for (int i = 0; i < node.ChildNodes.Count; i++)
                        {
                            x++;
                            if ((node.LocalName == "boundaryEvent" || node.LocalName == "startEvent" || node.LocalName == "endEvent" || node.LocalName == "intermediateThrowEvent" || node.LocalName == "intermediateCatchEvent" || node.LocalName == "userTask" || node.LocalName == "subProcess") && inLoop != true)
                            {
                                bool checkInside = false;
                                for (int j = 0; j < node.ChildNodes.Count; j++)
                                {
                                    if (node.ChildNodes[j].LocalName == "potentialOwner")
                                    {
                                        isPotentialOwner = true;
                                    }
                                    if (node.ChildNodes[j].LocalName == "incoming" || node.ChildNodes[j].LocalName == "outgoing")
                                    {
                                        checkInside = true;
                                        inLoop = true;
                                    }
                                }

                                //if (isPotentialOwner && checkInside)
                                //    i = node.ChildNodes.Count - 3;
                                //else if (checkInside)
                                //    i = node.ChildNodes.Count - 2;
                                //else
                                //    i = node.ChildNodes.Count - 1;
                            }
                            nodesX = nodesX + test(node.ChildNodes[i], null, x);
                            ch = true;
                            x--;
                        }
                        if (node.LocalName == "subProcess")
                        {
                            string nameLastChildren = null;
                            using (SqlCommand cmd = new SqlCommand("SELECT tagType FROM [Tags] a WHERE a.IdProcess = @IdProcess ORDER BY Id DESC OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY", cnConnection))
                            {
                                // define parameters
                                cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = row["Id"];
                                // open connection, execute query, close connection
                                cnConnection.Open();
                                object returnObj = cmd.ExecuteScalar();

                                if (returnObj != null)
                                {
                                    nameLastChildren = returnObj.ToString();
                                }

                                cnConnection.Close();
                            }

                            if (nameLastChildren != "subProcess")
                            {
                                using (SqlCommand cmd = new SqlCommand("INSERT INTO [TAGS](IdProcess, tagType) VALUES(@IdProcess, ISNULL(@tagType,'null'));SELECT SCOPE_IDENTITY();", cnConnection))
                                {
                                    // define parameters
                                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = row["Id"];
                                    cmd.Parameters.Add("@tagType", SqlDbType.VarChar).Value = "/" + node.LocalName;
                                    // open connection, execute query, close connection
                                    cnConnection.Open();
                                    object returnObj = cmd.ExecuteScalar();

                                    if (returnObj != null)
                                    {
                                        if (node.LocalName == "boundaryEvent" || node.LocalName == "startEvent" || node.LocalName == "endEvent" || node.LocalName == "intermediateThrowEvent" || node.LocalName == "intermediateCatchEvent")
                                            int.TryParse(returnObj.ToString(), out returnboundaryEventId);
                                        else
                                            int.TryParse(returnObj.ToString(), out returnId);
                                    }

                                    cnConnection.Close();
                                }
                            }
                            else
                            {
                                using (SqlCommand cmd = new SqlCommand("UPDATE [TAGS] set IdProcess = @IdProcess, subTagType = 'rolled' WHERE Id = @rowid", cnConnection))
                                {
                                    // define parameters
                                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = row["Id"];
                                    cmd.Parameters.Add("@rowid", SqlDbType.Int).Value = returnId;
                                    // open connection, execute query, close connection
                                    cnConnection.Open();
                                    cmd.ExecuteScalar();
                                    cnConnection.Close();
                                }
                            }

                        }
                    }
                    else
                    {
                        using (SqlCommand cmd = new SqlCommand("UPDATE [TAGS] set subTagType = @subTagType WHERE Id = @rowid", cnConnection))
                        {
                            // define parameters
                            cmd.Parameters.Add("@rowid", SqlDbType.Int).Value = returnboundaryEventId;
                            cmd.Parameters.Add("@subTagType", SqlDbType.VarChar).Value = node.LocalName.ToString();
                            // open connection, execute query, close connection
                            cnConnection.Open();
                            cmd.ExecuteScalar();
                            cnConnection.Close();
                        }
                    }
                }
                else
                {
                    try
                    {
                        if (node.ParentNode.LocalName != "boundaryEvent" && node.ParentNode.LocalName != "startEvent" && node.ParentNode.LocalName != "endEvent" && node.ParentNode.LocalName != "intermediateThrowEvent" && node.ParentNode.LocalName != "intermediateCatchEvent")
                        {
                            for (int i = 0; i < node.Attributes.Count; i++)
                            {
                                nodesX = nodesX + "\n" + node.Attributes[i].Name + " = " + node.Attributes[i].Value;

                                try
                                {
                                    if (node.LocalName != "#text")
                                    {
                                        if (i == 0)
                                        {
                                            using (SqlCommand cmd = new SqlCommand("INSERT INTO [TAGS](IdProcess, IdTag, tagType) VALUES(@IdProcess, ISNULL(@IdTag,'null'), ISNULL(@tagType,'null'));SELECT SCOPE_IDENTITY();", cnConnection))
                                            {
                                                // define parameters
                                                cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = row["Id"];
                                                if (node.Attributes[i].Name == "id")
                                                    cmd.Parameters.Add("@IdTag", SqlDbType.VarChar).Value = node.Attributes[i].Value;
                                                cmd.Parameters.Add("@tagType", SqlDbType.VarChar).Value = node.LocalName;
                                                // open connection, execute query, close connection
                                                cnConnection.Open();
                                                object returnObj = cmd.ExecuteScalar();

                                                if (returnObj != null)
                                                {
                                                    int.TryParse(returnObj.ToString(), out returnId);
                                                }

                                                cnConnection.Close();
                                            }
                                        }
                                        else
                                        {
                                            using (SqlCommand cmd = new SqlCommand("UPDATE [TAGS] set IdProcess = @IdProcess, name = CASE WHEN name is null THEN @name ELSE name end, sourceRef = CASE WHEN sourceRef is null THEN @sourceRef ELSE sourceRef end, targetRef = CASE WHEN targetRef is null THEN @targetRef ELSE targetRef end, tagType = @tagType WHERE Id = @rowid", cnConnection))
                                            {
                                                // define parameters
                                                cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = row["Id"];
                                                cmd.Parameters.Add("@rowid", SqlDbType.Int).Value = returnId;
                                                if (node.Attributes[i].Name == "name")
                                                    cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = node.Attributes[i].Value;
                                                else
                                                    cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = DBNull.Value;
                                                if (node.Attributes[i].Name == "sourceRef")
                                                    cmd.Parameters.Add("@sourceRef", SqlDbType.VarChar).Value = node.Attributes[i].Value;
                                                else
                                                    cmd.Parameters.Add("@sourceRef", SqlDbType.VarChar).Value = DBNull.Value;
                                                if (node.Attributes[i].Name == "targetRef")
                                                    cmd.Parameters.Add("@targetRef", SqlDbType.VarChar).Value = node.Attributes[i].Value;
                                                else
                                                    cmd.Parameters.Add("@targetRef", SqlDbType.VarChar).Value = DBNull.Value;
                                                cmd.Parameters.Add("@tagType", SqlDbType.VarChar).Value = node.LocalName;
                                                // open connection, execute query, close connection
                                                cnConnection.Open();
                                                cmd.ExecuteScalar();
                                                cnConnection.Close();
                                            }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    //MessageBox.Show(e.ToString());
                                }
                            }
                        }
                        else
                        {
                            using (SqlCommand cmd = new SqlCommand("UPDATE [TAGS] set subTagType = @subTagType WHERE Id = @rowid", cnConnection))
                            {
                                // define parameters
                                cmd.Parameters.Add("@rowid", SqlDbType.Int).Value = returnboundaryEventId;
                                cmd.Parameters.Add("@subTagType", SqlDbType.VarChar).Value = node.LocalName.ToString();
                                // open connection, execute query, close connection
                                cnConnection.Open();
                                cmd.ExecuteScalar();
                                cnConnection.Close();
                            }
                        }
                    }
                    catch (Exception e) { /*MessageBox.Show(e.ToString());*/ }
                }

                if (node.LocalName == "#text")
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE [TAGS] set IdProcess = @IdProcess, incoming = CASE WHEN incoming is null THEN @incoming ELSE incoming end, incoming2 = CASE WHEN incoming is not null and incoming2 is null THEN @incoming ELSE incoming2 end, incoming3 = CASE WHEN incoming is not null and incoming2 is not null and incoming3 is null THEN @incoming ELSE incoming3 end, outgoing = CASE WHEN outgoing is null THEN @outgoing ELSE outgoing end, outgoing2 = CASE WHEN outgoing is not null and outgoing2 is null THEN @outgoing ELSE outgoing2 end, outgoing3 = CASE WHEN outgoing is not null and outgoing2 is not null and outgoing3 is null THEN @outgoing ELSE outgoing3 end WHERE Id = @rowid", cnConnection))
                    {
                        // define parameters
                        cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = row["Id"];

                        if (node.ParentNode.ParentNode.LocalName == "boundaryEvent" || node.ParentNode.ParentNode.LocalName == "startEvent" || node.ParentNode.ParentNode.LocalName == "endEvent" || node.ParentNode.ParentNode.LocalName == "intermediateThrowEvent" || node.ParentNode.ParentNode.LocalName == "intermediateCatchEvent")
                            cmd.Parameters.Add("@rowid", SqlDbType.Int).Value = returnboundaryEventId;
                        else
                            cmd.Parameters.Add("@rowid", SqlDbType.Int).Value = returnId;

                        if (node.ParentNode.LocalName == "incoming")
                            cmd.Parameters.Add("@incoming", SqlDbType.VarChar).Value = node.Value;
                        else
                            cmd.Parameters.Add("@incoming", SqlDbType.VarChar).Value = DBNull.Value;

                        if (node.ParentNode.LocalName == "outgoing")
                            cmd.Parameters.Add("@outgoing", SqlDbType.VarChar).Value = node.Value;
                        else
                            cmd.Parameters.Add("@outgoing", SqlDbType.VarChar).Value = DBNull.Value;

                        // open connection, execute query, close connection
                        cnConnection.Open();
                        cmd.ExecuteScalar();
                        cnConnection.Close();
                    }
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
            catch (Exception e) { /*MessageBox.Show(e.ToString());*/ return null; };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            XmlDocument xmlDoc = new XmlDocument();

            SqlConnection cnConnection = new SqlConnection(cnString);
            openFileDialog.Filter = "XML Files (*.xml)|*.xml|Custom type files|*.bpmn";


            if (openFileDialog.ShowDialog() == true)
            {
                string xmlContent = File.ReadAllText(openFileDialog.FileName);

                xmlDoc.Load(openFileDialog.FileName);
                using (SqlCommand cmd = new SqlCommand("INSERT INTO [Table](struktura_xml, nazwa_pliku, wynik_weryfikacji) VALUES (@xml, @fileName, @resultVerify)", cnConnection))
                {
                    // define parameters
                    cmd.Parameters.Add("@xml", SqlDbType.VarChar, -1).Value = xmlContent;
                    cmd.Parameters.Add("@fileName", SqlDbType.VarChar).Value = openFileDialog.SafeFileName;
                    cmd.Parameters.Add("@resultVerify", SqlDbType.VarChar).Value = "Nie zweryfikowano";
                    // open connection, execute query, close connection
                    cnConnection.Open();
                    cmd.ExecuteNonQuery();
                    cnConnection.Close();
                    loadDataToGrid();
                }

            }
            //load();
        }

        private void deleteRowBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SqlConnection cnConnection = new SqlConnection(cnString);
                DataRowView row = (DataRowView)tableDataGrid1.SelectedItem;

                try
                {
                    using (SqlCommand cmd = new SqlCommand("DELETE FROM [Tags] WHERE IdProcess = @rowid", cnConnection))
                    {
                        // define parameters
                        cmd.Parameters.Add("@rowid", SqlDbType.VarChar).Value = row["Id"];

                        // open connection, execute query, close connection
                        cnConnection.Open();
                        cmd.ExecuteNonQuery();
                        cnConnection.Close();
                    }
                }
                catch { }

                using (SqlCommand cmd = new SqlCommand("DELETE FROM [Table] WHERE Id = @rowid", cnConnection))
                {
                    // define parameters
                    cmd.Parameters.Add("@rowid", SqlDbType.VarChar).Value = row["Id"];

                    // open connection, execute query, close connection
                    cnConnection.Open();
                    cmd.ExecuteNonQuery();
                    cnConnection.Close();
                    loadDataToGrid();
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(e.ToString());
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadDataToGrid();

        }

        void loadDataToGrid()
        {
            WpfAppMag.XMLDBDataSet1 xMLDBDataSet1 = ((WpfAppMag.XMLDBDataSet1)(this.FindResource("xMLDBDataSet1")));
            // Załaduj dane do tabeli Table. Możesz modyfikować ten kod w razie potrzeby.
            WpfAppMag.XMLDBDataSet1TableAdapters.TableTableAdapter xMLDBDataSet1TableTableAdapter = new WpfAppMag.XMLDBDataSet1TableAdapters.TableTableAdapter();
            xMLDBDataSet1TableTableAdapter.Fill(xMLDBDataSet1.Table);
            System.Windows.Data.CollectionViewSource tableViewSource1 = ((System.Windows.Data.CollectionViewSource)(this.FindResource("tableViewSource1")));
            tableViewSource1.View.MoveCurrentToFirst();
        }

        private void DataGridCell_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            DataRowView row = (DataRowView)tableDataGrid1.SelectedItem;

            xmlInfo winXml = new xmlInfo(row);
            winXml.Show();
        }
        private void DataGridCell_MouseDoubleClick_ErrorDetails(object sender, MouseButtonEventArgs e)
        {
            DataRowView row = (DataRowView)tableDataGrid1.SelectedItem;

            if (row["wynik_weryfikacji"].ToString() == "Negatywny")
            {
                errorDetailsWin errorDetailsWin = new errorDetailsWin(row);
                errorDetailsWin.Show();
            }
            else if (row["wynik_weryfikacji"].ToString() == "Pozytywny")
                MessageBox.Show("Weryfikacja pozytywna - brak błędów!");
            else
                MessageBox.Show("Nie przeprowadzono analizy!");
        }

        private void analyzeRowBtn_Click(object sender, RoutedEventArgs e)
        {
            DataRowView row = (DataRowView)tableDataGrid1.SelectedItem;
            XmlDocument tests = new XmlDocument();
            SqlConnection cnConnection = new SqlConnection(cnString);
            Button srcButton = e.Source as Button;
            int checkAnalyzed = 0;

            try
            {
                using (SqlCommand cmd = new SqlCommand("SELECT count(*) FROM [Tags] WHERE IdProcess = @IdProcess", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = row["Id"];

                    cnConnection.Open();
                    object returnObj = cmd.ExecuteScalar();

                    if (returnObj != null)
                    {
                        checkAnalyzed = Convert.ToInt32(returnObj);
                    }
                    cnConnection.Close();
                }
            }
            catch { }

            if (checkAnalyzed == 0)
            {

                tests.LoadXml(row["struktura_xml"].ToString());
                load(tests);

                string result = checkRules(row["id"].ToString());

                try
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE [Table] set wynik_weryfikacji = @result WHERE Id = @rowid", cnConnection))
                    {
                        cmd.Parameters.Add("@rowid", SqlDbType.Int).Value = row["Id"];
                        cmd.Parameters.Add("@result", SqlDbType.VarChar).Value = result;

                        cnConnection.Open();
                        cmd.ExecuteScalar();
                        cnConnection.Close();
                    }
                }
                catch { }

                loadDataToGrid();
            }
            else
                MessageBox.Show("Plik był już analizowany");
        }

        public string checkRules(string rowId)
        {
            SqlConnection cnConnection = new SqlConnection(cnString);
            string rules = null;
            string rulesTmp;

            rulesTmp = rule01(rowId);
            if (rulesTmp != null)
                rules = rules + "\n" + rulesTmp;
            rulesTmp = rule02(rowId);
            if (rulesTmp != null)
                rules = rules + "\n" + rulesTmp;
            rulesTmp = rule03(rowId);
            if (rulesTmp != null)
                rules = rules + "\n" + rulesTmp;
            rulesTmp = rule04(rowId);
            if (rulesTmp != null)
                rules = rules + "\n" + rulesTmp;
            rulesTmp = rule05(rowId);
            if (rulesTmp != null)
                rules = rules + "\n" + rulesTmp;
            rulesTmp = rule06(rowId);
            if (rulesTmp != null)
                rules = rules + "\n" + rulesTmp;
            rulesTmp = rule07(rowId);
            if (rulesTmp != null)
                rules = rules + "\n" + rulesTmp;
            rulesTmp = rule08(rowId);
            if (rulesTmp != null)
                rules = rules + "\n" + rulesTmp;
            rulesTmp = rule09(rowId);
            if (rulesTmp != null)
                rules = rules + "\n" + rulesTmp;
            rulesTmp = rule10(rowId);
            if (rulesTmp != null)
                rules = rules + "\n" + rulesTmp;
            rulesTmp = rule11(rowId);
            if (rulesTmp != null)
                rules = rules + "\n" + rulesTmp;
            rulesTmp = rule12(rowId);
            if (rulesTmp != null)
                rules = rules + "\n" + rulesTmp;
            rulesTmp = rule13(rowId);
            if (rulesTmp != null)
                rules = rules + "\n" + rulesTmp;
            //rules = rules + rule14(rowId);
            rulesTmp = rule15(rowId);
            if (rulesTmp != null)
                rules = rules + "\n" + rulesTmp;
            rulesTmp = rule16(rowId);
            if (rulesTmp != null)
                rules = rules + "\n" + rulesTmp;
            rulesTmp = rule17(rowId);
            if (rulesTmp != null)
                rules = rules + "\n" + rulesTmp;

            try
            {
                using (SqlCommand cmd = new SqlCommand("UPDATE [Table] set bledy_weryfikacji = @errors WHERE Id = @rowid", cnConnection))
                {
                    // define parameters
                    cmd.Parameters.Add("@rowid", SqlDbType.Int).Value = rowId;
                    cmd.Parameters.Add("@errors", SqlDbType.VarChar).Value = rules;

                    // open connection, execute query, close connection
                    cnConnection.Open();
                    cmd.ExecuteScalar();
                    cnConnection.Close();
                }
            }
            catch { };

            if (rules == null || rules == "")
                return "Pozytywny";
            else
                return "Negatywny";
        }

        public string rule01(string rowId)
        {
            string ruleInside = null;
            int i = 1;

            SqlConnection cnConnection = new SqlConnection(cnString);

            try
            {
                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT IdTag FROM [Tags] WHERE (subTagType in ('startEvent', 'messageEventDefinition', 'timerEventDefinition', 'escalationEventDefinition', 'cancelEventDefinition', 'compensateEventDefinition', 'conditionalEventDefinition', 'signalEventDefinition', 'signalEventDefinition', 'conditionalEventDefinition', 'escalationEventDefinition', 'timerEventDefinition', 'messageEventDefinition') and tagType not in ('endEvent','intermediateCatchEvent')) and incoming is not null and IdProcess = @IdProcess", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            if (i == 1)
                                ruleInside = ruleInside + "Poniższe obiekty nie mogą mieć przepływu wejściowego" + "\n";

                            ruleInside = ruleInside + "- " + oReader["IdTag"].ToString() + "\n";
                            i++;
                        }

                        i = 1;
                        cnConnection.Close();
                    }
                    // open connection, execute query, close connection

                    cnConnection.Close();
                }

                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT IdTag FROM [Tags] a WHERE subTagType not in ('startEvent', 'messageEventDefinition', 'timerEventDefinition', 'escalationEventDefinition', 'cancelEventDefinition', 'compensateEventDefinition', 'conditionalEventDefinition', 'signalEventDefinition', 'signalEventDefinition', 'conditionalEventDefinition', 'escalationEventDefinition', 'timerEventDefinition', 'messageEventDefinition', 'errorEventDefinition', 'potentialOwner') and tagType not in ('startEvent', 'process', 'sequenceFlow', 'collaboration', 'participant', 'group', 'category', 'categoryValue', 'laneSet', '/subProcess', 'messageFlow', 'lane', 'association', 'dataObject', 'dataObjectReference', 'dataOutputAssociation', 'dataInputAssociation', 'textAnnotation', 'property', 'dataStoreReference') and NOT EXISTS (SELECT * FROM [Tags] b WHERE b.tagType = 'messageFlow' and b.targetRef = a.IdTag) and incoming is null and IdProcess = @IdProcess", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            if (i == 1)
                                ruleInside = ruleInside + "Poniższe obiekty muszą posiadać przepływ wejściowy" + "\n";

                            ruleInside = ruleInside + "- " + oReader["IdTag"].ToString() + "\n";
                            i++;
                        }

                        i = 1;
                        cnConnection.Close();
                    }
                    // open connection, execute query, close connection

                    cnConnection.Close();
                }
            }
            catch (Exception e) { /*MessageBox.Show(e.ToString());*/ }

            return ruleInside;
        }

        public string rule02(string rowId)
        {
            string ruleInside = null;
            int i = 1;

            SqlConnection cnConnection = new SqlConnection(cnString);

            try
            {
                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT IdTag FROM [Tags] a WHERE ((subTagType in ('startEvent', 'messageEventDefinition', 'timerEventDefinition', 'escalationEventDefinition', 'cancelEventDefinition', 'compensateEventDefinition', 'conditionalEventDefinition', 'signalEventDefinition', 'signalEventDefinition', 'conditionalEventDefinition', 'escalationEventDefinition', 'timerEventDefinition', 'messageEventDefinition') and tagType != 'endEvent') or tagType in ('subProcess','transaction','callActivity') or tagType like '%Gateway%' or tagType like '%Task%') and NOT EXISTS (SELECT * FROM [Tags] b WHERE b.tagType = 'messageFlow' and b.sourceRef = a.IdTag) and outgoing is null and IdProcess = @IdProcess", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            if (i == 1)
                                ruleInside = ruleInside + "Poniższe obiekty muszą posiadać przepływ wyjściowy" + "\n";

                            ruleInside = ruleInside + "- " + oReader["IdTag"].ToString() + "\n";
                            i++;
                        }

                        i = 1;
                        cnConnection.Close();
                    }
                    // open connection, execute query, close connection

                    cnConnection.Close();
                }

                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT IdTag FROM [Tags] WHERE tagType = 'endEvent' and outgoing is not null and IdProcess = @IdProcess", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            if (i == 1)
                                ruleInside = ruleInside + "Poniższe obiekty nie mogą posiadać przepływu wyjściowego" + "\n";

                            ruleInside = ruleInside + "- " + oReader["IdTag"].ToString() + "\n";
                            i++;
                        }

                        i = 1;
                    }
                    // open connection, execute query, close connection

                    cnConnection.Close();
                }
            }
            catch (Exception e) { /*MessageBox.Show(e.ToString());*/ }

            return ruleInside;
        }

        public string rule03(string rowId)
        {
            int returnCountSub = 0;
            int returnCountStart = 0;
            string comOut = null;

            SqlConnection cnConnection = new SqlConnection(cnString);

            try
            {
                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; Select Count(*) FROM [Tags] t1 WHERE tagType = 'subProcess' and subTagType is null and  IdProcess = @IdProcess", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    object returnObj = cmd.ExecuteScalar();

                    if (returnObj != null)
                    {
                        returnCountSub = Convert.ToInt32(returnObj);
                    }
                    // open connection, execute query, close connection

                    cnConnection.Close();
                }

                if (returnCountSub > 0)
                {
                    int minRow = -1;
                    int maxRow = -1;
                    int intMin;
                    int counter = 0;
                    var list = new List<Tuple<int, string, string>>();
                    var listMin = new List<Tuple<int, string>>();
                    var listMax = new List<Tuple<int, string>>();
                    int returnCountError = 0;
                    int returnCountEscalation = 0;

                    using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT * FROM [Tags] t1 WHERE (tagType = 'subProcess' or tagType = '/subProcess') and subTagType is null and  t1.IdProcess = @IdProcess order by Id", cnConnection))
                    //using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT Min(Id) as min, Max(Id) as max FROM [Tags] t1 WHERE (tagType = 'subProcess' or tagType = '/subProcess') and subTagType is null and  IdProcess = @IdProcess", cnConnection))
                    {
                        cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                        cnConnection.Open();
                        using (SqlDataReader oReader = cmd.ExecuteReader())
                        {
                            while (oReader.Read())
                            {
                                //minRow = Convert.ToInt32(oReader["min"]);
                                //maxRow = Convert.ToInt32(oReader["max"]);
                                list.Add(new Tuple<int, string, string>(Convert.ToInt32(oReader["Id"]), oReader["tagType"].ToString(), oReader["IdTag"].ToString()));
                            }
                        }
                        cnConnection.Close();
                    }

                    for (int k = 0; k < list.Count(); k++)
                    {
                        if (list.ElementAt(k).Item2 == "subProcess")
                        {
                            listMin.Add(new Tuple<int, string>(list.ElementAt(k).Item1, list.ElementAt(k).Item3));
                        }
                        if (list.ElementAt(k).Item2 == "/subProcess")
                        {
                            listMax.Add(new Tuple<int, string>(list.ElementAt(k).Item1, list.ElementAt(k).Item3));
                        }
                    }

                    for (int h = -1; h <= (listMin.Count() - 1); h++)
                    {
                        minRow = listMin.ElementAt(counter).Item1;

                        try
                        {
                            intMin = listMin.ElementAt(h + 1).Item1;
                        }
                        catch
                        {
                            intMin = listMin.ElementAt(h).Item1;
                        }

                        if (intMin < listMax.ElementAt(counter).Item1 && (h + 1) != listMin.Count())
                        {
                            continue;
                        }
                        else
                        {
                            maxRow = listMax.ElementAt(h).Item1;

                            if ((minRow + 1) != maxRow)
                            {
                                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT COUNT(*) FROM [Tags] t1 WHERE tagType = 'startEvent' and Id between @min and @max;", cnConnection))
                                {
                                    cmd.Parameters.Add("@min", SqlDbType.Int).Value = minRow;
                                    cmd.Parameters.Add("@max", SqlDbType.Int).Value = maxRow;

                                    cnConnection.Open();
                                    object returnObj = cmd.ExecuteScalar();

                                    if (returnObj != null)
                                    {
                                        returnCountStart = returnCountStart + Convert.ToInt32(returnObj);
                                    }
                                    cnConnection.Close();

                                }

                                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT COUNT(*) FROM [Tags] t1 WHERE subTagType = 'errorEventDefinition' and tagType = 'endEvent' and Id between @min and @max;", cnConnection))
                                {
                                    cmd.Parameters.Add("@min", SqlDbType.Int).Value = minRow;
                                    cmd.Parameters.Add("@max", SqlDbType.Int).Value = maxRow;

                                    cnConnection.Open();
                                    object returnObj = cmd.ExecuteScalar();

                                    if (returnObj != null)
                                    {
                                        returnCountError = returnCountError + Convert.ToInt32(returnObj);
                                    }
                                    cnConnection.Close();

                                }

                                if (returnCountError > 0)
                                {
                                    returnCountError = 0;
                                    string attachedToRef = listMin.ElementAt(h).Item2;
                                    using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT COUNT(*) FROM [Tags] t1 WHERE tagType = 'boundaryEvent' and subTagType = 'errorEventDefinition' and attachedToRef = @attachedToRef;", cnConnection))
                                    {
                                        cmd.Parameters.Add("@attachedToRef", SqlDbType.VarChar).Value = attachedToRef;

                                        cnConnection.Open();
                                        object returnObj = cmd.ExecuteScalar();

                                        if (returnObj != null)
                                        {
                                            returnCountError = returnCountError + Convert.ToInt32(returnObj);
                                        }
                                        cnConnection.Close();

                                    }
                                    if (returnCountError == 0)
                                        comOut = "Jeżeli w podprocesie występuje zdarzenie błędu, podproces musi mieć zdarzenie graniczne błędu\n";
                                }

                                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT COUNT(*) FROM [Tags] t1 WHERE subTagType = 'escalationEventDefinition' and tagType = 'intermediateThrowEvent' and Id between @min and @max;", cnConnection))
                                {
                                    cmd.Parameters.Add("@min", SqlDbType.Int).Value = minRow;
                                    cmd.Parameters.Add("@max", SqlDbType.Int).Value = maxRow;

                                    cnConnection.Open();
                                    object returnObj = cmd.ExecuteScalar();

                                    if (returnObj != null)
                                    {
                                        returnCountEscalation = returnCountEscalation + Convert.ToInt32(returnObj);
                                    }
                                    cnConnection.Close();

                                }

                                if (returnCountEscalation > 0)
                                {
                                    returnCountEscalation = 0;
                                    string attachedToRef = listMin.ElementAt(h).Item2;
                                    using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT COUNT(*) FROM [Tags] t1 WHERE tagType = 'boundaryEvent' and subTagType = 'escalationEventDefinition' and attachedToRef = @attachedToRef;", cnConnection))
                                    {
                                        cmd.Parameters.Add("@attachedToRef", SqlDbType.VarChar).Value = attachedToRef;

                                        cnConnection.Open();
                                        object returnObj = cmd.ExecuteScalar();

                                        if (returnObj != null)
                                        {
                                            returnCountEscalation = returnCountEscalation + Convert.ToInt32(returnObj);
                                        }
                                        cnConnection.Close();

                                    }
                                    if (returnCountEscalation == 0)
                                        comOut = "Jeżeli w podprocesie występuje zdarzenie eskalacji, podproces musi mieć zdarzenie graniczne eskalacji\n";
                                }
                            }
                            else
                            {
                                returnCountStart += 1;
                            }
                            counter = h + 1;
                        }
                    }
                }

            }
            catch (Exception e) { /*MessageBox.Show(e.ToString());*/ }



            if (returnCountStart < returnCountSub)
                return comOut + "Nie poprawna liczba zdarzeń początkowych w stosunku do podprocesów ( " + (returnCountSub - returnCountStart) + " ), \nkażdy podproces musi mieć zdarzenie początkowe\n";
            else if (returnCountStart > returnCountSub)
                return comOut + "Nie poprawna liczba zdarzeń początkowych w stosunku do podprocesów ( " + (returnCountSub - returnCountStart) + " ), \nkażdy podproces może mieć tylko jedno zdarzenie początkowe\n";
            else
                return comOut;
        }

        public string rule04(string rowId)
        {
            string ruleInside = null;
            int i = 1;

            SqlConnection cnConnection = new SqlConnection(cnString);

            try
            {
                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT * FROM [Tags] t1 WHERE t1.TagType = 'startEvent' and outgoing in (Select incoming FROM [Tags] t2 WHERE t2.tagType = 'endEvent' and t2.incoming = t1.outgoing)  and  IdProcess = @IdProcess", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            if (i == 1)
                                ruleInside = ruleInside + "Zdarzenie początkowe nie może być połączone z wydarzeniem końcowym." + "\n";

                            ruleInside = ruleInside + "- " + oReader["IdTag"].ToString() + "\n";
                            i++;
                        }

                        i = 1;
                        cnConnection.Close();
                    }
                    // open connection, execute query, close connection

                    cnConnection.Close();
                }
            }
            catch (Exception e) { /*MessageBox.Show(e.ToString());*/ }

            return ruleInside;
        }

        public string rule05(string rowId)
        {
            string ruleInside = null;
            int i = 1;
            SqlConnection cnConnection = new SqlConnection(cnString);

            try
            {
                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT IdTag FROM [Tags] a WHERE a.subTagType = 'errorEventDefinition' and tagType = 'boundaryEvent' and a.outgoing is null and IdProcess = @IdProcess", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    // open connection, execute query, close connection
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            if (i == 1)
                                ruleInside = ruleInside + "Zdarzenie błędu granicznego musi być połączone z pasującym zdarzeniem zgłoszenia błędu." + "\n";

                            ruleInside = ruleInside + "- " + oReader["IdTag"].ToString() + "\n";
                            i++;
                        }

                        i = 1;
                        cnConnection.Close();
                    }
                    cnConnection.Close();
                }
            }
            catch { return null; }

            return ruleInside;
        }

        public string rule06(string rowId)
        {
            string ruleInside = null;
            int i = 1;
            SqlConnection cnConnection = new SqlConnection(cnString);

            try
            {
                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT IdTag FROM[Tags] a WHERE a.subTagType = 'escalationEventDefinition' and EXISTS(SELECT * FROM[Tags] b WHERE b.IdTag = a.attachedToRef AND b.tagType != 'subProcess') and IdProcess = @IdProcess", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    // open connection, execute query, close connection
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            if (i == 1)
                                ruleInside = ruleInside + "Zdarzenie graniczne eskalacji musi być zdarzeniem granicznym wyłącznie dla podprocesów." + "\n";

                            ruleInside = ruleInside + "- " + oReader["IdTag"].ToString() + "\n";
                            i++;
                        }

                        i = 1;
                        cnConnection.Close();
                    }
                    cnConnection.Close();
                }
            }
            catch { return null; }

            return ruleInside;
        }

        public string rule07(string rowId)
        {
            string ruleInside = null;
            int i = 1;
            SqlConnection cnConnection = new SqlConnection(cnString);

            try
            {
                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT IdTag FROM[Tags] a WHERE a.subTagType = 'escalationEventDefinition' and EXISTS(SELECT * FROM[Tags] b WHERE b.IdTag = a.attachedToRef AND b.tagType != 'subProcess') and IdProcess = @IdProcess", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    // open connection, execute query, close connection
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            if (i == 1)
                                ruleInside = ruleInside + "Zdarzenie graniczne eskalacji musi być zdarzeniem granicznym wyłącznie dla podprocesów." + "\n";

                            ruleInside = ruleInside + "- " + oReader["IdTag"].ToString() + "\n";
                            i++;
                        }

                        i = 1;
                        cnConnection.Close();
                    }
                    cnConnection.Close();
                }
            }
            catch { return null; }

            return ruleInside;
        }

        public string rule08(string rowId)
        {
            string ruleInside = null;
            int i = 1;
            SqlConnection cnConnection = new SqlConnection(cnString);

            try
            {
                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT * FROM [Tags] a WHERE a.tagType like '%Gateway' and ((a.outgoing is not null and a.outgoing2 is null and a.incoming is not null and a.incoming2 is null) or (a.outgoing is not null and a.outgoing2 is not null and a.incoming is not null and a.incoming2 is not null)) and IdProcess = @IdProcess", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    // open connection, execute query, close connection
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            if (i == 1)
                                ruleInside = ruleInside + "Bramki logiczne dzielące muszą mieć przynajmniej dwa wyjścia,\nnatomiast bramki logiczne łączące muszą posiadać przynajmniej dwa wejścia.\nNie mogą mieć ilości dwóch wejść oraz wyjść na raz\n";

                            ruleInside = ruleInside + "- " + oReader["IdTag"].ToString() + "\n";
                            i++;
                        }

                        i = 1;
                        cnConnection.Close();
                    }
                    cnConnection.Close();
                }
            }
            catch { return null; }

            return ruleInside;
        }

        public string rule09(string rowId)
        {
            string ruleInside = null;
            int i = 1;
            SqlConnection cnConnection = new SqlConnection(cnString);

            try
            {
                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT * FROM [Tags] a WHERE a.tagType = 'messageFlow' and EXISTS (SELECT * FROM [Tags] b WHERE b.IdTag = a.targetRef and (b.tagType not in ('intermediate', 'intermediateThrowEvent', 'receiveTask', 'userTask', 'serviceTask', 'subProcess', 'participant') and (b.tagType != 'startEvent' and (b.subTagType != 'messageEventDefinition' or b.subTagType is null)) and (b.tagType != 'intermediateCatchEvent' and (b.subTagType != 'messageEventDefinition' or b.subTagType is null))) and b.IdProcess = @IdProcess) and a.IdProcess = @IdProcess", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    // open connection, execute query, close connection
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            if (i == 1)
                                ruleInside = ruleInside + "Przepływ komunikatów może przejść tylko do zdarzenia rozpoczęcia komunikatu lub zdarzenia pośredniego;\nZadania typu odbioru, użytkownika lub usługi; Podprocesu" + "\n";

                            ruleInside = ruleInside + "- " + oReader["targetRef"].ToString() + "\n";
                            i++;
                        }

                        i = 1;
                        cnConnection.Close();
                    }
                    cnConnection.Close();
                }
            }
            catch { return null; }

            return ruleInside;
        }

        public string rule10(string rowId)
        {
            string ruleInside = null;
            int i = 1;
            SqlConnection cnConnection = new SqlConnection(cnString);

            try
            {
                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT * FROM [Tags] a WHERE a.tagType = 'messageFlow' and EXISTS (SELECT * FROM [Tags] b WHERE b.IdTag = a.sourceRef and (b.tagType not in ('intermediate', 'intermediateThrowEvent', 'sendTask', 'userTask', 'serviceTask', 'subProcess', 'participant') and (b.tagType != 'endEvent' and b.subTagType is null)) and b.IdProcess = @IdProcess) and IdProcess = @IdProcess", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    // open connection, execute query, close connection
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            if (i == 1)
                                ruleInside = ruleInside + "Przepływ komunikatów może pochodzić tylko ze zdarzenia końcowego lub zdarzenia pośredniego Messege;\nZadania typu wyślij, użytkownik lub usługa; Podprocesu" + "\n";

                            ruleInside = ruleInside + "- " + oReader["sourceRef"].ToString() + "\n";
                            i++;
                        }

                        i = 1;
                        cnConnection.Close();
                    }
                    cnConnection.Close();
                }
            }
            catch { return null; }

            return ruleInside;
        }

        public string rule11(string rowId)
        {
            string ruleInside = null;
            int i = 1;
            SqlConnection cnConnection = new SqlConnection(cnString);

            try
            {
                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT IdTag FROM [Tags] a WHERE (a.tagType like '%task%' or a.tagType like '%start%' or a.tagType like '%end%' or tagType in ('boundaryEvent','intermediateThrowEvent','intermediateCatchEvent', 'participant', 'lane')) and (subTagType != 'messageEventDefinition' or subTagType is null) and a.name is null and IdProcess = @IdProcess", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    // open connection, execute query, close connection
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            if (i == 1)
                                ruleInside = ruleInside + "Aktywność musi posiadać etykiete" + "\n";

                            ruleInside = ruleInside + "- " + oReader["IdTag"].ToString() + "\n";
                            i++;
                        }

                        i = 1;
                        cnConnection.Close();
                    }
                    cnConnection.Close();
                }
            }
            catch { return null; }

            return ruleInside;
        }

        public string rule12(string rowId)
        {
            string ruleInside = null;
            int i = 1;
            SqlConnection cnConnection = new SqlConnection(cnString);

            try
            {
                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT name FROM [Tags] a WHERE a.IdProcess = @IdProcess and (a.name is not null and a.name != '' and lower(a.name) not in ('yes','no', 'tak', 'nie')) GROUP BY a.name HAVING COUNT(*) > 1", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    // open connection, execute query, close connection
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            if (i == 1)
                                ruleInside = ruleInside + "Nie może być dwóch aktywności o tej samej etykiecie" + "\n";

                            ruleInside = ruleInside + "- " + oReader["name"].ToString() + "\n";
                            i++;
                        }

                        i = 1;
                        cnConnection.Close();
                    }
                    cnConnection.Close();
                }
            }
            catch { return null; }

            return ruleInside;
        }
        public string rule13(string rowId)
        {
            string resultRule = null;
            int j = 0;
            SqlConnection cnConnection = new SqlConnection(cnString);
            var listOut = new List<Tuple<string, string, string, string>>();

            try
            {
                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT IdTag, outgoing, outgoing2, outgoing3 FROM [Tags] a WHERE (a.tagType like '%Gateway%' and a.tagType != 'parallelGateway') and a.IdProcess = @IdProcess and a.outgoing is not null and a.outgoing2 is not null", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    // open connection, execute query, close connection
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            listOut.Add(new Tuple<string, string, string, string>(oReader["IdTag"].ToString(), oReader["outgoing"].ToString(), oReader["outgoing2"].ToString(), oReader["outgoing3"].ToString()));
                        }
                        cnConnection.Close();
                    }
                    cnConnection.Close();
                }

                for (int i = 0; i < listOut.Count(); i++)
                {
                    int countSeq = 0;
                    using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT count(*) FROM [Tags] a WHERE a.IdProcess = @IdProcess and a.IdTag = @IdTag and name is null", cnConnection))
                    {
                        cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                        cmd.Parameters.Add("@IdTag", SqlDbType.VarChar).Value = listOut[i].Item2;
                        cnConnection.Open();
                        // open connection, execute query, close connection
                        object returnObj = cmd.ExecuteScalar();

                        if (returnObj != null)
                        {
                            countSeq = Convert.ToInt32(returnObj);
                        }
                        cnConnection.Close();
                    }

                    using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT count(*) FROM [Tags] a WHERE a.IdProcess = @IdProcess and a.IdTag = @IdTag and name is null", cnConnection))
                    {
                        cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                        cmd.Parameters.Add("@IdTag", SqlDbType.VarChar).Value = listOut[i].Item3;
                        cnConnection.Open();
                        // open connection, execute query, close connection
                        object returnObj = cmd.ExecuteScalar();

                        if (returnObj != null)
                        {
                            countSeq += Convert.ToInt32(returnObj);
                        }
                        cnConnection.Close();
                    }

                    using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT count(*) FROM [Tags] a WHERE a.IdProcess = @IdProcess and a.IdTag = @IdTag and name is null", cnConnection))
                    {
                        cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                        cmd.Parameters.Add("@IdTag", SqlDbType.VarChar).Value = listOut[i].Item4;
                        cnConnection.Open();
                        // open connection, execute query, close connection
                        object returnObj = cmd.ExecuteScalar();

                        if (returnObj != null)
                        {
                            countSeq += Convert.ToInt32(returnObj);
                        }
                        cnConnection.Close();
                    }
                    if (countSeq > 1 && j == 0)
                    {
                        resultRule = "Bramka wyłączna (ALBO), integracyjna (LUB), sterowana zdarzeniami (Event) mogą mieć maksymalnie jedną sekwencję wychodząca bez nazwy(domyślną)\n - " + listOut[i].Item1;
                        j++;
                    }
                    else if (countSeq > 1)
                        resultRule = resultRule + "\n - " + listOut[i].Item1;
                }
            }
            catch { return null; }

            return resultRule;
        }

        public string rule14(string rowId)
        {
            string resultRule = null;
            int j = 0;
            SqlConnection cnConnection = new SqlConnection(cnString);
            var listOut = new List<Tuple<string, string, string, string>>();

            try
            {
                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT IdTag, outgoing, outgoing2, outgoing3 FROM [Tags] a WHERE a.tagType = 'parallelGateway' and a.IdProcess = @IdProcess and a.outgoing is not null and a.outgoing2 is not null", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    // open connection, execute query, close connection
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            listOut.Add(new Tuple<string, string, string, string>(oReader["IdTag"].ToString(), oReader["outgoing"].ToString(), oReader["outgoing2"].ToString(), oReader["outgoing3"].ToString()));
                        }
                        cnConnection.Close();
                    }
                    cnConnection.Close();
                }

                for (int i = 0; i < listOut.Count(); i++)
                {
                    int countSeq = 0;
                    using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT count(*) FROM [Tags] a WHERE a.IdProcess = @IdProcess and a.IdTag = @IdTag and name is null", cnConnection))
                    {
                        cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                        cmd.Parameters.Add("@IdTag", SqlDbType.VarChar).Value = listOut[i].Item2;
                        cnConnection.Open();
                        // open connection, execute query, close connection
                        object returnObj = cmd.ExecuteScalar();

                        if (returnObj != null)
                        {
                            countSeq = Convert.ToInt32(returnObj);
                        }
                        cnConnection.Close();
                    }

                    using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT count(*) FROM [Tags] a WHERE a.IdProcess = @IdProcess and a.IdTag = @IdTag and name is null", cnConnection))
                    {
                        cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                        cmd.Parameters.Add("@IdTag", SqlDbType.VarChar).Value = listOut[i].Item3;
                        cnConnection.Open();
                        // open connection, execute query, close connection
                        object returnObj = cmd.ExecuteScalar();

                        if (returnObj != null)
                        {
                            countSeq += Convert.ToInt32(returnObj);
                        }
                        cnConnection.Close();
                    }

                    using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT count(*) FROM [Tags] a WHERE a.IdProcess = @IdProcess and a.IdTag = @IdTag and name is null", cnConnection))
                    {
                        cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                        cmd.Parameters.Add("@IdTag", SqlDbType.VarChar).Value = listOut[i].Item4;
                        cnConnection.Open();
                        // open connection, execute query, close connection
                        object returnObj = cmd.ExecuteScalar();

                        if (returnObj != null)
                        {
                            countSeq += Convert.ToInt32(returnObj);
                        }
                        cnConnection.Close();
                    }
                    if (countSeq > 0 && j == 0)
                    {
                        resultRule = "Bramka logiczna równoległa (AND) nie może mieć sekwencji wychodzącej bez nazwy (ścieżki domyślnej)\n - " + listOut[i].Item1;
                        j++;
                    }
                    else if (countSeq > 0)
                        resultRule = resultRule + "\n - " + listOut[i].Item1;
                }
            }
            catch { return null; }

            return resultRule;
        }

        public string rule15(string rowId)
        {
            string resultRule = null;
            int j = 0;
            SqlConnection cnConnection = new SqlConnection(cnString);
            var listOut = new List<Tuple<string>>();

            try
            {
                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT name FROM [Tags] a WHERE a.tagType = 'subProcess' and a.IdProcess = @IdProcess and a.subTagType = 'rolled'", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    // open connection, execute query, close connection
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            listOut.Add(new Tuple<string>(oReader["name"].ToString()));
                        }
                        cnConnection.Close();
                    }
                    cnConnection.Close();
                }

                for (int i = 0; i < listOut.Count(); i++)
                {
                    int countSeq = 0;
                    using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT count(*) FROM [Tags] a WHERE a.IdProcess = @IdProcess and a.name = @name and a.tagType = 'subProcess' and a.subTagType is null", cnConnection))
                    {
                        cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                        cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = listOut[i].Item1;
                        cnConnection.Open();
                        // open connection, execute query, close connection
                        object returnObj = cmd.ExecuteScalar();

                        if (returnObj != null)
                        {
                            countSeq = Convert.ToInt32(returnObj);
                        }
                        cnConnection.Close();
                    }

                    if (countSeq == 0 && j == 0)
                    {
                        resultRule = "Zdefiniowany jest podproces ''zwinięty'' bez przedstawionego procesu ''rozwiniętego''\n - " + listOut[i].Item1;
                        j++;
                    }
                    else if (countSeq > 1 && j == 0)
                    {
                        resultRule = "Zdefiniowanych jest wiele procesów ''rozwiniętych'' posiadających tą samą nazwę\n - " + listOut[i].Item1;
                        j++;
                    }
                    else if (countSeq > 0)
                        resultRule = resultRule + "\n - " + listOut[i].Item1;
                }
            }
            catch { return null; }

            return resultRule;
        }

        public string rule16(string rowId)
        {
            string ruleInside = null;
            int i = 1;
            SqlConnection cnConnection = new SqlConnection(cnString);
            var listOut = new List<Tuple<string>>();

            try
            {
                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT IdTag FROM [Tags] a WHERE a.tagType = 'sendTask' and a.IdProcess = @IdProcess and NOT EXISTS (SELECT * FROM [TAGS] b WHERE b.sourceRef = a.IdTag and b.tagType = 'messageFlow' and b.IdProcess = @IdProcess)", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    // open connection, execute query, close connection
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            if (i == 1)
                                ruleInside = ruleInside + "Aktywność wysyłania wiadomości musi mieć przepływ komunikatu jako wyjście (messageFlow)" + "\n";

                            ruleInside = ruleInside + "- " + oReader["IdTag"].ToString() + "\n";
                            i++;
                        }

                        i = 1;
                        cnConnection.Close();
                    }
                    cnConnection.Close();
                }

            }
            catch { return null; }

            return ruleInside;
        }

        public string rule17(string rowId)
        {
            string ruleInside = null;
            int i = 1;
            SqlConnection cnConnection = new SqlConnection(cnString);
            var listOut = new List<Tuple<string>>();

            try
            {
                using (SqlCommand cmd = new SqlCommand("SET ANSI_NULLS OFF; SELECT IdTag FROM [Tags] a WHERE a.tagType = 'receiveTask' and a.IdProcess = @IdProcess and NOT EXISTS (SELECT * FROM [TAGS] b WHERE b.targetRef = a.IdTag and b.tagType = 'messageFlow' and b.IdProcess = @IdProcess)", cnConnection))
                {
                    cmd.Parameters.Add("@IdProcess", SqlDbType.Int).Value = rowId;
                    cnConnection.Open();
                    // open connection, execute query, close connection
                    using (SqlDataReader oReader = cmd.ExecuteReader())
                    {
                        while (oReader.Read())
                        {
                            if (i == 1)
                                ruleInside = ruleInside + "Aktywność odbierania wiadomości musi mieć przepływ komunikatu jako wejście (messageFlow)" + "\n";

                            ruleInside = ruleInside + "- " + oReader["IdTag"].ToString() + "\n";
                            i++;
                        }

                        i = 1;
                        cnConnection.Close();
                    }
                    cnConnection.Close();
                }

            }
            catch { return null; }

            return ruleInside;
        }
    }
}