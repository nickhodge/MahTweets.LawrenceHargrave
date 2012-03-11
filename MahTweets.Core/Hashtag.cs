using System;
using System.Collections.Generic;
using MahTweets.Core.Interfaces;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core
{
    public class Hashtag : Notify
    {
        private readonly List<IMicroblog> _sources = new List<IMicroblog>();

        public Hashtag(string Tag, IMicroblog source)
        {
            this.Tag = Tag;
            _sources.Add(source);
            FirstSeen = DateTime.UtcNow;
            LastSeen = DateTime.UtcNow;
        }

        /// <summary>
        /// Hashtag, eg: #auteched
        /// </summary>
        public string Tag { get; private set; }

        public IEnumerable<IMicroblog> Sources
        {
            get { return _sources; }
        }

        public DateTime FirstSeen { get; private set; }
        public DateTime LastSeen { get; private set; }


        public override string ToString()
        {
            return Tag;
        }

        public void Seen()
        {
            LastSeen = DateTime.UtcNow;
        }

        public void AddSource(IMicroblog Source)
        {
            lock (this)
            {
                if (_sources.Contains(Source))
                {
                    return;
                }

                _sources.Add(Source);
            }
        }
    }
}