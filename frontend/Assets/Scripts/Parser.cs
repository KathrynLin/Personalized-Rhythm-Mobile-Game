using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEngine.Networking;

public class Parser
{
    static Parser instance;
    public static Parser Instance
    {
        get
        {
            if (instance == null)
                instance = new Parser();
            return instance;
        }
    }

    enum Step
    {
        Description,
        Audio,
        Note,
    }
    Step currentStep = Step.Description;

    public Sheet sheet;
    string externalPath = $"{Application.persistentDataPath}/Sheet";
    string streamingAssetsPath = $"{Application.streamingAssetsPath}/Sheet";

    public AudioClip clip;
    public Sprite img;

    public IEnumerator IEParseInternal(string title)
    {

        // Initialize UnityEngine.Random with a seed (e.g., sheet title hash code) //yy
        UnityEngine.Random.InitState(title.GetHashCode());

        sheet = new Sheet(true);
        string readLine = string.Empty;

        string sheetPath = $"file://{streamingAssetsPath}/{title}/{title}.sheet";
#if UNITY_ANDROID
        sheetPath = $"{streamingAssetsPath}/{title}/{title}.sheet";
#endif
        string tempSheetPath = $"{Application.temporaryCachePath}/{title}.sheet";
        UnityWebRequest request =
            new(sheetPath, "GET", new DownloadHandlerFile(tempSheetPath), null);

        yield return request.SendWebRequest();

        string finalSheetPath = tempSheetPath;
        ParseHelper(finalSheetPath);

        yield return
            IEGetClipInternal(title);
        yield return
            IEGetImgInternal(title);

        sheet.clip = clip;
        sheet.img = img;

        // SaveModifiedSheet(); // 保存到newsheet.sheet//yy
    }

    public IEnumerator IEParseExternal(string title)
    {

        // Initialize UnityEngine.Random with a seed (e.g., external sheet title hash code) //yy
        UnityEngine.Random.InitState(title.GetHashCode());

        sheet = new Sheet(false);
        string readLine = string.Empty;

        string finalSheetPath = $"{externalPath}/{title}/{title}.sheet";
        ParseHelper(finalSheetPath);

        yield return
            IEGetClipExternal(title);
        yield return
            IEGetImgExternal(title);

        sheet.clip = clip;
        sheet.img = img;

        // SaveModifiedSheet(); // 保存到newsheet.sheet //yy
    }

    private void ParseHelper(string finalSheetPath)
    {
        List<int> type0NoteIndices = new List<int>();
        string readLine = string.Empty;
        
        using (StreamReader sr = new StreamReader(finalSheetPath))
        {
            int noteIndex = 0;
            readLine = sr.ReadLine();

            while (readLine != null)
            {
                if (readLine.StartsWith("[Description]"))
                {
                    currentStep = Step.Description;
                    readLine = sr.ReadLine();
                }
                else if (readLine.StartsWith("[Audio]"))
                {
                    currentStep = Step.Audio;
                    readLine = sr.ReadLine();
                }
                else if (readLine.StartsWith("[Note]"))
                {
                    currentStep = Step.Note;
                    readLine = sr.ReadLine();
                }

                if (currentStep == Step.Description)
                {
                    if (readLine.StartsWith("Title"))
                        sheet.title = readLine.Split(':')[1].Trim();
                    else if (readLine.StartsWith("Artist"))
                        sheet.artist = readLine.Split(':')[1].Trim();
                }
                else if (currentStep == Step.Audio)
                {
                    if (readLine.StartsWith("BPM"))
                        sheet.bpm = int.Parse(readLine.Split(':')[1].Trim());
                    else if (readLine.StartsWith("Offset"))
                        sheet.offset = int.Parse(readLine.Split(':')[1].Trim());
                    else if (readLine.StartsWith("Signature"))
                    {
                        string[] s = readLine.Split(':');
                        s = s[1].Split('/');
                        int[] sign = { int.Parse(s[0].Trim()), int.Parse(s[1].Trim()) };
                        sheet.signature = sign;
                    }
                }
                else if (currentStep == Step.Note)
                {
                    if (string.IsNullOrEmpty(readLine))
                        break;

                    string[] s = readLine.Split(',');
                    int time = int.Parse(s[0].Trim());
                    int type = int.Parse(s[1].Trim());
                    int line = int.Parse(s[2].Trim());
                    int tail = -1;
                    if (s.Length > 3)
                        tail = int.Parse(s[3].Trim());
                    Note note = new Note(time, type, line, tail);
                    sheet.notes.Add(note);

                    if (type == 0)
                    {
                        type0NoteIndices.Add(noteIndex);
                    }

                    noteIndex++;
                }

                readLine = sr.ReadLine();
            }
        }

        int numberOfNotesToModify = Mathf.CeilToInt(type0NoteIndices.Count * 0.05f);
        int halfNumberOfNotesToModify = numberOfNotesToModify / 2;

        List<int> selectedIndices = new List<int>();
        while (selectedIndices.Count < numberOfNotesToModify)
        {
            int randomIndex = UnityEngine.Random.Range(0, type0NoteIndices.Count);
            if (!selectedIndices.Contains(randomIndex))
            {
                selectedIndices.Add(randomIndex);
            }
        }

        for (int i = 0; i < selectedIndices.Count; i++)
        {
            int noteIndex = type0NoteIndices[selectedIndices[i]];
            Note note = sheet.notes[noteIndex];
            note.line = i < halfNumberOfNotesToModify ? 5 : 6; //yy 0803
            note.type = i < halfNumberOfNotesToModify ? 2 : 3;
            sheet.notes[noteIndex] = note;
        }
    }



    public IEnumerator IEGetClipExternal(string title)
    {
        string clipPath = $"{externalPath}/{title}/{title}.mp3";

        using UnityWebRequest request =
            UnityWebRequestMultimedia.GetAudioClip($"file://{clipPath}", AudioType.MPEG);
        yield return request.SendWebRequest();
        clip = DownloadHandlerAudioClip.GetContent(request);
        clip.name = title;
    }

    public IEnumerator IEGetImgExternal(string title)
    {
        string texturePath = $"{externalPath}/{title}/{title}.jpg";

        using UnityWebRequest request =
            UnityWebRequestTexture.GetTexture($"file://{texturePath}");
        yield return request.SendWebRequest();
        Texture2D t = DownloadHandlerTexture.GetContent(request);
        img = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
        img.name = title;
    }
    public IEnumerator IEGetClipInternal(string title)
    {
        string clipPath = $"file://{streamingAssetsPath}/{title}/{title}.mp3";
#if UNITY_ANDROID
        clipPath = $"{streamingAssetsPath}/{title}/{title}.mp3";
#endif
        using UnityWebRequest request =
            UnityWebRequestMultimedia.GetAudioClip(clipPath, AudioType.MPEG);
        yield return request.SendWebRequest();
        clip = DownloadHandlerAudioClip.GetContent(request);
        clip.name = title;
    }

    public IEnumerator IEGetImgInternal(string title)
    {
        string coverImgPath = $"file://{streamingAssetsPath}/{title}/{title}.jpg";
#if UNITY_ANDROID
        coverImgPath = $"{streamingAssetsPath}/{title}/{title}.jpg";
#endif
        using UnityWebRequest request =
            UnityWebRequestTexture.GetTexture(coverImgPath);
        yield return request.SendWebRequest();
        Texture2D t = DownloadHandlerTexture.GetContent(request);
        img = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
        img.name = title;
    }



    private void SaveModifiedSheet()
    {
        string path = $"{streamingAssetsPath}/newsheet.sheet";
        
        using (StreamWriter sw = new StreamWriter(path))
        {
            // Write Description section
            sw.WriteLine("[Description]");
            sw.WriteLine($"Title: {sheet.title}");
            sw.WriteLine($"Artist: {sheet.artist}");
            sw.WriteLine();

            // Write Audio section
            sw.WriteLine("[Audio]");
            sw.WriteLine($"BPM: {sheet.bpm}");
            sw.WriteLine($"Offset: {sheet.offset}");
            sw.WriteLine($"Signature: {sheet.signature[0]}/{sheet.signature[1]}");
            sw.WriteLine();

            // Write Note section
            sw.WriteLine("[Note]");
            foreach (Note note in sheet.notes)
            {
                if (note.type == (int)NoteType.Long)
                {
                    sw.WriteLine($"{note.time}, {note.type}, {note.line}, {note.tail}");
                }
                else
                {
                    sw.WriteLine($"{note.time}, {note.type}, {note.line}");
                }
            }
        }
    }

}
