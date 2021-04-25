using DALServer;
using MessageServerClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestClient
{
    public partial class Form1 : Form
    {
        Socket sendSocket = null;
        int Id;
        List<TestDLL.Test> testDLL;
        TestDLL.Result testResult;
        public Form1()
        {
            InitializeComponent();
            sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPHostEntry iPHost = Dns.GetHostEntry("localhost");
            IPAddress iPAddress = iPHost.AddressList[1];//Мережева картка
            int port = 33333;
            IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, port);

            testResult = new TestDLL.Result();

            sendSocket.Connect(iPEndPoint);

            groupBox2.Visible = false;

            Thread thread = new Thread(ReseiveServerMsg);
            thread.IsBackground = true;
            thread.Start(sendSocket);
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Mess mess = new Mess();
            mess.type = "join";
            User user = new User();
            user.Login = textBox1.Text;
            user.Password = textBox2.Text;

            BinaryFormatter binary = new BinaryFormatter();

            MemoryStream stream = new MemoryStream();
            binary.Serialize(stream, user);
            mess.info = stream.ToArray();


            stream = new MemoryStream();
            binary.Serialize(stream, mess);

            byte[] sendData = stream.ToArray();

            sendSocket.Send(sendData);
        }

        private void ReseiveServerMsg(object sender)
        {
            while (true) //постійно читаєм( чекаєм на повідомлення сервера)
            {

                Socket receiveSocked = sender as Socket;
                if (receiveSocked == null) throw new ArgumentException("Receive Socket Exception");
                Byte[] receivebyte = new Byte[8192];
                //Читання

                Int32 nCount = receiveSocked.Receive(receivebyte);//Receive() -  блокуюча функція - чекає доки не буде повідомлення

                MemoryStream ms = new MemoryStream(receivebyte);
                BinaryFormatter bf = new BinaryFormatter();
                Mess d = (Mess)bf.Deserialize(ms);

                switch (d.type)
                {
                    case "join":
                        if (d.id != 0)
                        {
                            groupBox1.Invoke(new Action(() => groupBox1.Visible = false));
                            Id = d.id;
                            groupBox2.Invoke(new Action(() => { groupBox2.Visible = true;groupBox2.Text = textBox1.Text; }));

                        }
                        else
                        {
                            Byte[] receive = new Byte[1024];
                            Int32 cont = d.info.Length;
                            String receiceString = Encoding.ASCII.GetString(receive, 0, cont);
                            MessageBox.Show(receiceString);
                        }
                        break;
                    case "Tests":
                        MemoryStream mS = new MemoryStream(d.info);

                        BinaryFormatter bF = new BinaryFormatter();
                        List<TestDLL.Test> tests = (List<TestDLL.Test>)bF.Deserialize(mS);
                        testDLL = tests;
                        dataGridView1.Invoke(new Action(() => { dataGridView1.DataSource = null; dataGridView1.DataSource = tests; })) ;
                        break;
                    case "passes":
                        MemoryStream mSs = new MemoryStream(d.info);

                        BinaryFormatter bFf = new BinaryFormatter();
                        List<TestDLL.Result> testss = (List<TestDLL.Result>)bFf.Deserialize(mSs);
                        dataGridView1.Invoke(new Action(() => { dataGridView1.DataSource = null; dataGridView1.DataSource = testss; }));
                        break;
                    default:
                        break;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button3.Visible = true;
            Mess mess = new Mess();
            mess.type = "Tests";
            mess.id = Id;
     

            BinaryFormatter binary = new BinaryFormatter();

            MemoryStream stream = new MemoryStream();

            binary.Serialize(stream, mess);

            byte[] sendData = stream.ToArray();

            sendSocket.Send(sendData);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                MessageBox.Show(  $"{testDLL[dataGridView1.SelectedRows[0].Index].Time.ToString()} for decision");
                FormPassTest formPassTest = new FormPassTest(testDLL[dataGridView1.SelectedRows[0].Index],this,testResult);
                formPassTest.ShowDialog();

                
                Mess mess = new Mess();
                mess.type = "result";
                mess.id = Id;

                BinaryFormatter binary = new BinaryFormatter();
                MemoryStream stream = new MemoryStream();


                binary.Serialize(stream, testResult);
                mess.info = stream.ToArray();

                stream = new MemoryStream();
                binary.Serialize(stream, mess);

                byte[] sendData = stream.ToArray();

                sendSocket.Send(sendData);


            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            button3.Visible = false;
            Mess mess = new Mess();
            mess.type = "passes";
            mess.id = Id;


            BinaryFormatter binary = new BinaryFormatter();

            MemoryStream stream = new MemoryStream();

            binary.Serialize(stream, mess);

            byte[] sendData = stream.ToArray();

            sendSocket.Send(sendData);
        }
    }
}
