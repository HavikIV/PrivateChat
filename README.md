This project is to create a messeger app that will connect to a privately owned/operated server(s) that either the user or someone
that they know has set up the server. Current;y the plan is to only handle text messages, but I plan on to add handling of sending
pictures as I learn more about socket programming. I will also create a sister software for the server that will handle routing the
messages to the right users.


# Server Application:
The server application is fairly simple to use and understand. First of all, the **Public IP Address** that is shown is the IP address that is assigned to your modem by your Internet Service Provider (ISP). This is the IP address that you will provide your friends and/or family members with so that they may connect to your server through the global internet. 

**Caution:** In most cases, this IP Address is dynamically allocated to each of your ISP's customers. By dynamically allocated, I mean that this IP address isn't permantently assigned to you, it can change after the alloated time is over (businesses on the other hand will tend to have static IP addresses). Most of the times, modems are generally good at renewing their IP address in a timely manner so that they will retain the same IP address for long periods of time. 

The **Local IP Address** is the IP address of your computer in network (if you have one, most likely you do). In most homes and businesses today, there are WiFi routers in use so you have to find out what is your computer's IP Address and provide it to the server application. In the case that all connections will only be made through the local network, then this is the IP address that should be given out.

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

The **Port Number** is used to identify which process that the computer should forward any incoming messages through the internet. A port number can be any number between 1-65535. It's best to assign one that isn't being used at the moment by the computer which can be checked by using the command **netstat -an - p tcp** in the administrator command prompt. The port numbers are listed after the **:** (ex. 0.0.0.0:80, port number is 80). You should only check the port numbers for IP address that you will provide the server application.

![cmd4](/Readmeimgs/cmd4.png)

## PrivateChat App:
