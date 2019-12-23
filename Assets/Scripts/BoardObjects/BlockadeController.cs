using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockadeController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GridController.gridController.ReportPosition(this.gameObject, transform.position);
        transform.SetParent(GameObject.FindGameObjectWithTag("BoardObjects").transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
