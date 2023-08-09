using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class MapNameChooser : MonoBehaviour
{
    //This is the Dropdown
    Dropdown m_Dropdown;

    void Start()
    {
        //Fetch the Dropdown GameObject the script is attached to
        m_Dropdown = GetComponent<Dropdown>();
        //Clear the old options of the Dropdown menu
        m_Dropdown.ClearOptions();

        m_Dropdown.AddOptions(findAllMaps());

    }


    List<string> findAllMaps()
    {
        string[] test= Directory.GetFiles(Application.dataPath, "*.json");

        List<string> mapList = new List<string>();

        foreach(string map in test)
        {
            if(!Path.GetFileName(map).Contains("Runtime") && !Path.GetFileName(map).Contains("Scripting"))
            {
                mapList.Add(Path.GetFileName(map));
            }
            
        }

        return mapList;
    }
}