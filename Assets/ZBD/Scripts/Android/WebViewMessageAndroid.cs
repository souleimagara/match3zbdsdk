using System;
using Newtonsoft.Json;
using UnityEngine;

namespace ZBD
{
    class WebViewMessage : AndroidJavaProxy
    {

        public WebViewMessage() : base("io.zebedee.zbdsdk.WebViewMessage")
        {
        }

        public void onMessageReceived(string message)
        {
            ModalController.HandleWebviewMessage(message);
        }

    }
}