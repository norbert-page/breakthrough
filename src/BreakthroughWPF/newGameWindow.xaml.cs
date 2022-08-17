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
using System.Threading;

namespace BreakthroughWPF
{
    /// <summary>
    /// Interaction logic for newGameWindow.xaml
    /// </summary>
    public partial class newGameWindow : Window
    {
        private Window1 host;

        public newGameWindow(Window1 host)
        {
            InitializeComponent();
            this.host = host;
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            host.pause.IsEnabled = true;
            host.resume.IsEnabled = false;
            host.undo.IsEnabled = false;
            host.redo.IsEnabled = false;
            host.Game.State.Value |= GameState.Paused;
            host.movesExpanderPlayerWhite.Items.Clear();
            host.movesExpanderPlayerBlack.Items.Clear();
            host.BoardCanvas.Children.Clear();
            host.Game.PlayerBlack.Terminate.Value = true;
            host.Game.PlayerWhite.Terminate.Value = true;
            host.Game = new Game(host.BoardCanvas, host, radioButton4.IsChecked.GetValueOrDefault(), radioButton2.IsChecked.GetValueOrDefault(), false);
            if (host.Game != null) host.Log("New game started.");
            else host.Log("New game cannot be started.");
            this.Close();
        }
    }
}
