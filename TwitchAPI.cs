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

namespace TwitchAPI
{
    [BepInDependency("etgmodding.etg.mtgapi")]
    [BepInPlugin(GUID, NAME, VERSION)]
    public class TwitchAPI : BaseUnityPlugin
    {
        public const string GUID = "panda.etg.twitchAPI";
        public const string NAME = "TWITCHAPI";
        public const string VERSION = "1.0.0";
        public const string TEXT_COLOR = "#6441a5";

        public void Start()
        {
            ETGModMainBehaviour.WaitForGameManagerStart(GMStart);
        }

        static ConfigFile twitchConfig;
        static ConfigEntry<string> ChannelName;
        static string randomLoginString = "SCHMOOPIE";
        static string anonLoginUser = "justinfan1337";

        public void GMStart(GameManager g)
        {
            twitchConfig = base.Config;
            ChannelName = twitchConfig.Bind<string>("TwitchAPI:", "ChannelName", " ", "The name of your channel");
            ETGModConsole.Commands.AddGroup("twitchapi:toggle", new Action<string[]>(this.ToggleIntegration));
            ETGModConsole.Commands.AddGroup("twitchapi:reload", new Action<string[]>(this.Reload));
            Log("Twitch API " + VERSION + " loaded successfully. Type \"twitchapi:toggle\" to turn it on", TEXT_COLOR);
        }

        public static void Log(string text, string color = "#FFFFFF")
        {
            ETGModConsole.Log($"<color={color}>{text}</color>");
        }
        public void Reload(string[] args)
        {
            Config.Reload();
            ChannelName = twitchConfig.Bind<string>("TwitchAPI:", "ChannelName", " ", "The name of your channel");
        }
        //mostly stolen from kyle, but basically tries to load info from file and start listening to chat, or stop listening to chat to disable twitch mod. thanks kyle (:
        public void ToggleIntegration(string[] args)
        {
            if (!TwitchAPI.integrationEnabled)
            {
                if (!ChannelName.Value.IsNullOrWhiteSpace())
                {
                    if (TwitchAPI.listener != null && TwitchAPI.listener.Connected)
                    {
                        TwitchAPI.listener.StopListening();
                        TwitchAPI.integrationEnabled = false;
                    }
                    if (TwitchAPI.listener == null)
                    {
                        TwitchAPI.listener = new ChatListener(anonLoginUser, randomLoginString, ChannelName.Value);
                        TwitchAPI.listener.Connect();
                        TwitchAPI.listener.OnChatMessage += ActivateGlobalDelegate;
                        TwitchAPI.listener.StartListening();
                        TwitchAPI.integrationEnabled = true;
                    }
                    else if (!listener.Connected)
                    {
                        listener.Connect();
                        TwitchAPI.listener.StartListening();
                        TwitchAPI.integrationEnabled = true;
                    }                  
                }
                else
                {
                    Log("Seems as though The config file has not been filled yet.");
                    Log("You may minimize the game, fill the config file and then type \"twitchapi:reload\" in the console to reload the config file");
                    Log("after reloading the config, try toggling twitch integration on again");
                }
            }
            else
            {
                this.Disable();
            }
            TwitchAPI.LogActiveStatus();
        }
        //stops listening to irc responses from twitch. Related events will not trigger anymore
        public void Disable()
        {
            if (TwitchAPI.listener != null)
            {
                TwitchAPI.listener.StopListening();
            }
            TwitchAPI.integrationEnabled = false;
        }

        void OnApplicationQuit()
        {
            if (TwitchAPI.listener != null)
            {
                TwitchAPI.listener.StopListening();
            }
            TwitchAPI.integrationEnabled = false;
        }
        // fancy little status logger stolen from kyle. thanks kyle (:
        public static void LogActiveStatus()
        {
            string color = TwitchAPI.integrationEnabled ? "<color=#00FF00FF>" : "<color=#FF0000FF>";
            string text = TwitchAPI.integrationEnabled ? "enabled" : "disabled";
            ETGModConsole.Log("EnemyRenamer Twitch Mode " + color + text + "</color>", false);
        }

        public static ChatListener.ChatDelegate GlobalChatDelegate;

        static void ActivateGlobalDelegate(string user, string message, string channel)
        {
            GlobalChatDelegate(user, message, channel);
        }
        public static bool integrationEnabled = false;

        public static ChatListener listener = null;

        public static TwitchAPI instance;
        static string logFilePath;
    
    }
}
