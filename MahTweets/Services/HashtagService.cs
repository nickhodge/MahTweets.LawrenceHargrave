using System.Collections;
using System.Collections.ObjectModel;
using MahTweets.Core;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Services
{
    public class HashtagService : IHashtagService
    {
        private readonly Hashtable _hashtable = new Hashtable();
        private readonly ObservableCollection<Hashtag> _hashtags = new ObservableCollection<Hashtag>();

        #region IHashtagService Members

        public ObservableCollection<Hashtag> Hashtags
        {
            get { return _hashtags; }
        }

        public Hashtag GetOrCreateHashtag(string Tag,
                                          IMicroblog microblog)
        {
            lock (this)
            {
                string normalisedTagName = NormaliseTag(Tag);
                Hashtag tag;
                if (_hashtable.ContainsKey(normalisedTagName))
                {
                    tag = (Hashtag) _hashtable[normalisedTagName];

                    tag.AddSource(microblog);
                    tag.Seen();
                    return tag;
                }

                tag = new Hashtag(Tag,
                                  microblog);

                _hashtable.Add(normalisedTagName,
                               tag);
                Hashtags.Add(tag);
                return tag;
            }
        }

        #endregion

        private string NormaliseTag(string Tag)
        {
            return Tag.ToLower();
        }
    }
}