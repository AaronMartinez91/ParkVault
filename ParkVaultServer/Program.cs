using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata;
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
        
        static Dictionary<string, string> registeredUsers = new Dictionary<string, string>();
        static List<string> loggedUsers = new List<string>();

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

            //Thread.Sleep(1000);

            if (order == 0) //register
            {

                if (userExists)
                {
                    SendInt(-2, loginServiceSocket);
                }
                else
                {
                    // comprobamos que no esté ya logueado
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
                    if (loggedUsers.Contains(user))
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
                    if (!loggedUsers.Contains(user))
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

        static void Main(string[] args)
        {

            Thread userThread = new Thread(UserServiceServer);
            userThread.Start(); 
            

        }
    }
}
