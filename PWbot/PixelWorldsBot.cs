using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using PixelWorldsBot.BSON;
using System.Threading.Tasks;
using System.Numerics;

namespace PixelWorldsBot
{
    public class PixelWorldsBot
    {
        // Network stuff
        public String ip = "44.194.163.69";
        public TcpClient socket;
        public List<BSONObject> packetQueue = new List<BSONObject>();
        private NetworkStream stream;
        private PacketHandler packetHandler;
        public Requests requests;
        public Send send;
        Connection connection;

        public Logger logger = new Logger();

        public World world;
        public string worldname = "";
        public Vector2 worldPosition;
        public bool spawned = false;
        private Timer _SyncTimeTick = null;

        public string name = "", pass = "";
        public string coid = "", tk = "";
        public string client_id = "";

        public Thread thr = null;
        Stopwatch watch = new Stopwatch();
        public bool awake = true;

        public bool cognito = false;
        public bool username = false;
        public bool loadPlayerFully = true;
        public bool spawnOnGWC = true;
        public bool JustSyncTime = false;

        public Dictionary<string, string> NetworkPlayers = new Dictionary<string, string>();

        public PixelWorldsBot() {
            BotManager bm = new BotManager();
            requests = new Requests(this);
            packetHandler = new PacketHandler(this);
            send = new Send(this);
        }

        public void UpdateLoggerConfig(Logger _config)
        {
            logger = _config;
        }

        public void Run(bool thread = true)
        {
            if (thread)
            {
                thr = new Thread((ThreadStart)Start);
                thr.Start();
                return;
            }

            Start();
        }

        public void Start()
        {
            worldname = worldname.ToUpper();

            requests.ConnectCognito();

            if (cognito)
            {
                requests.loginInfo = new Requests.LoginInfo()
                {
                    identityId = coid,
                    logintoken = tk
                };

            }
            else if (username)
            {
                requests.LoginWithUsernameAndPassword(name.ToUpper(), pass);
            }
            else
            {
                
            }

            Connect();
        }

        public void Connect()
        {
            client_id = "";
            this.spawned = false;
            connection = new Connection(this);
            NetworkPlayers.Clear();
            connection.Connect();
            BotManager.instance.AddBot(this);
        }

        public void Disconnect()
        {
            logger.LogMessage("Disconnected");

            if (_SyncTimeTick != null) _SyncTimeTick.Dispose();
            _SyncTimeTick = null;
            NetworkPlayers.Clear();
            this.connection.Disconnect();
            BotManager.instance.RemoveBot(this);
            OnDisconnect();
        }

        private void SyncTick(Object o)
        {
            if (!JustSyncTime)
            {
                if (!spawned)
                {
                    PushPacket(new BSONObject
                    {
                        ["ID"] = "ST",
                        ["Stime"] = Utils.GetTimeStamp()
                    });
                    return;
                }
                else if (spawned)
                {
                    this.send.SendMove(this.worldPosition);
                    return;
                }
            }

            PushPacket(new BSONObject
            {
                ["ID"] = "ST",
                ["Stime"] = Utils.GetTimeStamp()
            });
        }

        /// <summary>
        /// Adds the packet to the queue
        /// </summary>
        /// <param name="bson">The packet you would like to push to the queue</param>
        /// <param name="regardless">Add the packet to the queue even if it already contains that packet. This can cause errors.</param>
        public void PushPacket(BSONObject bson, bool regardless = false)
        {
            // Push if the queue doesnt contain the packet already

            if (regardless)
            {
                this.packetQueue.Add(bson);
                return;
            }

            bool dismiss = false;
            for (int i = 0; i < this.packetQueue.Count; i++)
            {
                if (this.packetQueue[i]["ID"] == bson["ID"])
                {
                    dismiss = true;
                    break;
                }
            }
            if (!dismiss)
            {
                this.packetQueue.Add(bson);
            }
        }

        public void SendAll()
        {
            try
            {
                if (this.connection.socket != null && this.connection.connected)
                {
                    BSONObject bson = new BSONObject(); 
                    
                    for (int i = 0; i < packetQueue.Count; i++)
                    {
                        bson["m" + i] = packetQueue[i];
                        this.logger.LogMessage("Client: ");
                        this.logger.LogBSON(packetQueue[i]);
                    }

                    bson["mc"] = this.packetQueue.Count;

                    MemoryStream memoryStream = new MemoryStream();
                    using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
                    {
                        byte[] bsonDump = SimpleBSON.Dump(bson);

                        binaryWriter.Write(bsonDump.Length + 4);
                        binaryWriter.Write(bsonDump);
                    }
                    byte[] buf = memoryStream.ToArray();
                    if (this.connection.connected)
                        this.connection.socket.GetStream().BeginWrite(buf, 0, buf.Length, null, null);
                    packetQueue.Clear();
                    OnSendAll();
                }
            }
            catch (Exception _ex)
            {
                logger.LogMessage($"{_ex}\n");
            }
        }

        private bool OnWorld()
        {
            if (this.world == null)
                return false;

            return Utils.GetTimeStamp() - this.world.loadedAt >= 2000;
        }

        public void ProcessPacket(BSONObject packet)
        {
            watch.Restart();
            logger.LogMessage("Server: ");
            logger.LogBSON(packet);

            string packetID = packet["ID"].stringValue;
            
            if (this.OnWorld() && !spawned && loadPlayerFully)
            {
                
                spawned = true;
                logger.LogMessage("On world called.");

                StopSyncing();
                worldPosition = new Vector2(-2, -2);    
                float spawn_x = world.mainDoorX / 3.2f;
                float spawn_y = world.mainDoorY / 3.2f;
                
                send.SendMove(spawn_x, spawn_y);
                StartSyncing();

               
                OnWorldLoaded();

            }

            switch (packetID)
            {
                case "OoIP":
                    this.ip = Utils.GetIPFromDNS(packet["IP"].stringValue);
                    OnOoIP(packet);
                    packetHandler.HandleOtherOwnerIPMessage(packet);
                    break;
                case "VChk":
                    OnVChk(packet);
                    packetHandler.HandleVersionNumberCheck();
                break;
                case "GPd":
                    OnGPd(packet);
                    packetHandler.HandleGetPlayerData(packet);
                    StartSyncing();
                    break;
                case "TTjW":
                    OnTTjW(packet);
                    packetHandler.HandleTryToJoinWorld(packet);
                    break;
                case "GWC":
                    packetHandler.HandleGetWorldMessage(packet);
                    if (spawnOnGWC)
                        this.send.ReadyToPlay();
                    SendAll();
                    OnGWC(packet);
                    break;
                case "AnP":
                    OnAnP(packet);
                    packetHandler.HandleAddNewPlayer(packet);
                    break;
                case "PL":
                    OnPL(packet);
                    packetHandler.HandlePlayerLeft(packet);
                    break;
                case "AC":
                    OnAC(packet);
                    break;
                #region Trade packets

                case "Trade":
                    OnTrade(packet);
                    break;

                #endregion
                case "BGM":
                    OnBGM(packet);
                    break;
                case "FRQM":
                    OnFRQM(packet);
                    break;
                case "GetWorldError":
                    OnGetWorldError(packet);
                    break;
                case "LW":
                    OnLW(packet);
                    break;
                case "ST":
                    OnST(packet);
                    break;
            }
        }

        public void StopSyncing()
        {
            if (_SyncTimeTick != null)
            {
                _SyncTimeTick.Dispose();
            }
        }

        public void StartSyncing()
        {
            _SyncTimeTick = new Timer((TimerCallback)SyncTick, null, 0, 2000);
        }


        #region events
        public virtual void OnOoIP(BSONObject packet) { }
        public virtual void OnVChk(BSONObject packet) { }
        public virtual void OnGPd(BSONObject packet) { }
        public virtual void OnLW(BSONObject packet) { }
        public virtual void OnTTjW(BSONObject packet) { }
        public virtual void OnGWC(BSONObject packet) { }
        public virtual void OnST(BSONObject packet) { }
        public virtual void OnPL(BSONObject packet) { }
        public virtual void OnAnP(BSONObject packet) { }
        public virtual void OnTrade(BSONObject packet) { }
        public virtual void OnBGM(BSONObject packet) { }
        public virtual void OnFRQM(BSONObject packet) { }
        public virtual void OnWorldLoaded() { }
        public virtual void OnGetWorldError(BSONObject packet) { }
        public virtual void OnSendAll() { }
        public virtual void OnDisconnect() { }
        public virtual void OnAC(BSONObject packet) { }
        public virtual void OnLoginFailed() { }
        #endregion


    }
}
