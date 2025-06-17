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
            if (pollQueue.Count > 0 && downTimer <= 0 && !isPollActive && TwitchAPI.IntegrationEnabled && !GameManager.Instance.IsFoyer && GameManager.Instance.PrimaryPlayer && !GameManager.Instance.IsPaused)
            {
                ActivatePoll(pollQueue.Dequeue());
            }
            if (!isPollActive && downTimer >= 0 && !GameManager.Instance.IsPaused)
            {
                downTimer -= Time.unscaledDeltaTime;
            }
            if (isPollActive && !GameManager.Instance.IsPaused)
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
            if (isPollActive && !GameManager.Instance.IsPaused) 
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
            Voters.Clear();
            List<VoteOption> winningVotes = FindWinningVotes(poll);
            ResolvePoll(poll, winningVotes);
        }
        static public List<VoteOption> FindWinningVotes(Poll poll)
        {
            List <VoteOption> winningVotes = new List<VoteOption>();
            int highestVote = 0;
            for (int i = 0; i < poll.options.Count; i++)
            {
                if (poll.options[i].votes > highestVote)
                    highestVote = poll.options[i].votes;
            }
            for (int i = 0; i < poll.options.Count; i++)
            {
                if (poll.options[i].votes == highestVote)
                    winningVotes.Add(poll.options[i]);
            }
            return winningVotes;

        }
        void ResolvePoll(Poll poll,List<VoteOption> winningVotes)
        {

            switch(poll.resolveSettings)
            {
                case resolvePollOptions.mainOnly:
                    poll.CallBack(); break;
                case resolvePollOptions.randomIfTie:
                    poll.CallBack();
                    winningVotes[UnityEngine.Random.Range(0, winningVotes.Count)].CallBack();
                    break;
                case resolvePollOptions.noneIfTie:
                    poll.CallBack();
                    if(winningVotes.Count == 1)
                        winningVotes[0].CallBack(); break;
                case resolvePollOptions.AllIfTie:
                    poll.CallBack();
                    foreach (VoteOption vote in winningVotes)
                        vote.CallBack();
                    break;
                default: poll.CallBack(); break;
            }
                

        }

        public bool SubmitPoll(Poll poll)
        {
            if (!TwitchAPI.IntegrationEnabled) return false;
            if(!VerifyPoll(poll)) return false;
            Poll poll2 = new Poll(poll);
            if (!SanitizeInpuPoll(poll2)) return false;
            pollQueue.Enqueue(poll2);
            return true;
        }

        private bool VerifyPoll(Poll poll) 
        {
            if (poll == null) return false;
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
            if (poll.time < 1)
                poll.time = 1;
            if (!poll.title.IsNullOrWhiteSpace() && poll.title.Length > 50)
                poll.title = poll.title.Substring(0, 50);
            while (poll.options.Count > 4) { poll.options.RemoveAt(poll.options.Count - 1); }
            foreach (var option in poll.options) 
            {
                option.votes = 0;
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
