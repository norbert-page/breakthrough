# Breakthrough
[![GitHub](https://img.shields.io/github/license/norbert-page/breakthrough)](https://github.com/norbert-page/breakthrough/blob/main/LICENSE)


Breakthrough is a Windows GUI app that implements a [breakthrough](https://en.wikipedia.org/wiki/Breakthrough_(board_game)) board game with rich features such as animations, computer player, and network play. It was developed as an individual programming project at a university **in 2009** using .NET Framework (WPF, WCF).

You can watch some videos of the app in action [on YouTube](https://www.youtube.com/playlist?list=PLnw-SyEulTuayiBLVrXf_kNalQ0NuzB-O) or in [this repository](videos). You can also see some [screenshots](screenshots) of the app. The main features of the app are:
* Playing games on the same computer with other people, against a computer, or watching games between two computer players. The computer player uses a MinMax algorithm to decide its moves.
* Playing games over the network with remote players. The app uses P2P, serverless communication to dynamically discover available players, both in the local network and on the Internet (no need to share IP addresses).
* Animating the interface. For example, clicking on a pawn shows the available moves for that pawn.
* Modifying the board dynamically. You can add or remove pawns during the game.
* Showing the full game history. You can click on any of the past moves to rewind the game to that point.
* Loading and saving the game state in *.board files.
* Asking the computer for move suggestions. The app will show you the suggested moves with an animation on the board.

## Screenshot
Additional [screenshots](screenshots) are available. However, videos are better for showing the time-based animations and you can watch them [on youtube](https://www.youtube.com/playlist?list=PLnw-SyEulTuayiBLVrXf_kNalQ0NuzB-O) or in the [videos folder](videos) in this repository.
![Screenshot](screenshots/breakthrough_-_screenshot_4.png)

## Technology stack
The app was written for a university assignment in 2008 and 2009. The original task was to implement a [breakthrough board game](https://en.wikipedia.org/wiki/Breakthrough_(board_game)) using [Delphi](https://en.wikipedia.org/wiki/Delphi_(software)). I decided to use C# and .NET Framework 3.5 technologies instead, including Windows Presentation Foundation (WPF) and Windows Communication Foundation (WCF).

I chose WPF because it uses DirectX, which allows for smooth time-based animations. WCF was chosen because it enables P2P-based, serverless discovery of available players, both in the local network and on the Internet, without requiring IP addresses. Breakthrough is IPv6-native and to work in IPv4-only networks makes use of Teredo tunneling, a technology implemented by Microsoft. These technologies have aged well, although .NET Core (or simply .NET) seems to be the successor to .NET Framework as explained [here](https://devblogs.microsoft.com/dotnet/net-core-is-the-future-of-net/).

## Installation & configuration
You can download all the files in the repository using this [link](https://github.com/norbert-page/breakthrough/archive/refs/heads/main.zip). You only need the files from the `release` directory, but you can also explore the other files if you are interested. To install the app, run `setup.exe` located in the `release` directory.

The installer may ask you to allow the installation of .NET Framework 3.5, which is required for the app to run.

The app uses an `App.config` file for configuration, which includes the default MeshID and port. Most likely, you don’t need to change anything in this file. The app will use the NetTcpPortSharing service to allow multiple instances to use the same port.

### Play over network
To play over the network, you need to do some additional configuration on Windows 10 and 11. You need to either disable your firewall or allow connections for Breakthrough (the firewall should prompt you when you try to start a network game). In addition, the following Windows Services must be enabled:
- NetTcpPortSharing
- PNRPsvc (Peer Name Resolution Protocol)

You can enable these services in Windows Services (start with services.msc in command line) or use the following commands in a command line with administrator privileges (you can use `auto` or `demand` as the start option):
```
sc.exe config NetTcpPortSharing start=auto
sc.exe config PNRPsvc start=auto
```

The app uses Teredo tunneling to enable P2P discovery over IPv6, even if you don’t have an IPv6 internet connection. You may need to use some additional commands to configure Teredo, such as:
```
netsh interface ipv6 install
netsh interface ipv6 set teredo client
netsh interface teredo set state type=enterpriseclient    // was needed on Windows 10 last time I tried, wasn't required on Windows 11
netsh interface teredo set state client
netsh interface teredo show state
netsh p2p pnrp peer set machinename publish=start autopublish=enable
netsh p2p pnrp peer show machinename
```
**You may need to restart your computer for the changes to take effect.** To verify that P2P network is working properly, start a network discovery of players in the game and use `netsh p2p pnrp cloud show list` to check if the `GLOBAL` P2P cloud is available. 

Finally, for network play, it is easiest to run Breakthrough app with administrator privileges to avoid potential issues with permissions. You can do this by starting a command line with administrator privileges, navigating to the directory where Breakthrough is installed (you can check this by inspecting the Start Menu shortcut for the game), and running the following command:
```
start ./Breakthrough.appref-ms
```

#### Errors related to networking
If you encounter errors related to network games, you can try these solutions:

1. **"Unable to access a key"** (Windows 10)

    This error may occur if the Local Service account does not have access to the MachineKeys. The solution that worked for me ([source](https://answers.microsoft.com/en-us/windows/forum/all/unable-to-start-peer-name-resolution-protocol/2b37dc4c-2153-443c-b0d5-adda6771ceb5)):
    > go to C:\ProgramData\Microsoft\Crypto\RSA and right-click on MachineKeys - Properties - Security and add Local Service to the Groups or user names.  To do so, click edit - Add - and type "local service" and hit check names, then click ok and click full control under permissions for local service, click ok.  you may get an error as it tries to set user "local service" for all keys which it can not because permissions are not allowed.

3. **"The service endpoint failed to listen on the URI because access was denied"**

    This error may occur if the NetTcpPortSharing service does not have permission to listen on a port. You can fix this by following [these steps](https://stackoverflow.com/questions/24576646/wcf-error-with-net-tcp-the-service-endpoint-failed-to-listen-on-the-uri-because) (or their [archived copy](https://web.archive.org/web/20220818223621/https://stackoverflow.com/questions/24576646/wcf-error-with-net-tcp-the-service-endpoint-failed-to-listen-on-the-uri-because)).

Enjoy!
