using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City
{
    public string Name { get => name; set => name = value; }
    public HexCoordinates Center { get => cityCenter; set => cityCenter = value; }

    int index;
    string name;
    ResourceStack[] resources;
    Project[] activeProjects;
    HexCoordinates cityCenter;
}
