using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Threading;
using MahTweets.Core.Composition;
using MahTweets.Core.Interfaces;
using MahTweets.Core.Interfaces.Application;
using MahTweets.Core.Interfaces.Plugins;
using MahTweets.Core.Utilities;

namespace MahTweets.Core
{
    /// <summary>
    /// Class for storing Contact information
    /// </summary>
    [DataContract]
    public class Contact : Notify, IContact
    {
        private readonly IStorage _storage;
        private string _bio;

        /// <summary>
        /// Full name of contact (for use in Profile column), for SocialNetworks where Contact.Name is a nickname/handle
        /// </summary>
        private string _fullName;

        private string _id;
        [DataMember(Name = "ImageUrl")] private Uri _imageUrl;

        [DataMember(Name = "ImageUrlSourceUpdateTimestamp")] private DateTime? _imageUrlSourceUpdateTimestamp;

        private string _name;

        private bool _protected;

        public Contact()
        {
            _storage = CompositionManager.Get<IStorage>();
        }

        #region IContact Members

        /// <summary>
        /// Identifier for contact
        /// </summary>
        public string ID
        {
            get { return _id; }
            set
            {
                _id = value;
                RaisePropertyChanged(() => ID);
            }
        }

        /// <summary>
        /// Name of contact
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        /// <summary>
        /// Name of contact
        /// </summary>
        [DataMember]
        public bool IsProtected
        {
            get { return _protected; }
            set
            {
                _protected = value;
                RaisePropertyChanged(() => Name);
            }
        }


        public string FullName
        {
            get { return _fullName; }
            set
            {
                _fullName = value;
                RaisePropertyChanged(() => FullName);
            }
        }

        [DataMember]
        public string Bio
        {
            get { return _bio; }
            set
            {
                _bio = value;
                RaisePropertyChanged(() => Bio);
            }
        }

        /// <summary>
        /// Source of the contact
        /// </summary>
        public IMicroblogSource Source { get; set; }

        /// <summary>
        /// Icon for contact
        /// </summary>
        public Uri ImageUrl
        {
            get
            {
                if (_imageUrl != null)
                    return _imageUrl;
                return new Uri(Directory.GetCurrentDirectory() + @"\default_avatar_image.png", UriKind.Absolute);
            }
            private set
            {
                _imageUrl = value;
                RaisePropertyChanged(() => ImageUrl);
            }
        }

        #endregion

        /// <summary>
        /// Update icon for contact
        /// </summary>
        /// <param name="avatarUri">Uri to image</param>
        /// <param name="UpdateTimestamp">Timestamp to check</param>
        public void SetContactImage(Uri avatarUri, DateTime? UpdateTimestamp)
        {
            // If the updated timestamp has been set, and it's newer than the current timestamp we won't set the contact image. 
            // This is to prevent the 'flicking' that happens when a user has recently updated their avatar - as twitter caches the old image value. 
            if (_imageUrlSourceUpdateTimestamp.HasValue && UpdateTimestamp.HasValue &&
                _imageUrlSourceUpdateTimestamp.Value > UpdateTimestamp.Value)
            {
                return;
            }
            _imageUrlSourceUpdateTimestamp = UpdateTimestamp;

            if (avatarUri.Scheme == "http" || avatarUri.Scheme == "https")
            {
                if (!Directory.Exists(_storage.CombineFullPath("Avatars/")))
                {
                    Directory.CreateDirectory(_storage.CombineFullPath("Avatars/"));
                }
                string imageUrl = avatarUri.AbsolutePath.Replace("/", "_").Replace(":", "_");
                string localPath = Path.Combine(_storage.CombineFullPath(string.Format("Avatars/{0}", imageUrl)));

                if (!File.Exists(localPath))
                {
                    //TaskEx.Run (
                    //   () =>
                    //   {
                    bool gotMutex = false;
                    Mutex mtxFileInfo = null;
                    try
                    {
                        string fileunique = Hash.GetSHA1(localPath);
                        mtxFileInfo = new Mutex(false, fileunique);

                        gotMutex = mtxFileInfo.WaitOne();

                        var fi = new FileInfo(localPath);
                        if (fi.Exists)
                        {
                            ImageUrl = new Uri(localPath, UriKind.Absolute);
                            return;
                        }

                        var _fetcher = new AsyncWebFetcher();
                        _fetcher.FetchAndStoreAsync(avatarUri, localPath);

                        ImageUrl = new Uri(localPath, UriKind.Absolute);
                    }
                    catch (WebException ex)
                    {
                        CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                    }
                    catch (AbandonedMutexException ex)
                    {
                        CompositionManager.Get<IExceptionReporter>().ReportHandledException(ex);
                    }
                    finally
                    {
                        if (mtxFileInfo != null && gotMutex)
                        {
                            mtxFileInfo.ReleaseMutex();
                        }
                    }
                    //  });
                }
                else
                {
                    ImageUrl = new Uri(localPath, UriKind.Absolute);
                }
            }
            else
            {
                ImageUrl = avatarUri;
            }
        }
    }
}