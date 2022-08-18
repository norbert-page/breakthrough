# Breakthrough
[![GitHub](https://img.shields.io/github/license/norbert-page/breakthrough)](https://github.com/norbert-page/breakthrough/blob/main/LICENSE)

This project implements a [Breakthrough](https://en.wikipedia.org/wiki/Breakthrough_(board_game)) board game as a Windows GUI application. For a quick glimpse you may want to check [videos](videos) or [screenshots](screenshots). The features include:
* Games on the same computer with other people, playing with a computer or observing games between two computer players. Computer uses a MinMax algorithm.
* Games over network with remote players. Uses P2P, serverless communication to allow for dynamic discovery of available players, both in local network and globally over the Internet (no explicit IP address sharing needed).
* Animated interface. For instance, clicking on a pawn shows moves that are available.
* Dynamic modifications to the board: addition and removal of the pawns mid-game.
* Displays full game history. Clicking on any of the entries for past moves rewinds the game to the selected point in game history.
* Loading and saving of the game state in \*.board files.
* Players can ask a computer for move suggestions. Those are presented with an animation on the board.

## Screenshot 
![Screenshot](screenshots/breakthrough_-_screenshot_4.png)

## Technology stack
The game was written for a university assignment in 2008 and 2009. The task was to implement a breakthrough board game using [Delphi](https://en.wikipedia.org/wiki/Delphi_(software)). I was quite surprised by this as the last time I had used Pascal was during primary school. To do something more exciting and progressive, I have learned and used C# and .NET Framework 3.5 technologies instead, including [Windows Presentation Foundation (WPF)](https://en.wikipedia.org/wiki/Windows_Presentation_Foundation) and [Windows Communication Foundation (WCF)](https://en.wikipedia.org/wiki/Windows_Communication_Foundation).

First, WPF makes use of DirectX which allows for smooth time-based animations. Second, WCF enabled P2P-based, serverless discovery of available players, both in the local network and on the global Internet, without the need to exchange IPs. Breakthrough is IPv6-native and to work in IPv4-only networks makes use of Teredo tunneling technology implemented by Microsoft. All these thechnologies aged quite well, although [.Net Core](https://en.wikipedia.org/wiki/.NET) (or simply .NET) seems to be a successor to .NET Framework as explained [here](https://devblogs.microsoft.com/dotnet/net-core-is-the-future-of-net/).

## 2009: Original readme in polish
Aplikacja wymaga kilku usług niedawno wprowadzonych przez Microsoft, dlatego proszę testować pod Windows Vista z najnowszymi łatami, gdyż przy wcześniejszych wersjach mogą być problemy.

Proszę wyłączyć firewall systemowy.

Aplikacja wymaga sieci IPv6 lub tunelowania Teredo, które domyślnie jest wyłączone np. w Windows XP.
Musi być aktywna w windowsie usługa netTcpPortSharing (w wer. PL może mieć inną nazwę) i usługi związane z PNRP (Peer Name Resolution Protocol).

W windowsie mogą pomóc następujące polecenia (proszę najpierw spróbować zwyczajnie odpalić):
netsh interface ipv6 install
netsh interface ipv6 set teredo client

netsh interface teredo set state client
netsh interface teredo show state

netsh p2p pnrp peer set machinename publish=start autopublish=enable
netsh p2p pnrp peer show machinename

Następujące polecenie powinno wyświetlić chmurę "Global" po starcie aplikacji, gdy zacznie się szukanie hostów, w przeciwnym przypadku coś jest nie tak:
netsh p2p pnrp cloud show list

Jeżeli wszystko dobrze jest skonfigurowane i jest połączenie z internetem, to wykrywanie hostów powinno działać globalnie, a nie tylko w sieci lokalnej.

Testowany na Windows Vista SP1 Business Edition EN z najnowszymi łatami, kompilowane pod MS Visual Studio 2008 SP1 z najnowszymi łatami.
Program nie kompiluje się na starszych wersjach Visual Studio (bez SP).

Program korzysta z pliku konfiguracyjnego App.config, gdzie jest zapisany wzorzec adresu z MeshID i domyślny port. Można uruchomić kilka instancji aplikacji na jednym komputerze - używają wtedy one tego samego portu, jednak ze względu na wykorzystanie współdzielenia portu powinny działać bez problemu (przetestowane) (inny jest endpoint, kończy się identyfikatorem użytkownika i doklejonym na końcu losowym łancuchem znaków).

Proszę o cierpliwość podczas wyszukiwania hostów, jednym razem może zadziałać od razu, a innym razem może trwać dłużej.

Jeżeli będzie Pan testował na jednym komputerze, to proszę pamiętać, że można zmieniać rozmiar okna głównego gry oraz chować cały prawy panel z opcjami :)
