using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Xml.Serialization;

namespace App
{

    public partial class Form1 : Form
    {

        static string pathDown;
        static int serverPort = default;
        static int clientPort = default;
        static string localHost = "127.0.0.1";
        static IPAddress ip = IPAddress.Parse (localHost);
        static Socket serverSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

        Thread receiveThread = new Thread (() => { });
        Thread startThreadListener = new Thread (() => { });
        NetworkStream ns;

        delegate void forInvoke (Socket clientSocket); //делегат для того, чтобы иметь доступ к элементам управления формы из разных потоков
        forInvoke invoke; 

        public Form1 ()
        {
            InitializeComponent ();

            comboBox1.SelectedIndex = 0;
            textBoxServPort.Text = 8004.ToString ();
            textBoxClPort.Text = 8004.ToString ();
            textBoxServPort.KeyPress += textBox_KeyPress;
            textBoxClPort.KeyPress += textBox_KeyPress;
        }

        
        public void BinaryFormat (Socket clientSocket)
        {
            Person person = new Person ("Binarov", 22);

            BinaryFormatter formatter = new BinaryFormatter ();
            ns = new NetworkStream (clientSocket);
            formatter.Serialize (ns, person);
        }

        public void JSONFormat (Socket clientSocket)
        {
            List <Student> students = new List <Student> ();

            students.Add (new Student ("Ivanov", 25));
            students.Add (new Student ("Ivanova", 20));

            ns = new NetworkStream (clientSocket);

            var JsonFormatter = new DataContractJsonSerializer (typeof (List <Student>));
            JsonFormatter.WriteObject (ns, students);
        }

        public void XMLFormat (Socket clientSocket)
        {
            var persons = new List <Person> ();

            persons.Add (new Person ("Petrov", 24));
            persons.Add (new Person ("Petrova", 19));

            ns = new NetworkStream (clientSocket);
            var xmlFormatter = new XmlSerializer (typeof (List <Person>));
            xmlFormatter.Serialize (ns, persons);
        }

        public void Receive (Socket clientSocket)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                {
                    BinaryFormatter binaryFormat = new BinaryFormatter ();
                    ns = new NetworkStream (clientSocket);
                    Person person = (Person) binaryFormat.Deserialize (ns);                       
                    ns.Close ();

                    MessageBox.Show ("Данные \"" + person.Name + "\" \"" + person.Age  + "\" получены.", "Результат работы программы");

                    break;
                }
                case 1:
                {
                    DataContractJsonSerializer jsonFormat = new DataContractJsonSerializer (typeof (List <Student>));
                    ns = new NetworkStream (clientSocket);
                    var students = jsonFormat.ReadObject (ns) as List <Student>;
                    using(var file = new FileStream (pathDown + "file.json", FileMode.Create))
                        jsonFormat.WriteObject (file, students);
                    ns.Close ();

                    MessageBox.Show ($"Файл file.json сохранен.", "Результат работы программы");

                    break;
                }
                case 2:
                {
                    XmlSerializer xmlFormat = new XmlSerializer (typeof (List <Person>));
                    ns = new NetworkStream (clientSocket);
                    var persons = xmlFormat.Deserialize (ns) as List <Person>;
                    using (var file = new FileStream (pathDown + "file.xml", FileMode.Create))
                        xmlFormat.Serialize (file, persons);
                    ns.Close ();

                    MessageBox.Show ($"Файл file.xml сохранен.", "Результат работы программы");

                    break;
                }
            }

            clientSocket.Close ();
            receiveThread.Abort ();
        }

        private void Send (int clientPort, string filename = "")
        {
            IPAddress ipAddress = IPAddress.Parse (localHost);
            IPEndPoint clientEP = new IPEndPoint (ipAddress, clientPort);
            Socket clientSocket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            clientSocket.Connect (clientEP);

            switch (comboBox1.SelectedIndex)
            {
                case 0:
                {
                    BinaryFormat (clientSocket);
                    ns.Close ();
                    break;
                }
                case 1:
                {
                    JSONFormat (clientSocket);
                    ns.Close ();
                    break;
                }
                case 2:
                {
                    XMLFormat (clientSocket);
                    ns.Close ();
                    break;
                }
            }

            clientSocket.Close ();
        }

        private void Form1_FormClosed (object sender, FormClosedEventArgs e)
        {
            serverSocket.Close ();
            startThreadListener.Abort ();
            receiveThread.Abort ();
        }

        private void button1_Click (object sender, EventArgs e)
        {
            if (textBoxClPort.Text != string.Empty)
            {
                clientPort = Int32.Parse (textBoxClPort.Text);
                Send (clientPort); 
            }
            else
                MessageBox.Show ("Введите номер порта.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button2_Click (object sender, EventArgs e)
        {
            if (textBoxServPort.Text != string.Empty)
            {
                button2.Enabled = false;
                serverPort = Int32.Parse (textBoxServPort.Text);
                IPEndPoint serverEP = new IPEndPoint (ip, serverPort);
                invoke = Receive;

                startThreadListener = new Thread (() =>
                {
                    serverSocket.Bind (serverEP);
                    serverSocket.Listen (serverPort);
                    while (true)
                    {
                        Socket clientSocket = serverSocket.Accept ();
                        receiveThread = new Thread (() =>
                        {
                            Invoke (invoke, clientSocket);
                        });
                        receiveThread.Start ();
                    }
                });

                startThreadListener.Start ();
            }
            else
                MessageBox.Show ("Введите номер порта.", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void button3_Click (object sender, EventArgs e)
        {
            FolderBrowserDialog FBD = new FolderBrowserDialog ();

            FBD.ShowNewFolderButton = false;

            if (FBD.ShowDialog () == DialogResult.OK)
                pathDown = FBD.SelectedPath + @"\";

            button3.Enabled = false;
        }

        private void textBox_KeyPress (object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit (e.KeyChar) && !char.IsControl (e.KeyChar);
        }


        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
