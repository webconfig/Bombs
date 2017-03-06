using BombsServer.Network;
using Comm.Network.Iocp;
using google.protobuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BombsServer.Game
{
    public class RoomManager
    {
        private Dictionary<int, Room> rooms = new Dictionary<int, Room>();

        /// <summary>
        /// 读取配置文件
        /// </summary>
        /// <param name="filePath"></param>
        private void LoadData(string filePath)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(filePath);
            XmlNode root = xml.LastChild;
            foreach (XmlNode item in root.ChildNodes)
            {
                switch (item.Name)
                {
                    case "rooms":
                        foreach (XmlNode obj in item.ChildNodes)
                        {
                            Room room = new Room();
                            room.Data = new google.protobuf.RooomItem();
                            room.Data.id = Convert.ToByte(obj.Attributes["id"].Value);
                            room.Data.name = obj.Attributes["name"].Value;
                            room.Data.max= Convert.ToByte(obj.Attributes["max"].Value);
                            room.scene = GameEngine.GameManager.Instance.CreateScene(GameEngine.GameManager.GetXmlAttrInt(obj, "sceneid"));
                            rooms.Add(room.Data.id, room);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="config"></param>
        public void Init(string config)
        {
            LoadData(config);
        }

        /// <summary>
        /// 获取所有房间信息
        /// </summary>
        /// <returns></returns>
        public Roooms GetAllRoomMsg()
        {
            Roooms datas = new Roooms();
            foreach(var item in rooms)
            {
                datas.datas.Add(item.Value.Data);
            }
            return datas;
        }


        /// <summary>
        /// 加入房间
        /// </summary>
        /// <param name="room_id"></param>
        public void Join(int room_id,Session<Player> player)
        {
            if(!rooms.ContainsKey(room_id))
            {
                return ;
            }

            rooms[room_id].AddPlayer(player);
        }

        /// <summary>
        /// 加入房间
        /// </summary>
        /// <param name="room_id"></param>
        public void PlayerReady(int room_id, int  player_id)
        {
            if (!rooms.ContainsKey(room_id))
            {
                return;
            }

            rooms[room_id].PlayerReady(player_id);
        }
    }
   
}
