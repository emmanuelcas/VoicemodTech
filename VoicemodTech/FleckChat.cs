using System;
using System.Collections.Generic;
using Fleck;
using System.Net.WebSockets;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VoicemodTech
{
   
    public class FleckChat
    {
        private Dictionary<Fleck.IWebSocketConnection, string> ListaSockets = new Dictionary<IWebSocketConnection, string>();
        private Fleck.WebSocketServer server = null;// Fleck WebSocketServer object.
        private string connectionInfo = string.Empty; // Sets the connection information how to open the WebSocket.
        private int port; //Puerto


        //Inicializo una instancia del server de websocket, pasandole el puerto a usar
        public FleckChat(int port)
        {
            this.port = port;
            //FleckLog.Level = LogLevel.Info;
        }

        /// <summary>
        /// Simple main method creating the WebSocketChatServer
        /// </summary>
        /// <param name="args">Command line arguments: hostname port</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("Escriba Puerto a usar: ");
            string val = Console.ReadLine();

            FleckChat server;
            server = new FleckChat(Convert.ToInt32(val));
            
            try
            {
                server.SoyCliente(val).Wait();
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
                try
                {
                    //server = new FleckChat(Convert.ToInt32(val));
                    Console.WriteLine("Escriba su nombre: ");
                    string name = Console.ReadLine();
                    server.SoyServer(val, name);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

        }


        //Metodo que inicia el websocket escuchando en el puerto definido
        public void SoyServer(string puerto, string name)
        {
            try
            {
                this.connectionInfo = "ws://0.0.0.0:" + puerto; // Instancio el websocket WebSocketServer object
                this.server = new Fleck.WebSocketServer(this.connectionInfo);

                // Abre el websocket en el puerto indicado
                this.server.Start(socket =>
                {
                    
                    socket.OnClose = () =>
                    {
                        
                        if (ListaSockets.ContainsKey(socket))
                        {
                            foreach (KeyValuePair<Fleck.IWebSocketConnection, string> client in ListaSockets)
                            {
                                if (!client.Key.Equals(socket))
                                {
                                    client.Key.Send("El user " + ListaSockets[socket] + " se fue del chat");
                                }
                            }
                            Console.WriteLine("El user " + ListaSockets[socket] + " se fue del chat");
                            ListaSockets.Remove(socket);
                        }
                    };


                    //ACA TOMA LOS MENSAJES DE OTROS USERS, LO MUESTRA POR PANTALLA EN EL SERVER Y LO RETRASNMITE A LOS DEMAS USERS
                    //EL PRIMER MENSAJE ES TOMADO COMO EL NOMBRE
                    //CUANDO SE SUMA ALGUIEN AL CHAT TAMBIEN LO MUESTRA POR CONSOLA EN EL SERVER Y SE LO MUESTRA A LOS OTROS TAMBIEN
                    socket.OnMessage = mensaje =>
                    {
                        if (!ListaSockets.ContainsKey(socket))
                        {
                            ListaSockets.Add(socket, mensaje);
                            foreach (KeyValuePair<Fleck.IWebSocketConnection, string> client in ListaSockets)
                            {
                                client.Key.Send("El user " + mensaje + " se ha unido al chat");
                            }

                            Console.WriteLine("El user " + mensaje + " se ha unido al chat");
                        }
                        else
                        {
                            string tiempo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            foreach (KeyValuePair<Fleck.IWebSocketConnection, string> client in ListaSockets)
                            {
                                if (!client.Key.Equals(socket))
                                {
                                    client.Key.Send(tiempo + " - " + ListaSockets[socket] + ": " + mensaje);
                                }
                            }
                            Console.WriteLine(tiempo + " - " + ListaSockets[socket] + ": " + mensaje);
                        }

                    };
                });
            }
            catch (Exception e)
            {
                FleckLog.Error(e.ToString());
            }


            //ACA TOMA LO QUE SE ESCRIBE POR CONSOLA (EN CASO DE SER EL SERVER) HASTA QUE ESCRIBA EXIT
            string mens = Console.ReadLine();
            while (mens != "exit")
            {
                string tiempo = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                foreach (KeyValuePair<Fleck.IWebSocketConnection, string> client in ListaSockets)
                {
                    client.Key.Send(tiempo + " - "+ name + ": " + mens);
                }
                mens = Console.ReadLine();
            }
            if (mens == "exit")
            {
                foreach (KeyValuePair<Fleck.IWebSocketConnection, string> client in ListaSockets)
                {
                    client.Key.Send("SE HA CERRADO LA SALA DE CHAT");
                }
            }
        }


        public async Task SoyCliente(string puerto)
        {

            var cancell = new CancellationTokenSource();
            var socket = new ClientWebSocket();
            await socket.ConnectAsync(new Uri(string.Format("ws://127.0.0.1:" + puerto)), cancell.Token);
            Console.Write("Ingrese su nombre: ");

            Task.Factory.StartNew(async () =>
            {
                var rcvBytes = new byte[128];
                var rcvBuffer = new ArraySegment<byte>(rcvBytes);
                while (true)
                {
                    WebSocketReceiveResult rcvResult = await socket.ReceiveAsync(rcvBuffer, cancell.Token);
                    byte[] msgBytes = rcvBuffer.Skip(rcvBuffer.Offset).Take(rcvResult.Count).ToArray();
                    string rcvMsg = Encoding.UTF8.GetString(msgBytes);
                    Console.WriteLine(rcvMsg);
                }
            }, cancell.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            string mens = Console.ReadLine();
            while (mens != "exit")
            {
                if (mens == "exit")
                {
                    cancell.Cancel();
                    return;
                }
                
                byte[] sendBytes = Encoding.UTF8.GetBytes(mens);
                var sendBuffer = new ArraySegment<byte>(sendBytes);
                await socket.SendAsync(sendBuffer, WebSocketMessageType.Text, endOfMessage: true, cancellationToken: cancell.Token);
                mens = Console.ReadLine();
            }

            socket.CloseAsync(WebSocketCloseStatus.NormalClosure,String.Empty, cancell.Token);

            // this will cause OnError on the server if not closed first
            cancell.Cancel();


        }



    }
}
