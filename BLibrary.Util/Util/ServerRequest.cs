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
using System.IO;
using System.Text;
using System.Reflection;
using System.Globalization;
using System.Net;

namespace BLibrary.Util {

    struct PostData {
        public string Key;
        public string Value;

        public PostData (string key, string value) {
            Key = key;
            Value = value;
        }

        public PostData (string key, bool value) {
            Key = key;
            Value = value ? "1" : "0";
        }
    }

    public delegate void WebTextCallback (string text);

    sealed class ServerRequest {

        const int BUFFER_SIZE = 1024;

        public abstract class RequestState {
            public byte[] bufferRead;

            public WebRequest request;
            public WebResponse response;

            public Stream responseStream;

            public byte[] postData;

            public RequestState () {
                bufferRead = new byte[BUFFER_SIZE];
                request = null;
                responseStream = null;
            }

            public abstract void CompleteRequest ();

            public abstract void HandlePartial (int readyBytes);
        }

        public sealed class TextRequestState : RequestState {

            StringBuilder requestData;
            WebTextCallback _callback;

            public TextRequestState (WebTextCallback callback) {
                requestData = new StringBuilder ("");
                _callback = callback;
            }

            public override void HandlePartial (int readyBytes) {
                requestData.Append (Encoding.ASCII.GetString (bufferRead, 0, readyBytes));
            }

            public override void CompleteRequest () {
                if (requestData.Length > 1) {
                    string stringResponse = requestData.ToString ();
                    _callback (stringResponse);
                }
            }
        }

        public void GetText (WebTextCallback callback, string url, params PostData[] args) {

            // Create a new instance of the RequestState.
            TextRequestState requestState = new TextRequestState (callback);

            if (args.Length > 0) {
                ASCIIEncoding ascii = new ASCIIEncoding ();
                requestState.postData = ascii.GetBytes (WebUtils.CreateRequestData (args));
            }

            WebRequest request = null;
            try {
                request = WebRequest.Create (new Uri (url));///.CreateHttp (new Uri (url)); // Mono hangs here.
            } catch (Exception ex) {
                Console.Out.WriteLine (ex.Message);
                try {
                    // Since mono is stupid, we'll just use reflection to fix it.
                    BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
                    CultureInfo culture = CultureInfo.InvariantCulture;
                    request = (HttpWebRequest)Activator.CreateInstance (typeof(HttpWebRequest), flags, null, new object[] { new Uri (url) }, culture);
                } catch (Exception ex2) {
                    Console.Out.WriteLine (ex2.Message);
                }
            }

            //request.UserAgent = "Bees'n'Trees Launcher " + PlatformUtils.GetEXEVersion ().ToString ();
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = requestState.postData.Length;

            // The 'WebRequest' object is associated to the 'RequestState' object.
            requestState.request = request;
            // Request the data stream.
            requestState.request.BeginGetRequestStream (new AsyncCallback (RequestCallback), requestState);
        }

        void RequestCallback (IAsyncResult asynchronousResult) {

            RequestState requestState = (TextRequestState)asynchronousResult.AsyncState;

            Stream dataStream = requestState.request.EndGetRequestStream (asynchronousResult);
            dataStream.Write (requestState.postData, 0, requestState.postData.Length);
            dataStream.Flush ();
            dataStream.Close ();

            // Start the Asynchronous call for response.
            requestState.request.BeginGetResponse (new AsyncCallback (ResponseCallback), requestState);
        }

        void ResponseCallback (IAsyncResult asynchronousResult) {

            // Set the State of request to asynchronous.
            TextRequestState requestState = (TextRequestState)asynchronousResult.AsyncState;
            WebRequest request = requestState.request;
            // End the Asynchronous response.
            requestState.response = request.EndGetResponse (asynchronousResult);
            // Read the response into a 'Stream' object.
            Stream responseStream = requestState.response.GetResponseStream ();
            requestState.responseStream = responseStream;
            // Begin the reading of the contents of the HTML page and print it to the console.
            responseStream.BeginRead (requestState.bufferRead, 0, BUFFER_SIZE, new AsyncCallback (ReadCallBack), requestState);

        }

        void ReadCallBack (IAsyncResult asyncResult) {
            // Result state is set to AsyncState.
            TextRequestState requestState = (TextRequestState)asyncResult.AsyncState;
            Stream responseStream = requestState.responseStream;
            int read = responseStream.EndRead (asyncResult);
            // Read the contents of the HTML page and then print to the console.
            if (read > 0) {
                requestState.HandlePartial (read);
                responseStream.BeginRead (requestState.bufferRead, 0, BUFFER_SIZE, new AsyncCallback (ReadCallBack), requestState);
            } else {
                requestState.CompleteRequest ();
                responseStream.Close ();
                // Release the WebResponse resource.
                requestState.response.Close ();
            }
        }
    }

    sealed class WebUtils {
        public static string CreateRequestData (params PostData[] args) {
            StringBuilder postData = new StringBuilder ();
            for (int i = 0; i < args.Length; i++) {
                postData.Append (string.Format ("{0}={1}", args [i].Key, System.Web.HttpUtility.UrlEncode (args [i].Value)));
                if (i < args.Length - 1) {
                    postData.Append ("&");
                }
            }
            return postData.ToString ();
        }
    }

}
