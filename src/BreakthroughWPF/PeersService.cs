using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows;
using BreakthroughWPF;
using System.Windows.Controls;
using System.ServiceModel.Channels;

namespace BreakthroughWPF
{
    [ServiceContract(Namespace = "http://BreakthroughWPF", CallbackContract = typeof(IPeers))]
    public interface IPeers
    {
        [OperationContract(IsOneWay = true)]
        void Join(string member, string suffix, string endpoint);

        [OperationContract(IsOneWay = true)]
        void Leave(string member, string suffix, string endpoint);

        [OperationContract(IsOneWay = true)]
        void Alive(string member, string suffix, string endpoint);
    }

    public interface IPeersChannel : IPeers, IClientChannel
    {
    }

    public class PeersService : IPeers
    {
        // member id for this instance
        string member;
        string suffix;
        ParticipantsWindow.addPlayerDelegate add;
        ParticipantsWindow.delPlayerDelegate del;
        ParticipantsWindow playersWindow;

        public PeersService(string member, string suffix, ParticipantsWindow.addPlayerDelegate add, ParticipantsWindow.delPlayerDelegate del, ParticipantsWindow playersWindow)
        {
            this.member = member;
            this.suffix = suffix;
            this.add = add;
            this.del = del;
            this.playersWindow = playersWindow;
        }

        public void Join(string member, string suffix, string endpointAddress)
        {
            if (member != this.member || suffix != this.suffix)
            {
                add(member, suffix, endpointAddress);
                playersWindow.player.Alive(this.member, this.suffix, playersWindow.host.endpointAddress.Value);
            }
        }

        public void Leave(string member, string suffix, string endpointAddress)
        {
            if (member != this.member || suffix != this.suffix)
            {
                del(member, suffix, endpointAddress);
            }
        }

        public void Alive(string member, string suffix, string endpointAddress)
        {
            add(member, suffix, endpointAddress);
        }

    }
}
