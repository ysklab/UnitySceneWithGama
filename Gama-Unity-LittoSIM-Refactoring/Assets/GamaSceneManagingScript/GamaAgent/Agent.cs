using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Nextzen.VectorData;
using ummisco.gama.unity.GamaConcepts;

namespace ummisco.gama.unity.GamaAgent
{
    public class Agent : MonoBehaviour
    {
        private string _geometry;

        public string agentName { set; get; }
        public GamaCoordinateSequence agentCoordinate { set; get; }
        public GamaColor color { set; get; }
        public Vector3 rotation { set; get; }
        public Vector3 scale { set; get; }
        public bool isRotate { set; get; }
        public bool isOnInputMove { set; get; }
        public Rigidbody rb { set; get; }
        public float speed { set; get; }
        public string species { set; get; }
        public int index { set; get; }
        public string nature { get; set; }
        public string geometry { get; set; }

        public string type { get; set; }
        public float height { get; set; }

        public bool isDrawed { get; set; }


        public Agent(string agentName)
        {
            this.agentName = agentName;
            this.isDrawed = false;
            this.height = 0.0f;
        }

        public Agent()
        {
            this.isDrawed = false;
            this.height = 0.0f;
        }

        public string getCollection()
        {
            switch (geometry)
            {
                case IGamaConcept.POLYGON:
                    return "buildings";
                case IGamaConcept.LINE_STRING:
                    return "rouds";
                case IGamaConcept.POINTS:
                    return "objects";
                case IGamaConcept.WATER:
                    return "water";
                case IGamaConcept.LANDUSE:
                    return "landuse";
                default:
                    return "earth";
            }
        }

        public string getLayer()
        {
            switch (geometry)
            {
                case IGamaConcept.POLYGON:
                    return "Buildings";
                case IGamaConcept.LINE_STRING:
                    return "Rouds";
                case IGamaConcept.POINTS:
                    return "Objects";
                case IGamaConcept.WATER:
                    return "Water";
                case IGamaConcept.LANDUSE:
                    return "Landuse";
                default:
                    return "Earth";
            }
        }

    }
}

