using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ParkVault
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int selectedBoxRow;
        int selectedBoxColumn;
        enum OpCode
        {
            register,
            unregister,
            login,
            logout
        };


        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        // A partir de un puerto, establecemos conexión con el servidor y devolvemos el socket creado para poder seguir trabajando con él
        private Socket CreateSocketAndConnectServer(int port)
        {
            IPAddress address = IPAddress.Parse(ServerText.Text);
            IPEndPoint endPoint = new IPEndPoint(address, port);
            Socket socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(endPoint);
            
            return socket;
        }

        private int userPort = 1000;
        private int boxPort = 1001;

        // A partir de un string y un socket, enviamos por este la cantidad de bytes a enviar y luego estos en cuestión
        private void SendString(string s, Socket socket)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            int count = bytes.Length;
            byte[] bytesCount = BitConverter.GetBytes(count);
            socket.Send(bytesCount);
            socket.Send(bytes);
        }

        // Envía un int a través de un socket
        private void SendInt(int i, Socket socket)
        {
            byte[] bytes = BitConverter.GetBytes(i);
            socket.Send(bytes);
        }

        // Recibe bytes desde un socket y devuelve el int resultante
        static int ReceiveInt(Socket socket)
        {
            byte[] bytes = new byte[sizeof(int)];
            socket.Receive(bytes);
            int numRecibido = BitConverter.ToInt32(bytes);
            
            return numRecibido;
        }

        private void MainWindow_Loaded(object sender,RoutedEventArgs e)
        {
            ServerText.Text = "127.0.0.1";
            UserNameText.Text = "";
            PasswordText.Text = "";
            
            SelectedBoxText.Text = "None";
            BoxStatusText.Text = "Unknown";

            AccessCodeText.Text = "";

            selectedBoxRow = -1;
            selectedBoxColumn = -1;

            OperatePanel.Visibility = Visibility.Visible;
            WaitPanel.Visibility = Visibility.Hidden;
        }

        private void Window_MouseDown(object sender,MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void LoginButton_Click(object sender,RoutedEventArgs e)
        {
            //////////////////////////////
            // Loguearse en el servidor //
            //////////////////////////////
            
            Socket socket = CreateSocketAndConnectServer(userPort);

            // Primero enviamos el num que determina la orden que va a recibir el servidor
            
            int code = (int)OpCode.login;
            SendInt(code, socket);

            // Hacemos dos envíos, uno de username y otro de password
            SendString(UserNameText.Text, socket);
            SendString(PasswordText.Text, socket);

            // Recibimos respuesta del server y la mostramos
            int response = ReceiveInt(socket);

            if (response == 0)
            {
                MessageBox.Show("OK");
            }
            else if (response == -1)
            {
                MessageBox.Show("Password incorrecto");
            }
            else if(response == -2)
            {
                MessageBox.Show("Usuario no registrado");
            }
            else if (response == -3)
            {
                MessageBox.Show("Usuario ya logueado");
            }

        }

        private void LogoutButton_Click(object sender,RoutedEventArgs e)
        {
            /////////////////////////////////
            // Desloguearse en el servidor //
            /////////////////////////////////

            Socket socket = CreateSocketAndConnectServer(userPort);

            // Primero enviamos el num que determina la orden que va a recibir el servidor

            int code = (int)OpCode.logout;
            SendInt(code, socket);

            // Hacemos dos envíos, uno de username y otro de password
            SendString(UserNameText.Text, socket);
            SendString(PasswordText.Text, socket);

            // Recibimos respuesta del server y la mostramos
            int response = ReceiveInt(socket);

            if (response == 0)
            {
                MessageBox.Show("OK");
            }
            else if (response == -1)
            {
                MessageBox.Show("Password incorrecto");
            }
            else if (response == -2)
            {
                MessageBox.Show("Usuario no logueado");
            }
            else if (response == -3)
            {
                MessageBox.Show("Usuario no registrado");
            }
        }

        private void RegisterButton_Click(object sender,RoutedEventArgs e)
        {
            ////////////////////////////////
            // Registrarse en el servidor //
            ////////////////////////////////

            Socket socket = CreateSocketAndConnectServer(userPort);

            // Primero enviamos el num que determina la orden que va a recibir el servidor

            int code = (int)OpCode.register;
            SendInt(code, socket);

            // Hacemos dos envíos, uno de username y otro de password
            SendString(UserNameText.Text, socket);
            SendString(PasswordText.Text, socket);

            // Recibimos respuesta del server y la mostramos
            int response = ReceiveInt(socket);

            if (response == 0)
            {
                MessageBox.Show("OK");
            }
            else if (response == -1)
            {
                MessageBox.Show("Password incorrecto, mínimo 8 caracteres");
            }            
            else if (response == -2)
            {
                MessageBox.Show("Usuario ya registrado");
            }
        }

        private void UnregisterButton_Click(object sender,RoutedEventArgs e)
        {
            ///////////////////////////////////
            // Desregistrarse en el servidor //
            ///////////////////////////////////

            Socket socket = CreateSocketAndConnectServer(userPort);

            // Primero enviamos el num que determina la orden que va a recibir el servidor

            int code = (int)OpCode.unregister;
            SendInt(code, socket);

            // Hacemos dos envíos, uno de username y otro de password
            SendString(UserNameText.Text, socket);
            SendString(PasswordText.Text, socket);

            // Recibimos respuesta del server y la mostramos
            int response = ReceiveInt(socket);

            if (response == 0)
            {
                MessageBox.Show("OK");
            }
            else if (response == -1)
            {
                MessageBox.Show("Password incorrecto");
            }
            else if (response == -2)
            {
                MessageBox.Show("Usuario no registrado");
            }
        }

        private void GetContentsButton_Click(object sender,RoutedEventArgs e)
        {
            ////////////////////////////////////////////
            // Intentar obtener el fichero de la caja //
            ////////////////////////////////////////////

            Socket socket = CreateSocketAndConnectServer(boxPort);
        }

        private void PutContentsButton_Click(object sender,RoutedEventArgs e)
        {
            ////////////////////////////////////////////
            // Intentar poner un fichero en la caja   //
            ////////////////////////////////////////////

            Socket socket = CreateSocketAndConnectServer(boxPort);
        }

        private void ClearContentsButton_Click(object sender,RoutedEventArgs e)
        {
            ///////////////////////////////////////////////
            // Intentar eliminar el contenido de la caja //
            ///////////////////////////////////////////////

            Socket socket = CreateSocketAndConnectServer(boxPort);
        }

        private void ShowWaitPanel()
        {
            OperatePanel.Visibility = Visibility.Hidden;
            WaitPanel.Visibility = Visibility.Visible;
        }

        private void HideWaitPanel()
        {
            OperatePanel.Visibility = Visibility.Visible;
            WaitPanel.Visibility = Visibility.Hidden;
        }

        private void Box0AButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "0A";

            selectedBoxRow = 0;
            selectedBoxColumn = 0;

        }

        private void Box0BButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "0B";

            selectedBoxRow = 0;
            selectedBoxColumn = 1;
        }

        private void Box0CButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "0C";

            selectedBoxRow = 0;
            selectedBoxColumn = 2;
        }

        private void Box0DButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "0D";

            selectedBoxRow = 0;
            selectedBoxColumn = 3;
        }

        private void Box1AButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "1A";

            selectedBoxRow = 1;
            selectedBoxColumn = 0;
        }

        private void Box1BButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "1B";

            selectedBoxRow = 1;
            selectedBoxColumn = 1;

        }

        private void Box1CButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "1C";

            selectedBoxRow = 1;
            selectedBoxColumn = 2;

        }

        private void Box1DButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "1D";

            selectedBoxRow = 1;
            selectedBoxColumn = 3;

        }

        private void Box2AButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "2A";

            selectedBoxRow = 2;
            selectedBoxColumn = 0;

        }

        private void Box2BButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "2B";

            selectedBoxRow = 2;
            selectedBoxColumn = 1;

        }

        private void Box2CButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "2C";

            selectedBoxRow = 2;
            selectedBoxColumn = 2;

        }

        private void Box2DButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "2D";

            selectedBoxRow = 2;
            selectedBoxColumn = 3;

        }

        private void Box3AButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "3A";

            selectedBoxRow = 3;
            selectedBoxColumn = 0;

        }

        private void Box3BButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "3B";

            selectedBoxRow = 3;
            selectedBoxColumn = 1;

        }

        private void Box3CButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "3C";

            selectedBoxRow = 3;
            selectedBoxColumn = 2;

        }

        private void Box3DButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "3D";

            selectedBoxRow = 3;
            selectedBoxColumn = 3;

        }
        
    }
}