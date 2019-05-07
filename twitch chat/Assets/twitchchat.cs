using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.IO;
using UnityEngine.UI;

public class twitchchat : MonoBehaviour
{
    private TcpClient twitchClient;
    private StreamReader reader;
    private StreamWriter writer;

    public string username, password, channelName;//Get the password from https://twitchapps.com/tmi

    public Text chatBox;

    void Start()
    {
        Connect();
    }

    
    void Update()
    {
     if(!twitchClient.Connected)
        {
            Connect();
        }

        ReadChat();
        if (Input.GetKeyDown(KeyCode.A))
        {
            SendChat("A키를 누른거 같은데 맞나요?");
        }else if (Input.GetKeyDown(KeyCode.B))
        {
            SendChat("B키를 누른거 같은데 맞나요?");
        }

    }

    private void Connect()
    {
        twitchClient = new TcpClient("irc.chat.twitch.tv", 6667);
        reader = new StreamReader(twitchClient.GetStream());
        writer = new StreamWriter(twitchClient.GetStream());

        writer.WriteLine("PASS " + password);
        writer.WriteLine("NICK " + username);
        writer.WriteLine("USER " + username + " 8 * :" + username);
        writer.WriteLine("JOIN #" + channelName);
        writer.Flush();
    }

    public void SendChat(string data)
    {
        string message_to_send = "PRIVMSG #" + channelName + " :" + data;
        writer.WriteLine(message_to_send);
        writer.Flush();
        print(String.Format("{0}: {1}", "BotAI", data));
        chatBox.text = chatBox.text + "\n" + String.Format("{0}: {1}", "BotAI", data);
    }
   
    private void ReadChat()
    {
        if (twitchClient.Available > 0)
        {
            var message = reader.ReadLine();

            if (message.Contains("PRIVMSG"))
            {
                //Get the users name by splitting it from the string
                var splitPoint = message.IndexOf("!", 1);
                var chatName = message.Substring(0, splitPoint);
                chatName = chatName.Substring(1);

                //Get the users message by splitting it from the string
                splitPoint = message.IndexOf(":", 1);
                message = message.Substring(splitPoint + 1);
                print(String.Format("{0}: {1}", chatName, message));
                chatBox.text = chatBox.text + "\n" + String.Format("{0}: {1}", chatName, message);
               
            }
       
        }
    }
}
