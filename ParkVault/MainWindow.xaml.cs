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
using Microsoft.Win32;

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

        enum BoxOpCode
        {
            status,
            put,
            obtain,
            clear
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
        private int ReceiveInt(Socket socket)
        {
            byte[] bytes = new byte[sizeof(int)];
            socket.Receive(bytes);
            int numRecibido = BitConverter.ToInt32(bytes);
            
            return numRecibido;
        }

        // Función que a través de un socket, envía los parámetros necesarios para comprobar el estado de una caja y recobe a su vez la respuesta del servidor, cambiando la interfaz
        private void StatusRequest()
        {
            // Hay que pedir el estado de la caja y cambiar el BoxStatusText.Text en función de la respuesta

            Socket socket = CreateSocketAndConnectServer(boxPort);

            // Primero enviamos el num que determina la orden que va a recibir el servidor

            int code = (int)BoxOpCode.status;
            SendInt(code, socket);

            // Después el resto de parámetros: Nombre de usuario, Password, fila y columna
            SendString(UserNameText.Text, socket);
            SendString(PasswordText.Text, socket);
            SendInt(selectedBoxRow, socket);
            SendInt(selectedBoxColumn, socket);

            // Recibimos respuesta del server y la mostramos
            int response = ReceiveInt(socket);

            if (response == 0)
            {
                BoxStatusText.Text = "Empty";
            }
            else if (response == 1)
            {
                BoxStatusText.Text = "Occupied";
            }
            else if (response == -1)
            {
                BoxStatusText.Text = "Incorrect password";
            }
            else if (response == -2)
            {
                BoxStatusText.Text = "User not logged in";
            }
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

            // controlamos que siempre se envíe un código para que el servidor no esté esperando infinitamente en caso de error
            if (AccessCodeText.Text == "")
            {
                MessageBox.Show("Debes introducir un código en acces code");
                return;
            }


            Socket socket = CreateSocketAndConnectServer(boxPort);

            // Primero enviamos el num que determina la orden que va a recibir el servidor

            int code = (int)BoxOpCode.obtain;
            SendInt(code, socket);

            // Después enviamos user, password, boxRow, boxColumn, boxCode y boxData
            SendString(UserNameText.Text, socket);
            SendString(PasswordText.Text, socket);
            SendInt(selectedBoxRow, socket);
            SendInt(selectedBoxColumn, socket);            
            SendString(AccessCodeText.Text, socket);

            // Recibimos respuesta del server y la mostramos
            int response = ReceiveInt(socket);

            if (response == 0)
            {
                MessageBox.Show("OK");

                // Solo cuando sea ok, recibimos el archivo.
                int contentLenght = ReceiveInt(socket);
                byte[] content = new byte[contentLenght];
                socket.Receive(content);

                // Abrir el diálogo para guardar el archivo
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Guardar archivo";
                saveFileDialog.Filter = "Todos los archivos (*.*)|*.*";

                if (saveFileDialog.ShowDialog() == true)
                {
                    System.IO.File.WriteAllBytes(saveFileDialog.FileName, content);
                    MessageBox.Show("Archivo guardado correctamente");
                }
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
                MessageBox.Show("Caja no ocupada"); 
            }
            else if (response == -4)
            {
                MessageBox.Show("Código incorrecto");
            }

        }

        private void PutContentsButton_Click(object sender,RoutedEventArgs e)
        {
            ////////////////////////////////////////////
            // Intentar poner un fichero en la caja   //
            ////////////////////////////////////////////

            // controlamos que siempre se envíe un código para que el servidor no esté esperando infinitamente en caso de error
            if (AccessCodeText.Text == "")
            {
                MessageBox.Show("Debes introducir un código en acces code");
                return;
            }

            Socket socket = CreateSocketAndConnectServer(boxPort);

            // Primero enviamos el num que determina la orden que va a recibir el servidor

            int code = (int)BoxOpCode.put;
            SendInt(code, socket);

            // Después enviamos user, password, boxRow, boxColumn y boxCode
            SendString(UserNameText.Text, socket);
            SendString(PasswordText.Text, socket);
            SendInt(selectedBoxRow, socket);
            SendInt(selectedBoxColumn, socket);
            SendString(AccessCodeText.Text, socket);
                      

            // Abrir diálogo para seleccionar fichero
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Selecciona un archivo";
            openFileDialog.Filter = "Todos los archivos (*.*)|*.*";

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                // Ruta del fichero seleccionado
                string filePath = openFileDialog.FileName;

                // Leer todo el archivo en bytes
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

                // para que el servidor sepa si va a recibir o no archivo, enviamos un dato de control (1 para true, 0 para false), usamos int para reutilizar función
                int archiveSent = 1;
                SendInt(archiveSent, socket);

                // AQUÍ ya tengo los bytes listos para enviar
                int fileBytesLenght = fileBytes.Length;
                SendInt(fileBytesLenght, socket);
                socket.Send(fileBytes);
            }
            else
            {
                MessageBox.Show("No se seleccionó ningún archivo");
                // para que el servidor sepa si va a recibir o no archivo, enviamos un dato de control (1 para true, 0 para false), usamos int para reutilizar función
                int archiveSent = 0;
                SendInt(archiveSent, socket);

                return;
            }
             

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
                MessageBox.Show("Caja ocupada");
            }
        }

        private void ClearContentsButton_Click(object sender,RoutedEventArgs e)
        {
            ///////////////////////////////////////////////
            // Intentar eliminar el contenido de la caja //
            ///////////////////////////////////////////////
            
            // controlamos que siempre se envíe un código para que el servidor no esté esperando infinitamente en caso de error
            if (AccessCodeText.Text == "")
            {
                MessageBox.Show("Debes introducir un código en acces code");
                return;
            }

            Socket socket = CreateSocketAndConnectServer(boxPort);

            // Primero enviamos el num que determina la orden que va a recibir el servidor

            int code = (int)BoxOpCode.clear;
            SendInt(code, socket);

            // Después enviamos user, password, boxRow, boxColumn y boxCode
            SendString(UserNameText.Text, socket);
            SendString(PasswordText.Text, socket);
            SendInt(selectedBoxRow, socket);
            SendInt(selectedBoxColumn, socket);
            SendString(AccessCodeText.Text, socket);

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
                MessageBox.Show("Caja no ocupada");
            }
            else if (response == -4)
            {
                MessageBox.Show("Código incorrecto");
            }
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

            StatusRequest();

        }

        private void Box0BButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "0B";

            selectedBoxRow = 0;
            selectedBoxColumn = 1;

            StatusRequest();
        }

        private void Box0CButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "0C";

            selectedBoxRow = 0;
            selectedBoxColumn = 2;

            StatusRequest();
        }

        private void Box0DButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "0D";

            selectedBoxRow = 0;
            selectedBoxColumn = 3;

            StatusRequest();
        }

        private void Box1AButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "1A";

            selectedBoxRow = 1;
            selectedBoxColumn = 0;

            StatusRequest();
        }

        private void Box1BButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "1B";

            selectedBoxRow = 1;
            selectedBoxColumn = 1;

            StatusRequest();

        }

        private void Box1CButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "1C";

            selectedBoxRow = 1;
            selectedBoxColumn = 2;

            StatusRequest();

        }

        private void Box1DButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "1D";

            selectedBoxRow = 1;
            selectedBoxColumn = 3;

            StatusRequest();

        }

        private void Box2AButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "2A";

            selectedBoxRow = 2;
            selectedBoxColumn = 0;

            StatusRequest();

        }

        private void Box2BButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "2B";

            selectedBoxRow = 2;
            selectedBoxColumn = 1;

            StatusRequest();
        }

        private void Box2CButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "2C";

            selectedBoxRow = 2;
            selectedBoxColumn = 2;

            StatusRequest();
        }

        private void Box2DButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "2D";

            selectedBoxRow = 2;
            selectedBoxColumn = 3;

            StatusRequest();
        }

        private void Box3AButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "3A";

            selectedBoxRow = 3;
            selectedBoxColumn = 0;

            StatusRequest();
        }

        private void Box3BButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "3B";

            selectedBoxRow = 3;
            selectedBoxColumn = 1;

            StatusRequest();
        }

        private void Box3CButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "3C";

            selectedBoxRow = 3;
            selectedBoxColumn = 2;

            StatusRequest();
        }

        private void Box3DButton_Click(object sender,RoutedEventArgs e)
        {
            SelectedBoxText.Text = "3D";

            selectedBoxRow = 3;
            selectedBoxColumn = 3;

            StatusRequest();
        }
        
    }
}