using System;
using MahTweets.Core.Interfaces.Plugins;

namespace MahTweets.Core.Interfaces.Application
{
    public interface IContact
    {
        /// <summary>
        /// Identifier for contact
        /// </summary>
        string ID { get; set; }

        /// <summary>
        /// Name of contact
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Full name of contact
        /// </summary>
        string FullName { get; set; }

        /// <summary>
        /// Some details about the contact
        /// </summary>
        string Bio { get; set; }

        /// <summary>
        /// Source of the contact
        /// </summary>
        IMicroblogSource Source { get; set; }

        /// <summary>
        /// Icon for contact
        /// </summary>
        Uri ImageUrl { get; }

        /// <summary>
        /// Is the Contact Protected contact
        /// </summary>
        bool IsProtected { get; set; }
    }
}