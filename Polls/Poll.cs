using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace TwitchAPI.Polls
{
    public class Poll
    {
        /// <summary>Creates an instnace of a poll</summary>
        /// <param name="time">The time the poll will stay up. By default, will be a maximum of 90.</param>
        /// <param name="title">Additional text that will appear on the poll. May be null. ex: "choose the streamers next gun!" </param>
        /// <param name="options">A list of all the options chat may vote for. By default only the first 4 options will be used. Additional options will not appear.</param>
        /// <param name="callBack">A method callback. Will be called when the poll ends. Must not be null.</param>
        /// <param name="options">A list of all the options chat may vote for. By default only the first 4 options will be used. Additional options will not appear.</param>
        public Poll(int time, List<VoteOption> options, string title, Action<Poll> callBack, string name = null) 
        {
            this.time = time;
            if (options != null)
                this.options = new List<VoteOption>(options);
            this.name = name;
            this.callBack = callBack;
            this.title = title;
        }

        public Poll(Poll poll)
        {
            this.time = poll.time;
            this.options = new List<VoteOption>();
            foreach (var option in poll.options)
            {
                this.options.Add(new VoteOption(option));
            }
            
            this.name = poll.name;
            this.callBack = poll.callBack;
        }

        public int time;
        public string name;
        public string title;
        public List<VoteOption> options;
        public Action<Poll> callBack = null;
    }
    public class VoteOption
    {
        // <summary>Creates an instnace of a poll</summary>
        /// <param name="diplayText"> Text appearing in the vote option</param>
        /// <param name="callBack">callBack for a specific vote. Will be called if said vote wins. May be null" </param>
        /// <param name="options">A list of all the options chat may vote for. By default only the first 4 options will be used. Additional options will not appear.</param>
        public VoteOption(string displayText, Action callBack = null)
        {
            this.displayText = displayText;
            this.callBack = callBack;
        }
        public VoteOption(VoteOption option)
        {
            this.displayText = option.displayText;
            this.votes = option.votes;
            this.callBack = option.callBack;
        }

        public string displayText;
        public int votes;
        private Action callBack = null;
    }
}
