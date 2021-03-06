﻿using System;
using System.Collections;
using System.Collections.Generic;
using ummisco.gama.unity.messages;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt.Messages;

public class LittosimManager : MonoBehaviour
{

    // Use this for initialization

    public int actionToDo = 0;
    public static int gameNbr = 0;

    public string LITTOSSIM_TOPIC = "littosim";

    public GameObject ActionPrefab;
    public GameObject RecapActionPrefab;
    public GameObject MessagePrefab;


    public List<GameObject> actionsList = new List<GameObject>();
    public List<GameObject> recapActionsList = new List<GameObject>();
    public List<GameObject> messagesList = new List<GameObject>();

    public Vector3 initialPosition = new Vector3(1976.2f, 1159.5f, 0.0f);
    public Vector3 lastPosition = new Vector3(1976.2f, 1159.5f, 0.0f);

    public Vector3 initialRecapPosition = new Vector3(2020.0f, -135.3f, 0.0f);
    public Vector3 lastRecapPosition = new Vector3(2020.0f, -135.3f, 0.0f);

    public Vector3 initialMessagePosition;
    public Vector3 lastMessagePosition;

    public int elementCounter = 0;
    public int actionCounter = 0;
    public int recapActionCounter = 0;
    public int messageCounter = 0;

    public GameObject valider;
    public GameObject valider_text;
    public GameObject valider_montant;

    public float lineHeight = 80f;
    public float zCoordinate = 60;

    public Canvas uiCanvas;

    void Start()
    {

        initialRecapPosition = new Vector3(2020.0f, -135.3f, 0.0f);
        lastRecapPosition = new Vector3(2020.0f, -135.3f, 0.0f);


        // initialPosition = GameObject.Find("Panel-Action-1").transform.position;
        // lastPosition = initialPosition;

        Debug.Log("The hight of Panel-Action-1 is : " + GameObject.Find("Panel-Message-1").GetComponent<RectTransform>().rect.height);


        initialMessagePosition = new Vector3(-836.3f, -136.2f, 0.0f);
        lastMessagePosition = initialMessagePosition;

        Destroy(GameObject.Find("Panel-Action-1"));
        Destroy(GameObject.Find("Panel-Message-1"));
        Destroy(GameObject.Find("Panel-Recap-Action-1"));

        deactivateValider();
    }


    void Awak()
    {


    }

    void Update()
    {

        if (GameObject.Find("Panel-Message-0-0"))
        {
            Debug.Log("--+-> " + GameObject.Find("Panel-Message-0-0").transform.position);
            //   GameObject.Find("Panel-Message-0-0").transform.position = initialMessagePosition;
        }

        Vector3 mouse = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            GameObject bj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            if (bj is GameObject)
                if (bj.tag.Equals("DeleteActionButton"))
                {
                    Debug.Log("Goooood");
                    Debug.Log("Goooood--------->  to delete is : " + bj.name);
                    sendDeleteAction(bj.transform.parent.name);
                }
            // Debug.Log("Goooood--------->  Selected is : "+bj.name);
        }

        // Left-half of the screen.
        //Rect bounds = new Rect(0, 0, Screen.width / 2, Screen.height);

        Rect bounds = new Rect(58, 843, 1320, 200);



        bounds = new Rect(58, 43, 1320, 200);

        bounds = new Rect(268, 113, 1020, 200);

        GameObject panel = GameObject.Find("Panel-Map");



        var firstPlayerHand = GameObject.Find("First Player Hand");
        var position1 = panel.transform.position;
        var position2 = panel.GetComponent<RectTransform>().rect;

        bounds = new Rect(position2.x, position2.y, position2.width, position2.height);

    }

    public void createNewElement()
    {
        Vector3 position = Input.mousePosition;
        Debug.Log("Mouse position is : " + position);
        position = worldToUISpace(uiCanvas, position);
        position.z = -zCoordinate;
        sendGamaMessage(position);

        // to delete
        GameObject game = GameObject.CreatePrimitive(PrimitiveType.Cube);
        game.transform.position = position;
        game.transform.localScale = new Vector3(30, 30, 30);
        Renderer rend = game.GetComponent<Renderer>();
        rend.material.color = Color.red;
        Debug.Log("Final created position is :" + position);

    }

    public void createNewElementOld()
    {
        Vector3 position = Input.mousePosition;
        Vector3 panelPosition = position;
        Debug.Log("Mouse position is : " + position);
        GameObject Object = GameObject.Find("Object");
        Vector3 objectPosition = Object.transform.position;
        Vector3 objectLocalPosition = Object.transform.localPosition;

        Debug.Log("Object Position is : " + Object.transform.position);
        Debug.Log("Object local Position is : " + Object.transform.localPosition);
        Debug.Log("Object transform local Position is : " + Object.transform.TransformPoint(objectLocalPosition.x, objectLocalPosition.y, objectLocalPosition.z));


        GameObject Canvas_Actions = GameObject.Find("Canvas-Actions");
        //GameObject Canvas_Actions = GameObject.Find("Panel-Map");
        RectTransform Canvas = Canvas_Actions.GetComponent<RectTransform>();
        Camera cam = GamaManager.MainCamera.GetComponent<Camera>();

        Debug.Log("1 Panel cam position is: " + cam.WorldToViewportPoint(position));
        Debug.Log("2 Panel cam position is: " + cam.WorldToScreenPoint(position));
        Debug.Log("3 Panel cam position is: " + cam.ScreenToWorldPoint(position));
        Debug.Log("4 Panel cam position is: " + cam.ScreenToViewportPoint(position));

        Debug.Log("5 Object cam position is: " + cam.WorldToViewportPoint(objectPosition));
        Debug.Log("6 Object cam position is: " + cam.WorldToScreenPoint(objectPosition));
        Debug.Log("7 Object cam position is: " + cam.ScreenToWorldPoint(objectPosition));
        Debug.Log("8 Object cam position is: " + cam.ScreenToViewportPoint(objectPosition));

        panelPosition = worldToUISpace(uiCanvas, position);

        Debug.Log("WorldToCanvasPosition position is: " + panelPosition);

        Vector3[] localCorners = new Vector3[4];
        Vector3[] worldCorners = new Vector3[4];
        Canvas.GetLocalCorners(localCorners);
        Canvas.GetWorldCorners(worldCorners);
        Rect newRect = new Rect(localCorners[0], localCorners[2] - localCorners[0]);

        Object.transform.localPosition = localCorners[2];

        Debug.Log("newRect Before : 0 = " + localCorners[0] + " 1 = " + localCorners[1] + " 2 = " + localCorners[2] + " 3 = " + localCorners[3]);
        Debug.Log("newRect After : 0 = " + cam.WorldToScreenPoint(localCorners[0]) + " 1 = " + cam.WorldToScreenPoint(localCorners[1]) + " 2 = " + cam.WorldToScreenPoint(localCorners[2]) + " 3 = " + cam.WorldToScreenPoint(localCorners[3]));

        Debug.Log("Panel position is: " + panelPosition);

        //game.transform.position  = position - cam.WorldToScreenPoint(corners[0]);
        //Debug.Log("-----> "+newRect.Contains(Input.mousePosition));

    }


    public Vector3 worldToUISpace(Canvas parentCanvas, Vector3 worldPos)
    {
        //Convert the world for screen point so that it can be used with ScreenPointToLocalPointInRectangle function
        // Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        //Vector3 screenPos = Camera.main.ScreenToWorldPoint(worldPos);
        Vector3 screenPos = worldPos;
        Vector2 movePos;

        //Convert the screenpoint to ui rectangle local point
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPos, parentCanvas.worldCamera, out movePos);
        //Convert the local point to world point
        return parentCanvas.transform.TransformPoint(movePos);
    }

    void OnGUI()
    {
        Rect bounds = new Rect(58, -843, 1320, 200);
        GUI.Label(bounds, "test");
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.red);
        texture.Apply();
        GUI.skin.box.normal.background = texture;
        GUI.Box(bounds, GUIContent.none);

        // --------
        if (GUI.Button(new Rect(150, 1, 150, 20), "Send Mqtt message!"))
        {
            string message = MsgSerialization.serialization(new LittosimMessage("Unity", "GamaMainAgent", 1, 0, 0, DateTime.Now.ToString()));
            message = "ceci est un test de message MQTT de unity vers Gama";
            string topic = "li";
            //int msgId = GamaManager.client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
            int msgId = GamaManager.client.Publish(topic, System.Text.Encoding.UTF8.GetBytes(message));
            Debug.Log("msgId is: " + msgId + " -> " + message);
            Debug.Log("Message sent to topic: " + topic);
        }

    }

    public void onAddButtonClicked(int actionId)
    {

        actionToDo = actionId;

    }

    public void sendGamaMessage(Vector3 position)
    {
        string message = "";
        switch (actionToDo)
        {
            case 1:
                message = MsgSerialization.serialization(new LittosimMessage("Unity", "GamaMainAgent", 1, position.x, position.y, DateTime.Now.ToString()));
                publishMessage(message);
                break;
            case 2:
                message = MsgSerialization.serialization(new LittosimMessage("Unity", "GamaMainAgent", 2, position.x, position.y, DateTime.Now.ToString()));
                publishMessage(message);
                break;
            case 3:
                message = MsgSerialization.serialization(new LittosimMessage("Unity", "GamaMainAgent", 3, position.x, position.y, DateTime.Now.ToString()));
                publishMessage(message);
                break;
            case 4:
                message = MsgSerialization.serialization(new LittosimMessage("Unity", "GamaMainAgent", 4, position.x, position.y, DateTime.Now.ToString()));
                publishMessage(message);
                break;
            case 5:
                message = MsgSerialization.serialization(new LittosimMessage("Unity", "GamaMainAgent", 5, position.x, position.y, DateTime.Now.ToString()));
                publishMessage(message);
                break;
        }
        actionToDo = 0;
        gameNbr++;
    }



    public void addCube(Vector3 position, Color color, int type, string name, string texte, int delay, int montant)
    {
        GameObject game = GameObject.CreatePrimitive(PrimitiveType.Cube);
        game.transform.position = position;
        game.transform.localScale = new Vector3(40, 40, 40);
        Renderer rend = game.GetComponent<Renderer>();
        rend.material.color = color;
        addObjectOnPanel(type, name, texte, delay, montant);
    }

    public void addSphere(Vector3 position, Color color, int type, string name, string texte, int delay, int montant)
    {
        GameObject game = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        game.transform.position = position;
        game.transform.localScale = new Vector3(40, 40, 40);
        Renderer rend = game.GetComponent<Renderer>();
        rend.material.color = color;
        addObjectOnPanel(type, name, texte, delay, montant);
    }

    public void addCapsule(Vector3 position, Color color, int type, string name, string texte, int delay, int montant)
    {
        GameObject game = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        game.transform.position = position;
        game.transform.localScale = new Vector3(40, 40, 40);
        Renderer rend = game.GetComponent<Renderer>();
        rend.material.color = color;
        addObjectOnPanel(type, name, texte, delay, montant);
    }

    public void addCylinder(Vector3 position, Color color, int type, string name, string texte, int delay, int montant)
    {
        GameObject game = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        game.transform.position = position;
        game.transform.localScale = new Vector3(40, 40, 40);
        Renderer rend = game.GetComponent<Renderer>();
        rend.material.color = color;
        addObjectOnPanel(type, name, texte, delay, montant);

    }

    public void addCube2(Vector3 position, Color color, int type, string name, string texte, int delay, int montant)
    {
        GameObject game = GameObject.CreatePrimitive(PrimitiveType.Cube);
        game.transform.position = position;
        game.transform.localScale = new Vector3(40, 40, 40);
        Renderer rend = game.GetComponent<Renderer>();
        rend.material.color = color;
        addObjectOnPanel(type, name, texte, delay, montant);
    }





    public void gamaAddElement(object args)
    {
        object[] obj = (object[])args;
        int type = Int32.Parse((string)obj[0]);
        string name = (string)obj[1];
        string texte = (string)obj[2];
        int delay = Int32.Parse((string)obj[3]);
        int montant = Int32.Parse((string)obj[4]);
        float x = float.Parse((string)obj[5]);
        float y = float.Parse((string)obj[6]);
        float z = float.Parse((string)obj[7]);

        Vector3 position = new Vector3(x, y, z);

        switch (type)
        {
            case 1:
                addCube(position, Color.red, type, name, texte, delay, montant);
                break;
            case 2:
                addSphere(position, Color.red, type, name, texte, delay, montant);
                break;
            case 3:
                addCapsule(position, Color.red, type, name, texte, delay, montant);
                break;
            case 4:
                addCylinder(position, Color.red, type, name, texte, delay, montant);
                break;
            case 5:
                addCube2(position, Color.red, type, name, texte, delay, montant);
                break;
        }

        elementCounter++;
    }


    public void deleteActionFromList(object args)
    {
        object[] obj = (object[])args;
        string name = (string)obj[0];

        if (actionsList.Count > 0)
        {
            GameObject gameObj = GameObject.Find(name);

            actionsList.Remove(gameObj);

            foreach (var gameOb in actionsList)
            {
                if (gameOb.name.Equals(name))
                {
                    actionsList.Remove(gameOb);
                    break;
                }
            }

            Destroy(gameObj);
            lastPosition = initialPosition;
            foreach (var gameObject in actionsList)
            {
                gameObject.transform.position = lastPosition;
                lastPosition.y = lastPosition.y - lineHeight;
            }
        }
        //Debug.Log("Here to delete the action from the list the action " + name);
        updateValiderPosition();
    }



    public void gamaAddValidElement(object args)
    {
        object[] obj = (object[])args;
        int type = Int32.Parse((string)obj[0]);
        string name = (string)obj[1];
        string texte = (string)obj[2];
        int delay = Int32.Parse((string)obj[3]);

        GameObject panelParent = GameObject.Find("Content-Panel-Recap-Actions");
        createRecapActionPaneChild(type, name, panelParent, texte, delay);
    }



    public void publishMessage(string message)
    {
        //int msgId = GamaManager.client.Publish(LITTOSSIM_TOPIC, System.Text.Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
        int msgId = GamaManager.client.Publish(LITTOSSIM_TOPIC, System.Text.Encoding.UTF8.GetBytes(message));
        Debug.Log("msgId is: " + msgId + " -> " + message);
    }

    public void addObjectOnPanel(int type, string name, string texte, int delay, int montant)
    {
        GameObject ActionsPanelParent = GameObject.Find("Content-Panel-Actions");
        activateValider();

        switch (type)
        {
            case 1:
                createActionPaneChild(type, name, ActionsPanelParent, texte, delay.ToString(), montant.ToString());
                break;
            case 2:
                createActionPaneChild(type, name, ActionsPanelParent, texte, delay.ToString(), montant.ToString());
                break;
            case 3:
                createActionPaneChild(type, name, ActionsPanelParent, texte, delay.ToString(), montant.ToString());
                break;
            case 4:
                createActionPaneChild(type, name, ActionsPanelParent, texte, delay.ToString(), montant.ToString());
                break;
            case 5:
                createActionPaneChild(type, name, ActionsPanelParent, texte, delay.ToString(), montant.ToString());
                break;
        }



    }

    public Vector3 getAtActionPanelPosition()
    {
        if (actionsList.Count > 0)
        {
            lastPosition = (actionsList[actionsList.Count - 1]).transform.position;
            lastPosition.y = lastPosition.y - lineHeight;
            return lastPosition;
        }
        else
        {
            return initialPosition;
        }
    }

    public Vector3 getAtRecapActionPanelPosition()
    {
        if (recapActionsList.Count > 0)
        {
            lastRecapPosition = (recapActionsList[(recapActionsList.Count - 1)]).transform.position;
            lastRecapPosition.y = lastRecapPosition.y - lineHeight;
            return lastRecapPosition;
        }
        else
        {
            return initialRecapPosition;
        }
    }

    public void updateRecapActionPosition()
    {
        if (recapActionsList.Count > 0)
        {
            lastRecapPosition = initialRecapPosition;
            foreach (var gameObject in recapActionsList)
            {
                gameObject.transform.position = lastRecapPosition;
                lastRecapPosition.y = lastRecapPosition.y - lineHeight;
            }
        }

    }



    public Vector3 getAtMessagePanelPosition()
    {
        if (messagesList.Count > 0)
        {
            lastMessagePosition = (messagesList[(messagesList.Count - 1)]).transform.position;
            lastMessagePosition.y = lastMessagePosition.y - lineHeight;
            return lastMessagePosition;
        }
        else
        {
            return initialMessagePosition;
        }
    }

    public void createActionPaneChild(int type, string name, GameObject panelParent, string texte, string delay, string montant)
    {

        GameObject panelChild = Instantiate(ActionPrefab);
        panelChild.name = name;
        panelChild.transform.position = getAtActionPanelPosition();
        panelChild.transform.SetParent(panelParent.transform);
        actionsList.Add(panelChild);
        panelChild.transform.Find("Texte_type").GetComponent<Text>().text = (texte);

        panelChild.transform.Find("Image_nombre").transform.Find("Texte_nombre").GetComponent<Text>().text = (delay);
        // panelChild.transform.Find("Texte_nombre").GetComponent<Text>().text = (delay);
        panelChild.transform.Find("Texte_montant").GetComponent<Text>().text = (montant);
        panelChild.transform.Find("Close-button").transform.name = "close_" + name;

        updateValiderPosition();

        if (actionsList.Count >= 10)
        {
            RectTransform rt = panelParent.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, (rt.sizeDelta.y + (actionsList.Count - 10) * lineHeight));
        }
        actionCounter++;
    }

    public void updateValiderPosition()
    {
        if (actionsList.Count > 0)
        {
            Vector3 newPosition = actionsList[actionsList.Count - 1].transform.position;
            Vector3 position = valider.transform.position;
            position.y = newPosition.y - lineHeight;
            valider.transform.position = position;

            position = valider_text.transform.position;
            position.y = newPosition.y - lineHeight;
            valider_text.transform.position = position;

            position = valider_montant.transform.position;
            position.y = newPosition.y - lineHeight;
            valider_montant.transform.position = position;
        }
        else
        {
            deactivateValider();
        }

    }

    public void deactivateValider()
    {
        valider.active = false;
        valider_text.active = false;
        valider_montant.active = false;
    }

    public void activateValider()
    {
        valider.active = true;
        valider_text.active = true;
        valider_montant.active = true;
    }

    public void validateActionList()
    {
        string message = MsgSerialization.serialization(new LittosimMessage("Unity", "GamaMainAgent", 100, 0, 0, DateTime.Now.ToString()));
        publishMessage(message);
    }

    public void sendDeleteAction(string name)
    {
        string message = MsgSerialization.serialization(new LittosimMessage("Unity", "GamaMainAgent", 101, name, 0, 0, DateTime.Now.ToString()));
        publishMessage(message);
    }


    public void destroyElement(string name)
    {
        if (GameObject.Find(name))
        {
            GameObject obj = GameObject.Find(name);
            actionsList.Remove(obj);
            Destroy(obj);
            if (actionsList.Count == 0)
            {
                deactivateValider();
            }
        }
    }

    public void createRecapActionPaneChild(int type, string name, GameObject panelParent, string texte, int delay)
    {
        GameObject panelChild = Instantiate(RecapActionPrefab);
        string ImageNombre = "Image_nombre_" + name;
        string ImageValid = "Image_valid_" + name;
        string ImageN = "Image_n_" + name;
        string TextType = "Texte_type_" + name;


        panelChild.name = name;
        panelChild.transform.position = getAtRecapActionPanelPosition();
        panelChild.transform.SetParent(panelParent.transform);
        panelChild.transform.Find("Texte_type").transform.name = TextType;
        panelChild.transform.Find("Image_nombre").transform.name = ImageNombre;
        panelChild.transform.Find("Image_valid").transform.name = ImageValid;
        panelChild.transform.Find("Image_n").transform.name = ImageN;


        panelChild.transform.Find(TextType).GetComponent<Text>().text = (texte);
        panelChild.transform.Find(ImageNombre).transform.Find("Texte_nombre").GetComponent<Text>().text = (delay.ToString());

        GameObject.Find(ImageNombre).SetActive(false);
        GameObject.Find(ImageValid).SetActive(false);
        GameObject.Find(ImageN).SetActive(false);

        //panelChild.transform.Find("Texte_nombre").GetComponent<Text>().text = (delay.ToString());
        recapActionsList.Add(panelChild);

        if (recapActionsList.Count >= 5)
        {
            RectTransform rt = panelParent.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, (rt.sizeDelta.y + ((recapActionsList.Count - 5) * lineHeight)));
        }
        recapActionCounter++;

        updateRecapActionPosition();
    }

    public void newInfoMessage(object args)
    {
        object[] obj = (object[])args;
        int type = Int32.Parse((string)obj[0]);
        string name = (string)obj[1];
        string texte = (string)obj[2];

        GameObject panelParent = GameObject.Find("Panel-Messages");
        GameObject panelChild = Instantiate(MessagePrefab);
        panelChild.name = name;
        panelChild.transform.position = getAtMessagePanelPosition();
        panelChild.transform.SetParent(panelParent.transform);
        panelChild.transform.Find("Texte_type").GetComponent<Text>().text = (texte);
        messagesList.Add(panelChild);

        if (messagesList.Count > 5)
        {
            GameObject gameObj = messagesList[0];
            messagesList.RemoveAt(0);
            Destroy(gameObj);
            lastMessagePosition = initialMessagePosition;
            foreach (var gameObject in messagesList)
            {
                gameObject.transform.position = lastMessagePosition;
                lastMessagePosition.y = lastMessagePosition.y - lineHeight;
            }
        }

        messageCounter++;
    }


    public void setInitialBudget(object args)
    {
        object[] obj = (object[])args;
        string value = (string)obj[0];
        GameObject.Find("BudgetInitialValue").GetComponent<Text>().text = value;
    }

    public void setRemainingBudget(object args)
    {
        object[] obj = (object[])args;
        string value = (string)obj[0];
        GameObject.Find("BudgetRestantValue").GetComponent<Text>().text = value;
    }

    public void setValiderMontant(string montant)
    {
        if (GameObject.Find("Valider_montant"))
            GameObject.Find("Valider_montant").GetComponent<Text>().text = montant;
    }

    public void setValidActionActiveIcon(object args)
    {
        object[] obj = (object[])args;
        string actionName = (string)obj[0];
        bool icon1 = Boolean.Parse((string)obj[1]);
        bool icon2 = Boolean.Parse((string)obj[2]);
        bool icon3 = Boolean.Parse((string)obj[3]);

        Debug.Log("The action name to set valid icon is : " + actionName);

        GameObject Parent = GameObject.Find(actionName);

        Parent.transform.Find("Image_valid_" + actionName).gameObject.SetActive(icon1);
        Parent.transform.Find("Image_nombre_" + actionName).gameObject.SetActive(icon2);
        Parent.transform.Find("Image_n_" + actionName).gameObject.SetActive(icon3);
    }

    public void setValidActionText(object args)
    {
        object[] obj = (object[])args;
        string actionName = (string)obj[0];
        string value1 = (string)obj[1];
        string value2 = (string)obj[2];

        GameObject Parent = GameObject.Find(actionName);

        //Parent.transform.Find("Image_valid_"+actionName).GetComponentgameObject.SetActive(icon1);
        GameObject OB = Parent.transform.Find("Image_nombre_" + actionName).gameObject;
        OB.transform.Find("Texte_nombre").GetComponent<Text>().text = value1;

        OB = Parent.transform.Find("Image_n_" + actionName).gameObject;
        OB.transform.Find("Texte_nombre_i").GetComponent<Text>().text = value2;
    }

}
