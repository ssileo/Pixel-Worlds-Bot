using PixelWorldsBot.BSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixelWorldsBot
{
    public class World
    {
        public string name;
        public long loadedAt;
        public int worldSizeX;
        public int worldSizeY;
        public int mainDoorX;
        public int mainDoorY;

        public List<World.Tile> Tiles;
        public List<BSONObject> worldItemsDataList;
        public BSONObject worldItemsData;
        public Tile[,] tiles;

        public BSONObject worldLock;

        public void SetupWorld(string _name, byte[] data)
        {
            this.loadedAt = Utils.GetTimeStamp();

            worldItemsDataList = new List<BSONObject>();
            worldItemsData = new BSONObject();
            Tiles = new List<Tile>();
            BSONObject bson = SimpleBSON.Load(data);

            this.name = _name;
            

            this.worldSizeX = bson["WorldSizeSettingsType"]["WorldSizeX"].int32Value;
            this.worldSizeY = bson["WorldSizeSettingsType"]["WorldSizeY"].int32Value;
            this.tiles = new Tile[this.worldSizeX, this.worldSizeY];

            this.mainDoorX = bson["WorldStartPoint"]["x"].int32Value;
            this.mainDoorY = bson["WorldStartPoint"]["y"].int32Value;

            var fgLayer = bson["BlockLayer"].binaryValue;
            var bgLayer = bson["BackgroundLayer"].binaryValue;

            for (int y = 0; y < this.worldSizeY - 1; y++)
            {
                for (int x = 0; x < this.worldSizeX - 1; x++)
                {
                    var index = x + y * this.worldSizeX;
                    int start = index * 2;
                    int end = index * 2 + 2;

                    byte[] fgLayerCur = fgLayer.Skip(start).Take(end).ToArray();
                    byte[] bgLayerCur = bgLayer.Skip(start).Take(end).ToArray();

                    Int16 foregroundID = BitConverter.ToInt16(fgLayerCur, 0);
                    Int16 backgroundID = BitConverter.ToInt16(bgLayerCur, 0);
                    Tiles.Add(new Tile(foregroundID, backgroundID));

                    tiles[x, y] = new Tile(foregroundID, backgroundID);
                }
            }


            BSONObject worldItemsBson = bson["WorldItems"] as BSONObject;
            worldItemsData = worldItemsBson;

            foreach (string obj in worldItemsBson.Keys)
            {
                BSONObject bsonobject2 = worldItemsBson[obj] as BSONObject;
                //Utils.ReadBSON(bsonobject2, new Logger());
                worldItemsDataList.Add(bsonobject2);
            }
            worldLock = new BSONObject();
            //SetLockWorldHelper();
        }

        public int GetBlockAt(int x, int y)
        {
            return tiles[x, y].foreID;
        }

        public void SetLockWorldHelper()
        {
            if (this.worldItemsData != null)
            {
                foreach (BSONObject data in worldItemsData)
                {
                    int blockType = data["blockType"].int32Value;
                    if (blockType == 413 || // LockWorld
                        blockType == 414 || // LockGold
                        blockType == 415 || // LockDiamond 
                        blockType == 416 || // LockClan
                        blockType == 796 || // LockPlatinum
                        blockType == 882 || // LockWorldDark
                        blockType == 882 || // LockWorldDark
                        blockType == 1131 || // LockWorldBattle
                        blockType == 2212 || // LockWorldNoob
                        blockType == 3606 // LockWorldBattleFaction
                        )
                    {
                        worldLock = data;
                        // this.logger.LogBSON(worldLock);
                    }
                }
            }
        }


        public class Tile
        {
            public Int16 foreID;
            public Int16 backID;
            public Tile(Int16 _foreID, Int16 _backID)
            {
                this.foreID = _foreID;
                this.backID = _backID;
            }
        }
    }
}
