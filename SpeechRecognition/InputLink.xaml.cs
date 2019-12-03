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
using System.Windows.Shapes;

namespace SpeechRecognition
{
    /// <summary>
    /// Interaction logic for InputLink.xaml
    /// </summary>
    public partial class InputLink : Window
    {
        private NickNamePairs nickNamePairs;

        public InputLink()
        {
            InitializeComponent();

            nickNamePairs = new NickNamePairs();

            this.NameLabel.Content = "Моя гиперссылка";
            this.LinkLabel.Content = "https://www.example.com";

            this.LayoutUpdated += Labels;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        public object GetNickNamePairs() => nickNamePairs as object;

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.NameTextBox.Text == "" | this.NameTextBox.Text == "")
            {
                MessageBox.Show("Поля не могут быть пустыми!", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            string fullLink = this.LinkTextBox.Text;

            if (!this.LinkTextBox.Text.Contains("https://"))
                fullLink = "https://" + this.LinkTextBox.Text;

            nickNamePairs.Path = fullLink;
            nickNamePairs.FileName = fullLink;
            nickNamePairs.NickName = this.NameTextBox.Text;
            
            this.DialogResult = true;
            this.Close();
        }

        private void Labels(object sender, EventArgs e)
        {
            if (this.NameTextBox.Text != "")
                this.NameLabel.Content = "";
            else
                this.NameLabel.Content = "Моя гиперссылка";

            if (this.LinkTextBox.Text != "")
                this.LinkLabel.Content = "";
            else
                this.LinkLabel.Content = "https://www.example.com";
        }
    }
}
