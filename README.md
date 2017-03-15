## Times - Server [Login] ##
An C# Event driven, multi threaded AS3 CPPS Emulator.
This is login server, view other branches for other types of server

THIS IS FOR THE LOGIN SERVER. View more branches for different types of server.

Visit https://times-0.github.io/Times/ To view Times docs.
## Usage ##
 - All you need to do is, add this file to your C# Solution.
 
## SETUP ##
 - Then, to get the debug available you must implement your-wished custom Debug-handler, and catch the debug events using *Log > Debugger* class.
 - You must also specify the type of server, you are running. Either a Login or World or Redeem server. Currently this has no advantage of multi-server processing.
 - You need to create a new solution, or compile this once-again to create another type of server. The Server-type can be changed in *Shell.cs* 
 
## Handling Packets and Custom-callback handlers ##
This uses a custom event based class, that handles all the Packet-event received. If any packet is malformed or not recognized, anything than XT and XML Type ain't recognized the server will kick the user. 
All the packets are received by an async loop that runs parallel with every other penguins connected. 
This server, converts all packet received into a special format, to handle that packets, as mentioned in *Packets.cs*:
 - **XT** : *Format = #xtCATEGORY-HANDLER/*, where category is like "s", "z", and handler is for example, "j#jr", "j#js" etc..,.
 - **XML** : *Format = #xmlACTION/*, where ACTION is the attribute "action" contained in "body" tag.

The handler will then call the function, which corresponds to the callback-format as said above. The callback method will be given the following parameters resp,

```
Event = {type=callback-name, client=penguin.cs class}, vars = For XT, the remainig vars after "-1 or intid"; for XML, the body tag-class itself.
```

You no need to import the classes that handles callback. All you need to do is, create a new .cs file in *Client/Dependencies* and the Server will auto add it in runtime. LoginHandler is one of the example. 

## DEPENDENCIES ##

 - [XmlToDynamic](https://github.com/jonathanconway/XmlToDynamic/)
 - MySQL :: Data, auto-installed when installed MySQL in PC
 - .NET Framework 4 +, install the latest if you have anything lesser than 4.
 - MS Linq
 - XMLSerializer :: Linq, add to reference if not available
 - Sockets, auto added in reference.

## STARTING THE SERVER ##
First of all you have to do this to init a server.
```
var loginServer = new Airtower("IP/HOST NAME", port);
var Shell = new Shell();
```

Next, do all the remaining stuffs you need to setup before you start the server. Then atlast, to start the server execute.
```
loginServer.startConnection();
```

## CONTRIBUTION ##
Feel free to contribute anything to this server. It can be any bugs, glitch fixes, updates, security improvement, handlers, ideas, performance fixes, etc..,.
