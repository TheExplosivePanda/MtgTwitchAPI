using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using BepInEx;

namespace TwitchAPI.Polls
{
    public class MainPollController : MonoBehaviour
    {       
        public static MainPollController instance;

        private Queue<Poll> pollQueue = new Queue<Poll>();

        int downTime = 10;
        float downTimer = 0;
        bool isPollActive = false;
        float pollTimer = 0;
        int pollTimerInt = 0;
        Poll activePoll = null;
        Dictionary<string,string> Voters = new Dictionary<string,string>();

        void Update()
        {
            if (pollQueue.Count > 0 && downTimer <= 0 && !isPollActive && TwitchAPI.integrationEnabled && !GameManager.Instance.IsFoyer && GameManager.Instance.PrimaryPlayer)
            {
                ActivatePoll(pollQueue.Dequeue());
            }
            if (!isPollActive && downTimer >= 0)
            {
                downTimer -= Time.unscaledDeltaTime;
            }
            if (isPollActive)
            {
                pollTimer -= Time.unscaledDeltaTime;
                if (pollTimer + 1 < pollTimerInt)
                {
                    pollTimerInt = (int)Math.Floor(pollTimer) + 1;
                    TwitchAPI.ui.UpdateTimer(pollTimerInt, activePoll.time);
                    
                }
                if (pollTimer <= 0) 
                {
                    ConcludePoll(activePoll);
                }
            }
        }
        void Start()
        {
            instance = this;
            TwitchAPI.GlobalChatDelegate += ChatListener;
        }

        void ChatListener(string user,string message,string channel)
        {
            if (isPollActive) 
            {
                int voteIndex = 0;
                if(int.TryParse(message, out voteIndex) && !Voters.ContainsKey(user))
                {
                    activePoll.options[voteIndex - 1].votes++;
                    Voters.Add(user, message);
                    TwitchAPI.ui.UpdateVotes(activePoll.options.ToArray());
                }
            }
        }

        void ActivatePoll(Poll poll)
        {
            activePoll = poll;
            pollTimerInt = poll.time;
            pollTimer = poll.time;
            isPollActive = true;
            TwitchAPI.ui.SetOptions(poll.options.ToArray(),poll.title);
            TwitchAPI.ui.UpdateVotes(poll.options.ToArray());
            TwitchAPI.ui.UpdateTimer(pollTimerInt, poll.time);
            TwitchAPI.ui.panelOut = true;
        }

        void ConcludePoll(Poll poll)
        {
            activePoll = null;
            pollTimerInt = 0;
            pollTimer = 0;
            downTimer = downTime;
            isPollActive = false;
            TwitchAPI.ui.panelOut = false;

            poll.callBack(poll);
        }

        public bool SubmitPoll(Poll poll)
        {
            if (!TwitchAPI.integrationEnabled) return false;
            if(!VerifyPoll(poll)) return false;
            Poll poll2 = new Poll(poll);
            if (!SanitizeInpuPoll(poll2)) return false;
            pollQueue.Enqueue(poll2);
            return true;
        }

        private bool VerifyPoll(Poll poll) 
        {
            if (poll == null) return false;
            if (poll.callBack == null) return false;
            if (poll.options == null || poll.options.Count <= 0)
                return false;
            for (int i = 0; i < poll.options.Count; i++) 
            {
                if (poll.options[i].displayText.IsNullOrWhiteSpace())
                    return false;
            }
            return true;
        }

        private bool SanitizeInpuPoll(Poll poll)
        {
            if (!VerifyPoll(poll)) return false;
            if (poll.time > 120)
                poll.time = 120;
            if (poll.time < 30)
                poll.time = 30;
            foreach (var option in poll.options) 
            {
                option.displayText.Replace(Environment.NewLine, "");
                if (option.displayText.Length >= 50)
                {
                    option.displayText = option.displayText.Substring(0, 50);
                }
            }
            return true;
        }
    }
}
