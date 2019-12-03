using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Win32;

namespace SpeechRecognition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SpeechAsistant assistant;
        private ObservableCollection<NickNamePairs> dictonary;

        System.Windows.Forms.ContextMenu contextMenu;
        System.Windows.Forms.MenuItem menuItem;
        System.Windows.Forms.NotifyIcon notifyIcon;

        public string NickName { get; set; }
        public double Confidence { get; private set; } = 0.5;

        public MainWindow()
        {
            var currentProcName = Process.GetCurrentProcess().ProcessName;

            if (Process.GetProcessesByName(currentProcName).Length > 1)
            {
                MessageBox.Show("Voice Asistant already running!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Exclamation);

                this.Close();
                return;
            }

            InitializeComponent();

            this.Slider.Value = 50;
            this.NickName = NickNameLabel.Text;
            this.Confidence = Slider.Value / 100;

            contextMenu = new System.Windows.Forms.ContextMenu();
            menuItem = new System.Windows.Forms.MenuItem();

            contextMenu.MenuItems.AddRange(
                    new System.Windows.Forms.MenuItem[] { menuItem });

            menuItem.Index = 0;
            menuItem.Text = "Exit";
            menuItem.Click += (object sender, EventArgs args) =>
            {
                this.SaveAppState();
                this.Close();
            };
            
            notifyIcon = new System.Windows.Forms.NotifyIcon();

            notifyIcon.Icon = Properties.Resources.icon;
            notifyIcon.Visible = true;
            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.Click += (object sender, EventArgs args) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            };

            dictonary = new ObservableCollection<NickNamePairs>();
            ProgramsListBox.ItemsSource = dictonary;

            assistant = new SpeechAsistant(this);

            MainToggle.ToggleEvent += (object sender, bool e) =>
            {
                if (e)
                {
                    assistant.Reload();
                    assistant.StartListening();
                }
                else
                {
                    assistant.StopListening();
                }

                MainLabel.Content = e ? "On" : "Off";
            };

            SturtupToggle.ToggleEvent += (object sender, bool e) =>
            {
                var registryKey = Registry.CurrentUser.OpenSubKey
                        ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                var assembly = Assembly.GetExecutingAssembly().Location;

                if (e)
                {
                    registryKey.SetValue("VoiceAssistant by MostRush", assembly);
                }
                else
                {
                    registryKey.DeleteValue("VoiceAssistant by MostRush");
                }

                SturtupLabel.Content = e ? "On" : "Off";
            };

            this.MainToggle.Toggle();
            this.LoadAppState();
            this.Hide();
        }

        private async void SavedNotify()
        {
            SavedNotifyLabel.Visibility = Visibility.Visible;
            await Task.Delay(3000);
            SavedNotifyLabel.Visibility = Visibility.Hidden;
        }

        public object GetDictonary() => dictonary as object;

        private void SaveAppState()
        {
            RegistryKey keyState = Registry.CurrentUser.CreateSubKey(
                @"SOFTWARE\ByMostRush\VoiceAssist\State");

            keyState.SetValue("Confidence", this.Slider.Value);
            keyState.SetValue("AssistName", this.NickName);
            keyState.SetValue("OnStartUp", this.SturtupToggle.IsToggled);
            keyState.Close();

            RegistryKey keyLines = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\ByMostRush\VoiceAssist\", true);

            try
            {
                if (keyLines != null)
                    keyLines.DeleteSubKeyTree("Lines");
            } 
            catch { }

            keyLines = Registry.CurrentUser.CreateSubKey(
                @"SOFTWARE\ByMostRush\VoiceAssist\Lines");

            foreach (NickNamePairs item in dictonary)
            {
                keyLines.SetValue(item.Path, item.NickName);
            }

            keyLines.Close();
        }

        private void LoadAppState()
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey(
                @"SOFTWARE\ByMostRush\VoiceAssist");

                var state = key.OpenSubKey("State");
                this.Slider.Value = double.Parse(state.GetValue("Confidence") as string);
                this.NickNameLabel.Text = state.GetValue("AssistName") as string;

                if (state.GetValue("OnStartUp") as string == "True")
                    this.SturtupToggle.Toggle();

                var lines = key.OpenSubKey("Lines");
                string[] names = lines.GetValueNames();

                foreach (string fullName in names)
                {
                    string fileName = "";

                    if (!fullName.Contains("https://"))
                        fileName = Path.GetFileName(fullName);
                    else
                        fileName = fullName;

                    string nickName = lines.GetValue(fullName) as string;

                    dictonary.Add(new NickNamePairs(nickName, fullName, fileName));
                }

                this.NickName = NickNameLabel.Text;

                if (assistant != null)
                    assistant.Reload();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void MinimiseLauncher_Click(object sender, RoutedEventArgs e)
        {
            this.SaveAppState();
            this.Hide();
        }

        private void CloseLauncher_Click(object sender, RoutedEventArgs e)
        {
            notifyIcon.Dispose();
            this.SaveAppState();
            this.Close();
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            dictonary.Remove(ProgramsListBox.SelectedItem as NickNamePairs);

            if (assistant != null)
                assistant.Reload();

            this.SaveAppState();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < ProgramsListBox.Items.Count; i++)
            {
                ListBoxItem myListBoxItem = ProgramsListBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(myListBoxItem);
                DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
                TextBox target = myDataTemplate.FindName("NickNameTextBox", myContentPresenter) as TextBox;

                var pair = ProgramsListBox.Items[i] as NickNamePairs;
                pair.NickName = target.Text;
            }

            if (assistant != null)
                assistant.Reload();

            this.SaveAppState();
            this.SavedNotify();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var contextMenu = button.ContextMenu;

            contextMenu.PlacementTarget = button;
            contextMenu.IsOpen = true;
            e.Handled = true;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ConfidenceLabel.Content = e.NewValue;
            this.Confidence = e.NewValue / 100;
        }

        private void SaveNameButton_Click(object sender, RoutedEventArgs e)
        {
            this.NickName = NickNameLabel.Text;

            if (assistant != null)
                assistant.Reload();

            this.SaveAppState();
        }

        private void ContextLink_Click(object sender, RoutedEventArgs e)
        {
            var linkWindow = new InputLink();
            linkWindow.ShowDialog();

            if (linkWindow.DialogResult == true)
            {
                dictonary.Add(linkWindow.GetNickNamePairs() as NickNamePairs);
            }
        }

        private void ContextPath_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();

            if (fileDialog.ShowDialog() == true)
            {
                string fullName = fileDialog.FileName;
                string fileName = Path.GetFileName(fileDialog.FileName);

                dictonary.Add(new NickNamePairs("", fullName, fileName));
            }
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj)
             where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
    }

    class NickNamePairs
    {
        public string Path { get; set; }
        public string NickName { get; set; }
        public string FileName { get; set; }

        public NickNamePairs(string nickName, string path, string fileName)
        {
            Path = path;
            NickName = nickName;
            FileName = fileName;
        }

        public NickNamePairs() { }
    }
}
