using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MahTweets.Core.Interfaces.Application;

namespace MahTweets.Core.Interfaces.Plugins
{
    [InheritedExport]
    public interface IMicroblog : IPlugin
    {
        /// <summary>
        /// The contact object associated with the account. 
        /// </summary>
        /// <remarks>For example: "@aeoth" for aeoth's account</remarks>
        IContact Owner { get; }

        /// <summary>
        /// Supported UpdateTypes (base and custom) which can be filtered by the application
        /// </summary>
        IList<UpdateType> SupportedTypes { get; }

        IMicroblogSource Source { get; }

        bool ReadOnly { get; }

        /// <summary>
        /// Initial configuration of IMicroblog
        /// </summary>
        /// <returns>Credentials containing username, password, accountname, protocol</returns>
        new void Setup();

        /// <summary>
        /// Connect to the microblogging service
        /// </summary>
        /// <remarks>Can also fetch data at this point</remarks>
        void Connect();

        /// <summary>
        /// Disconnect this plugin from the microblogging service
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Fetch new data from the microblogging service
        /// </summary>
        /// <param name="isRequired"></param>
        void Refresh(bool isRequired);

        /// <summary>
        /// Update this microblogging service with a new update
        /// </summary>
        /// <param name="text"></param>
        void NewStatusUpdate(string text);

        /// <summary>
        /// Change the avatar/profile image of the user associated with this connect.
        /// Recommended (but not required) that the Contact object is also updated (using ContactManager.GetOrCreate(Contact,IMicroblog))
        /// </summary>
        /// <param name="image">Location of image on disk</param>
        void UpdateAvatar(String image);

        /// <summary>
        /// Handle an event for this plugin
        /// </summary>
        /// <param name="eventName">Name of incoming event</param>
        /// <param name="update">Status update associated with the event</param>
        /// <param name="obj">Additional data</param>
        bool HandleEvent(string eventName, IStatusUpdate update, params object[] obj);

        /// <summary>
        /// Register events for this plugin to listen for
        /// </summary>
        void ListenForEvents();
    }
}