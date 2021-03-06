﻿using System.Collections.Generic;
using UnityEngine;

public class HecShapesPool : MonoBehaviour
{

    [Header("Hecatia heads")]
    public List<HecShapesHecatia> heads;

    [Header("Available shapes")]
    public List<HecShapesCelestialBody> availableShapes;

    [Header("Shape prefab settings")]
    public HecShapesSlottable slottableTemplate;
    public float startSpacing;
    public int spaceAttempts = 1;

    [Header("Remove colors to make the game harder")]
    public bool noir;

    MouseGrabbableGroup grabGroup;
    Collider2D zone;
    
    List<HecShapesCelestialBody> shapes;

    void Awake()
    {
        this.grabGroup = GetComponent<MouseGrabbableGroup>();
        this.zone = GetComponent<Collider2D>();

        GenerateShapes();
    }

    void Start()
    {
        // Set victory checking for each head
        foreach (HecShapesHecatia head in this.heads)
        {
            head.AddOnFillAction(CheckWin);

            if (this.noir)
                head.MakeGray();
        }
        
        // Make the planets and Hecatias
        List<Vector2> takenPositions = new List<Vector2>();
        for (int i = 0; i < this.shapes.Count; i++)
        {
            if (i < this.heads.Count)
                this.heads[i].SetStyle(this.shapes[i].shape);

            // Calculate random start position
            Vector2 start = FindSpace(this.zone.bounds, takenPositions);
            takenPositions.Add(start);

            InstantiateShape(this.shapes[i], start);
        }
    }

    void GenerateShapes()
    {
        this.shapes = new List<HecShapesCelestialBody>();
        while (availableShapes.Count > 0)
        {
            int index = Random.Range(0, availableShapes.Count);
            this.shapes.Add(availableShapes[index]);
            availableShapes.RemoveAt(index);
        }
    }

    Vector2 FindSpace(Bounds bounds, List<Vector2> takenPositions)
    {
        // Attempt to find a position that isn't too close to occupied positions
        Vector2 space = Vector2.zero;
        for (int i = 0; i < this.spaceAttempts; i++)
        {
            space = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y));

            bool good = true;
            foreach (Vector2 taken in takenPositions)
            {
                if (Vector2.Distance(space, taken) < this.startSpacing)
                {
                    good = false;
                    break;
                }
            }
            
            if (good)
                break;
        }

        return space;
    }
    
    void InstantiateShape(HecShapesCelestialBody shapeTemplate, Vector2 start)
    {
        // This is the planet's generic object
        HecShapesSlottable slottable = Instantiate(
                this.slottableTemplate,
                start,
                new Quaternion(),
                this.transform);

        // Instantiate the planet's sprite and collider holding object as a child
        var shape = Instantiate(shapeTemplate, slottable.transform);

        if (this.noir)
            shape.MakeGray();

        // Make the planet grabbable
        var grabbable = slottable.gameObject.AddComponent<MouseGrabbable>();
        grabbable.disableOnLoss = true;
        grabbable.disableOnVictory = true;

        grabbable._collider2D = shape.GetComponent<Collider2D>();

        // Add to grabbable group
        this.grabGroup.addGrabbable(grabbable, false);
    }
    
    public void CheckWin()
    {
        bool win = true;
        foreach (HecShapesHecatia head in this.heads)
        {
            if (!head.IsFilled())
            {
                win = false;
                break;
            }
        }

        if (win)
        {
            MicrogameController.instance.setVictory(true, true);

            foreach (HecShapesHecatia head in this.heads)
                head.Win();
        }
    }
    
}
