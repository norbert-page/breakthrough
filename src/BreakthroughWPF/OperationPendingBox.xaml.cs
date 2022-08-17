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
using System.ServiceModel;

namespace BreakthroughWPF
{
    /// <summary>
    /// Interaction logic for OperationPendingBox.xaml
    /// </summary>
    public partial class OperationPendingBox : Window
    {
        public Window1 host;
        public bool response;
        public bool error;

        public delegate bool NoArgDelegate();
        public delegate void NoArgDelegateV();

        public OperationPendingBox(Window1 host, string message)
        {
            InitializeComponent();
            messageLabel.Content = message;
            this.host = host;
            error = false;

            NoArgDelegate DoWork = new NoArgDelegate(Invite);
            DoWork.BeginInvoke(new AsyncCallback(Completed), new bool());
        }

        public void Completed(IAsyncResult result)
        {
            this.Dispatcher.BeginInvoke(new NoArgDelegateV(this.Close));
            if (!error)
            {
                if (!response) MessageBox.Show("Your invitation was not accepted.", "Not accepted", MessageBoxButton.OK, MessageBoxImage.Information);
                else MessageBox.Show("Your invitation was accepted.", "Accepted", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("This player could not be invited. Player went offline or there was a communication error.", "Inaccesible player", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public bool Invite()
        {
            try
            {
                host.GameConnectionClient.Open();
                response = host.GameConnectionClient.Invite(host.memberID, host.endpointAddress.Value);
                if (!response) host.GameConnectionClient.Close();
                return response;
            }
            catch (Exception)
            {
                host.GameConnectionClient.Abort();
                error = true;
                return false;
            }
        }

      
    }
}
