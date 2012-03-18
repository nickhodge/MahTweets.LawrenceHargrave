using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Interfaces.Settings;
using MahTweets.Core.Media;

namespace MahTweets.TweetProcessors.WordTextProcessors
{
    public class DeAcronymify : IWordTransfomProvider
    {
        private readonly ITextProcessorEngine _textProcessorEngine;
        private readonly IAcronymSettingsProvider _acronymSettingsProvider;

        public DeAcronymify()
        {
            _textProcessorEngine = CompositionManager.Get<ITextProcessorEngine>(); //get & cache
            _acronymSettingsProvider = CompositionManager.Get<IAcronymSettingsProvider>(); //get & cache
        }

        #region IWordTransfomProvider Members

        public string Name
        {
            get { return "DeAcronymify"; }
        }

        public int Priority
        {
            get { return 100; }
        }

        public InlineLink Match(String word, Brush lBrush, IStatusUpdate oStatusUpdate)
        {
            if (word == null) return null;
            var result =
                _acronymSettingsProvider.AcronymMapping.AsParallel().Any(p => p.Acronym.ToLower() == word.ToLower());
            if (!result) return null;

            var newMeaning =
                _acronymSettingsProvider.AcronymMapping.SingleOrDefault(p => p.Acronym.ToLower() == word.ToLower());
            if (newMeaning != null)
            {
                var il = new InlineLink
                             {
                                 FontFamily = _textProcessorEngine.FfDefault,
                                 FontSize = _textProcessorEngine.FsDefault,
                                 Foreground = _textProcessorEngine.BrText,
                                 FontStyle = FontStyles.Italic,
                                 Text = newMeaning.Meaning,
                                 ToolTip = word
                             };
                return il;
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}