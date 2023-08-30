using PixelWorldsBot.BSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PixelWorldsBot
{
    class Connection
    {
        public NetworkStream stream;
        public TcpClient socket;
        public PixelWorldsBot Bot;
        public bool connected = false;

        private byte[] receivedData;
        private byte[] receiveBuffer;

        public int dataBufferSize = 4096 * 248;

        public String ip = "44.194.163.69";
        public int port = 10001;

        public Connection(PixelWorldsBot _bot)
        {
            Bot = _bot;
            this.ip = Bot.ip;
        }

        public void Connect()
        {
            connected = false;
            this.Bot.spawned = false;
            this.Bot.world = null;

            this.socket = new TcpClient
            {
                ReceiveBufferSize = dataBufferSize,
                SendBufferSize = dataBufferSize
            };

            this.receiveBuffer = new byte[dataBufferSize];

            this.socket.BeginConnect(this.ip, this.port, this.ConnectCallback, this.socket);
        }

        private void ConnectCallback(IAsyncResult _result)
        {

            this.socket.EndConnect(_result);

            if (!this.socket.Connected)
            {
                return;
            }

            this.stream = this.socket.GetStream();

            this.receivedData = new byte[dataBufferSize];

            this.stream.BeginRead(this.receiveBuffer, 0, this.dataBufferSize, ReceiveCallback, null);

            Bot.logger.LogMessage("Connected");
            connected = true;

            this.Bot.send.VChk();

            this.Bot.SendAll();

            this.Bot.send.gLSI();

        }

        private void ReceiveCallback(IAsyncResult _result)
        {
            try
            {
                int _byteLength = stream.EndRead(_result);
                if (_byteLength <= 0)
                {
                    Disconnect();
                    return;
                }

                byte[] _data = new byte[_byteLength];
                Array.Copy(receiveBuffer, _data, _byteLength);
                HandleData(_data);
                receivedData = null;
                this.stream.BeginRead(this.receiveBuffer, 0, this.dataBufferSize, this.ReceiveCallback, null);
            }
            catch
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            connected = false;
            Bot.packetQueue.Clear();
            socket = null;
            Bot.spawned = false;
            Bot.world = null;
            receivedData = null;
            receiveBuffer = null;
        }

        private void HandleData(byte[] _revdata)
        {
            byte[] data = new byte[_revdata.Length - 4];
            Buffer.BlockCopy(_revdata, 4, data, 0, data.Length);
            BSONObject packets = null;

            try
            {
                packets = SimpleBSON.Load(data);
            }
            catch
            { }

            if (packets == null || !packets.ContainsKey("mc"))
                return;

            int length = packets["mc"].int32Value;

            for (int i = 0; i < length; i++)
            {
                BSONObject packet = packets["m" + i] as BSONObject;

                this.Bot.ProcessPacket(packet);
            }

            Bot.SendAll();
        }


    }
}
