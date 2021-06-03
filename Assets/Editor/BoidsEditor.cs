using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BoidsController))]
public class BoidsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BoidsController myController = (BoidsController)target;

        myController.numBoids = EditorGUILayout.IntField("Number Of Boids", myController.numBoids);

    }
}
