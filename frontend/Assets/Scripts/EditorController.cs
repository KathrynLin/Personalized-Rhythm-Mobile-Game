using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorController : MonoBehaviour
{
    static EditorController instance;
    public static EditorController Instance
    {
        get
        {
            return instance;
        }
    }

    public GameObject cursorPrefab;
    GameObject cursorObj;

    public bool isCtrl;
    float scrollValue;
    Coroutine coCtrl;

    Camera cam;
    Vector3 worldPos;

    InputManager inputManager;
    AudioManager audioManager;
    Editor editor;

    public Action<int> GridSnapListener;

    GameObject selectedNoteObject;
    Vector3 selectedGridPosition;
    Vector3 lastSelectedGridPosition;
    Transform headTemp;
    int selectedLine = 0;

    int longNoteMakingCount = 0;
    GameObject longNoteTemp;
    bool isDispose;

    public bool isShortNoteActive;
    public bool isLongNoteActive;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    void Start()
    {
        cam = Camera.main;

        inputManager = FindObjectOfType<InputManager>();
        audioManager = AudioManager.Instance;
        editor = Editor.Instance;

        cursorObj = Instantiate(cursorPrefab);
    }

    void Update()
    {
        // You need to find the location by shooting a ray on the grid.
        // Depending on the current snap, we need to figure out where to snap to.
        //Debug.Log(inputManager.mousePos);
        Vector3 mousePos = inputManager.mousePos;
        mousePos.z = -cam.transform.position.z;
        worldPos = cam.ScreenToWorldPoint(mousePos);
        //int layerMask = (1 << LayerMask.NameToLayer("Grid")) + (1 << LayerMask.NameToLayer("Note"));

        // cursor coordinates
        cursorObj.transform.position = worldPos;

        Debug.DrawRay(worldPos, cam.transform.forward * 2, Color.red, 0.2f);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, cam.transform.forward, 2f);
        if (hit.transform == null)
        {
            isDispose = false;
            selectedNoteObject = null;
            return;
        }
        else if (hit.transform.CompareTag("Note"))
        {
            //Debug.Log("note");
            isDispose = false;
            selectedNoteObject = hit.transform.gameObject;
        }
        else
        {
            //Debug.Log("grid");
            int beat = int.Parse(hit.transform.name.Split('_')[1]);
            int index = hit.transform.parent.GetComponent<GridObject>().index;
            float y = hit.transform.TransformDirection(hit.transform.position).y; // Local Position To World Position

            if (worldPos.x < -1f && worldPos.x > -2f)
            {
                //Debug.Log($"0 burn lane : {index} burn grid : {beat} beat");
                selectedLine = 0;
            }
            else if (worldPos.x < 0f && worldPos.x > -1f)
            {
                //Debug.Log($"1 burn lane : {index} burn grid : {beat} beat");
                selectedLine = 1;
            }
            else if (worldPos.x < 1f && worldPos.x > 0f)
            {
                //Debug.Log($"2 burn lane : {index} burn grid : {beat} beat");
                selectedLine = 2;
            }
            else if (worldPos.x < 2f && worldPos.x > 1f)
            {
                //Debug.Log($"3 burn lane : {index} burn grid : {beat} beat");
                selectedLine = 3;
            }


            /*
             * Longnote batch bug fix solution
                Take the header and scroll up or down
                When you touch the tail, the head follows it up or down.
             */
            if (longNoteMakingCount == 0)
                headTemp = hit.transform; // head Remember it and use it


            selectedGridPosition = new Vector3(NoteGenerator.Instance.linePos[selectedLine], y, 0f);
            cursorObj.transform.position = selectedGridPosition;

            isDispose = true;
        }
    }

    /// <summary>
    /// Space - Play/Pause ( Space - Play/Puase )
    /// </summary>
    public void Space()
    {
        Editor.Instance.Play();
    }

    /// <summary>
    /// Left click - Place notes ( Mouse leftBtn - Dispose note )
    /// Right click - Delete note ( Mouse rightBtn - Cancel note )
    /// </summary>
    /// <param name="btnName"></param>
    public void MouseBtn(string btnName)
    {
        if (btnName == "leftButton")
        {
            if (selectedNoteObject != null)
            {
                Debug.Log("Note already exists");
            }
            if (isDispose)
            {
                if (isLongNoteActive)
                {
                    if (longNoteMakingCount == 0)
                    {
                        lastSelectedGridPosition = selectedGridPosition;

                        NoteGenerator.Instance.DisposeNoteLong(longNoteMakingCount, new Vector3[] { lastSelectedGridPosition, selectedGridPosition });

                        longNoteMakingCount++;
                    }
                    else if (longNoteMakingCount == 1)
                    {
                        Vector3 tailPositon = selectedGridPosition;
                        // Since long notes cannot be written diagonally,
                        // they remain in the same position even if written on a different line (x).
                        tailPositon.x = lastSelectedGridPosition.x; 
                        lastSelectedGridPosition.y = headTemp.TransformDirection(headTemp.transform.position).y;

                        // If the tail is placed lower than the head, it must be turned over.
                        if (lastSelectedGridPosition.y < tailPositon.y)
                        {
                            NoteGenerator.Instance.DisposeNoteLong(longNoteMakingCount, new Vector3[] { lastSelectedGridPosition, tailPositon });
                        }
                        else
                        {
                            NoteGenerator.Instance.DisposeNoteLong(longNoteMakingCount, new Vector3[] { tailPositon, lastSelectedGridPosition });
                        }

                        longNoteMakingCount = 0;
                    }
                }
                else if (isShortNoteActive)
                {
                    NoteGenerator.Instance.DisposeNoteShort(NoteType.Short, selectedGridPosition );
                }
            }
        }
        else if (btnName == "rightButton")
        {
            if (selectedNoteObject != null)
            {
                //Debug.Log("Delete Note");
                if (isLongNoteActive)
                {
                    // long finds the parent and disables it
                    selectedNoteObject.transform.parent.gameObject.SetActive(false);
                }
                else if (isShortNoteActive)
                {
                    selectedNoteObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Mouse Wheel - Move music and grid position ( Mouse wheel - Move music and grids pos )
    /// </summary>
    /// <param name="value"></param>
    public void Scroll(float value)
    {
        scrollValue = value;

        // Move as much as the corresponding snap when scrolling (only when the control key is not pressed)
        if (!isCtrl)
        {
            float snap = Editor.Instance.Snap;
            if (scrollValue > 0)
            {
                Editor.Instance.objects.transform.position += Vector3.down * snap * 0.25f;         
                AudioManager.Instance.MovePosition(GameManager.Instance.sheets[GameManager.Instance.title].BeatPerSec * 0.001f * snap);
                //Debug.Log(GameManager.Instance.sheets[GameManager.Instance.title].BeatPerSec * 0.001f * snap);
            }
            else if (scrollValue < 0)
            {
                Editor.Instance.objects.transform.position += Vector3.up * snap * 0.25f;
                AudioManager.Instance.MovePosition(-GameManager.Instance.sheets[GameManager.Instance.title].BeatPerSec * 0.001f * snap);
                //Debug.Log(-GameManager.Instance.sheets[GameManager.Instance.title].BeatPerSec * 0.001f * snap);
            }
        }
    }

    /// <summary>
    /// Control + Mouse Wheel - Change Grid Snap ( Ctrl + Mouse wheel - Change snap of grids )
    /// </summary>
    public void Ctrl()
    {
        if (coCtrl != null)
        {
            StopCoroutine(coCtrl);
        }
        coCtrl = StartCoroutine(IEWaitMouseWheel());
    }

    IEnumerator IEWaitMouseWheel()
    {
        while (isCtrl)
        {
            if (scrollValue > 0)
            {
                // snap up
                Editor.Instance.Snap /= 2;
                GridSnapListener.Invoke(Editor.Instance.Snap);
            }
            else if (scrollValue < 0)
            {
                // snap down
                Editor.Instance.Snap *= 2;
                GridSnapListener.Invoke(Editor.Instance.Snap);
            }
            scrollValue = 0;

            yield return null;
        }
    }

    //private void OnGUI()
    //{
    //    GUIStyle style = new GUIStyle();
    //    style.fontSize = 36;
    //    GUI.Label(new Rect(100, 100, 100, 100), "Mouse Pos : " + inputManager.mousePos.ToString(), style);
    //    GUI.Label(new Rect(100, 200, 100, 100), "ScreenToWorld : " + worldPos.ToString(), style);
    //    GUI.Label(new Rect(100, 300, 100, 100), "CurrentBar : " + Editor.Instance.currentBar.ToString(), style);
    //    GUI.Label(new Rect(100, 400, 100, 100), "Snap : " + Editor.Instance.Snap.ToString(), style);
    //}
}
