using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;

namespace ParkVaultServer
{
    //public class LoginOperation
    //{
    //    Socket socket;
    //    public LoginOperation(Socket s)
    //    {
    //        socket = s;
    //    }

    //    public void Execute()
    //    {
    //        //...
    //    }

    //}


    //LoginOperation o = new LoginOperation(socket);
    //Thread thread = new Thread(o.Execute);


    internal class Program
    {
        class Box
        {
            public string id;
            public string accessCode;
            public bool isOccupied;
            public byte[] content;
        }

        static Dictionary<string, string> registeredUsers = new Dictionary<string, string>();
        static List<string> loggedUsers = new List<string>();
        static List<Box> boxes = new List<Box>();
        enum BoxLetters
        {
            A, B, C, D
        }

        static void UserServiceServer()
        {
            // Login de clientes
            IPAddress address = IPAddress.Parse("127.0.0.1");
            IPEndPoint loginEndPoint = new IPEndPoint(address, 1000);
            Socket loginServerSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket loginServiceSocket;

            loginServerSocket.Bind(loginEndPoint);
            loginServerSocket.Listen();

            while (loginServerSocket.IsBound)
            {
                loginServiceSocket = loginServerSocket.Accept();

                ParameterizedThreadStart threadStart = new ParameterizedThreadStart(UserService);
                Thread userThread = new Thread(threadStart);
                userThread.Start(loginServiceSocket);


                //foreach (KeyValuePair<string, string> par in registeredUsers)
                //{
                //    Console.WriteLine($"Clave: {par.Key}, Valor: {par.Value}");
                //}

                //foreach (string user in loggedUsers)
                //{
                //    Console.WriteLine($"Usuario logeado: {user}");
                //}
            }
        }

        static void BoxServiceServer()
        {
            IPAddress address = IPAddress.Parse("127.0.0.1");
            IPEndPoint loginEndPoint = new IPEndPoint(address, 1001);
            Socket loginServerSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            Socket loginServiceSocket;

            loginServerSocket.Bind(loginEndPoint);
            loginServerSocket.Listen();

            while (loginServerSocket.IsBound)
            {
                loginServiceSocket = loginServerSocket.Accept();

                ParameterizedThreadStart threadStart = new ParameterizedThreadStart(BoxService);
                Thread userThread = new Thread(threadStart);
                userThread.Start(loginServiceSocket);
            }


        }

        static void UserService(object o)
        {
            int order;
            string user;
            string password;

            Socket loginServiceSocket = (Socket)o;  

            // primero saber qué orden envía!!
            order = ReceiveInt(loginServiceSocket);
            // Recibimos valores de user y password
            user = ReceiveString(loginServiceSocket);
            password = ReceiveString(loginServiceSocket);

            // comprobar si el usuario existe o no
            bool userExists = UserExists(user);

            // comprobar si el usuario está logeado o no
            bool isLogged = UserIsLogged(user);

            //Thread.Sleep(1000);

            if (order == 0) //register
            {

                if (userExists)
                {
                    SendInt(-2, loginServiceSocket);
                }
                else
                {
                    // comprobamos que la contraseña tenga al menos 8 caracteres
                    if (password.Length < 8)
                    {
                        SendInt(-1, loginServiceSocket);
                    }
                    else
                    {
                        registeredUsers.Add(user, password);
                        SendInt(0, loginServiceSocket);
                    }
                }

            }
            else if (order == 1) //unregister
            {

                if (!userExists)
                {
                    SendInt(-2, loginServiceSocket);
                }
                else // exists
                {
                    // comprobamos password
                    if (registeredUsers[user] == password)
                    {
                        registeredUsers.Remove(user);
                        SendInt(0, loginServiceSocket);
                    }
                    else
                    {
                        SendInt(-1, loginServiceSocket);
                    }
                }
            }
            else if (order == 2) //login
            {

                if (userExists)
                {
                    // comprobamos si ya está logeado
                    if (isLogged)
                    {
                        SendInt(-3, loginServiceSocket);
                    }
                    else // existe y no está logeado
                    {
                        // comprobamos si pw es correcta
                        if (registeredUsers[user] == password)
                        {
                            loggedUsers.Add(user);
                            SendInt(0, loginServiceSocket);
                        }
                        else // pw incorrecta
                        {
                            SendInt(-1, loginServiceSocket);
                        }
                    }

                }
                else // no existe el usuario
                {
                    SendInt(-2, loginServiceSocket);
                }
            }
            else // order 3 (logout)
            {
                if (userExists)
                {
                    // comprobamos si no está logeado
                    if (!isLogged)
                    {
                        SendInt(-2, loginServiceSocket);
                    }
                    else // existe y está logeado
                    {
                        // comprobamos si pw es correcta
                        if (registeredUsers[user] == password)
                        {
                            loggedUsers.Remove(user);
                            SendInt(0, loginServiceSocket);
                        }
                        else // pw incorrecta
                        {
                            SendInt(-1, loginServiceSocket);
                        }
                    }

                }
                else // no existe el usuario
                {
                    SendInt(-3, loginServiceSocket);
                }
            }

            loginServiceSocket.Close();
        }

        static void BoxService(object o)
        {
            int order;
            string user;
            string password;
            int boxRow;
            int boxColumn;
            string boxCode;
            int dataReceived;
            byte[] boxData = null;
            Box auxBox = null;

            Socket BoxServiceSocket = (Socket)o;

            // primero saber qué orden envía!!
            order = ReceiveInt(BoxServiceSocket);

            // en funcion de la orden recibida, recibiremos más o menos datos, por lo que filtramos aquí

            if (order == 0) // status
            {
                // Recibimos valores de user, password, boxRow y boxColumn
                user = ReceiveString(BoxServiceSocket);
                password = ReceiveString(BoxServiceSocket);
                boxRow = ReceiveInt(BoxServiceSocket);
                boxColumn = ReceiveInt(BoxServiceSocket);

                // primero comprobamos el login
                bool isLogged = UserIsLogged(user);

                // guardamos el ID de la caja
                string boxId = $"{boxRow}{(BoxLetters)boxColumn}";

                if (!isLogged)
                {
                    SendInt(-2, BoxServiceSocket);
                }
                else
                {
                    // comprobamos si la contraseña es correcta
                    if (registeredUsers[user] != password)
                    {
                        SendInt(-1, BoxServiceSocket);
                    }
                    else
                    {
                        // ahora debemos mirar si la caja está ocupada o no
                        foreach (Box b in boxes)
                        {
                            if (b.id == boxId)
                            {
                                if (b.isOccupied)
                                {
                                    SendInt(1, BoxServiceSocket);
                                }
                                else
                                {
                                    SendInt(0, BoxServiceSocket);
                                }
                            }
                        }
                    }
                }
            }
            else if (order == 1) // put item
            {
                // Recibimos valores de user, password, boxRow, boxColumn, boxCode y boxData
                user = ReceiveString(BoxServiceSocket);
                password = ReceiveString(BoxServiceSocket);
                boxRow = ReceiveInt(BoxServiceSocket);
                boxColumn = ReceiveInt(BoxServiceSocket);
                boxCode = ReceiveString(BoxServiceSocket);

                dataReceived = ReceiveInt(BoxServiceSocket);
                
                // solo si el cliente envía un 1, recibimos un archivo
                if (dataReceived == 1)
                {
                    int filesLenght = ReceiveInt(BoxServiceSocket);
                    boxData = new byte[filesLenght];

                    BoxServiceSocket.Receive(boxData); 

                    Console.WriteLine($"Archivo recibido: {boxData}");
                }                

                // primero comprobamos el login
                bool isLogged = UserIsLogged(user);

                // guardamos el ID de la caja
                string boxId = $"{boxRow}{(BoxLetters)boxColumn}";

                if (!isLogged)
                {
                    SendInt(-2, BoxServiceSocket);
                }
                else
                {
                    // comprobamos si la contraseña es correcta
                    if (registeredUsers[user] != password)
                    {
                        SendInt(-1, BoxServiceSocket);
                    }
                    else
                    {
                        
                        // ahora debemos mirar si la caja está ocupada o no
                        foreach (Box b in boxes)
                        {
                            // obtenemos la caja con la que vamos a trabajar
                            if (b.id == boxId)
                            {
                                auxBox = b;
                            }
                        }
                        if (auxBox.isOccupied)
                        {
                            SendInt(-3, BoxServiceSocket);
                        }
                        else
                        {
                            auxBox.accessCode = boxCode;
                            auxBox.isOccupied = true;
                            auxBox.content = boxData;

                            Thread.Sleep(1000);
                            SendInt(0, BoxServiceSocket);
                        }                    
                    }
                }

            }
            else if (order == 2) // get item
            {
                // Recibimos valores de user, password, boxRow, boxColumn y boxCode
                user = ReceiveString(BoxServiceSocket);
                password = ReceiveString(BoxServiceSocket);
                boxRow = ReceiveInt(BoxServiceSocket);
                boxColumn = ReceiveInt(BoxServiceSocket);
                boxCode = ReceiveString(BoxServiceSocket);

                // primero comprobamos el login
                bool isLogged = UserIsLogged(user);

                // guardamos el ID de la caja
                string boxId = $"{boxRow}{(BoxLetters)boxColumn}";

                if (!isLogged)
                {
                    SendInt(-2, BoxServiceSocket);
                }
                else
                {
                    // comprobamos si la contraseña es correcta
                    if (registeredUsers[user] != password)
                    {
                        SendInt(-1, BoxServiceSocket);
                    }
                    else
                    {
                        // ahora debemos mirar si la caja está ocupada o no
                        foreach (Box b in boxes)
                        {
                            // obtenemos la caja con la que vamos a trabajar
                            if (b.id == boxId)
                            {
                                auxBox = b;
                            }
                        }
                        if (!auxBox.isOccupied)
                        {
                            SendInt(-3, BoxServiceSocket);
                        }
                        else
                        {
                            // comprobamos si el código es correcto
                            if (auxBox.accessCode != boxCode)
                            {
                                SendInt(-4, BoxServiceSocket);
                            }
                            else
                            {
                                // enviamos datos y borramos
                                SendInt(0, BoxServiceSocket);

                                byte[] sendContent = auxBox.content;
                                int contentLenght = sendContent.Length;

                                SendInt(contentLenght, BoxServiceSocket);
                                BoxServiceSocket.Send(sendContent);

                                auxBox.accessCode = null;
                                auxBox.isOccupied = false;
                                auxBox.content = null;

                                Thread.Sleep(1000);
                            }

                        }
                    }
                }
            }
            else if (order == 3) // get item
            {
                // Recibimos valores de user, password, boxRow, boxColumn y boxCode
                user = ReceiveString(BoxServiceSocket);
                password = ReceiveString(BoxServiceSocket);
                boxRow = ReceiveInt(BoxServiceSocket);
                boxColumn = ReceiveInt(BoxServiceSocket);
                boxCode = ReceiveString(BoxServiceSocket);

                // primero comprobamos el login
                bool isLogged = UserIsLogged(user);

                // guardamos el ID de la caja
                string boxId = $"{boxRow}{(BoxLetters)boxColumn}";

                if (!isLogged)
                {
                    SendInt(-2, BoxServiceSocket);
                }
                else
                {
                    // comprobamos si la contraseña es correcta
                    if (registeredUsers[user] != password)
                    {
                        SendInt(-1, BoxServiceSocket);
                    }
                    else
                    {
                        // ahora debemos mirar si la caja está ocupada o no
                        foreach (Box b in boxes)
                        {
                            // obtenemos la caja con la que vamos a trabajar
                            if (b.id == boxId)
                            {
                                auxBox = b;
                            }
                        }
                        if (!auxBox.isOccupied)
                        {
                            SendInt(-3, BoxServiceSocket);
                        }
                        else
                        {
                            // comprobamos si el código es correcto
                            if (auxBox.accessCode != boxCode)
                            {
                                SendInt(-4, BoxServiceSocket);
                            }
                            else
                            {
                                // borramos datos
                                                 
                                auxBox.accessCode = null;
                                auxBox.isOccupied = false;
                                auxBox.content = null;

                                Thread.Sleep(1000);
                                SendInt(0, BoxServiceSocket);
                            }

                        }
                    }
                }
            }

                BoxServiceSocket.Close();
        }

        static string ReceiveString(Socket socket)
        {
            byte[] bytesCount = new byte[sizeof(int)];
            socket.Receive(bytesCount);
            int count = BitConverter.ToInt32(bytesCount);
            byte[] bytes = new byte[count];
            socket.Receive(bytes);
            string s = Encoding.UTF8.GetString(bytes);
            return s;
        }

        static int ReceiveInt(Socket socket)
        {
            byte[] bytes = new byte[sizeof(int)];
            socket.Receive(bytes);
            int numRecibido = BitConverter.ToInt32(bytes);

            return numRecibido;
        }

        static void SendInt(int i, Socket socket)
        {
            byte[] bytes = BitConverter.GetBytes(i);
            socket.Send(bytes);
        }
              
        static bool UserExists (string user)
        {
            bool userExists = false;

            if (registeredUsers.ContainsKey(user))
            {
                userExists = true;
            }

            return userExists;
        }

        static bool UserIsLogged(string user)
        {
            bool userLogged = false;
            if (loggedUsers.Contains(user))
            {
                userLogged = true;
            }
            return userLogged;
        }

        static void Main(string[] args)
        {

            Thread userThread = new Thread(UserServiceServer);
            userThread.Start(); 
            
            // por el momento creamos e inicializamos las 16 cajas de 0, más tarde se crearán y cargarán con la info de los archivos

            for(int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    Box box = new Box();
                    box.id = $"{i}{(BoxLetters)j}";
                    box.accessCode = null;
                    box.isOccupied = false;
                    box.content = null;
                    boxes.Add(box);
                }
            }

            Thread boxThread = new Thread(BoxServiceServer);
            boxThread.Start();
        }
    }
}
