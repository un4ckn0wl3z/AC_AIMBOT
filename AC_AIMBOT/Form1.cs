using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Memory;
using System.Threading;
using System.Runtime.InteropServices;

namespace AC_AIMBOT
{
    public partial class Form1 : Form
    {

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);

        #region MEM

        string PLAYERBASE = "ac_client.exe+0x109B74";
        string ENTITYLIST = "ac_client.exe+0x110D90";
        string HP = ",0xF8";
        string X = ",0x4";
        string Y = ",0x8";
        string Z = ",0xC";
        string VIEW_Y = ",0x44";
        string VIEW_X = ",0x40";

        #endregion


        Mem m = new Mem();


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            int PID = m.GetProcIdFromName("ac_client");
            if (PID > 0)
            {
                m.OpenProcess(PID);
                Thread ABThread = new Thread(Aimbot) { IsBackground = true };
                ABThread.Start();
            }
        }
    
        void Aimbot()
        {
            while (true)
            {

                if (GetAsyncKeyState(Keys.X) < 0)
                {
                    var LocalPlayer = GetLocal();
                    var Players = GetPlayers(LocalPlayer);
                    Players = Players.OrderBy(o => o.Magnitude).ToList();
                    if (Players.Count != 0)
                    {
                        Aim(LocalPlayer, Players[0]);
                    }

                }

                Thread.Sleep(2);

            }
        }

        Player GetLocal()
        {
            var Player = new Player 
            {
                X = m.ReadFloat(PLAYERBASE + X),
                Y = m.ReadFloat(PLAYERBASE + Y),
                Z = m.ReadFloat(PLAYERBASE + Z),
            };

            return Player;

        }

        List<Player> GetPlayers(Player local)
        {
            var players = new List<Player>();

            for (int i=0; i < 20; i++)
            {
                var currentString = ENTITYLIST + ",0x" + (i * 0x4).ToString("x");

                var Player = new Player
                {
                    X = m.ReadFloat(currentString + X),
                    Y = m.ReadFloat(currentString + Y),
                    Z = m.ReadFloat(currentString + Z),
                    Health = m.ReadInt(currentString + HP),
                };
                Player.Magnitude = GetMag(local, Player);

                if (Player.Health > 0 && Player.Health < 102)
                {
                    players.Add(Player);
                }
            }
            return players;
        }
    
        void Aim(Player player, Player ememy)
        {
            float deltaX = ememy.X - player.X;
            float deltaY = ememy.Y - player.Y;
            float deltaZ = ememy.Z - player.Z;

            float viewX = (float)(Math.Atan2(deltaY, deltaX) * 180 / Math.PI) + 90;
            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
            float viewY = (float)(Math.Atan2(deltaZ, distance) * 180 / Math.PI);

            m.WriteMemory(PLAYERBASE + VIEW_X, "float", viewX.ToString());
            m.WriteMemory(PLAYERBASE + VIEW_Y, "float", viewY.ToString());

        }

        float GetMag(Player player, Player entity)
        {
            float mag;
            mag = (float)Math.Sqrt(Math.Pow(entity.X - player.X, 2) +
                                   Math.Pow(entity.Y - player.Y, 2) +
                                   Math.Pow(entity.Z - player.Z, 2));
            return mag;

        }
    }
}
