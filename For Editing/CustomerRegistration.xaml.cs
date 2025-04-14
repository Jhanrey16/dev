using System.Windows;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Themes;
using HandyControl.Tools;
using MahApps.Metro.IconPacks;
using DerkAndresBoardingHauzManagementSystem.Properties;
using NewMessageBox = HandyControl.Controls.MessageBox;
using System.Globalization;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media.Animation;
using CurrencyTextBoxControl;
using System.Data.SqlClient;


namespace DerkAndresBoardingHauzManagementSystem.Core.Pages
{
    /// <summary>
    /// Interaction logic for CustomerRegistration.xaml
    /// </summary>
    public partial class CustomerRegistration : Page
    {

        private int progressIncrement = 100 / 20;
        private int currentProgress;
        private int filledCount;

        public CustomerRegistration()
        {
            InitializeComponent();
            ConfigHelper.Instance.SetLang("en");

            filledCount = 0;
            currentProgress = 0;
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            filledCount = 0;

            foreach (var child in Utilities.VisualChildren.Get<HandyControl.Controls.TextBox>(MainGrid))
            {
                if (child is HandyControl.Controls.TextBox textBox && !string.IsNullOrEmpty(textBox.Text)
                    && !textBox.Text.Contains('_') && textBox.IsEnabled && textBox.Text == "0")
                {
                    filledCount++;
                }
            }

            UpdateProgressBar(filledCount);
        }

        private void InputDatePicker_TextChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var child in Utilities.VisualChildren.Get<HandyControl.Controls.DatePicker>(MainGrid))
            {
                if (child is HandyControl.Controls.DatePicker datePicker && !string.IsNullOrEmpty(datePicker.SelectedDate.ToString())
                    && !datePicker.SelectedDate.ToString().Contains('_'))
                {
                    filledCount++;
                }
            }

            UpdateProgressBar(filledCount);
        }

        private void UpdateProgressBar(int filledCount)
        {
            currentProgress = filledCount * progressIncrement;

            if (MainProgressBar.Value < (int)MainProgressBar.Maximum)
            {
                HandyControl.Controls.VisualElement.SetText(MainProgressBar, $"{currentProgress}%".ToString());
                MainProgressBar.Style = (Style)FindResource("ProgressBarInfo");
            }
            else
            {
                MainProgressBar.Style = (Style)FindResource("ProgressBarSuccess");
                HandyControl.Controls.VisualElement.SetText(MainProgressBar, "READY FOR SUBMISSION");
            }

            MainProgressBar.Value = currentProgress;
        }


        private void InputField_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is HandyControl.Controls.TextBox textBox)
                Utilities.TextBoxBehaviors.CapitalizeFirstLetter(textBox);

            if (MainProgressBar.Value < (int)MainProgressBar.Maximum)
            {
                HandyControl.Controls.VisualElement.SetText(MainProgressBar, $"{currentProgress}%".ToString());
                MainProgressBar.Style = (Style)FindResource("ProgressBarInfo");
                StartAnimation("end");
            }
            else
            {
                MainProgressBar.Style = (Style)FindResource("ProgressBarSuccess");
                HandyControl.Controls.VisualElement.SetText(MainProgressBar, "READY FOR SUBMISSION");
                StartAnimation("start");
            }
        }

        private void StartAnimation(string action)
        {
            Storyboard storyboard = (Storyboard)FindResource("ReadyAnimation");

            if (action == "start") storyboard.Begin();
            else storyboard.Stop();
        }

        private void DegreeField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is HandyControl.Controls.ComboBox comboBox)
            {
                ComboBoxItem comboBoxItem = (ComboBoxItem)comboBox.SelectedItem;

                if (comboBoxItem.Content.ToString() == "Strand")
                {
                    MajorField.IsEnabled = false;
                    InfoElement.SetPlaceholder(CourseField, Properties.Resources.StrandFieldDesc);
                    InfoElement.SetTitle(DegreeField, Properties.Resources.StrandFieldTitle);
                    InfoElement.SetPlaceholder(YearLevelField, "11/12");
                    InfoElement.SetTitle(YearLevelField, Properties.Resources.GradeLevel);
                    InfoElement.SetPlaceholder(MajorField, "N/A");
                }
                else if (MajorField.IsEnabled == false)
                {
                    MajorField.IsEnabled = true;
                    InfoElement.SetPlaceholder(CourseField, Properties.Resources.CourseFieldDesc);
                    InfoElement.SetTitle(DegreeField, Properties.Resources.CourseFieldTitle);
                    InfoElement.SetPlaceholder(YearLevelField, Properties.Resources.YearLevelFieldDesc);
                    InfoElement.SetTitle(YearLevelField, Properties.Resources.YearLevelFieldTitle);
                    InfoElement.SetPlaceholder(MajorField, Properties.Resources.MajorFieldDesc);
                }
            }
        }

        private void NoLandline_Checked(object sender, RoutedEventArgs e)
        {
            HomeLandlineField.IsEnabled = false;
            HomeLandlineField.Text = "N/A";
        }

        private void NoLandline_Unchecked(object sender, RoutedEventArgs e)
        {
            HomeLandlineField.IsEnabled = true;
            HomeLandlineField.Text = null;
        }

        private void CashRegister_TextChanged(object sender, TextChangedEventArgs e)
        {
            TotalAmountField.Text = TotalAmountFieldPop.Text;
            TenderField.Text = TenderFieldPop.Text;
            ChangeField.Text = ChangeFieldPop.Text;
        }

        private void CashRegister_PopUp(object sender, RoutedEventArgs e)
        {
            CashRegister.IsOpen = true;
            TenderFieldPop.Focus();
        }

        private void CashRegister_ClosePopUp(object sender, RoutedEventArgs e)
        {
            CashRegister.IsOpen = false;
        }

        private void NumpadButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                string buttonContent = button.Content.ToString();
                string value = TenderFieldPop.Text;
                string currencySymbol = string.IsNullOrEmpty(value) ? "" : value.Substring(0, 1);

                if (char.IsDigit(buttonContent[0]))
                {
                    string numericPart = string.IsNullOrEmpty(value) ? "0" : value.Substring(1).Replace(".", "").Trim();

                    string newValue = numericPart + buttonContent;

                    if (newValue.Length == 1)
                    {
                        newValue = "0" + newValue;
                    }
                    else if (newValue.Length > 2)
                    {
                        string wholePart = newValue.Substring(0, newValue.Length - 2);
                        string centsPart = newValue.Substring(newValue.Length - 2);
                        newValue = wholePart + "." + centsPart;
                    }
                    else
                    {
                        newValue = "0." + newValue;
                    }

                    decimal amount;
                    if (decimal.TryParse(newValue, out amount))
                    {
                        TenderFieldPop.Text = currencySymbol + amount.ToString("N2");
                    }
                    else
                    {
                        TenderFieldPop.Text = currencySymbol + "0.00";
                    }
                }
                else if (button.Name == "Backspace")
                {
                    if (value.Length > 0)
                    {
                        string newValue2 = value.Substring(0, value.Length - 1);

                        if (newValue2.Length == 0)
                        {
                            TenderFieldPop.Text = "";
                        }
                        else
                        {
                            if (newValue2.Length == 1 && newValue2[0] == '0')
                            {
                                TenderFieldPop.Text = currencySymbol + "0.00";
                            }
                            else
                            {
                                string numericPart = newValue2.Substring(1).Replace(".", "").Trim();
                                if (numericPart.Length == 0)
                                {
                                    TenderFieldPop.Text = currencySymbol + "0.00";
                                }
                                else
                                {
                                    if (numericPart.Length == 1)
                                    {
                                        TenderFieldPop.Text = currencySymbol + "0." + numericPart;
                                    }
                                    else if (numericPart.Length > 2)
                                    {
                                        string wholePart = numericPart.Substring(0, numericPart.Length - 2);
                                        string centsPart = numericPart.Substring(numericPart.Length - 2);
                                        TenderFieldPop.Text = currencySymbol + wholePart + "." + centsPart;
                                    }
                                    else
                                    {
                                        TenderFieldPop.Text = currencySymbol + "0." + numericPart;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (button.Name == "Clear")
                {
                    TenderFieldPop.Text = currencySymbol + "0.00";
                }
            }
        }
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
{
    string connectionString = "Data Source=localhost;Initial Catalog=BoardingHauzDB;Integrated Security=True";

    string query = "INSERT INTO Tenants (FullName, course, Yrlvl, major,Homeland, tender, change) VALUES (@FullName, @Course, @YearLevel, @Major, @Homeland, @Tender, @Change)";

    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        using (SqlCommand cmd = new SqlCommand(query, conn))
        {
            
            cmd.Parameters.AddWithValue("@FullName", FullNameField.Text);
            cmd.Parameters.AddWithValue("@Course", CourseField.Text);
            cmd.Parameters.AddWithValue("@YearLevel", YearLevelField.Text);
            cmd.Parameters.AddWithValue("@Major", MajorField.Text);
            cmd.Parameters.AddWithValue("@Homeland", HomeLandlineField.Text);
            cmd.Parameters.AddWithValue("@Tender", TenderField.Text);
            cmd.Parameters.AddWithValue("@Change",  ChangeField.Text);

            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
                NewMessageBox.Show("Customer Registered Successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                NewMessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

    }
}
