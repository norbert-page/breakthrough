using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BreakthroughWPF
{
    /// <summary>
    /// Interaction logic for NewNetworkGameWindow.xaml
    /// </summary>
    public partial class NewNetworkGameWindow : Window
    {
        private Window1 host;

        public NewNetworkGameWindow(Window1 host)
        {
            InitializeComponent();
            this.host = host;
            this.nickNameTextBox.Focus();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            if (nickNameTextBox.Text == "")
            {
                MessageBox.Show(this, "Nickname field cannot be empty.", "Mandatory field", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            this.Close();
            string memberID = this.nickNameTextBox.Text;
            host.choosePlayerWindow = new ParticipantsWindow(host, memberID);
            host.choosePlayerWindow.Owner = host;
            host.choosePlayerWindow.ShowDialog();
        }
    }
}
