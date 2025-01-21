using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SheetStorage : MonoBehaviour
{
    /*
     * save
    1) Read the note object and calculate time based on y coordinate
    BarPerSec / 16 * note y coordinate = time to be saved

    In the case of long notes
    Head y coordinate = NoteLong y coordinate
    Tail y coordinate = NoteLong.y + tail.y is the final coordinate
     */


    void Start()
    {

    }

    public void Save()
    {
        Sheet sheet = GameManager.Instance.sheets[GameManager.Instance.title];

        List<Note> notes = new List<Note>();
        string noteStr = string.Empty;
        float baseTime = sheet.BarPerSec / 16;
        foreach (NoteObject note in NoteGenerator.Instance.toReleaseList)
        {
            if (!note.gameObject.activeSelf) // If it is disabled, it is a deleted note and will be ignored.
                continue;

            float line = note.transform.position.x;
            int findLine = 0;
            if (line < -1f && line > -2f)
            {
                findLine = 0;
            }
            else if (line < 0f && line > -1f)
            {
                findLine = 1;
            }
            else if (line < 1f && line > 0f)
            {
                findLine = 2;
            }
            else if (line < 2f && line > 1f)
            {
                findLine = 3;
            }
            else if (line < -2f && line > -3f) //yy 0803
            {
                findLine = 4; //yy right
            }
            else if (line < 3f && line > 2f) //yy 0803
            {
                findLine = 5;
            }

            if (note is NoteShort)
            {
                NoteShort noteShort = note as NoteShort;
                int noteTime = (int)(noteShort.transform.localPosition.y * baseTime * 1000) + sheet.offset;

                notes.Add(new Note(noteTime, (int)NoteType.Short, findLine + 1, -1));
                //noteStr += $"{noteTime}, {(int)NoteType.Short}, {findLine + 1}\n";
            }
            else if (note is NoteLong)
            {
                NoteLong noteLong = note as NoteLong;
                int headTime = (int)(noteLong.transform.localPosition.y * baseTime * 1000) + sheet.offset;
                int tailTime = (int)((noteLong.transform.localPosition.y + noteLong.tail.transform.localPosition.y) * baseTime * 1000) + sheet.offset;

                notes.Add(new Note(headTime, (int)NoteType.Long, findLine + 1, tailTime));
                //noteStr += $"{headTime}, {(int)NoteType.Long}, {findLine + 1}, {tailTime}\n";
            }
            else if (note is NoteLeftrotate) //yy 0803
            {
                NoteLeftrotate noteLeftrotate = note as NoteLeftrotate;
                int noteTime = (int)(noteLeftrotate.transform.localPosition.y * baseTime * 1000) + sheet.offset;

                notes.Add(new Note(noteTime, (int)NoteType.Leftrotate, findLine + 1, -1));
                //noteStr += $"{noteTime}, {(int)NoteType.Short}, {findLine + 1}\n";
            }
            else if (note is NoteRightrotate) //yy 0803
            {
                NoteRightrotate noteRightrotate = note as NoteRightrotate;
                int noteTime = (int)(noteRightrotate.transform.localPosition.y * baseTime * 1000) + sheet.offset;

                notes.Add(new Note(noteTime, (int)NoteType.Rightrotate, findLine + 1, -1));
                //noteStr += $"{noteTime}, {(int)NoteType.Short}, {findLine + 1}\n";
            }
        }

        notes = notes.OrderBy(a => a.time).ToList();

        foreach (Note n in notes)
        {
            switch (n.type)
            {
                case (int)NoteType.Short:
                    noteStr += $"{n.time}, {n.type}, {n.line}\n";
                    break;
                case (int)NoteType.Long:
                    noteStr += $"{n.time}, {n.type}, {n.line}, {n.tail}\n";
                    break;
                case (int)NoteType.Leftrotate: //yy 0803
                    noteStr += $"{n.time}, {n.type}, {n.line}\n";
                    break;
                case (int)NoteType.Rightrotate: //yy 0803
                    noteStr += $"{n.time}, {n.type}, {n.line}\n";
                    break;
            }
        }


        string writer = $"[Description]\n" +
            $"Title: {sheet.title}\n" +
            $"Artist: {sheet.artist}\n\n" +
            $"[Audio]\n" +
            $"BPM: {sheet.bpm}\n" +
            $"Offset: {sheet.offset}\n" +
            $"Signature: {sheet.signature[0]}/{sheet.signature[1]}\n\n" +
            $"[Note]\n" +
            $"{noteStr}";

        writer.TrimEnd('\r', '\n');

        string pathExternSheet =
            $"{Application.persistentDataPath}/Sheet/{sheet.title}/{sheet.title}.sheet";
        if (File.Exists(pathExternSheet))
        {
            try
            {
                File.Delete(pathExternSheet);
            }
            catch (IOException e)
            {
                Debug.LogError(e.Message);
                return;
            }
        }

        if (!File.Exists(pathExternSheet))
        {
            using (FileStream fs = File.Create(pathExternSheet))
            {

            }
        }
        else
        {
            Debug.LogError($"{sheet.title}.sheet already exists");
            return;
        }

        using (StreamWriter sw = new StreamWriter(pathExternSheet))
        {
            sw.Write(writer);
        }
    }
}
