using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class NoteGenerator : MonoBehaviour
{
    static NoteGenerator instance;
    public static NoteGenerator Instance
    {
        get
        {
            return instance;
        }
    }

    public GameObject parent;
    public GameObject notePrefab;
    public GameObject noteLeftrotatefab; //yy 0803
    public GameObject noteRightrotatefab; //yy 0803
    public Material lineRendererMaterial;

    public readonly float[] linePos = { -1.5f, -0.5f, 0.5f, 1.5f, -2.5f, 2.5f}; //yy 0803
    // 1x speed reference point (defines the degree to which an entire bar is drawn on the screen)
    readonly float defaultInterval = 0.005f;
    public float Interval { get; private set; }

    IObjectPool<NoteLeftrotate> poolLeftrotate; //yy 0803
    public IObjectPool<NoteLeftrotate> PoolLeftrotate
    {
        get
        {
            if (poolLeftrotate == null)
            {
                poolLeftrotate = new ObjectPool<NoteLeftrotate>(CreatePooledLeftrotate, defaultCapacity: 2048);
            }
            return poolLeftrotate;
        }
    }
    NoteLeftrotate CreatePooledLeftrotate()
    {
        GameObject note = Instantiate(noteLeftrotatefab, parent.transform);
        note.AddComponent<NoteLeftrotate>();
        return note.GetComponent<NoteLeftrotate>();
    }

    IObjectPool<NoteRightrotate> poolRightrotate; //yy 0803
    public IObjectPool<NoteRightrotate> PoolRightrotate
    {
        get
        {
            if (poolRightrotate == null)
            {
                poolRightrotate = new ObjectPool<NoteRightrotate>(CreatePooledRightrotate, defaultCapacity: 2048);
            }
            return poolRightrotate;
        }
    }
    NoteRightrotate CreatePooledRightrotate()
    {
        GameObject note = Instantiate(noteRightrotatefab, parent.transform);
        note.AddComponent<NoteRightrotate>();
        return note.GetComponent<NoteRightrotate>();
    } //yy 0803

    IObjectPool<NoteShort> poolShort;
    public IObjectPool<NoteShort> PoolShort
    {
        get
        {
            if (poolShort == null)
            {
                poolShort = new ObjectPool<NoteShort>(CreatePooledShort, defaultCapacity: 2048);
            }
            return poolShort;
        }
    }
    NoteShort CreatePooledShort()
    {
        GameObject note = Instantiate(notePrefab, parent.transform);
        note.AddComponent<NoteShort>();
        return note.GetComponent<NoteShort>();
    }

    IObjectPool<NoteLong> poolLong;
    public IObjectPool<NoteLong> PoolLong
    {
        get
        {
            if (poolLong == null)
            {
                poolLong = new ObjectPool<NoteLong>(CreatePooledLong, defaultCapacity: 64);
            }
            return poolLong;
        }
    }
    NoteLong CreatePooledLong()
    {
        GameObject note = new GameObject("NoteLong");
        note.transform.parent = parent.transform;

        GameObject head = Instantiate(notePrefab);
        head.name = "head";
        head.transform.parent = note.transform;

        GameObject tail = Instantiate(notePrefab);
        tail.transform.parent = note.transform;
        tail.name = "tail";

        GameObject line = new GameObject("line");
        line.transform.parent = note.transform;

        line.AddComponent<LineRenderer>();
        LineRenderer lineRenderer = line.GetComponent<LineRenderer>();
        lineRenderer.material = lineRendererMaterial;
        lineRenderer.sortingOrder = 3;
        lineRenderer.widthMultiplier = 0.8f;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = false;

        note.AddComponent<NoteLong>();
        return note.GetComponent<NoteLong>();
    }

    int currentBar = 3; // When playing for the first time, create 3 words first
    int next = 0;
    int prev = 0;
    public List<NoteObject> toReleaseList = new List<NoteObject>();

    Coroutine coGenTimer;
    Coroutine coReleaseTimer;
    Coroutine coInterpolate;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    // Pooling-based generation (used when playing games)
    public void StartGen()
    {
        Interval = defaultInterval * GameManager.Instance.Speed;
        // Search for note objects to be created for each measure of music
        coGenTimer = StartCoroutine(IEGenTimer(GameManager.Instance.sheets[GameManager.Instance.title].BarPerMilliSec * 0.001f));
        // Search for note objects to be released in half the time of one measure
        coReleaseTimer = StartCoroutine(IEReleaseTimer(GameManager.Instance.sheets[GameManager.Instance.title].BarPerMilliSec * 0.001f * 0.5f)); 
        coInterpolate = StartCoroutine(IEInterpolate(0.1f, 4f));
    }

    // Create them all at once (used when editing)
    public void GenAll()
    {
        Gen2();
    }

    public void StopGen()
    {
        if (coGenTimer != null)
        {
            StopCoroutine(coGenTimer);
            coGenTimer = null;
        }
        if (coReleaseTimer != null)
        {
            StopCoroutine(coReleaseTimer);
            coReleaseTimer = null;
        }
        ReleaseCompleted();
        Editor.Instance.objects.transform.position = Vector3.zero;

        toReleaseList.Clear();
        currentBar = 3;
        next = 0;
        prev = 0;
    }

    void Gen()
    {
        List<Note> notes = GameManager.Instance.sheets[GameManager.Instance.title].notes;
        List<Note> reconNotes = new List<Note>();

        for (; next < notes.Count; next++)
        {
            if (notes[next].time > currentBar * GameManager.Instance.sheets[GameManager.Instance.title].BarPerMilliSec)
            {
                break;
            }
        }
        for (int j = prev; j < next; j++)
        {
            reconNotes.Add(notes[j]);
        }
        prev = next;

        float currentTime = AudioManager.Instance.GetMilliSec();
        float noteSpeed = Interval * 1000;
        foreach (Note note in reconNotes)
        {
            NoteObject noteObject = null;

            switch (note.type)
            {
                case (int)NoteType.Short:
                    noteObject = PoolShort.Get();
                    noteObject.SetPosition(new Vector3[] { new Vector3(linePos[note.line - 1], (note.time - currentTime) * Interval, 0f) });
                    break;
                case (int)NoteType.Long:
                    noteObject = PoolLong.Get();
                    noteObject.SetPosition(new Vector3[] // Position is note time - current music time
                    {
                        new Vector3(linePos[note.line - 1], (note.time - currentTime) * Interval, 0f),
                        new Vector3(linePos[note.line - 1], (note.tail - currentTime) * Interval, 0f)
                    });
                    break;
                case (int)NoteType.Leftrotate: //yy 0803
                    noteObject = PoolLeftrotate.Get();
                    noteObject.SetPosition(new Vector3[] { new Vector3(linePos[note.line - 1], (note.time - currentTime) * Interval, 0f) });
                    break;
                case (int)NoteType.Rightrotate: //yy 0803
                    noteObject = PoolRightrotate.Get();
                    noteObject.SetPosition(new Vector3[] { new Vector3(linePos[note.line - 1], (note.time - currentTime) * Interval, 0f) });
                    break;
                default:
                    break;
            }
            noteObject.speed = noteSpeed;
            noteObject.note = note;
            noteObject.life = true;
            noteObject.gameObject.SetActive(true);
            noteObject.SetCollider();
            noteObject.Move();
            toReleaseList.Add(noteObject);
        }
    }

    /// <summary>
    /// Editor Gen method, note movement is not handled by the note itself.
    /// </summary>
    void Gen2()
    {
        Sheet sheet = GameManager.Instance.sheets[GameManager.Instance.title];

        List<Note> notes = sheet.notes;

        // (note time - offset) / 1 bit (32 bit time value per night) = position of note
        // Note position * 0.25 (interval based on 32 beats per night of grid) = Final note position

        float gridLineInterval = 0.25f;

        float shortPrevPos = 0;
        int shortPrevTime = 0;

        float headLongPrevPos = 0;
        int headLongPrevTime = 0;

        float tailLongPrevPos = 0;
        int tailLongPrevTime = 0;

        foreach (Note note in notes)
        {
            NoteObject noteObject = null;
            switch (note.type)
            {
                // case (int)NoteType.Leftrotate: //yy 0803
                //     noteObject = PoolLeftrotate.Get();
                //     if (LeftrotatePrevTime == 0)
                //     {
                //         // int pos = Mathf.RoundToInt((note.time - LeftrotatePrevTime - sheet.offset) / sheet.BeatPerSec);
                //         // LeftrotatePrevPos += pos;

                //         noteObject.SetPosition(new Vector3[] { new Vector3(linePos[note.line - 1], LeftrotatePrevPos, 0f) });
                //     }
                //     else
                //     {
                //         // int pos = Mathf.RoundToInt((note.time - LeftrotatePrevTime) / sheet.BeatPerSec);
                //         // LeftrotatePrevPos += pos;

                //         noteObject.SetPosition(new Vector3[] { new Vector3(linePos[note.line - 1], LeftrotatePrevPos * gridLineInterval, 0f) });
                //     }

                //     LeftrotatePrevTime = note.time;

                //     break;
                // case (int)NoteType.Rightrotate: //yy 0803
                //     noteObject = PoolRightrotate.Get();
                //     if (RightrotatePrevTime == 0)
                //     {
                //         int pos = Mathf.RoundToInt((note.time - RightrotatePrevTime - sheet.offset) / sheet.BeatPerSec);
                //         RightrotatePrevPos += pos;

                //         noteObject.SetPosition(new Vector3[] { new Vector3(linePos[note.line - 1], RightrotatePrevPos, 0f) });
                //     }
                //     else
                //     {
                //         int pos = Mathf.RoundToInt((note.time - RightrotatePrevTime) / sheet.BeatPerSec);
                //         RightrotatePrevPos += pos;

                //         noteObject.SetPosition(new Vector3[] { new Vector3(linePos[note.line - 1], RightrotatePrevPos * gridLineInterval, 0f) });
                //     }

                //     RightrotatePrevTime = note.time;

                //     break;
                case (int)NoteType.Short:
                    noteObject = PoolShort.Get();
                    if (shortPrevTime == 0)
                    {
                        int pos = Mathf.RoundToInt((note.time - shortPrevTime - sheet.offset) / sheet.BeatPerSec);
                        shortPrevPos += pos;

                        noteObject.SetPosition(new Vector3[] { new Vector3(linePos[note.line - 1], shortPrevPos, 0f) });
                    }
                    else
                    {
                        int pos = Mathf.RoundToInt((note.time - shortPrevTime) / sheet.BeatPerSec);
                        shortPrevPos += pos;

                        noteObject.SetPosition(new Vector3[] { new Vector3(linePos[note.line - 1], shortPrevPos * gridLineInterval, 0f) });
                    }

                    shortPrevTime = note.time;

                    break;
                case (int)NoteType.Long:
                    {
                        noteObject = PoolLong.Get();
                        if (headLongPrevTime == 0)
                        {
                            int pos = Mathf.RoundToInt((note.time - headLongPrevTime - sheet.offset) / sheet.BeatPerSec);
                            headLongPrevPos += pos;

                            int pos2 = Mathf.RoundToInt((note.tail - tailLongPrevTime - sheet.offset) / sheet.BeatPerSec);
                            tailLongPrevPos += pos2;

                            noteObject.SetPosition(new Vector3[]
                            {
                                new Vector3(linePos[note.line - 1], headLongPrevPos * gridLineInterval, 0f),
                                new Vector3(linePos[note.line - 1], tailLongPrevPos * gridLineInterval, 0f)
                            });
                        }
                        else
                        {
                            int pos = Mathf.RoundToInt((note.time - headLongPrevTime) / sheet.BeatPerSec);
                            headLongPrevPos += pos;

                            int pos2 = Mathf.RoundToInt((note.tail - tailLongPrevTime) / sheet.BeatPerSec);
                            tailLongPrevPos += pos2;

                            noteObject.SetPosition(new Vector3[]
                            {
                            new Vector3(linePos[note.line - 1], headLongPrevPos * gridLineInterval, 0f),
                            new Vector3(linePos[note.line - 1], tailLongPrevPos * gridLineInterval, 0f)
                            });
                        }
                        headLongPrevTime = note.time;
                        tailLongPrevTime = note.tail;

                        break;
                    }
                default:
                    break;
            }
            noteObject.note = note;
            noteObject.life = true;
            noteObject.gameObject.SetActive(true);
            noteObject.SetCollider();
            //noteObject.Move();
            toReleaseList.Add(noteObject); // When you are finished editing, call Release to release it.
        }
    }

    public void DisposeNoteLeftrotate(NoteType type, Vector3 pos)
    {
        NoteObject noteObject = PoolLeftrotate.Get();
        noteObject.SetPosition(new Vector3[] { pos });
        noteObject.gameObject.SetActive(true);
        noteObject.SetCollider();
        toReleaseList.Add(noteObject);
    }

    public void DisposeNoteRightrotate(NoteType type, Vector3 pos)
    {
        NoteObject noteObject = PoolRightrotate.Get();
        noteObject.SetPosition(new Vector3[] { pos });
        noteObject.gameObject.SetActive(true);
        noteObject.SetCollider();
        toReleaseList.Add(noteObject);
    }

    public void DisposeNoteShort(NoteType type, Vector3 pos)
    {
        NoteObject noteObject = PoolShort.Get();
        noteObject.SetPosition(new Vector3[] { pos });
        noteObject.gameObject.SetActive(true);
        noteObject.SetCollider();
        toReleaseList.Add(noteObject);
    }

    NoteObject noteObjectTemp;
    public void DisposeNoteLong(int makingCount, Vector3[] pos)
    {
        if (makingCount == 0)
        {
            noteObjectTemp = PoolLong.Get();
            noteObjectTemp.SetPosition(new Vector3[] { pos[0], pos[1] });
            noteObjectTemp.gameObject.SetActive(true);
        }
        else if (makingCount == 1)
        {
            noteObjectTemp.SetPosition(new Vector3[] { pos[0], pos[1] });
            noteObjectTemp.SetCollider();
            toReleaseList.Add(noteObjectTemp);
        }
    }

    void ReleaseCompleted()
    {
        foreach (NoteObject note in toReleaseList)
        {
            note.gameObject.SetActive(false);

            if (note is NoteShort)
                PoolShort.Release(note as NoteShort);
            else if (note is NoteLeftrotate) //yy 0803
                PoolLeftrotate.Release(note as NoteLeftrotate);
            else if (note is NoteRightrotate) //yy 0803
                PoolRightrotate.Release(note as NoteRightrotate);
            else
                PoolLong.Release(note as NoteLong);
        }
    }

    void Release()
    {
        List<NoteObject> reconNotes = new List<NoteObject>();
        foreach (NoteObject note in toReleaseList)
        {
            if (!note.life)
            {
                if (note is NoteShort)
                    PoolShort.Release(note as NoteShort);
                else if (note is NoteLeftrotate) //yy 0803
                    PoolLeftrotate.Release(note as NoteLeftrotate);
                else if (note is NoteRightrotate) //yy 0803
                    PoolRightrotate.Release(note as NoteRightrotate);
                else
                    PoolLong.Release(note as NoteLong);

                note.gameObject.SetActive(false);
            }
            else
            {
                reconNotes.Add(note);
            }
        }
        toReleaseList.Clear();
        toReleaseList.AddRange(reconNotes);
    }

    public void Interpolate()
    {
        if (coInterpolate != null)
            StopCoroutine(coInterpolate);

        coInterpolate = StartCoroutine(IEInterpolate());
    }

    IEnumerator IEGenTimer(float interval)
    {
        while (true)
        {
            Gen();
            yield return new WaitForSeconds(interval);
            currentBar++;
        }
    }

    IEnumerator IEReleaseTimer(float interval)
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            Release();
        }
    }

    IEnumerator IEInterpolate(float rate = 1f, float duration = 1f)
    {
        float time = 0;
        Interval = defaultInterval * GameManager.Instance.Speed;
        float noteSpeed = Interval * 1000;
        while (time < duration)
        {
            float milli = AudioManager.Instance.GetMilliSec();

            foreach (NoteObject note in toReleaseList)
            {
                note.speed = noteSpeed;
                note.Interpolate(milli, Interval);
            }
            time += rate;
            yield return new WaitForSeconds(rate);
        }
    }
}
