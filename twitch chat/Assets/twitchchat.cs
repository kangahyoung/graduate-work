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
    //List<string> chatresultID = new List<string>();
    Dictionary<int, string> chatresultID = new Dictionary<int, string>();
    Dictionary<int, string> rchatresultID = new Dictionary<int, string>();
    int[] parsChatData = new int[4] { 0, 0, 0, 0 };
    List<string> parsChatNick = new List<string>();
    Queue<string> chatData = new Queue<string>();
    //실행시킬 결과값(결과값 모음은 rchatresultID안에 들어있음)
    //excute 변수값으로 딕셔너리 rchatresultID를 대응시키고 value(string)을 가져와서 그 값을 실행시키면 됨.
    int excute = 0;

    public enum ChatStateType
    {
        none = 0,
        chatRecvData,
    }
    public int chatStatus = 0;
    //List<string> rchatresultID = new List<string>();

    //타이머 GUI 구현
    public Text timeText;
    private float time;



    void Start()
    {
        Connect();
        time = 15f;

    }

    IEnumerator timecheck()
    {
        while (true)
        {
            if (time > 0)
                time -= 60 * Time.deltaTime;

            timeText.text = Mathf.Ceil(time).ToString();
            yield return new WaitForSeconds(1);

            if (time <= 0)
            {
                chatStatus = 0;
                SendChat_content("투표가 종료되었습니다.");
                timeText.text = " ";
                time = 15f;
                int max = 0;
               
                StopCoroutine(timecheck());
                for (int i = 1; i < 4; ++i)
                {
                    string content = i + "이 " + parsChatData[i] + "회";
                    SendChat_content(content);
                    if(max< parsChatData[i])
                    {
                        max = parsChatData[i];
                        excute = i;
                    }
                    
                }
                break;
            }
        }



    }

    void Update()
    {
        if (!twitchClient.Connected)
        {
            Connect();
        }

        ReadChat();
        if (Input.GetKeyDown(KeyCode.A) && chatStatus == (int)ChatStateType.none)
        {

            chatStatus = (int)ChatStateType.chatRecvData;
            SendChat_content("투표를 시작합니다.");
            //고른 값 출력
            PatternShuffle();
            foreach (var data in rchatresultID)
            {
                SendChat_vote("#" + (data.Key) + "  " + data.Value.ToString());
            }

            //for (int i = 0; i < 3; i++)
            //{
            //    SendChat_vote("#" + (i + 1) + "  " + rchatresultID[i].ToString());
            //}
            SendChat_content("1~3번 중 하나를 골라 #(숫자)투표해주세요.");

            StartCoroutine(timecheck());



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

    public void viewChat(string intputData)
    {
        chatData.Enqueue(intputData);
        if (chatBox.cachedTextGenerator.lineCount >= 11 && chatData.Count != 0)
        {
            chatData.Dequeue();
            chatData.Dequeue();
        }
        chatBox.text = "";
        foreach (var data in chatData)
        {
            chatBox.text = chatBox.text + "\n" + data.ToString();
        }
    }

    public void SendChat_content(string data)
    {
        string message_to_send = "PRIVMSG #" + channelName + " :" + data;
        writer.WriteLine(message_to_send);
        writer.Flush();
        print(String.Format("{0}: {1}", "BotAI", data));
        viewChat(String.Format("{0}: {1}", "BotAI", data));
    }

    public void SendChat_vote(string data)
    {
        string message_to_send = "PRIVMSG #" + channelName + " :" + data;
        writer.WriteLine(message_to_send);
        writer.Flush();
        print(String.Format("{0}: {1}", "BotAI", data));
        viewChat(String.Format("{0}: {1}", "BotAI", data));
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
                viewChat(String.Format("{0}: {1}", chatName, message));
                //chatBox.text = chatBox.text + "\n" + String.Format("{0}: {1}", chatName, message);

                // 투표 상태를 수집 할 경우
                if (chatStatus == (int)ChatStateType.chatRecvData)
                {

                    for (int i = 1; i < 4; ++i)
                    {
                        if (message.Contains("#" + i) == true && parsChatNick.Contains(chatName) == false)
                        {
                            parsChatNick.Add(chatName);
                            parsChatData[i]++;
                        }
                    }

                }

            }
        }
    }
    public void PatternShuffle()
    {
        for (int i = 0; i < 4; ++i)
        {
            parsChatData[i] = 0;
        }
        parsChatNick.Clear();
        chatresultID.Clear();
        chatresultID.Add(1, "1path");
        chatresultID.Add(2, "2path");
        chatresultID.Add(3, "3path");
        chatresultID.Add(4, "4path");
        chatresultID.Add(5, "5path");
        chatresultID.Add(6, "6path");
        chatresultID.Add(7, "7path");
        chatresultID.Add(8, "8path");

        rchatresultID = ShuffleList(chatresultID);

        foreach (var data in rchatresultID)
        {
            Debug.Log(data.Key + ", " + data.Value);
        }

    }

    private Dictionary<int, string> ShuffleList(Dictionary<int, string> inputList)
    {
        System.Random r = new System.Random();
        int randomIndex = 0;
        int maxCount = inputList.Count;
        while (inputList.Count > 3)
        {
            randomIndex = r.Next(0, maxCount);
            inputList.Remove(randomIndex);
        }

        Dictionary<int, string> resultList = new Dictionary<int, string>();
        int keyCount = 1;
        // 랜덤 추출한 데이터를 새롭게 넣어준다 1~3번 을 출력 하기 위하여
        foreach (var data in inputList)
        {
            resultList.Add(keyCount, data.Value);
            ++keyCount;
        }

        return resultList;
    }

 

    //private List<E> ShuffleList<E>(List<E> inputList)
    //{
    //    List<E> randomList = new List<E>();

    //    System.Random r = new System.Random();
    //    int randomIndex = 0;
    //    while (inputList.Count > 2)
    //    {
    //        randomIndex = r.Next(0, inputList.Count);
    //        randomList.Add(inputList[randomIndex]);
    //        inputList.RemoveAt(randomIndex);
    //    }


    //    return randomList;

    //}

    //public void timecheck()
    //{
    //    if (time > 0)
    //        time -= Time.deltaTime;
    //
    //    timeText.text = Mathf.Ceil(time).ToString(); 
    //
    //
    //}



}
