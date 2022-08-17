using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace BreakthroughWPF
{
    [ServiceContract(Namespace = "http://BreakthroughWPF", SessionMode=SessionMode.Required)]
    public interface IGameConnectionService
    {
        [OperationContract]
        bool IsAlive();

        [OperationContract]
        bool Invite(string nickName, string endpoint);

        [OperationContract]
        string InviteWelcome(string nickName, string endpoint);

        [OperationContract]
        ComputerAIPlayer.SMove NextMove(ComputerAIPlayer.SMove opponentMove);
    }
}
