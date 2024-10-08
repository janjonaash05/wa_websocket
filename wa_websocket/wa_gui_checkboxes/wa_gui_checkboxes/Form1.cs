using Microsoft.VisualBasic.Devices;
using Newtonsoft.Json;
using System.Dynamic;
using System.Net;
using System.Net.WebSockets;
using System.Security.Policy;
using System.Text;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace wa_gui_checkboxes
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        delegate void AddButtonCallback(Button b);

        Color onColor = Color.Blue;
        Color offColor = Color.Gray;


        WebSocket ws;

        Dictionary<int, Button> idButtonDict;

        bool buttonsCreated = false;

        CancellationTokenSource cts = new();

        CancellationToken cancellationToken;
        private void Form1_Load(object sender, EventArgs e)
        {
            cancellationToken = cts.Token;
            idButtonDict = new Dictionary<int, Button>();

            Task.Run(() => OpenConnectionAsync(cancellationToken)).ConfigureAwait(false);

        }

        private void Ws_OnMessage(string json)
        {

            dynamic data = JsonConvert.DeserializeObject<dynamic>(json);
            if (!buttonsCreated) 
            {
                int i = 0;
                foreach (dynamic d in data.data)
                {
                    //  MessageBox.Show(d.name);
                    i++;
                    var b = new Button
                    {
                        Name = "b" + (int)d.id,
                        Text = (string)d.name,
                        BackColor = (bool)d.state ? onColor : offColor,
                        Size = new Size(150, 50),
                        Font = new System.Drawing.Font("Segoe UI", 15F),
                };
                    b.Click += Button_Click;
                    AddButton(b);
                    idButtonDict.Add(i, b);
                };
                buttonsCreated = true;
                return;
            }

            foreach (dynamic d in data.data)
            {
            //    MessageBox.Show((int)d.id + " "+ (bool)d.state);
                Button button = idButtonDict[(int) d.id];
                button.Text = (string) d.name;
                button.BackColor = (bool) d.state ? onColor : offColor;
            }

        }


        void AddButton(Button b) 
        {
          
            if (this.flowLayoutPanel1.InvokeRequired)
            {
                AddButtonCallback add = new AddButtonCallback(AddButton);
                this.Invoke(add, new object[] { b });
            }
            else
            {
                flowLayoutPanel1.Controls.Add(b);
            }

        }

        private void Button_Click(object sender, EventArgs e)
        {
            var button = sender as Button;
            button.BackColor = button.BackColor == onColor ? offColor : onColor;
            dynamic changedButton = new ExpandoObject();
            changedButton.id = button.Name.ToString().Substring(1);
            changedButton.type = "changeState";
            changedButton.state = (button.BackColor == onColor).ToString().ToLower();

            var json = JsonConvert.SerializeObject(changedButton);
            SendAsync(json, cancellationToken);
        }

        ClientWebSocket wsClient = new ClientWebSocket();

        public async Task OpenConnectionAsync(CancellationToken token)
        {

            //Set keep alive interval
            wsClient.Options.KeepAliveInterval = TimeSpan.Zero;

            //Add options if compression is needed
            wsClient.Options.DangerousDeflateOptions = new WebSocketDeflateOptions
            {
                ServerContextTakeover = true,
                ClientMaxWindowBits = 15
            };

            await wsClient.ConnectAsync(new Uri("ws://tlacitka.thastertyn.xyz"), token).ConfigureAwait(false);
           _ = HandleMessagesAsync(token);
        }

        //Send message
        public async Task SendAsync(string message, CancellationToken token)
        {
            var messageBuffer = Encoding.UTF8.GetBytes(message);
            await wsClient.SendAsync(new ArraySegment<byte>(messageBuffer), WebSocketMessageType.Text, true, token).ConfigureAwait(false);
        }

        //Receiving messages
        private async Task ReceiveMessageAsync(byte[] buffer)
        {
            while (true)
            {
                try
                {
                    //MessageBox.Show("te pero");
                    var result = await wsClient.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).ConfigureAwait(false);

                    //Here is the received message as string
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    Ws_OnMessage(message);




                  
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    break;
                }
            }
        }

        public async Task HandleMessagesAsync(CancellationToken token)
        {
            var buffer = new byte[1024 * 4];
            while (wsClient.State == WebSocketState.Open)
            {
                await ReceiveMessageAsync(buffer);
            }
            if (wsClient.State != WebSocketState.Open)
            {
                MessageBox.Show(wsClient.State.ToString());
            }
        }

        class ButtonData
        {
            public int id;
            public string name;
            public bool isChecked;
        }
    }
}
