using BepInEx;
using BepInEx.Configuration;
using TwitchAPI;
using FullSerializer;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TwitchIRC;
using UnityEngine;
using TwitchAPI.Polls;

namespace TwitchAPI
{
    [BepInDependency("etgmodding.etg.mtgapi")]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class TwitchAPI : BaseUnityPlugin
    {
        public const string GUID = "panda.etg.twitchAPI";
        public const string NAME = "TWITCHAPI";
        public const string VERSION = "1.1.0";
        public const string TEXT_COLOR = "#6441a5";

        public void Start()
        {
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
            instance = this;
        }

        static ConfigFile twitchConfig;
        static ConfigEntry<string> ChannelName;
        static string randomLoginString = "SCHMOOPIE";
        static string anonLoginUser = "justinfan1337";

        public void GMStart(GameManager g)
        {
            twitchConfig = base.Config;
            ChannelName = twitchConfig.Bind<string>("TwitchAPI:", "ChannelName", " ", "The name of your channel");
            ETGModConsole.Commands.AddGroup("tapi:start", new Action<string[]>(this.Initilize));
            ETGModConsole.Commands.AddGroup("tapi:toggle", new Action<string[]>(this.ToggleIntegration));
            ETGModConsole.Commands.AddGroup("tapi:reload", new Action<string[]>(this.Reload));
            ETGModConsole.Commands.AddGroup("tapi:setchannel", new Action<string[]>(this.SetChannelName));
            //ETGModConsole.Commands.AddGroup("tapi:tapi", new Action<string[]>(this.DebugTapi));
            GameManager.Instance.gameObject.AddComponent<PollUIController>();
            GameManager.Instance.gameObject.AddComponent<MainPollController>();

            Log("Twitch API " + VERSION + " loaded successfully. Type \"tapi:toggle\" to turn it on", TEXT_COLOR);
        }
        public void DebugTapi(string[] args)
        {
            ui.panelOut = !ui.panelOut;
        }
        public static void Log(string text, string color = "#FFFFFF")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }
        public void Reload(string[] args)
        {
            Config.Reload();
        }

        public void SetChannelName(string[] args)
        {
            
            if(args==null || args.Length==0 || args[0].IsNullOrWhiteSpace())
            {
                Log("you must enter a channel name");
            }
            else
            {
                ChannelName.Value = args[0];
                Config.Reload();
                Log("channel name has been set to:" + ChannelName.Value);
            }
        }
        public void Initilize(string[] args)
        {
            if (!ChannelName.Value.IsNullOrWhiteSpace())
            {
                if (TwitchAPI.listener == null)
                {
                    TwitchAPI.listener = new ChatListener(anonLoginUser, randomLoginString, ChannelName.Value);
                    TwitchAPI.listener.Connect();
                    TwitchAPI.listener.OnChatMessage += ActivateGlobalOnChatMessageDelegate;
                    TwitchAPI.listener.StartListening();
                }
                else if (!listener.Connected)
                {
                    listener.Connect();
                }
                if (TwitchAPI.listener.Connected) 
                {
                    TwitchAPI.IntegrationEnabled = true;
                }
                else
                {
                    ETGModConsole.Log("TwitchAPI had trouble connecting to twitch. Please check the channel name is set properly.");
                }
                
            }
            else
            {
                Log("Seems as though The config file has not been filled yet.");
                Log("You can set your channel name by typing in the console \"tapi:setchannel <channelname>\" ");
                Log("Alternatively you can edit your config file via the mod manage and then type in console \"tapi:reload\"");
                Log("You should only need to do either of these actions once, unless you want to switch the channel youre joining");
                Log("after setting the config, try starting twitch mode again");
            }

            TwitchAPI.LogActiveStatus();
        }
        //mostly stolen from kyle, but basically tries to load info from file and start listening to chat, or stop listening to chat to disable twitch mod. thanks kyle (:
        public void ToggleIntegration(string[] args)
        {
            if (!TwitchAPI.IntegrationEnabled && listener!=null && listener.Connected)
            {
                TwitchAPI.IntegrationEnabled = true;
            }
            else
            {
                TwitchAPI.IntegrationEnabled = false;
            }
            TwitchAPI.LogActiveStatus();
        }

        void OnApplicationQuit()
        {
            if (TwitchAPI.listener != null)
            {
                TwitchAPI.listener.StopListening();
            }
            TwitchAPI.IntegrationEnabled = false;
        }
        // fancy little status logger stolen from kyle. thanks kyle (:
        public static void LogActiveStatus()
        {
            string color = TwitchAPI.IntegrationEnabled ? "<color=#00FF00FF>" : "<color=#FF0000FF>";
            string text = TwitchAPI.IntegrationEnabled ? "enabled" : "disabled";
            ETGModConsole.Log("TwitchAPI " + color + text + "</color>", false);
        }

        public static ChatListener.ChatDelegate GlobalChatDelegate;
        public delegate void ToggleStatusNotification(bool status);
        public static ToggleStatusNotification GlobalToggleStatusNotification;

        static void ActivationNotification(bool status)
        {

        }
        static void ActivateGlobalOnChatMessageDelegate(string user, string message, string channel)
        {
            if(IntegrationEnabled) 
                GlobalChatDelegate(user, message, channel);
        }
        private static bool integrationEnabled = false;
        public static bool IntegrationEnabled
        {
            get { return IntegrationEnabled; }
            set
            {
                IntegrationEnabled = value;
                GlobalToggleStatusNotification(value);
            }
        }


        public static bool hasBeenStarted = false;

        public static ChatListener listener = null;

        public static TwitchAPI instance;

        public static PollUIController ui = null;

        static string logFilePath;
    
    }
}
