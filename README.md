# Remote Control



* Original project from [here](https://github.com/Bediveren/RemoteControllerCsharp)  
* Requires .NET 4.0 or newer.    

* This project is to create application remote support like team viewer,anyedesk, vnc, etc.
* Remote will control Client and Client send screen to Remote. Remote and Client connected with Server to send and receive data;


Let's seek :

![image](https://user-images.githubusercontent.com/40256508/181424272-374f16b9-b74f-49ed-86e6-f2c2b7bc44a2.png)

This is the view of the application, we have Ip address, Port, Name(in the future i will adding address such as anydesk). And we have 3 button : connect, Remote and Host. 
Function of each button :
* Connect
  This button is client use for connecting to server. So Remote side can control this client.
* Remote
  This button is remote use for connecting to server. So this side can control client.
* Host
  This button is to creating server, it will receiving data from client and sending to remote and vice versa from remote to client. So client and remote can do communication.
  
![image](https://user-images.githubusercontent.com/40256508/181425403-56014e0e-2bd1-46a4-b437-a2b7574d2e13.png)

This is the view of client in the future i will add chat feature and maybe video call too.

![image](https://user-images.githubusercontent.com/40256508/181425506-0e314ccc-437e-4954-b603-2b6ca6e722a7.png)

This is the view of remote.

![image](https://user-images.githubusercontent.com/40256508/181425595-4a086a29-dae3-4d11-b3df-c02ef877756a.png)

This is the view of server, just don't know what feature i will add on this.

Notes:  
- I developing this is especially for finishing my thesis. 
- Still have many bugs.
- Mouse movement laggy.
- Sometimes crash.
- Confused with server sometimes not sending correct data.
