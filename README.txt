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

Norbert Pionka