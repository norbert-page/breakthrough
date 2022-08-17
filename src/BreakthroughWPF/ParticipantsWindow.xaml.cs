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
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.PeerResolvers;
using System.ServiceModel.Channels;
using System.Windows.Threading;
using System.Net;
using System.Net.Sockets;
using BreakthroughWPF.Properties;
using System.Windows.Media.Animation;
using BreakthroughWPF.GameConnection;
using System.ServiceModel.Description;
using System.Threading;

namespace BreakthroughWPF
{
    /// <summary>
    /// Interaction logic for ParticipantsWindow.xaml
    /// </summary>
    public partial class ParticipantsWindow : Window, IDisposable
    {
        public Window1 host;

        ReaderWriterCustomLock<bool> isP2POpen;

        private DuplexChannelFactory<IPeersChannel> factory;
        public IPeersChannel player;

        public delegate void addPlayerDelegate(string id, string suffix, string ea);
        public delegate void delPlayerDelegate(string id, string suffix, string ea);
        private Dictionary<string, string> players;

        System.Threading.Timer refresher;

        public ParticipantsWindow(Window1 host, string memberID)
        {
            InitializeComponent();
            this.host = host;
            host.memberID = memberID;

            byte[] randomBytes = new byte[15];
            new Random().NextBytes(randomBytes);
            host.suffix = System.Convert.ToBase64String(randomBytes);

            players = new Dictionary<string,string>();

            isP2POpen = new ReaderWriterCustomLock<bool>(false);

            try
            {
                // prepare channel for p2p
                InstanceContext instanceContext = new InstanceContext(new PeersService(memberID, host.suffix, new addPlayerDelegate(playerJoined), new delPlayerDelegate(playerLeaved), this));
                factory = new DuplexChannelFactory<IPeersChannel>(instanceContext, "PeerEndpoint");
                player = factory.CreateChannel();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + "\n\nINNER:\n " + (e.InnerException != null ? e.InnerException.Message : "none"));
                
            }
            /*
            // find first IPv4 address, ignore all IPv6 addresses
            IPAddress[] addr = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            for (int i = 0; i < addr.Length; ++i)
            {
                if (addr[i].AddressFamily.ToString() == ProtocolFamily.InterNetwork.ToString()){
                    host.hostIP = new ReaderWriterCustomLock<IPAddress>(addr[i]);
                    host.endpointAddress = new ReaderWriterCustomLock<string>(String.Format(
                        Settings.Default.endpointAddressPattern,
                        host.hostIP.Value, Settings.Default.defaultGameConnectionServicePort));
                    break;
                }
            }
            */

            host.endpointAddress = new ReaderWriterCustomLock<string>(string.Format(
                Settings.Default.endpointAddressPattern + host.memberID + host.suffix,
                Dns.GetHostName(),
                Settings.Default.defaultGameConnectionServicePort));

            // escape the endpointAddress
            host.endpointAddress.Value = new Uri(host.endpointAddress.Value).ToString();


            // prepare Tcp direct connection, start the service
            if (host.GameConnectionHost == null)
            {
                NetTcpBinding binding = new NetTcpBinding();
                binding.PortSharingEnabled = true;

                host.GameConnectionHost = new ServiceHost(typeof(GameConnectionService));
                host.GameConnectionHost.AddServiceEndpoint(typeof(IGameConnectionService), binding, host.endpointAddress.Value);

                /* 
                 * This is for development/testing purposes only
                 *
                System.ServiceModel.Channels.Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();

                ServiceMetadataBehavior metadataBehavior; 
                metadataBehavior = host.GameConnectionHost.Description.Behaviors.Find<ServiceMetadataBehavior>();
                if (metadataBehavior == null)
                {
                    metadataBehavior = new ServiceMetadataBehavior();
                    host.GameConnectionHost.Description.Behaviors.Add(metadataBehavior);
                }

                host.GameConnectionHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, "net.tcp://yey:8888/mex");
                //host.GameConnectionHost.AddServiceEndpoint(typeof(IMetadataExchange), mexBinding, host.endpointAddress.Value + "/mex");
                */
                 
                try { host.GameConnectionHost.Open(); }
                catch (CommunicationException ce)
                {
                    MessageBox.Show(ce.Message);
                    host.GameConnectionHost.Abort();
                    host.GameConnectionHost = null;
                }
            }

        }

        public void Dispose()
        {
            try
            {
                player.Leave(host.memberID, host.suffix, host.endpointAddress.Value);
            }
            catch (Exception) { };
            if (refresher != null) refresher.Dispose();
            if (player != null)
            {
                    if (player.State == CommunicationState.Faulted) player.Abort();
                    else { player.BeginClose(null, null); }
            }
        }

        public delegate void newPlayersListBoxDel(string id, string suf);
        public void newPlayersListBox(string id, string suf)
        {
            playersListBox.Items.Add(id + " (" + suf + ")");
        }

        public delegate void delPlayersListBoxDel(string id, string suf);
        public void delPlayersListBox(string id, string suf)
        {
            playersListBox.Items.Remove(id + " (" + suf + ")");
        }

        public delegate void clearPlayersListBoxDel();
        public void clearPlayersListBox()
        {
            playersListBox.Items.Clear();
        }

        public void playerJoined(string id, string suf, string ea)
        {
            if (players.ContainsKey(id + " (" + suf + ")") || (id == host.memberID && suf == host.suffix)) return;
            players.Add(id + " (" + suf + ")", ea);
            playersListBox.Dispatcher.BeginInvoke(new newPlayersListBoxDel(newPlayersListBox), id, suf);
        }

        public void playerLeaved(string id, string suf, string ea)
        {
            if (players.ContainsKey(id + " (" + suf + ")"))
            {
                players.Remove(id + " (" + suf + ")");
                playersListBox.Dispatcher.BeginInvoke(new delPlayersListBoxDel(delPlayersListBox), id, suf);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            player.Opening += new EventHandler(player_Opening);
            player.Opened += new EventHandler(player_Opened);
            player.Closing += new EventHandler(player_Closing);

            try
            {
                player.BeginOpen(null, new object());
            }
            catch (CommunicationException ce)
            {
                MessageBox.Show(ce.Message);
            }
        }

        void player_Closing(object sender, EventArgs e)
        {
            isP2POpen.Value = false;
            this.statusLabel.Dispatcher.BeginInvoke(new UpdateStatusLabelDelegate(changeStatusLabel), "Offline");
        }

        public delegate void UpdateStatusLabelDelegate(String newLabel);
        void changeStatusLabel(String newLabel)
        {
            statusLabel.Content = newLabel;
        }

        public delegate void animateRefreshLabelDelegate();
        void animateRefreshLabel()
        {
            DoubleAnimation RefreshAnimation = new DoubleAnimation();
            RefreshAnimation.From = 0.5;
            RefreshAnimation.To = 1;
            RefreshAnimation.AutoReverse = true;
            RefreshAnimation.RepeatBehavior = RepeatBehavior.Forever;
            RefreshAnimation.DecelerationRatio = 1;
            RefreshAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(250));

            discoveryLabel.BeginAnimation(Label.OpacityProperty, RefreshAnimation);
        }

        void keepAlive(Object state)
        {
            player.Alive(host.memberID, host.suffix, host.endpointAddress.Value);
        }

        public delegate void TimerCallbackDel(Object state);
        void player_Opened(object sender, EventArgs e)
        {
            isP2POpen.Value = true;
            this.statusLabel.Dispatcher.BeginInvoke(new UpdateStatusLabelDelegate(changeStatusLabel), "Online (Breakthrough network)");
            this.discoveryLabel.Dispatcher.BeginInvoke(new animateRefreshLabelDelegate(animateRefreshLabel));

            player.Join(host.memberID, host.suffix, host.endpointAddress.Value);

            // improves PNRP discovery in case of a very small number of peers, small period value because of small number of users expected ;)
            refresher = 
                new System.Threading.Timer(new System.Threading.TimerCallback(keepAlive), null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(2));
        }

        void player_Opening(object sender, EventArgs e)
        {
            this.statusLabel.Dispatcher.BeginInvoke(new UpdateStatusLabelDelegate(changeStatusLabel), "Connecting, please wait...");
        }

        private void inviteButton_Click(object sender, RoutedEventArgs e)
        {
            if (isP2POpen.Value)
            {
                if (playersListBox.SelectedIndex == -1)
                {
                    MessageBox.Show("You must choose a remote player from the list. Please check your connection if there are no players on the list or wait longer for discovery purposes.", "Please select a player.", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string chosen = playersListBox.Items[playersListBox.SelectedIndex].ToString();

                NetTcpBinding binding = new NetTcpBinding();
                binding.PortSharingEnabled = true;

                try { host.GameConnectionClient = new GameConnectionServiceClient(binding, new EndpointAddress(new Uri(players[chosen]))); }
                catch (CommunicationException ce)
                {
                    host.GameConnectionClient.Abort();
                    MessageBox.Show(ce.Message, "Communication error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                OperationPendingBox msg = new OperationPendingBox(host, "Waiting for remote player's confirmation...");
                msg.Owner = this;
                msg.ShowDialog();

                if (host.GameConnectionClient.State == CommunicationState.Opened)
                {
                    // connection with the client was successfull and the invitation was accepted
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
                    host.Game = new Game(host.BoardCanvas, host, true, false, true);
                    if (host.Game != null) host.Log("New network game started.");
                    else host.Log("New network game cannot be started.");
                    this.Close();
                }
            }
            else MessageBox.Show("You are not connected to the Breakthrough network - cannot invite.");
        }

        private void restartButton_Click(object sender, RoutedEventArgs e)
        {
            players.Clear();
            playersListBox.Dispatcher.BeginInvoke(new clearPlayersListBoxDel(clearPlayersListBox));
            if (isP2POpen.Value) player.Join(host.memberID, host.suffix, host.endpointAddress.Value);
        }
    }
}
