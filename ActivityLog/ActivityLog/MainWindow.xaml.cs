// 180311 by Johan Lång
using System;
using System.Collections.Generic;
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
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Text.RegularExpressions;

namespace ActivityLog
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



        private void ButtonAddDay_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    List<string> daysList = DayList(ListBoxDays);

                    if (daysList.Contains(LabelDate.Content))
                    {
                        MessageBox.Show("This Date already exists!");
                    }

                    else
                    {
                        string dayWeightStr = TextBoxDayWeight.Text;
                        dayWeightStr = Regex.Replace(dayWeightStr, ",", ".");

                        double dayWeight = 0;

                        if (dayWeightStr != "")
                        {
                            try
                            {
                                dayWeight = float.Parse(dayWeightStr);
                                dayWeight = Math.Round(dayWeight, 1);
                            }
                            catch
                            {
                                MessageBox.Show("Enter double value for Day Weight, ex 85.5");
                            }
                        }

                        ListBoxActivities.Items.Clear();

                        DateTime selectedDate = CalenderDays.SelectedDate.Value;
                        string date = selectedDate.ToString("yyyy-MM-dd");

                        // string date = CalenderDays.SelectedDate.Value.ToString();

                        using (SqlConnection conn = new SqlConnection(AppContext.connectionString))
                        {
                            try
                            {
                                conn.Open();
                                SqlCommand sqlUpdate = new SqlCommand(@"INSERT INTO day (Date, DayWeight) VALUES (@THEDATE, @THEDAYWEIGHT)", conn);

                                sqlUpdate.Parameters.AddWithValue("@THEDATE", date);
                                sqlUpdate.Parameters.AddWithValue("@THEDAYWEIGHT", dayWeight);
                                sqlUpdate.ExecuteNonQuery();
                            }

                            catch (SqlException ex)
                            {
                                MessageBox.Show(ex.Message);
                            }

                            finally
                            {
                                conn.Close();
                            }
                        }
                        ListBoxDays.Items.Add(date);

                        SortListBoxAscending(ListBoxDays);
                    }

                }, null);

            });

        }

        private void ListBoxDays_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    using (SqlConnection conn = new SqlConnection(AppContext.connectionString))
                    {
                        SqlCommand sqlQuery = new SqlCommand(@"SELECT FORMAT(Date, 'yyyy-MM-dd') FROM day ORDER BY Date ASC", conn);

                        try
                        {
                            conn.Open();

                            SqlDataReader reader = sqlQuery.ExecuteReader();

                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    ListBoxDays.Items.Add(reader[i]);
                                }
                            }
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                });
                    
        
            });
        }

        private void ComboBoxActivityID_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();
            data.Add("");
            data.Add("Cycling");
            data.Add("Running");
            data.Add("Swiming");
            data.Add("Walking");

            // ... Get the ComboBox reference.
            var comboBox = sender as ComboBox;

            // ... Assign the ItemsSource to the List.
            comboBox.ItemsSource = data;

            // ... Make the first item selected.
            comboBox.SelectedIndex = 0;
        }

        private void CalenderDays_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    // ... Get reference.
                    var calendar = sender as Calendar;

                    // ... See if a date is selected.
                    if (calendar.SelectedDate.HasValue)
                    {
                        // ... Display SelectedDate in Title.
                        DateTime date = calendar.SelectedDate.Value;
                        LabelDate.Content = date.ToString("yyyy-MM-dd");
                        ButtonAddDay.IsEnabled = true;
                        TextBoxDayWeight.IsEnabled = true;
                        TextBoxDayWeight.Text = "";

                    }

                }, null);
            });
        }

        private void ButtonAddActivity_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    string activityTimeStr = TextBoxActivityTime.Text;

                    TimeSpan activityTime = default(TimeSpan);

                    if (activityTimeStr != "")
                    {
                        try
                        {
                            activityTime = TimeSpan.Parse(activityTimeStr);

                        }
                        catch
                        {
                            MessageBox.Show("Enter timespan value for Activity time, ex 2:23:45 (Hours:Minutes:Seconds)");
                        }
                    }

                    string activityDistStr = TextBoxActivityDistance.Text;
                    activityDistStr = Regex.Replace(activityDistStr, ",", ".");

                    double activityDist = 0;

                    if (activityDistStr != "")
                    {
                        try
                        {
                            activityDist = float.Parse(activityDistStr);
                            activityDist = Math.Round(activityDist, 1);

                        }
                        catch
                        {
                            MessageBox.Show("Enter distance value for Activity distance, ex 12.3 (kilometers)");
                        }
                    }
                    string activity = ComboBoxActivityID.SelectedValue.ToString();

                    if (activity == "")
                        MessageBox.Show("Selcet activity in the combobox list!");

                    else
                    {
                        string selectedDate = ListBoxDays.SelectedItem.ToString().Substring(0, 10);
                        string sqlQueryId = $"SELECT Id FROM day WHERE Date = @THESELECTEDDATE";

                        SqlConnection connId = SqlConnect();

                        SqlCommand commandId = new SqlCommand(sqlQueryId, connId);
                        commandId.Parameters.AddWithValue("@THESELECTEDDATE", selectedDate);

                        string selectedDayIdStr = default(string);
                        int selectedDayId = 1; //default(int);

                        try
                        {
                            connId.Open();
                            SqlDataReader readerId = commandId.ExecuteReader();

                            while (readerId.Read())
                            {
                                for (int i = 0; i < readerId.FieldCount; i++)
                                {
                                    selectedDayIdStr = readerId[i].ToString();
                                }
                            }

                            selectedDayId = int.Parse(selectedDayIdStr);

                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            connId.Close();
                        }

                        string sqlQuery = $"INSERT INTO ActivityList (DayId, SessionId, Activity, ActivityTime, ActivityDist) VALUES (@SELECTEDDAYID, (SELECT ISNULL(MAX(SessionId) +1, 1) from ActivityList), @ACTIVITY, @THETIME, @ACTIVITYDIST)";

                        SqlConnection conn = SqlConnect();

                        SqlCommand command = new SqlCommand(sqlQuery, conn);

                        command.Parameters.AddWithValue("@SELECTEDDAYID", selectedDayId);
                        command.Parameters.AddWithValue("@ACTIVITY", activity);
                        command.Parameters.AddWithValue("@THETIME", activityTime);
                        command.Parameters.AddWithValue("@ACTIVITYDIST", activityDist);
                        
                        try
                        {
                            conn.Open();
                            SqlDataReader reader = command.ExecuteReader();
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            conn.Close();
                        }

                        ListBoxActivities.Items.Add(activity);

                        TextBoxActivityTime.Text = "";
                        TextBoxActivityDistance.Text = "";

                    }

                }, null);
            });
        }

        private void ListBoxDays_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    if (ListBoxDays.SelectedItem != null)
                    {
                        ButtonAddDay.IsEnabled = false;

                        string date = ListBoxDays.SelectedItem.ToString().Substring(0, 10);

                        LabelDate.Content = date;

                        ButtonAddActivity.IsEnabled = true;
                        ComboBoxActivityID.IsEnabled = true;
                        TextBoxActivityTime.IsEnabled = true;
                        TextBoxActivityDistance.IsEnabled = true;
                        ButtonDeleteDay.IsEnabled = true;
                        ButtonUpdateDay.IsEnabled = true;
                        TextBoxDayWeight.IsEnabled = true;
                        ButtonUpdateActivity.IsEnabled = false;
                        ButtonDeleteActivity.IsEnabled = false;
                        ComboBoxActivityID.SelectedIndex = 0;
                        TextBoxActivityTime.Text = "";
                        TextBoxActivityDistance.Text = "";

                        string sqlQueryW = "SELECT DayWeight FROM day WHERE Date = @THEDATE";

                        SqlConnection connW = SqlConnect();

                        SqlCommand commandW = new SqlCommand(sqlQueryW, connW);
                        commandW.Parameters.AddWithValue("@THEDATE", date);

                        try
                        {
                            TextBoxDayWeight.Text = "";
                            connW.Open();
                            SqlDataReader reader = commandW.ExecuteReader();

                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    TextBoxDayWeight.Text = reader[i].ToString();
                                }
                            }
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            connW.Close();
                        }

                        ListBoxActivities.Items.Clear();

                        string sqlQuery = "SELECT Activity FROM ActivityList " +
"INNER JOIN day ON ActivityList.DayId = day.Id WHERE day.Date = @THEDATE";

                        SqlConnection conn = SqlConnect();

                        SqlCommand command = new SqlCommand(sqlQuery, conn);
                        command.Parameters.AddWithValue("@THEDATE", date);

                        try
                        {
                            conn.Open();
                            SqlDataReader reader = command.ExecuteReader();

                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    ListBoxActivities.Items.Add(reader[i]);
                                }
                            }
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            conn.Close();
                        }

                        string dayWeight = TextBoxDayWeight.Text;

                        LabelSummaryDay.Content = $"Date: {date} , Day Weight: {dayWeight}";

                        ListBoxSummaryActivities.Items.Clear();
                        string sqlQueryA = "SELECT Activity, ActivityTime, ActivityDist FROM ActivityList " +
            "INNER JOIN day ON ActivityList.DayId = day.Id WHERE day.Date = @THEDATE";

                        SqlConnection connA = SqlConnect();

                        SqlCommand commandA = new SqlCommand(sqlQueryA, connA);
                        commandA.Parameters.AddWithValue("@THEDATE", date);

                        try
                        {
                            connA.Open();
                            SqlDataReader readerA = commandA.ExecuteReader();

                            while (readerA.Read())
                            {
                                for (int i = 0; i < readerA.FieldCount; i++)
                                {
                                    ListBoxSummaryActivities.Items.Add(readerA[i]);
                                }
                            }
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            connA.Close();
                        }

                    }
                    else
                    {
                        LabelDate.Content = string.Empty;
                    }
                }, null);

            });

        }


        private List<string> DayList(ListBox lb)
        {

            List<string> objectName = new List<string>();
            for (int i = 0; i < lb.Items.Count; i++)
            {
                objectName.Add(lb.Items.GetItemAt(i).ToString());
            }

            return objectName;
        }

        public SqlConnection SqlConnect()
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString =
                @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ActivityLog;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            return conn;
        }

        private void ButtonDeleteDay_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    if (ListBoxDays.SelectedItem != null)
                    {
                        MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Confirm delete", System.Windows.MessageBoxButton.YesNo);

                        if (messageBoxResult == MessageBoxResult.Yes)
                        {

                            string date = ListBoxDays.SelectedItem.ToString().Substring(0, 10);

                            string sqlQuery = $"DELETE FROM day WHERE Date = @THEDATE";


                            SqlConnection conn = SqlConnect();

                            SqlCommand command = new SqlCommand(sqlQuery, conn);
                            command.Parameters.AddWithValue("@THEDATE", date);


                            string sqlQueryS = "SELECT Id FROM day WHERE Date = @THEDATE";
                            SqlConnection connS = SqlConnect();

                            SqlCommand commandS = new SqlCommand(sqlQueryS, connS);

                            commandS.Parameters.AddWithValue("@THEDATE", date);

                            try
                            {
                                conn.Open();
                                SqlDataReader reader = command.ExecuteReader();
                                ListBoxDays.Items.Remove(date);
                            }
                            catch (SqlException ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            finally
                            {
                                conn.Close();
                            }
                        }
                        else
                        {
                            LabelDate.Content = string.Empty;
                        }
                    }
                }, null);

            });
        }

        private void ButtonUpdateDay_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    List<string> daysList = DayList(ListBoxDays);

                    string dateListBox = ListBoxDays.SelectedItem.ToString().Substring(0, 10);

                    daysList.Remove(dateListBox);

                    if (daysList.Contains(LabelDate.Content))
                    {
                        MessageBox.Show("This Date already exists!");
                    }

                    else
                    {
                        string dayWeightStr = TextBoxDayWeight.Text;
                        double dayWeight = 0;

                        if (dayWeightStr != "")
                        {
                            try
                            {
                                dayWeight = float.Parse(dayWeightStr);
                                dayWeight = Math.Round(dayWeight, 1);
                            }
                            catch
                            {
                                MessageBox.Show("Enter float value for Day Weight, ex 85.5");
                            }
                        }
                        string selectedDate = ListBoxDays.SelectedItem.ToString().Substring(0, 10);
                        string sqlQueryId = $"SELECT Id FROM day WHERE Date = @SELECTEDDATE";

                        SqlConnection connId = SqlConnect();

                        SqlCommand commandId = new SqlCommand(sqlQueryId, connId);
                        commandId.Parameters.AddWithValue("@SELECTEDDATE", selectedDate);

                        string selectedDayIdStr = default(string);
                        int selectedDayId = 1; //default(int);

                        try
                        {
                            connId.Open();
                            SqlDataReader readerId = commandId.ExecuteReader();

                            while (readerId.Read())
                            {
                                for (int i = 0; i < readerId.FieldCount; i++)
                                {
                                    selectedDayIdStr = readerId[i].ToString();
                                }
                            }

                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            connId.Close();
                        }
                        selectedDayId = int.Parse(selectedDayIdStr);

                        //DateTime selectedDate = CalenderDays.SelectedDate.Value;
                        string date = LabelDate.Content.ToString();

                        // string date = CalenderDays.SelectedDate.Value.ToString();

                        string sqlQuery = $"UPDATE day SET Date = @THEDATE, DayWeight = @DAYWEIGHT WHERE Id = @SELECTEDDAYID";

                        SqlConnection conn = SqlConnect();

                        SqlCommand command = new SqlCommand(sqlQuery, conn);
                        command.Parameters.AddWithValue("@THEDATE", date);
                        command.Parameters.AddWithValue("@DAYWEIGHT", dayWeight);
                        command.Parameters.AddWithValue("@SELECTEDDAYID", selectedDayId);
                        try
                        {
                            conn.Open();
                            SqlDataReader reader = command.ExecuteReader();

                            //ListBoxDays.Items.Add(date);
                            SortListBoxAscending(ListBoxDays);
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }

                }, null);

            });
        }

        private void SortListBoxAscending(ListBox lb)
        {
            List<string> dates = new List<string>();

            for (int i = 0; i < lb.Items.Count; i++)
                dates.Add(lb.Items.GetItemAt(i).ToString());

            lb.Items.Clear();

            var querySort = from date in dates
                            orderby date ascending
                            select date;

            foreach (var x in querySort)
                lb.Items.Add(x);
        }

        private void ListBoxActivities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    if (ListBoxActivities.SelectedItem != null)
                    {
                        ButtonAddDay.IsEnabled = false;

                        string activity = ListBoxActivities.SelectedItem.ToString();
                        string date = ListBoxDays.SelectedItem.ToString().Substring(0, 10);

                        ButtonAddActivity.IsEnabled = true;
                        ButtonUpdateActivity.IsEnabled = true;
                        ButtonDeleteActivity.IsEnabled = true;
                        ComboBoxActivityID.IsEnabled = true;
                        TextBoxActivityTime.IsEnabled = true;
                        TextBoxActivityDistance.IsEnabled = true;

                        string selectedDate = ListBoxDays.SelectedItem.ToString().Substring(0, 10);

                        string sqlQueryId = $"SELECT Id FROM day WHERE Date = @SELECTEDDATE";

                        SqlConnection connId = SqlConnect();

                        SqlCommand commandId = new SqlCommand(sqlQueryId, connId);
                        commandId.Parameters.AddWithValue("@SELECTEDDATE", selectedDate);

                        string selectedDayIdStr = default(string);
                        int selectedDayId = 1; //default(int);

                        try
                        {
                            connId.Open();
                            SqlDataReader readerId = commandId.ExecuteReader();

                            while (readerId.Read())
                            {
                                for (int i = 0; i < readerId.FieldCount; i++)
                                {
                                    selectedDayIdStr = readerId[i].ToString();
                                }
                            }

                            selectedDayId = int.Parse(selectedDayIdStr);

                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            connId.Close();
                        }

                        string sqlQueryT = $"SELECT ActivityTime FROM ActivityList WHERE DayId = @SELECTEDDAYID";

                        SqlConnection connT = SqlConnect();

                        SqlCommand commandT = new SqlCommand(sqlQueryT, connT);
                        commandT.Parameters.AddWithValue("@SELECTEDDAYID", selectedDayId);

                        try
                        {
                            TextBoxActivityTime.Text = "";
                            connT.Open();
                            SqlDataReader reader = commandT.ExecuteReader();

                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    TextBoxActivityTime.Text = reader[i].ToString();
                                }
                            }
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            connT.Close();
                        }

                        string sqlQueryD = $"SELECT ActivityDist FROM ActivityList WHERE DayId = @SELECTEDDAYID";

                        SqlConnection connD = SqlConnect();

                        SqlCommand commandD = new SqlCommand(sqlQueryD, connD);
                        commandD.Parameters.AddWithValue("@SELECTEDDAYID", selectedDayId);


                        try
                        {
                            TextBoxActivityDistance.Text = "";
                            connD.Open();
                            SqlDataReader reader = commandD.ExecuteReader();

                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    TextBoxActivityDistance.Text = reader[i].ToString();
                                }
                            }
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            connT.Close();
                        }

                        if (activity == "")
                        {
                            ComboBoxActivityID.SelectedIndex = 0;
                        }
                        else if (activity == "Cycling")
                        {
                            ComboBoxActivityID.SelectedIndex = 1;
                        }

                        else if (activity == "Running")
                        {
                            ComboBoxActivityID.SelectedIndex = 2;
                        }

                        else if (activity == "Swiming")
                        {
                            ComboBoxActivityID.SelectedIndex = 3;
                        }

                        else if (activity == "Walking")
                        {
                            ComboBoxActivityID.SelectedIndex = 4;
                        }

                        ListBoxSummaryActivities.Items.Clear();
                        string sqlQueryA = "SELECT Activity, ActivityTime, ActivityDist FROM ActivityList " +
            "INNER JOIN day ON ActivityList.DayId = day.Id WHERE day.Date = @THEDATE";

                        SqlConnection connA = SqlConnect();

                        SqlCommand commandA = new SqlCommand(sqlQueryA, connA);
                        commandA.Parameters.AddWithValue("@THEDATE", date);

                        try
                        {
                            connA.Open();
                            SqlDataReader readerA = commandA.ExecuteReader();

                            while (readerA.Read())
                            {
                                for (int i = 0; i < readerA.FieldCount; i++)
                                {
                                    ListBoxSummaryActivities.Items.Add(readerA[i]);
                                }
                            }
                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            connA.Close();
                        }
                    }

                    else
                    {
                        LabelDate.Content = string.Empty;
                    }


                }, null);

            });
        }
       
        private void ButtonDeleteActivity_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    if (ListBoxActivities.SelectedItem != null)
                    {
                        MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Confirm delete", System.Windows.MessageBoxButton.YesNo);

                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            string activity = ListBoxActivities.SelectedItem.ToString();

                            string sqlQueryId = $"SELECT ActivityId FROM ActivityList WHERE Activity = @ACTIVITY";

                            SqlConnection connId = SqlConnect();

                            SqlCommand commandId = new SqlCommand(sqlQueryId, connId);
                            commandId.Parameters.AddWithValue("@ACTIVITY", activity);


                            string selectedActivityStr = default(string);
                            int selectedActivityId = 1; //default(int);

                            try
                            {
                                connId.Open();
                                SqlDataReader readerId = commandId.ExecuteReader();

                                while (readerId.Read())
                                {
                                    for (int i = 0; i < readerId.FieldCount; i++)
                                    {
                                        selectedActivityStr = readerId[i].ToString();
                                    }
                                }

                                selectedActivityId = int.Parse(selectedActivityStr);

                            }
                            catch (SqlException ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            finally
                            {
                                connId.Close();
                            }

                            string sqlQuery = $"DELETE FROM ActivityList WHERE ActivityId = @SELECTEDACTIVITYID";

                            SqlConnection conn = SqlConnect();

                            SqlCommand command = new SqlCommand(sqlQuery, conn);
                            command.Parameters.AddWithValue("@SELECTEDACTIVITYID", selectedActivityId);

                            try
                            {
                                conn.Open();
                                SqlDataReader reader = command.ExecuteReader();
                                ListBoxActivities.Items.Remove(activity);
                            }
                            catch (SqlException ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            finally
                            {
                                conn.Close();
                            }
                        }
                        else
                        {
                            LabelDate.Content = string.Empty;
                        }
                    }
                }, null);

            });
        }

        private void ButtonUpdateActivity_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    string activityTimeStr = TextBoxActivityTime.Text;

                    TimeSpan activityTime = default(TimeSpan);

                    if (activityTimeStr != "")
                    {
                        try
                        {
                            activityTime = TimeSpan.Parse(activityTimeStr);

                        }
                        catch
                        {
                            MessageBox.Show("Enter timespan value for Activity time, ex 2:23:45 (Hours:Minutes:Seconds)");
                        }
                    }

                    string activityDistStr = TextBoxActivityDistance.Text;
                    activityDistStr = Regex.Replace(activityDistStr, ",", ".");

                    double activityDist = 0;

                    if (activityDistStr != "")
                    {
                        try
                        {
                            activityDist = float.Parse(activityDistStr);
                            activityDist = Math.Round(activityDist, 1);
                        }
                        catch
                        {
                            MessageBox.Show("Enter distance value for Activity distance, ex 12.3 (kilometers)");
                        }
                    }
                    string activity = ComboBoxActivityID.SelectedValue.ToString();

                    if (activity == "")
                        MessageBox.Show("Selcet activity in the combobox list!");

                    else
                    {
                        string sqlQueryId = $"SELECT ActivityId FROM ActivityList WHERE Activity = @ACTIVITY";

                        SqlConnection connId = SqlConnect();

                        SqlCommand commandId = new SqlCommand(sqlQueryId, connId);
                        commandId.Parameters.AddWithValue("@ACTIVITY", activity);

                        string selectedDayIdStr = default(string);
                        int selectedDayId = 1; //default(int);

                        try
                        {
                            connId.Open();
                            SqlDataReader readerId = commandId.ExecuteReader();

                            while (readerId.Read())
                            {
                                for (int i = 0; i < readerId.FieldCount; i++)
                                {
                                    selectedDayIdStr = readerId[i].ToString();
                                }
                            }

                            selectedDayId = int.Parse(selectedDayIdStr);

                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            connId.Close();
                        }

                        string sqlQuery = $"UPDATE ActivityList SET Activity = @ACTIVITY, ActivityTime = @THETIME, ActivityDist = @ACTIVITYDIST WHERE ActivityId = @SELECTEDDAYID";

                        SqlConnection conn = SqlConnect();

                        SqlCommand command = new SqlCommand(sqlQuery, conn);
                        command.Parameters.AddWithValue("@ACTIVITY", activity);
                        command.Parameters.AddWithValue("@THETIME", activityTime);
                        command.Parameters.AddWithValue("@ACTIVITYDIST", activityDist);
                        command.Parameters.AddWithValue("@SELECTEDDAYID", selectedDayId);
                        try
                        {
                            conn.Open();
                            SqlDataReader reader = command.ExecuteReader();

                            conn.Close();

                        }
                        catch (SqlException ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }


                }, null);

            });
        }
    }
}
