﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ummisco.gama.unity.messages;
using ummisco.gama.unity.utils;
using ummisco.gama.unity.notification;

using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using System;
using System.IO;

using System.Xml.Serialization;
using System.Xml.Linq;
using System.Linq;
using System.Xml;
using System.Reflection;
using System.ComponentModel;
using ummisco.gama.unity.topics;
using System.Globalization;
using ummisco.gama.unity.GamaAgent;
using Nextzen;

namespace ummisco.gama.unity.SceneManager
{
    public class GamaManager : MonoBehaviour
    {


        private static GamaManager m_Instance = null;
        public static GameObject MainCamera;

        public static GamaManager Instance { get { return m_Instance; } }
        //Static instance of GamaManager which allows it to be accessed by any other script.

        public string receivedMsg = "";
        public string clientId;
        public static MqttClient client;
        public GamaMethods gama = new GamaMethods();
        public TopicMessage currentMsg;

        public GameObject[] allObjects = null;
        public GameObject gamaManager = null;
        public GameObject targetGameObject = null;
        public GameObject topicGameObject = null;
        public object[] obj = null;

        public GameObject plane = null;

        public static List<Agent> gamaAgentList = new List<Agent>();

        public Material planeMaterial;
        public Material polygonMaterial;
        public Material lineMaterial;

        public GameObject setTopicManager, getTotpicManager, moveTopicManager, notificationTopicManager;


        List<MqttMsgPublishEventArgs> msgList = new List<MqttMsgPublishEventArgs>();



        void Awake()
        {
            m_Instance = this;
            //Check if instance already exists 	
            //If instance already exists and it's not this:
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a gamaManager.
            if (m_Instance == null)
                m_Instance = this;
            else if (m_Instance != this)
                Destroy(gameObject);

            //Sets this to not be destroyed when reloading scene
            DontDestroyOnLoad(gameObject);

            gamaManager = gameObject;
            MainCamera = GameObject.Find(IGamaManager.GAMA_MAIN_CAMERA);

            /* 
               // Create the plane game Object
               plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
               plane.transform.localScale = new Vector3(20, 1, 20);
               plane.GetComponent<Renderer>().material = planeMaterial;
            */

            // Create the Topic's manager GameObjects
            new GameObject(IGamaManager.COLOR_TOPIC_MANAGER).AddComponent<ColorTopic>().transform.SetParent(gamaManager.transform);
            new GameObject(IGamaManager.POSITION_TOPIC_MANAGER).AddComponent<PositionTopic>().transform.SetParent(gamaManager.transform);
            new GameObject(IGamaManager.SET_TOPIC_MANAGER).AddComponent<SetTopic>().transform.SetParent(gamaManager.transform);
            new GameObject(IGamaManager.GET_TOPIC_MANAGER).AddComponent<GetTopic>().transform.SetParent(gamaManager.transform);
            new GameObject(IGamaManager.MONO_FREE_TOPIC_MANAGER).AddComponent<MonoFreeTopic>().transform.SetParent(gamaManager.transform);
            new GameObject(IGamaManager.MULTIPLE_FREE_TOPIC_MANAGER).AddComponent<MultipleFreeTopic>().transform.SetParent(gamaManager.transform);
            new GameObject(IGamaManager.CREATE_TOPIC_MANAGER).AddComponent<CreateTopic>().transform.SetParent(gamaManager.transform);
            new GameObject(IGamaManager.DESTROY_TOPIC_MANAGER).AddComponent<DestroyTopic>().transform.SetParent(gamaManager.transform);
            new GameObject(IGamaManager.MOVE_TOPIC_MANAGER).AddComponent<MoveTopic>().transform.SetParent(gamaManager.transform);
            new GameObject(IGamaManager.NOTIFICATION_TOPIC_MANAGER).AddComponent<NotificationTopic>().transform.SetParent(gamaManager.transform);
            new GameObject(IGamaManager.PROPERTY_TOPIC_MANAGER).AddComponent<PropertyTopic>().transform.SetParent(gamaManager.transform);
            new GameObject(IGamaManager.MAIN_TOPIC_MANAGER).AddComponent<MainTopic>().transform.SetParent(gamaManager.transform);

            new GameObject(IGamaManager.CSVREADER).AddComponent<CSVReader>().transform.SetParent(gamaManager.transform);

        }


        // Use this for initialization
        void Start()
        {

            // To put only in start bloc

            //MqttSetting.SERVER_URL = "localhost";
            //MqttSetting.SERVER_PORT = 1883;
            var timestamp = DateTime.Now.ToFileTime();
            clientId = Guid.NewGuid().ToString() + timestamp;
            client = new MqttClient(MqttSetting.SERVER_URL, MqttSetting.SERVER_PORT, false, null);

            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            //client.Connect(clientId, MqttSetting.DEFAULT_USER, MqttSetting.DEFAULT_PASSWORD);
            client.Connect(clientId);

            client.Subscribe(new string[] { MqttSetting.MAIN_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            client.Subscribe(new string[] { MqttSetting.MONO_FREE_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            client.Subscribe(new string[] { MqttSetting.MULTIPLE_FREE_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            client.Subscribe(new string[] { MqttSetting.POSITION_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            client.Subscribe(new string[] { MqttSetting.COLOR_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            client.Subscribe(new string[] { MqttSetting.GET_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            client.Subscribe(new string[] { MqttSetting.SET_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            client.Subscribe(new string[] { MqttSetting.MOVE_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            client.Subscribe(new string[] { MqttSetting.PROPERTY_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            client.Subscribe(new string[] { MqttSetting.CREATE_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            client.Subscribe(new string[] { MqttSetting.DESTROY_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            client.Subscribe(new string[] { MqttSetting.NOTIFICATION_TOPIC }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

            // client.Subscribe(new string[] { "littosim" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

            //  client.Publish("littosim", System.Text.Encoding.UTF8.GetBytes(client.ClientId));

        }


        void FixedUpdate()
        {

            if (msgList.Count > 0)
            {

                MqttMsgPublishEventArgs e = msgList[0];
                if (!MqttSetting.getTopicsInList().Contains(e.Topic))
                {
                    Debug.Log("-> The Topic '" + e.Topic + "' doesn't exist in the defined list. Please check! (the message will be deleted!)");
                    msgList.Remove(e);
                    return;
                }

                receivedMsg = System.Text.Encoding.UTF8.GetString(e.Message);
                //Debug.Log ("-> Received Message is : " + receivedMsg);
                allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

                switch (e.Topic)
                {
                    case MqttSetting.MAIN_TOPIC:

                        Debug.Log("-> Topic to deal with is : " + MqttSetting.MAIN_TOPIC);
                        //Debug.Log("-> The message is : " + e.Message);

                        topicGameObject = gameObject;
                        GamaMessage gamaMessage = (GamaMessage)MsgSerialization.deserialization(receivedMsg, new GamaMessage());
                        targetGameObject = GameObject.Find(gamaMessage.receivers);

                        if (targetGameObject == null)
                        {
                            Debug.LogError(" Sorry, requested gameObject is null (" + gamaMessage.receivers + "). Please check you code! ");
                            break;
                        }

                        obj = new object[] { gamaMessage, targetGameObject };
                        //GamaManager obje = (GamaManager) FindObjectOfType(typeof(GamaManager));
                        GameObject.Find(IGamaManager.MAIN_TOPIC_MANAGER).GetComponent(IGamaManager.MAIN_TOPIC_SCRIPT).SendMessage("ProcessTopic", obj);


                        break;
                    case MqttSetting.MONO_FREE_TOPIC:

                        Debug.Log("-> Topic to deal with is : " + MqttSetting.MONO_FREE_TOPIC);
                        MonoFreeTopicMessage monoFreeTopicMessage = (MonoFreeTopicMessage)MsgSerialization.deserialization(receivedMsg, new MonoFreeTopicMessage());
                        targetGameObject = GameObject.Find(monoFreeTopicMessage.objectName);
                        obj = new object[] { monoFreeTopicMessage, targetGameObject };

                        if (targetGameObject == null)
                        {
                            Debug.LogError(" Sorry, requested gameObject is null (" + monoFreeTopicMessage.objectName + "). Please check your code! ");
                            break;
                        }
                        Debug.Log("The message is to " + monoFreeTopicMessage.objectName + " about the methode " + monoFreeTopicMessage.methodName + " and attribute " + monoFreeTopicMessage.attribute);
                        GameObject.Find(IGamaManager.MONO_FREE_TOPIC_MANAGER).GetComponent(IGamaManager.MONO_FREE_TOPIC_SCRIPT).SendMessage("ProcessTopic", obj);

                        break;
                    case MqttSetting.MULTIPLE_FREE_TOPIC:

                        Debug.Log("-> Topic to deal with is : " + MqttSetting.MULTIPLE_FREE_TOPIC);

                        MultipleFreeTopicMessage multipleFreetopicMessage = (MultipleFreeTopicMessage)MsgSerialization.deserialization(receivedMsg, new MultipleFreeTopicMessage());
                        targetGameObject = GameObject.Find(multipleFreetopicMessage.objectName);
                        obj = new object[] { multipleFreetopicMessage, targetGameObject };

                        if (targetGameObject == null)
                        {
                            Debug.LogError(" Sorry, requested gameObject is null (" + multipleFreetopicMessage.objectName + "). Please check you code! ");
                            break;
                        }

                        GameObject.Find(IGamaManager.MULTIPLE_FREE_TOPIC_MANAGER).GetComponent(IGamaManager.MULTIPLE_FREE_TOPIC_SCRIPT).SendMessage("ProcessTopic", obj);

                        break;
                    case MqttSetting.POSITION_TOPIC:

                        Debug.Log("-> Topic to deal with is : " + MqttSetting.POSITION_TOPIC);

                        PositionTopicMessage positionTopicMessage = (PositionTopicMessage)MsgSerialization.deserialization(receivedMsg, new PositionTopicMessage());
                        targetGameObject = GameObject.Find(positionTopicMessage.objectName);
                        obj = new object[] { positionTopicMessage, targetGameObject };

                        if (targetGameObject == null)
                        {
                            Debug.LogError(" Sorry, requested gameObject is null (" + positionTopicMessage.objectName + "). Please check your code! ");
                            break;
                        }
                        else
                        {
                            GameObject.Find(IGamaManager.POSITION_TOPIC_MANAGER).GetComponent(IGamaManager.POSITION_TOPIC_SCRIPT).SendMessage("ProcessTopic", obj);

                        }


                        break;

                    case MqttSetting.MOVE_TOPIC:

                        Debug.Log("-> Topic to deal with is : " + MqttSetting.MOVE_TOPIC);

                        MoveTopicMessage moveTopicMessage = (MoveTopicMessage)MsgSerialization.deserialization(receivedMsg, new MoveTopicMessage());
                        targetGameObject = GameObject.Find(moveTopicMessage.objectName);
                        obj = new object[] { moveTopicMessage, targetGameObject };

                        if (targetGameObject == null)
                        {
                            Debug.LogError(" Sorry, requested gameObject is null (" + moveTopicMessage.objectName + "). Please check you code! ");
                            break;
                        }

                        GameObject.Find(IGamaManager.MOVE_TOPIC_MANAGER).GetComponent(IGamaManager.MOVE_TOPIC_SCRIPT).SendMessage("ProcessTopic", obj);

                        break;

                    case MqttSetting.COLOR_TOPIC:

                        Debug.Log("-> Topic to deal with is : " + MqttSetting.COLOR_TOPIC);

                        ColorTopicMessage colorTopicMessage = (ColorTopicMessage)MsgSerialization.deserialization(receivedMsg, new ColorTopicMessage());
                        targetGameObject = GameObject.Find(colorTopicMessage.objectName);
                        obj = new object[] { colorTopicMessage, targetGameObject };

                        if (targetGameObject == null)
                        {
                            Debug.LogError(" Sorry, requested gameObject is null (" + colorTopicMessage.objectName + "). Please check you code! ");
                            break;
                        }

                        GameObject.Find(IGamaManager.COLOR_TOPIC_MANAGER).GetComponent(IGamaManager.COLOR_TOPIC_SCRIPT).SendMessage("ProcessTopic", obj);


                        break;

                    case MqttSetting.GET_TOPIC:

                        Debug.Log("-> Topic to deal with is : " + MqttSetting.GET_TOPIC);
                        string value = null;

                        GetTopicMessage getTopicMessage = (GetTopicMessage)MsgSerialization.deserialization(receivedMsg, new GetTopicMessage());
                        targetGameObject = GameObject.Find(getTopicMessage.objectName);


                        if (targetGameObject == null)
                        {
                            Debug.LogError(" Sorry, requested gameObject is null (" + getTopicMessage.objectName + "). Please check you code! ");
                            break;
                        }

                        obj = new object[] { getTopicMessage, targetGameObject, value };

                        GameObject.Find(IGamaManager.GET_TOPIC_MANAGER).GetComponent(IGamaManager.GET_TOPIC_SCRIPT).SendMessage("ProcessTopic", obj);
                        sendReplay(clientId, "GamaAgent", getTopicMessage.attribute, (string)obj[2]);

                        break;
                    case MqttSetting.SET_TOPIC:

                        Debug.Log("-> Topic to deal with is : " + MqttSetting.SET_TOPIC);

                        SetTopicMessage setTopicMessage = (SetTopicMessage)MsgSerialization.deserialization(receivedMsg, new SetTopicMessage());
                        // Debug.Log("-> Target game object name: " + setTopicMessage.objectName);
                        // Debug.Log("-> Message: " + receivedMsg);
                        targetGameObject = GameObject.Find(setTopicMessage.objectName);

                        if (targetGameObject == null)
                        {
                            Debug.LogError(" Sorry, requested gameObject is null (" + setTopicMessage.objectName + "). Please check you code! ");
                            break;
                        }

                        obj = new object[] { setTopicMessage, targetGameObject };

                        GameObject.Find(IGamaManager.SET_TOPIC_MANAGER).GetComponent(IGamaManager.SET_TOPIC_SCRIPT).SendMessage("ProcessTopic", obj);

                        break;
                    case MqttSetting.PROPERTY_TOPIC:

                        Debug.Log("-> Topic to deal with is : " + MqttSetting.PROPERTY_TOPIC);

                        try
                        {

                        }
                        catch (Exception er)
                        {
                            Debug.Log("Error : " + er.Message);
                        }

                        PropertyTopicMessage propertyTopicMessage = (PropertyTopicMessage)MsgSerialization.deserialization(receivedMsg, new PropertyTopicMessage());
                        Debug.Log("-> Target game object name: " + propertyTopicMessage.objectName);
                        targetGameObject = GameObject.Find(propertyTopicMessage.objectName);

                        if (targetGameObject == null)
                        {
                            Debug.Log(" Sorry, requested gameObject is null (" + propertyTopicMessage.objectName + "). Please check you code! ");
                            // break;
                        }
                        else
                        {
                            obj = new object[] { propertyTopicMessage, targetGameObject };
                            GameObject.Find(IGamaManager.PROPERTY_TOPIC_MANAGER).GetComponent(IGamaManager.PROPERTY_TOPIC_SCRIPT).SendMessage("ProcessTopic", obj);

                        }


                        break;

                    case MqttSetting.CREATE_TOPIC:

                        Debug.Log("-> Topic to deal with is : " + MqttSetting.CREATE_TOPIC);
                        // Debug.Log("-> Message: " + receivedMsg);
                        CreateTopicMessage createTopicMessage = (CreateTopicMessage)MsgSerialization.deserialization(receivedMsg, new CreateTopicMessage());
                        obj = new object[] { createTopicMessage };

                        GameObject.Find(IGamaManager.CREATE_TOPIC_MANAGER).GetComponent(IGamaManager.CREATE_TOPIC_SCRIPT).SendMessage("ProcessTopic", obj);

                        break;
                    case MqttSetting.DESTROY_TOPIC:

                        Debug.Log("-> Topic to deal with is : " + MqttSetting.DESTROY_TOPIC);

                        DestroyTopicMessage destroyTopicMessage = (DestroyTopicMessage)MsgSerialization.deserialization(receivedMsg, new DestroyTopicMessage());
                        obj = new object[] { destroyTopicMessage };

                        if (topicGameObject == null)
                        {
                            Debug.LogError(" Sorry, requested gameObject is null (" + destroyTopicMessage.objectName + "). Please check you code! ");
                            break;
                        }

                        GameObject.Find(IGamaManager.DESTROY_TOPIC_MANAGER).GetComponent(IGamaManager.DESTROY_TOPIC_SCRIPT).SendMessage("ProcessTopic", obj);

                        break;
                    case MqttSetting.NOTIFICATION_TOPIC:

                        Debug.Log("-> Topic to deal with is : " + MqttSetting.NOTIFICATION_TOPIC);

                        NotificationTopicMessage notificationTopicMessage = (NotificationTopicMessage)MsgSerialization.deserialization(receivedMsg, new NotificationTopicMessage());
                        obj = new object[] { notificationTopicMessage };


                        if (topicGameObject == null)
                        {
                            Debug.LogError(" Sorry, requested gameObject is null (" + notificationTopicMessage.objectName + "). Please check you code! ");
                            break;
                        }

                        GameObject.Find(IGamaManager.NOTIFICATION_TOPIC_MANAGER).GetComponent(IGamaManager.NOTIFICATION_TOPIC_SCRIPT).SendMessage("ProcessTopic", obj);


                        break;
                    default:

                        Debug.Log("-> Topic to deal with is : " + MqttSetting.DEFAULT_TOPIC);

                        break;
                }

                msgList.Remove(e);
            }

            checkForNotifications();
            GameObject mapBuilder = GameObject.Find("MapBuilder");
            //GameObject mapBuilder = GameObject.Find("MapBuilder");
            //regionMap = (RegionMap) FindObjectOfType(typeof(RegionMap));
            //GameObject mapBuilder  = (GameObject) FindObjectOfType(typeof(MapBuilder));

            if (mapBuilder != null)
            {
                mapBuilder.GetComponent<RegionMap>().SendMessage("DrawNewAgents");

            }
            else
            {
                Debug.Log("No such Object. Sorry");
            }


        }



        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            msgList.Add(e);
            receivedMsg = System.Text.Encoding.UTF8.GetString(e.Message);
            Debug.Log(">  New Message received on topic : " + e.Topic);
            Debug.Log(">  content is :" + e.Message);
        }



        void OnGUI()
        {
            if (GUI.Button(new Rect(20, 1, 100, 20), "Quitter!"))
            {
                client.Disconnect();
                Application.Quit();
            }
        }


        public void tester()
        {
            client.Publish("Gama", System.Text.Encoding.UTF8.GetBytes("Good, Bug fixed -> Sending from Unity3D!!! Good"), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
        }


        public void sendGotBoxMsg()
        {

            GamaReponseMessage msg = new GamaReponseMessage(clientId, "GamaAgent", "Got a Box notification", DateTime.Now.ToString());

            string message = MsgSerialization.msgSerialization(msg);
            client.Publish("Gama", System.Text.Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
            //client.Publish ("Gama", System.Text.Encoding.UTF8.GetBytes ("Good, Another box2"));
        }

        // à revoir en utilisant publishMessage
        public void sendReplay(string sender, string receiver, string fieldName, string fieldValue)
        {

            ReplayMessage msg = new ReplayMessage(sender, receiver, "content not set", fieldName, fieldValue, DateTime.Now.ToString());
            string message = MsgSerialization.serialization(msg);

            publishMessage(message, MqttSetting.REPLAY_TOPIC);
        }

        void OnDestroy()
        {
            Debug.Log("----> GamaManager GameObject Destroyed");
            m_Instance = null;
        }

        // Update is called once per frame
        void Update()
        {

        }

        // The method to call Game Objects methods
        //----------------------------------------
        public void sendMessageToGameObject(GameObject gameObject, string methodName, Dictionary<object, object> data)
        {

            int size = data.Count;
            List<object> keyList = new List<object>(data.Keys);

            System.Reflection.MethodInfo info = gameObject.GetComponent("PlayerController").GetType().GetMethod(methodName);
            ParameterInfo[] par = info.GetParameters();


            for (int j = 0; j < par.Length; j++)
            {
                System.Reflection.ParameterInfo par1 = par[j];

                Debug.Log("->>>>>>>>>>>>>>--> parametre Name >>=>>=>>=  " + par1.Name);
                Debug.Log("->>>>>>>>>>>>>>--> parametre Type >>=>>=>>=  " + par1.ParameterType);

            }

            switch (size)
            {
                case 0:
                    gameObject.SendMessage(methodName);
                    break;
                case 1:
                    gameObject.SendMessage(methodName, convertParameter(data[keyList.ElementAt(0)], par[0]));
                    break;

                default:
                    object[] obj = new object[size + 1];
                    int i = 0;
                    foreach (KeyValuePair<object, object> pair in data)
                    {
                        obj[i] = pair.Value;
                        i++;
                    }
                    gameObject.SendMessage(methodName, obj);
                    break;
            }

        }

        public object convertParameter(object val, ParameterInfo par)
        {
            object propValue = Convert.ChangeType(val, par.ParameterType);
            return propValue;
        }

        public void checkForNotifications()
        {
            if (NotificationRegistry.notificationsList.Count >= 1)
            {
                foreach (NotificationEntry el in NotificationRegistry.notificationsList)
                {
                    if (!el.isSent)
                    { // TODO Implement a mecanism of notification frequency! 
                        if (el.notify)
                        {
                            string msg = getReplayNotificationMessage(el);
                            publishMessage(msg, MqttSetting.NOTIFY_MSG);
                            el.notify = false;
                            el.isSent = true;
                            Debug.Log("------------------>>>>  Notification " + el.notificationId + " is sent");
                        }
                    }
                }
            }
        }

        public string getReplayNotificationMessage(NotificationEntry el)
        {
            NotificationMessage msg = new NotificationMessage("Unity", el.agentId, "Contents Not set", DateTime.Now.ToString(), el.notificationId);
            string message = MsgSerialization.serializationPlainXml(msg);
            return message;
        }

        public void publishMessage(string message, string topic)
        {
            client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
        }
    }
}