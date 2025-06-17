using BepInEx;
using ETGGUI;
using SGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TwitchAPI.Polls
{
    public class PollUIController : MonoBehaviour
    {
        public void Start()
        {
            this.panel = new SGroup();
            this.border = new SRect(new Color(1f, 1f, 1f, 1f));
            this.itemLabel = new SLabel("");
            this.votesLabel = new SLabel("");
            this.timerLabel = new SLabel("");
            this.debugLabel = new SLabel("");
            this.Setup();
            this.panel.Children.Add(this.border);
            this.panel.Children.Add(this.itemLabel);
            this.panel.Children.Add(this.votesLabel);
            this.panel.Children.Add(this.timerLabel);
            this.panel.Position = new Vector2((float)Screen.width / 2f - this.sizeX / 2f, startY);
            SGUIRoot.Main.Children.Add(this.panel);
            TwitchAPI.ui = this;
        }

        private void Setup()
        {
            this.sizeX = 500f;
            this.sizeY = 200f;
            this.startY = -this.sizeY - 10f;
            this.panel.Size = new Vector2(this.sizeX, this.sizeY);
            this.panel.Background = this.bgColor;
            this.panel.Foreground = Color.clear;
            this.panel.Position.y = (float)Screen.height / 2f;
            this.border.Filled = false;
            this.border.Thickness = 3f;
            this.border.Size = this.panel.InnerSize;
            this.itemLabel.Background = Color.clear;
            this.itemLabel.Foreground = Color.white;
            this.itemLabel.Position = new Vector2(0f, 0f);
            this.itemLabel.Size = this.panel.InnerSize;
            this.itemLabel.Alignment = TextAnchor.MiddleLeft;
            this.itemLabel.OnUpdateStyle = delegate (SElement elem)
            {
                elem.Size = this.panel.InnerSize;
                this.LoadFont();
                bool flag = this.font != null;
                if (flag)
                {
                    elem.Font = this.font;
                }
            };
            this.votesLabel.Background = Color.clear;
            this.votesLabel.Foreground = Color.white;
            this.votesLabel.Position = new Vector2(0f, 0f);
            this.votesLabel.Size = new Vector2(this.panel.InnerSize.x * 0.9f, this.panel.InnerSize.y);
            this.votesLabel.Alignment = TextAnchor.MiddleRight;
            this.votesLabel.OnUpdateStyle = delegate (SElement elem)
            {
                elem.Size = this.panel.InnerSize;
                this.LoadFont();
                bool flag = this.font != null;
                if (flag)
                {
                    elem.Font = this.font;
                }
            };
            this.timerLabel.Background = Color.clear;
            this.timerLabel.Foreground = Color.white;
            this.timerLabel.Position = new Vector2(0f, -15f);
            this.timerLabel.Size = new Vector2(this.panel.InnerSize.x * 0.9f, this.panel.InnerSize.y);
            this.timerLabel.Alignment = TextAnchor.UpperRight;
            this.timerLabel.OnUpdateStyle = delegate (SElement elem)
            {
                elem.Size = this.panel.InnerSize;
                this.LoadFont();
                bool flag = this.font != null;
                if (flag)
                {
                    elem.Font = this.font;
                }
            };
        }

        public void Hide()
        {
            this.panel.Position.y = this.startY;
            this.panelOut = false;
        }

        public void Update()
        {
            float t = Mathf.InverseLerp(startY, extendedY, this.panel.Position.y);
            bool extending = this.panelOut && !GameManager.Instance.IsPaused;
            float tOffset = extending ? lerpSpeed * Time.unscaledDeltaTime : -lerpSpeed * Time.unscaledDeltaTime;
            this.panel.Position.y = Mathf.Lerp(startY, extendedY,t + tOffset);
        }

        public void SetOptions(VoteOption[] options, string title = null)
        {
            bool flag = options == null;
            if (!flag)
            {
                string text = string.Empty;
                if (!title.IsNullOrWhiteSpace())
                {
                     text += string.Format(" {0}\n",title);
                }
                text += string.Format(" Vote by typing 1-{0}:\n",options.Length);
                for (int i = 0; i < options.Length; i++)
                {
                    text += string.Format(" {0}) {1} \n", i + 1, options[i].displayText);
                }
                this.itemLabel.Text = text;
            }
        }

        public void UpdateVotes(VoteOption[] votes)
        {
            bool flag = votes == null;
            if (!flag)
            {
                string text = "\n";
                var count = itemLabel.Text.Count(x => x == '\n');
                if (count - votes.Length >1 ) { text += '\n'; }

                for (int i = 0; i < votes.Length; i++)
                {
                    text += string.Format("[{0}] .\n", votes[i].votes);
                }
                this.votesLabel.Text = text;
            }
        }

        public void UpdateTimer(float time, float duration)
        {
            this.timerLabel.Text = string.Format("\nTime: {0}/{1}  .", time, duration);
        }

        public void LoadFont()
        { 
            this.font = (Font)SGUIRoot.Main.Backend.Font;
        }

        private Color bgColor = new Color(0f, 0f, 0f, 0.5f);

        private SGroup panel;

        private SRect border;

        public SLabel itemLabel;

        private SLabel votesLabel;

        private SLabel timerLabel;

        public SLabel debugLabel;

        private Font font;

        private dfFont gameFont;

        private float sizeX;

        private float sizeY;

        private float startY;

        private float extendedY = 10;

        private float lerpSpeed = 3f;

        public bool panelOut = false;
    }
}
