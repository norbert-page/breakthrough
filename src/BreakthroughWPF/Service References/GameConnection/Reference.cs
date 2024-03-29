﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.3074
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BreakthroughWPF.GameConnection {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="ComputerAIPlayer.SMove", Namespace="http://schemas.datacontract.org/2004/07/BreakthroughWPF")]
    [System.SerializableAttribute()]
    public partial struct ComputerAIPlayerSMove : System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        private bool beatenField;
        
        private int exField;
        
        private int eyField;
        
        private BreakthroughWPF.GameConnection.PiecesColor playerField;
        
        private int sxField;
        
        private int syField;
        
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public bool beaten {
            get {
                return this.beatenField;
            }
            set {
                if ((this.beatenField.Equals(value) != true)) {
                    this.beatenField = value;
                    this.RaisePropertyChanged("beaten");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public int ex {
            get {
                return this.exField;
            }
            set {
                if ((this.exField.Equals(value) != true)) {
                    this.exField = value;
                    this.RaisePropertyChanged("ex");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public int ey {
            get {
                return this.eyField;
            }
            set {
                if ((this.eyField.Equals(value) != true)) {
                    this.eyField = value;
                    this.RaisePropertyChanged("ey");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public BreakthroughWPF.GameConnection.PiecesColor player {
            get {
                return this.playerField;
            }
            set {
                if ((this.playerField.Equals(value) != true)) {
                    this.playerField = value;
                    this.RaisePropertyChanged("player");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public int sx {
            get {
                return this.sxField;
            }
            set {
                if ((this.sxField.Equals(value) != true)) {
                    this.sxField = value;
                    this.RaisePropertyChanged("sx");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute(IsRequired=true)]
        public int sy {
            get {
                return this.syField;
            }
            set {
                if ((this.syField.Equals(value) != true)) {
                    this.syField = value;
                    this.RaisePropertyChanged("sy");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "3.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="PiecesColor", Namespace="http://schemas.datacontract.org/2004/07/BreakthroughWPF")]
    public enum PiecesColor : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        White = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Black = 1,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        None = 2,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://BreakthroughWPF", ConfigurationName="GameConnection.IGameConnectionService", SessionMode=System.ServiceModel.SessionMode.Required)]
    public interface IGameConnectionService {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://BreakthroughWPF/IGameConnectionService/IsAlive", ReplyAction="http://BreakthroughWPF/IGameConnectionService/IsAliveResponse")]
        bool IsAlive();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://BreakthroughWPF/IGameConnectionService/Invite", ReplyAction="http://BreakthroughWPF/IGameConnectionService/InviteResponse")]
        bool Invite(string nickName, string endpoint);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://BreakthroughWPF/IGameConnectionService/InviteWelcome", ReplyAction="http://BreakthroughWPF/IGameConnectionService/InviteWelcomeResponse")]
        string InviteWelcome(string nickName, string endpoint);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://BreakthroughWPF/IGameConnectionService/NextMove", ReplyAction="http://BreakthroughWPF/IGameConnectionService/NextMoveResponse")]
        BreakthroughWPF.GameConnection.ComputerAIPlayerSMove NextMove(BreakthroughWPF.GameConnection.ComputerAIPlayerSMove opponentMove);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public interface IGameConnectionServiceChannel : BreakthroughWPF.GameConnection.IGameConnectionService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
    public partial class GameConnectionServiceClient : System.ServiceModel.ClientBase<BreakthroughWPF.GameConnection.IGameConnectionService>, BreakthroughWPF.GameConnection.IGameConnectionService {
        
        public GameConnectionServiceClient() {
        }
        
        public GameConnectionServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public GameConnectionServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public GameConnectionServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public GameConnectionServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public bool IsAlive() {
            return base.Channel.IsAlive();
        }
        
        public bool Invite(string nickName, string endpoint) {
            return base.Channel.Invite(nickName, endpoint);
        }
        
        public string InviteWelcome(string nickName, string endpoint) {
            return base.Channel.InviteWelcome(nickName, endpoint);
        }
        
        public BreakthroughWPF.GameConnection.ComputerAIPlayerSMove NextMove(BreakthroughWPF.GameConnection.ComputerAIPlayerSMove opponentMove) {
            return base.Channel.NextMove(opponentMove);
        }
    }
}
