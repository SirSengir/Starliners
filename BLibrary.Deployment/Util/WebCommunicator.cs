/*
* Copyright (c) 2014 SirSengir
* Starliners (http://github.com/SirSengir/Starliners)
*
* This file is part of Starliners.
*
* Starliners is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
*
* Starliners is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with Starliners.  If not, see <http://www.gnu.org/licenses/>.
*/

﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace BLibrary.Util {

    delegate void CommEventHandler (WebCommunicator sender, CustomEventArgs<bool, string> args);

    sealed class WebCommunicator {
        #region Constants

        public const string USER_ANON = "anon";
        public const string LAUNCHER_CHANNEL = "launcher";
        public const string LAUNCHER_EXE = "BLauncher.exe";

        const string URL_REQUESTS = "http://www.beesntrees.net/site/request/";
        const string SCRIPT_SESSION = URL_REQUESTS + "create_session.php";
        const string SCRIPT_CHANNEL_INFO = URL_REQUESTS + "channel_info.php";
        const string SCRIPT_MANIFEST = URL_REQUESTS + "manifest.php";
        const string SCRIPT_ASSET = URL_REQUESTS + "asset.php";
        const string SCRIPT_PROFILE_REGISTER = URL_REQUESTS + "register_profile.php";
        const string SCRIPT_PROFILE_UPDATE = URL_REQUESTS + "update_profile.php";
        const string SCRIPT_PROFILE_RETRIEVE = URL_REQUESTS + "retrieve_profile.php";
        const string SCRIPT_NEWS_RETRIEVE = URL_REQUESTS + "retrieve_news.php";
        const string SCRIPT_CHANGELOG_RETRIEVE = URL_REQUESTS + "retrieve_changelog.php";

        const string URL_ACCOUNT = "http://www.beesntrees.net/account/";
        const string SCRIPT_REGISTER = URL_ACCOUNT + "register.php";

        const string TOKEN_OK = "OK";
        const string TOKEN_DENIED = "DENIED";
        const string TOKEN_FAILED = "FAILED";

        #endregion

        #region Classes

        sealed class DownloadOrder {
            public readonly string Login;
            public readonly string SessionId;
            public readonly string Channel;
            public readonly PlatformOS OS;
            public readonly DirectoryInfo DownloadDirectory;
            public readonly Queue<FileAsset> Pending;

            public FileAsset Current {
                get;
                private set;
            }

            public DownloadOrder (string login, string password, string channel, PlatformOS os, DirectoryInfo downloadDir, Queue<FileAsset> assets) {
                Login = login;
                SessionId = password;
                Channel = channel;
                OS = os;
                DownloadDirectory = downloadDir;
                Pending = assets;
            }

            public bool Next () {
                if (Pending.Count <= 0) {
                    return false;
                }

                Current = Pending.Dequeue ();
                return true;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when session inited. Response string contains the session id.
        /// </summary>
        public event CommEventHandler SessionInited;
        /// <summary>
        /// Occurs when received channel update. Response string is the channel info formatted as json.
        /// </summary>
        public event CommEventHandler ReceivedChannelUpdate;
        /// <summary>
        /// Occurs when the news file was retrieved. Response string is the news file.
        /// </summary>
        public event CommEventHandler ReceivedNews;
        /// <summary>
        /// Occurs when a changelog was received. Response string is the changelog formatted as json.
        /// </summary>
        public event CommEventHandler ReceivedChangelog;
        /// <summary>
        /// Occurs when received manifest text. Response string ist the manifest formatted as json.
        /// </summary>
        public event CommEventHandler ReceivedManifest;
        public event CommEventHandler ProfileRegistrationComplete;
        public event CommEventHandler ProfileUpdateComplete;
        public event CommEventHandler ProfileRetrieved;
        /// <summary>
        /// Occurs when a single file download starts.
        /// </summary>
        public event CommEventHandler DownloadFileStart;
        /// <summary>
        /// Occurs when a single file download ends.
        /// </summary>
        public event CommEventHandler DownloadFileEnd;
        /// <summary>
        /// Occurs when all files of a download order are downloaded or the download failed.
        /// </summary>
        public event CommEventHandler DownloadOrderComplete;

        #endregion

        #region Properties

        public WebClient WebClient {
            get {
                return _webClient;
            }
        }

        #endregion

        WebClient _webClient;

        public WebCommunicator () {
            // Setup the web client.
            _webClient = new WebClient ();
            // Setup the web client
            _webClient.DownloadFileCompleted += new AsyncCompletedEventHandler (DownloadComplete);
        }

        /// <summary>
        /// Requests a session from the web server.
        /// </summary>
        /// <param name="login">Login.</param>
        /// <param name="password">Password.</param>
        public void RequestSession (string login, string password) {
            new ServerRequest ().GetText (new WebTextCallback (SessionResponse),
                SCRIPT_SESSION, new PostData ("login", login), new PostData ("password", password)/*, new PostData ("channel", _channel.Key)*/);
        }

        /// <summary>
        /// Handle the response to our session request.
        /// </summary>
        /// <param name="text"></param>
        void SessionResponse (string text) {
            string[] lines = text.Split (new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (!TOKEN_OK.Equals (lines [0]) || string.IsNullOrWhiteSpace (lines [1])) {
                SessionInited (this, new CustomEventArgs<bool, string> (false, lines [1]));
                return;
            }

            SessionInited (this, new CustomEventArgs<bool, string> (true, lines [1]));
        }

        /// <summary>
        /// Requests a channel update from the web server.
        /// </summary>
        /// <param name="login">Login.</param>
        /// <param name="sessionid">Sessionid.</param>
        public void UpdateChannels (string login, string sessionid) {
            new ServerRequest ().GetText (new WebTextCallback (ChannelResponse),
                SCRIPT_CHANNEL_INFO, new PostData ("login", login), new PostData ("sessionid", sessionid));
        }

        /// <summary>
        /// Handle the channel information.
        /// </summary>
        /// <param name="text">Text.</param>
        void ChannelResponse (string text) {
            string reason = string.Empty;
            if (!IsSuccessfulResponse (text, out reason)) {
                ReceivedChannelUpdate (this, new CustomEventArgs<bool, string> (false, reason));
            } else {
                ReceivedChannelUpdate (this, new CustomEventArgs<bool, string> (true, text));
            }
        }

        /// <summary>
        /// Retrieves the news from the web server.
        /// </summary>
        /// <param name="login">Login.</param>
        /// <param name="sessionid">Sessionid.</param>
        public void RetrieveNews (string login, string sessionid) {
            new ServerRequest ().GetText (new WebTextCallback (NewsResponse),
                SCRIPT_NEWS_RETRIEVE, new PostData ("login", login), new PostData ("sessionid", sessionid));
        }

        /// <summary>
        /// Handle the channel information.
        /// </summary>
        /// <param name="text">Text.</param>
        void NewsResponse (string text) {
            string reason = string.Empty;
            if (!IsSuccessfulResponse (text, out reason)) {
                ReceivedNews (this, new CustomEventArgs<bool, string> (false, reason));
            } else {
                ReceivedNews (this, new CustomEventArgs<bool, string> (true, text));
            }
        }

        /// <summary>
        /// Retrieves a changelog for the given channel from the web server.
        /// </summary>
        /// <param name="login">Login.</param>
        /// <param name="sessionid">Sessionid.</param>
        public void RetrieveChangelog (string login, string sessionid, string channel) {
            new ServerRequest ().GetText (new WebTextCallback (ChangelogResponse),
                SCRIPT_CHANGELOG_RETRIEVE, new PostData ("login", login), new PostData ("sessionid", sessionid), new PostData ("channel", channel));
        }

        /// <summary>
        /// Handle the changelog reponse.
        /// </summary>
        /// <param name="text">Text.</param>
        void ChangelogResponse (string text) {
            string reason = string.Empty;
            if (!IsSuccessfulResponse (text, out reason)) {
                ReceivedChangelog (this, new CustomEventArgs<bool, string> (false, reason));
            } else {
                ReceivedChangelog (this, new CustomEventArgs<bool, string> (true, text));
            }
        }

        /// <summary>
        /// Requests the manifset for the given channel.
        /// </summary>
        /// <param name="login">Login.</param>
        /// <param name="sessionid">Sessionid.</param>
        /// <param name="channel">Channel.</param>
        public void RequestManifset (string login, string sessionid, string channel) {
            new ServerRequest ().GetText (new WebTextCallback (ManifestResponse),
                SCRIPT_MANIFEST, new PostData ("login", login), new PostData ("sessionid", sessionid), new PostData ("channel", channel));
        }

        /// <summary>
        /// Parse the manifest file returned and initiate the update process if necessary.
        /// </summary>
        /// <param name="text"></param>
        void ManifestResponse (string text) {
            string reason = string.Empty;
            if (!IsSuccessfulResponse (text, out reason)) {
                ReceivedManifest (this, new CustomEventArgs<bool, string> (false, reason));
            } else {
                ReceivedManifest (this, new CustomEventArgs<bool, string> (true, text));
            }
        }

        public void RequestProfileUpdate (string login, string sessionid, string password, string confirm, string email, bool allowmail) {
            new ServerRequest ().GetText (new WebTextCallback (ProfileUpdateResponse),
                SCRIPT_PROFILE_UPDATE, new PostData ("login", login), new PostData ("sessionid", sessionid), new PostData ("password", password), new PostData ("confirm", confirm), new PostData ("email", email), new PostData ("allowmail", allowmail));
        }

        void ProfileUpdateResponse (string text) {
            string reason = string.Empty;
            if (!IsSuccessfulResponse (text, out reason)) {
                ProfileUpdateComplete (this, new CustomEventArgs<bool, string> (false, reason));
            } else {
                ProfileUpdateComplete (this, new CustomEventArgs<bool, string> (true, text));
            }
        }

        public void RequestProfileRegistration (string login, string password, string confirm, string email, bool allowmail) {
            new ServerRequest ().GetText (new WebTextCallback (ProfileRegistrationResponse),
                SCRIPT_PROFILE_REGISTER, new PostData ("login", login), new PostData ("password", password), new PostData ("confirm", confirm), new PostData ("email", email), new PostData ("allowmail", allowmail));
        }

        void ProfileRegistrationResponse (string text) {
            string reason = string.Empty;
            if (!IsSuccessfulResponse (text, out reason)) {
                ProfileRegistrationComplete (this, new CustomEventArgs<bool, string> (false, reason));
            } else {
                ProfileRegistrationComplete (this, new CustomEventArgs<bool, string> (true, text));
            }
        }

        public void RequestProfile (string login, string sessionid) {
            new ServerRequest ().GetText (new WebTextCallback (ProfileRetrieveResponse),
                SCRIPT_PROFILE_RETRIEVE, new PostData ("login", login), new PostData ("sessionid", sessionid));
        }

        void ProfileRetrieveResponse (string text) {
            string reason = string.Empty;
            if (!IsSuccessfulResponse (text, out reason)) {
                ProfileRetrieved (this, new CustomEventArgs<bool, string> (false, reason));
            } else {
                ProfileRetrieved (this, new CustomEventArgs<bool, string> (true, text));
            }
        }


        public void DownloadFiles (string login, string sessionid, string channel, PlatformOS os, DirectoryInfo downloadDir, IList<FileAsset> pending) {

            // Clear the old download directory if it exists.
            if (downloadDir.Exists) {
                downloadDir.Delete (true);
            }
            downloadDir.Create ();

            // Fill the download queue
            Queue<FileAsset> queue = new Queue<FileAsset> ();
            foreach (FileAsset asset in pending) {
                if (asset.MustDownload (os)) {
                    queue.Enqueue (asset);
                }
            }
            // Start downloading.
            DownloadOrder order = new DownloadOrder (login, sessionid, channel, os, downloadDir, queue);
            if (order.Next ()) {
                StartFile (order);
            }

        }

        /// <summary>
        /// Initiate a new file download.
        /// </summary>
        /// <param name="asset"></param>
        void StartFile (DownloadOrder order) {
            //Status = string.Format ("Downloading file '{0}'...", asset.Ident);
            if (DownloadFileStart != null) {
                DownloadFileStart (this, new CustomEventArgs<bool, string> (true, order.Current.Ident));
            }

            string target = order.Current.GetDownloadTarget (order.OS);
            string source = order.Current.GetDownloadSource (order.OS);

            FileInfo file = new FileInfo (Path.Combine (order.DownloadDirectory.FullName, target));
            // Create the file directory if necessary.
            if (!file.Directory.Exists) {
                file.Directory.Create ();
            }

            _webClient.DownloadFileAsync (new Uri (SCRIPT_ASSET
            + "?" + WebUtils.CreateRequestData (new PostData ("login", order.Login), new PostData ("sessionid", order.SessionId), new PostData ("channel", order.Channel), new PostData ("ident", source))),
                file.FullName, order);
        }

        /// <summary>
        /// Handle completion of a download, start the next queued item if necessary.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DownloadComplete (object sender, AsyncCompletedEventArgs e) {

            DownloadOrder order = (DownloadOrder)e.UserState;

            if (e.Cancelled || e.Error != null) {
                if (e.Error != null) {
                    Console.Out.WriteLine (e.Error.Message);
                }
                //Status = string.Format ("Failed to download {0}: {1}", e.UserState.ToString (), e.Error != null ? e.Error.Message : "Unknown reason");
                DownloadOrderComplete (this, new CustomEventArgs<bool, string> (false, string.Format ("{0}: {1}", order.Current.ToString (), e.Error != null ? e.Error.Message : "Unknown reason")));
                return;
            } else {
                if (DownloadFileEnd != null) {
                    DownloadFileEnd (this, new CustomEventArgs<bool, string> (true, string.Format ("Downloaded {0}.", order.Current.Ident)));
                }
            }

            // Do post-download actions.
            if (order.Current.IsCompressed) {
                ZipUtils.DecompressFile (new FileInfo (Path.Combine (order.DownloadDirectory.FullName, order.Current.GetDownloadTarget (order.OS))), true);
            }

            // Complete the update if the queue is empty.
            if (order.Next ()) {
                // Otherwise start the next file.
                StartFile (order);
            } else {
                DownloadOrderComplete (this, new CustomEventArgs<bool, string> (true, string.Empty));
            }
        }

        bool IsSuccessfulResponse (string text, out string reason) {
            if (string.IsNullOrWhiteSpace (text)) {
                reason = "Response was empty.";
                return false;
            }

            string[] lines = text.Split (new string[] { "\r\n", "\n" }, StringSplitOptions.None);
            if (TOKEN_DENIED.Equals (lines [0]) || TOKEN_FAILED.Equals (lines [0])) {
                reason = lines [1];
                return false;
            }

            reason = string.Empty;
            return true;
        }
    }
}

