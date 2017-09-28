This project is to create a messeger app that will connect to a privately owned/operated server(s) that either the user or someone
that they know has set up the server. Current;y the plan is to only handle text messages, but I plan on to add handling of sending
pictures as I learn more about socket programming. I will also create a sister software for the server that will handle routing the
messages to the right users.


# Server Application:
The server application is fairly simple to use and understand. First of all, the **Public IP Address** that is shown is the IP address that is assigned to your modem by your Internet Service Provider (ISP). This is the IP address that you will provide your friends and/or family members with so that they may connect to your server through the global internet. 

**Caution:** In most cases, this IP Address is dynamically allocated to each of your ISP's customers. By dynamically allocated, I mean that this IP address isn't permantently assigned to you, it can change after the alloated time is over (businesses on the other hand will tend to have static IP addresses). Most of the times, modems are generally good at renewing their IP address in a timely manner so that they will retain the same IP address for long periods of time. 

The **Local IP Address** is the IP address of your computer in your local network. In most homes and businesses today, there are WiFi routers in use so you have to find out what is your computer's IP Address that the router has assign it and input it in the Textbox indicated by **Local IP Address:** on the server application. In the case that all connections will only be made through the local network, then this is the IP address that should be given out.

One method of obtaining the IP address on a Windows PC is the following:

Open a command prompt with administrator permissions
![cmd](/Readmeimgs/cmd.png)

type in **ipconfig** and press Enter
![cmd1](/Readmeimgs/cmd1.png)

Look for the **Ethernet adapter** that has the ***Default Gateway*** filled in
![cmd2](/Readmeimgs/cmd2.png)

Your **Local IP Address** is indicated by the ***IPv4 Address***, it usually starts with 192
![cmd3](/Readmeimgs/cmd3.png)

Type in the IP address exactly as you see it in the command prompt as that is the format for IP addresses and the only way the application recongizes IP addresses.

The **Port Number** is used to identify which process that the computer should forward any incoming messages through the internet. A port number can be any number between 1-65535. It's best to assign one that isn't being used at the moment by the computer which can be checked by using the command **netstat -an - p tcp** in the administrator command prompt. The port numbers are listed after the **":"** (ex. 0.0.0.0:80, port number is 80). You should only check the port numbers for IP address that you will input into the server application.

![cmd4](/Readmeimgs/cmd4.png)

Once both the **Local IP Address** and **Port Number** is inputted, click on the Start button to run the server. You can Stop the server at any given moment. The box below the two buttons is where all of the logs will be will displayed while the server is running.

## PrivateChat App:
The PrivateChat App first requires the User to register their 10 digit phone number and full name. At the moment the phone number is used as a unique key for each user. In the case that of User B tried to connect with a phone number that User A has already registered as their own, User B's attempt will be rejected.

![Register](/Readmeimgs/Register.png)

Once the registeration is finished, the App is opened to the MainActivity which is where the user is able to add Servers that they wish to be connected to. In order to add a server, just click on the FAB in the lower right hand corner and it should open a dialog to enter the server's information

![Main](/Readmeimgs/Main.png)

You can give the server any name you wish, it's a name that only you know the server by. Enter each of the four octets of the server's IP Address and the Port Number before clicking the connect button.
![AddServer](/Readmeimgs/AddServer.png)

This Activity will display all of the saved conversations that the user is part of on the selected server. A conversation can be started by clicking on the FAB in the lower right hand corner
![Conversations](/Readmeimgs/Conversations.png)

In the textbox at the top, the user needs to input the full phone number of who to send the message to. Currently the server only supports up 5 recipients with each of their phone numbers separated by a space.
![NewMessage](/Readmeimgs/NewMessage.png)