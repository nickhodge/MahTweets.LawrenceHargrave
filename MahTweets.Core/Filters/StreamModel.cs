using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media;
using MahTweets.Core.Composition;
using MahTweets.Core.Extensions;
using MahTweets.Core.Interfaces;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Scripting;

namespace MahTweets.Core.Filters
{
    [KnownType(typeof (ContactFilter))]
    [KnownType(typeof (TextFilter))]
    [KnownType(typeof (UpdateTypeFilter))]
    [KnownType(typeof (ScriptFilter))]
    public class StreamModel : Notify
    {
        public static Color DefaultColor = (Color) ColorConverter.ConvertFromString("#00000000");
                            // maybe add this to the colour themes in the future?

        public static Color InvalidColor = (Color) ColorConverter.ConvertFromString("#00000000");
        private ListSortDirection _direction;
        private ObservableCollection<Filter> _filters;
        private string _groupName;
        private bool _selectAll;
        private string _uuid;

        public StreamModel()
        {
            SelectAll = true;
            Direction = ListSortDirection.Descending;
            Filters = new ObservableCollection<Filter>();
            ScriptFiltersActivated = new ObservableCollection<ScriptFilter>();
            _scriptmanager = CompositionManager.Get<IScriptingManager>();
            _scriptlibrary = CompositionManager.Get<IScriptingLibrarian>();
            _scriptconfiguration = CompositionManager.Get<IScriptingConfiguration>();
        }

        public ObservableCollection<Filter> Filters
        {
            get { return _filters; }
            set
            {
                _filters = value;
                RaisePropertyChanged(() => Filters);
            }
        }

        public ListSortDirection Direction
        {
            get { return _direction; }
            set
            {
                _direction = value;
                RaisePropertyChanged(() => Direction);
            }
        }

        public bool IsDefined
        {
            get { return SelectAll || (Filters.Count > 0) || (ScriptFiltersActivated.Count > 0); }
        }

        public bool SelectAll
        {
            get { return _selectAll; }
            set
            {
                _selectAll = value;
                RaisePropertyChanged(() => SelectAll);
            }
        }

        public string Uuid
        {
            get { return _uuid ?? (_uuid = Composition.Uuid.NewUuid()); }
            set { _uuid = value; }
        }

        public string GroupName
        {
            get { return _groupName; }
            set
            {
                _groupName = value;
                RaisePropertyChanged(() => GroupName);
            }
        }

        #region MahTweets scripting data manipulation stuff

        public void AddActiveScriptFilter(ScriptFilter sf)
        {
            ScriptFiltersActivated.Add(new ScriptFilter
                                           {ScriptKey = sf.ScriptKey, ScriptFilterColor = sf.ScriptFilterColor});
        }

        public void RemoveActiveScriptFilter(ScriptFilter sf)
        {
            // this code made possible by the warmth of kitteh
            if (ScriptFiltersActivated == null) return;
            List<ScriptFilter> asfl = ScriptFiltersActivated.ToList();
            foreach (ScriptFilter sfi in asfl.Where(sfi => sfi.ScriptKey == sf.ScriptKey))
            {
                ScriptFiltersActivated.Remove(sfi);
            }
        }

        public void UpdateScriptFilterColor(ScriptFilter sf, Color colour)
        {
            sf.ScriptFilterColor = colour;
            if (ScriptFiltersActivated == null) return;
            List<ScriptFilter> asfl = ScriptFiltersActivated.ToList();
            foreach (ScriptFilter sfi in asfl.Where(sfi => sfi.ScriptKey == sf.ScriptKey))
            {
                sfi.ScriptFilterColor = colour;
            }
        }

        public bool InScriptFilterActivated(ScriptFilter sf)
        {
            return ScriptFiltersActivated.Any(sfi => sfi.ScriptKey == sf.ScriptKey);
        }

        #endregion

        #region MahTweets Scripting

        private readonly ObservableCollection<ScriptFilter> _scriptFiltersActivated =
            new ObservableCollection<ScriptFilter>(); //list of all the scriptfilters known by the scriptcollection

        private readonly IScriptingConfiguration _scriptconfiguration;
        private readonly IScriptingLibrarian _scriptlibrary;

        /// <summary>
        /// Should this method be public?
        /// </summary>
        ///         
        private readonly IScriptingManager _scriptmanager;

        private readonly ScriptFilterEq _ss = new ScriptFilterEq();

        public ObservableCollection<ScriptFilter> ScriptFiltersActivated
        {
            get { return _scriptFiltersActivated; }
            set
            {
                foreach (ScriptFilter sf in value)
                {
                    if (_scriptlibrary.HasScript(sf.ScriptKey) == false &&
                        _scriptlibrary.IsAScript(sf.ScriptKey, _scriptconfiguration.ScriptFilterEntryPoint))
                    {
                        _scriptFiltersActivated.Remove(sf);
                    }
                    else
                    {
                        if (!_scriptFiltersActivated.Contains(sf, _ss))
                            _scriptFiltersActivated.Add(sf);
                                // the script is still there, just replace it out with the persisted version which will be the on/off & colour
                    }
                }
                RaisePropertyChanged(() => ScriptFiltersActivated);
            }
        }

        #endregion

        public static StreamModel CreateDefault()
        {
            return CreateDefault("Everything");
        }

        public static StreamModel CreateDefault(string name)
        {
            return new StreamModel {GroupName = name, Direction = ListSortDirection.Descending};
        }

        public Color GetColorForContact(IStatusUpdate update)
        {
            IEnumerable<ContactFilter> matchingContacts = Filters.OfType<ContactFilter>().Where(f => f.IsMatch(update))
                .Where(f => f.IsIncluded == FilterBehaviour.Include);

            if (matchingContacts.Any())
            {
                return matchingContacts.First().Color;
            }

            return InvalidColor;
        }

        public Color GetColorForMicroblog(IStatusUpdate update)
        {
            var applicableFilters = new List<Filter>();

            if (update.Parents != null)
                foreach (IMicroblog microblog in update.Parents)
                {
                    foreach (UpdateType updateType in update.Types)
                    {
                        //TODO: "Everything" column relies on the specific checkbox, not nothing being set!
                        IEnumerable<Filter> matchingUpdates =
                            GetFiltersFor(microblog, updateType).Where(
                                f => (f.IsIncluded == FilterBehaviour.Include || (!IsDefined || SelectAll)));
                        applicableFilters.AddRange(matchingUpdates);
                    }
                }

            if (applicableFilters.Any())
                return applicableFilters.First().Color;

            return InvalidColor;
        }

        public FilterBehaviour IsIgnored(IStatusUpdate update)
        {
            var applicableFilters = new List<Filter>();

            if (update.Parents != null)
                foreach (IMicroblog microblog in update.Parents.ToList())
                {
                    try
                    {
                        foreach (UpdateType updateType in update.Types)
                        {
                            IEnumerable<Filter> matchingUpdates =
                                GetFiltersFor(microblog, updateType).Where(
                                    f => f.IsIncluded != FilterBehaviour.NoBehaviour);
                            applicableFilters.AddRange(matchingUpdates);
                        }
                    }
                    catch (Exception ex)
                    {
                        CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                    }
                }

            // do we have any excludes for these UpdateTypes
            if (applicableFilters.HasItems(f => f.IsIncluded == FilterBehaviour.Exclude))
                return FilterBehaviour.Exclude;

            IEnumerable<ContactFilter> matchingContacts = Filters.OfType<ContactFilter>().Where(f => f.IsMatch(update))
                .Where(f => f.IsIncluded != FilterBehaviour.NoBehaviour);

            applicableFilters.AddRange(matchingContacts);

            // do we have any excludes for this contact
            if (applicableFilters.HasItems(f => f.IsIncluded == FilterBehaviour.Exclude))
                return FilterBehaviour.Exclude;

            IEnumerable<TextFilter> textFilters = Filters.OfType<TextFilter>()
                .Where(filter => filter.IsMatch(update))
                .Where(f => f.IsIncluded != FilterBehaviour.NoBehaviour);
            applicableFilters.AddRange(textFilters);

            #region MahTweets Scripting : ScriptFilters assemblate

            if (_scriptmanager != null && (_scriptmanager.IsScriptEngineActive && ScriptFiltersActivated != null))
            {
                if (ScriptFiltersActivated.Count > 0)
                {
                    foreach (FilterBehaviour fb in from sf in ScriptFiltersActivated
                                                   let fb = FilterBehaviour.NoBehaviour
                                                   select ExecuteFilterScript(sf.ScriptKey, update)
                                                   into fb where fb != FilterBehaviour.NoBehaviour select fb)
                    {
                        if (fb == FilterBehaviour.Include)
                        {
                            // update.ColorARGB = sf.ScriptFilterColor.ToString();
                        }
                        return fb;
                    }
                }
            }

            #endregion

            // do we have any excludes for this text
            if (applicableFilters.HasItems(f => f.IsIncluded == FilterBehaviour.Exclude))
                return FilterBehaviour.Exclude;

            if (applicableFilters.HasItems(f => f.IsIncluded == FilterBehaviour.Include))
                return FilterBehaviour.Include;

            return FilterBehaviour.NoBehaviour;
        }

        public FilterBehaviour IsIgnored(string text)
        {
            return Filters.OfType<TextFilter>()
                .Where(filter => filter.IsMatch(text))
                .Select(f => f.IsIncluded)
                .FirstOrDefault();
        }

        public IEnumerable<Filter> GetFiltersFor(IContact contact)
        {
            return Filters.OfType<ContactFilter>()
                .Where(f => f.ContactName.Matches(contact.Name))
                .Cast<Filter>()
                .ToList();
        }

        public void ClearAllContactFilters()
        {
            Filters = new ObservableCollection<Filter>();
            RaisePropertyChanged(() => Filters);
        }

        public IEnumerable<Filter> GetFiltersFor(IMicroblog microblog, UpdateType updateType)
        {
            IEnumerable<UpdateTypeFilter> filters = Filters.OfType<UpdateTypeFilter>();

            IEnumerable<UpdateTypeFilter> foundFiltersForMicroblog =
                filters.Where(
                    filter => filter.Microblog != null && filter.Microblog.Protocol.Matches(microblog.Protocol, true)
                              &&
                              filter.Microblog.Credentials.AccountName.Matches(microblog.Credentials.AccountName, true));

            if (!foundFiltersForMicroblog.Any())
                return new List<Filter>();

            IEnumerable<UpdateTypeFilter> foundFiltersForUpdate =
                foundFiltersForMicroblog.Where(
                    filter =>
                    (filter.UpdateType != null) ? filter.UpdateType.Type.Matches(updateType.Type, true) : false);

            if (!foundFiltersForUpdate.Any())
                return new List<Filter>();

            return foundFiltersForUpdate.Cast<Filter>().ToList();
        }

        public void Exclude(IContact contact)
        {
            IEnumerable<Filter> foundFilters = GetFiltersFor(contact);

            if (foundFilters.Any())
            {
                foreach (Filter f in foundFilters)
                    f.IsIncluded = FilterBehaviour.Exclude;

                return;
            }

            var newFilter = new ContactFilter(FilterBehaviour.Exclude, contact);
            Filters.Add(newFilter);
        }

        public void RemoveExclude(IContact contact)
        {
            IEnumerable<Filter> foundFilters = GetFiltersFor(contact);

            foreach (Filter filter in foundFilters)
                Filters.Remove(filter);
        }

        public void Exclude(IMicroblog blog, UpdateType updateType)
        {
            IEnumerable<Filter> filters = GetFiltersFor(blog, updateType);

            if (filters.Any())
            {
                foreach (Filter ff in filters)
                    ff.IsIncluded = FilterBehaviour.Exclude;

                return;
            }

            var fs = new UpdateTypeFilter(FilterBehaviour.Exclude, blog, updateType);
            Filters.Add(fs);
        }

        public void RemoveExclude(IMicroblog blog, UpdateType updateType)
        {
            IEnumerable<Filter> foundFilters = GetFiltersFor(blog, updateType);

            foreach (Filter filter in foundFilters)
                Filters.Remove(filter);
        }

        public void Color(IContact contact, Color color)
        {
            IEnumerable<Filter> foundFilters = GetFiltersFor(contact);

            if (foundFilters.Any())
            {
                foreach (Filter f in foundFilters)
                {
                    f.IsIncluded = FilterBehaviour.Include;
                    f.Color = color;
                }
                return;
            }

            var newFilter = new ContactFilter(FilterBehaviour.Include, contact) {Color = color};
            Filters.Add(newFilter);
        }

        public void Include(IMicroblog blog, UpdateType updateType, Color color)
        {
            IEnumerable<Filter> filters = GetFiltersFor(blog, updateType);

            if (filters.Any())
            {
                foreach (Filter ff in filters)
                {
                    ff.IsIncluded = FilterBehaviour.Include;
                    ff.Color = color;
                }
                return;
            }

            var fs = new UpdateTypeFilter(FilterBehaviour.Include, blog, updateType) {Color = color};
            Filters.Add(fs);
        }

        public void Include(IMicroblog blog, UpdateType updateType)
        {
            IEnumerable<Filter> filters = GetFiltersFor(blog, updateType);

            if (filters.Any())
            {
                foreach (Filter ff in filters)
                {
                    ff.IsIncluded = FilterBehaviour.Include;
                }
                return;
            }

            var fs = new UpdateTypeFilter(FilterBehaviour.Include, blog, updateType);
            Filters.Add(fs);
        }

        #region MahTweets Scripting, ScriptFilter attack! attack!

        public FilterBehaviour ExecuteFilterScript(string sfkey, IStatusUpdate update)
        {
            FilterBehaviour rfb = FilterBehaviour.NoBehaviour; // failover to nothing
            try
            {
                // add update & type & filterbehaviour to the scope
                return rfb = (FilterBehaviour) ExecuteFilter(sfkey, update);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
            return rfb;
        }

        public Boolean ExecuteGlobalFilterScript(string sfkey, IStatusUpdate update)
        {
            bool rfb = true; // failover to true
            try
            {
                rfb = ExecuteGlobalFilter(sfkey, update);
            }
            catch (Exception ex)
            {
                CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
            }
            return rfb;
        }

        public object ExecuteFilter(string key, object update)
        {
            object fb = FilterBehaviour.NoBehaviour;
            if (!_scriptmanager.IsScriptEngineActive) return fb;
            fb = _scriptmanager.ExecuteScript(key, _scriptconfiguration.ScriptFilterEntryPoint, update, this) ??
                 FilterBehaviour.NoBehaviour;
            return fb;
        }

        public bool ExecuteGlobalFilter(string key, object update)
        {
            bool? gf = true;

            //if (_scriptmanager.IsScriptEngineActive)
            //{
            //    gf = _scriptmanager.ExecuteScript(key, _scriptconfiguration.GlobalScriptFilterEntryPoint, update, this);
            //    if (!gf.HasValue)
            //        gf = true; // this will leave the update in as default
            //}
            return gf.Value;
        }

        #endregion
    }
}