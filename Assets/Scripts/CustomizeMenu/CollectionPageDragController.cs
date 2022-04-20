using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionPageDragController : MonoBehaviour
{
    public Image pageLine;
    public CollectionController collection;

    Vector3 offset = Vector3.zero;

    public void OnMouseDown()
    {
        offset = transform.localPosition - CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
    }

    public void OnMouseDrag()
    {
        transform.localPosition = new Vector3((CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)) + offset).x, transform.localPosition.y, 0f);
        transform.localPosition = new Vector3(Mathf.Clamp(transform.localPosition.x, -pageLine.rectTransform.sizeDelta.x / 2f, pageLine.rectTransform.sizeDelta.x / 2f), transform.localPosition.y, 0f);
        int currentPage = Mathf.RoundToInt((transform.localPosition.x + pageLine.rectTransform.sizeDelta.x / 2f) / (pageLine.rectTransform.sizeDelta.x / (collection.GetTotalPages() - 1)));
        if (currentPage != collection.GetCurrentPage())
            collection.SetPage(currentPage, false);
    }

    public void OnMouseUp()
    {
        transform.localPosition = new Vector3(pageLine.rectTransform.sizeDelta.x / -2f + pageLine.rectTransform.sizeDelta.x / (collection.GetTotalPages() - 1) * collection.GetCurrentPage(), transform.localPosition.y, 0f);
    }

    public void SelectedPageLine()
    {
        Vector3 tappedLoc = CameraController.camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0)) - pageLine.transform.position;
        int currentPage = Mathf.RoundToInt((tappedLoc.x + pageLine.rectTransform.sizeDelta.x / 2f) / (pageLine.rectTransform.sizeDelta.x / (collection.GetTotalPages() - 1)));

        if (currentPage != collection.GetCurrentPage())
            collection.SetPage(currentPage);
    }
}
