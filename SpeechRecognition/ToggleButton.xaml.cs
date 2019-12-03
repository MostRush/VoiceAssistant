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

namespace ToggleSwitch
{
    /// <summary>
    /// Interaction logic for ToggleButton.xaml
    /// </summary>
    public partial class ToggleButton : UserControl
    {
        Thickness LeftSide = new Thickness(-75, 0, 0, 0);
        Thickness RightSide = new Thickness(0, 0, -75, 0);

        SolidColorBrush BackColorOff1 = new SolidColorBrush(Color.FromRgb(50, 50, 50));
        SolidColorBrush BackColorOff2 = new SolidColorBrush(Color.FromRgb(38, 38, 38));

        SolidColorBrush DotColorOff = new SolidColorBrush(Color.FromRgb(64, 64, 64));
        SolidColorBrush DorColorOn = new SolidColorBrush(Color.FromRgb(73, 196, 238));

        public static readonly DependencyProperty ToggleEventDependency = DependencyProperty.Register(
            "ToggleEvent", typeof(EventHandler<bool>), typeof(ToggleButton));

        public event EventHandler<bool> ToggleEvent;
        
        public bool IsToggled { get; set; }

        public ToggleButton()
        {
            InitializeComponent();
            Dot.Fill = DotColorOff;
            Back.Fill = BackColorOff2;
            IsToggled = false;
            Dot.Margin = LeftSide;
        }

        private void Dot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Toggle();
        private void Back_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Toggle();

        public void Toggle()
        {
            if (!IsToggled)
            {
                Dot.Fill = DorColorOn;
                Front.Fill = DorColorOn;
                IsToggled = true;
                Dot.Margin = RightSide;
            }
            else
            {
                Dot.Fill = DotColorOff;
                Front.Fill = DotColorOff;
                IsToggled = false;
                Dot.Margin = LeftSide;
            }

            ToggleEvent.Invoke(this, IsToggled);
        }

        private void Grid_MouseEnter(object sender, MouseEventArgs e) => Back.Fill = BackColorOff1;
        private void Grid_MouseLeave(object sender, MouseEventArgs e) => Back.Fill = BackColorOff2;
    }
}
