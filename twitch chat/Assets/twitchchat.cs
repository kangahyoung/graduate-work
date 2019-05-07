using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.IO;
using UnityEngine.UI;
using static System.Random;


public class twitchchat : MonoBehaviour
{
    private TcpClient twitchClient;
    private StreamReader reader;
    private StreamWriter writer;

    public string username, password, channelName;//Get the password from https://twitchapps.com/tmi

    public Text chatBox;

    //C#컨테이너 생성부
    List<string> chatresultID = new List<string>();
    List<string> rchatresultID = new List<string>();



    void Start()
    {
        Connect();
                            
        chatresultID.Add("1path");
        chatresultID.Add("2path");
        chatresultID.Add("3path");
        chatresultID.Add("4path");
        chatresultID.Add("5path");
        chatresultID.Add("6path");
        chatresultID.Add("7path");

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
           

            SendChat_content("투표를 시작합니다.");
            //고른 값 출력
            PatternShuffle();
            for(int i=0;i<3;i++)
            {
                SendChat_vote( "#"+(i+1) + "  " + rchatresultID[i].ToString());
            }
            SendChat_content("1~3번 중 하나를 골라 #(숫자)투표해주세요.");
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

    public void SendChat_content(string data)
    {
        string message_to_send = "PRIVMSG #" + channelName + " :" + data;
        writer.WriteLine(message_to_send);
        writer.Flush();
        print(String.Format("{0}: {1}", "BotAI", data));
        chatBox.text = chatBox.text + "\n" + String.Format("{0}: {1}", "BotAI", data);
    }

    public void SendChat_vote(string data)
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
    public void PatternShuffle()
    {
        rchatresultID = ShuffleList<string>(chatresultID);
        //Debug.Log(ShuffleList<int>(chatresultID));
        for(int i=0;i<3;i++)
        Debug.Log(rchatresultID[i]);
    }

    private List<E>ShuffleList<E>(List<E>inputList)
    {
        List<E> randomList = new List<E>();

        System.Random r = new System.Random();
        int randomIndex = 0;
        while(inputList.Count>2)
        {
            randomIndex = r.Next(0, inputList.Count);
            randomList.Add(inputList[randomIndex]);
            inputList.RemoveAt(randomIndex);
        }
        
        
        return randomList;
       
    }


   
}
