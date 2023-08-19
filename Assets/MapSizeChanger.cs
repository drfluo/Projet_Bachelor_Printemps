using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapSizeChanger : MonoBehaviour
{

    public Material Plane;
    public PlacementManager placementManager;
    public GameObject Terrain;
    public new Camera camera;
    public Dropdown sizeChooser;

    void Start()
    {
        StartCoroutine(LateStart(0.2f));
    }

    IEnumerator LateStart(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        ChangeSize(0);
    }

    public void ChangeSize(int sizeChoseId)
    {
        placementManager.ClearCurrentMap();
        if (sizeChooser.options[sizeChoseId].text == "15*15"||sizeChoseId==0)//mapData.size = 15;
        {
            placementManager.height = 15;
            placementManager.width = 15;
            placementManager.placementGrid=new Grid(15, 15);

            Terrain.transform.position = new Vector3(7f, 0.0900000036f, 7f);
            Terrain.transform.localScale = new Vector3(1f, 1f, 1f);

            camera.orthographicSize = 8;
            camera.transform.position = new Vector3(7f, 20f, 7f);

            Plane.mainTextureScale = new Vector2(7.5f, 7.5f);
        }
        else if (sizeChooser.options[sizeChoseId].text == "30*30")//mapData.size = 30;
        {
            
            placementManager.height = 30;
            placementManager.width = 30;
            placementManager.placementGrid = new Grid(30, 30);

            Terrain.transform.position = new Vector3(14.5f, 0.0900000036f, 14.5f);
            Terrain.transform.localScale = new Vector3(2f, 2f, 2f);


            camera.orthographicSize = 16;
            camera.transform.position = new Vector3(14.5f, 20f, 14.5f);

            Plane.mainTextureScale = new Vector2(15, 15);
        }
        else //mapData.size = 45;
        {
            placementManager.height = 45;
            placementManager.width = 45;
            placementManager.placementGrid = new Grid(45, 45);

            Terrain.transform.position = new Vector3(22f, 0.0900000036f, 22f);
            Terrain.transform.localScale = new Vector3(3f, 3f, 3f);

            camera.orthographicSize = 24;
            camera.transform.position = new Vector3(22f, 20f, 22f);

            Plane.mainTextureScale = new Vector2(22.5f, 22.5f);

        }
    }
}
