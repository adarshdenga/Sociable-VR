using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class GameLoop : MonoBehaviour {
    [SerializeField]
    private UIBridge uiBridge;

    [SerializeField]
    private AudioSource debugSource; // for this prototype only

    [SerializeField]
    private TextAsset startScript;

    private JArray lines;
    private int lineIdx; // next line to execute; think of PC
    private bool lineLock; // single thread, dont panic

    private const bool autoStep = true;

    private Dictionary<string, int> scriptVars;
    private Dictionary<string, NpcFeedback> npcFeedbacks;
    private SpeechRecognizer recognizer;


    public void Step() {
        if(lineLock)
            return;
        lineLock = true;
        if(lineIdx >= lines.Count)
            return;

        var line = lines[lineIdx];
        Debug.Log($"Parse #{lineIdx}: {line}");
        var type = (string)line["type"];
        switch(type) {
            case "action":
                ParseAction(line);
                return;

            case "end":
                ParseEnd(line);
                return;

            case "choice":
                ParseChoice(line);
                return;

            case "jump":
                ParseJump(line);
                return;

            case "normal":
                ParseNormalUtterance(line);
                return;

            case "player":
                ParsePlayerUtterance(line);
                return;

            default:
                throw new SyntaxErrorException($"Scene json syntax error {line}");
        }
    }


    private void ParseAction(JToken line) {
        var func = (string)line["func"];
        var param = line["param"];
        switch(func) {
            case "change scene":
                var target = (string)param;
                ScoreUI.score = scriptVars["score"];
                SceneManager.LoadScene($"Scenes/{target.RSplit('.')}");
                return;

            case "map names":
                var names = (JObject)param;
                foreach(var (npc, gobj) in names) {
                    var go = GameObject.Find((string)gobj);
                    if(go == null)
                        continue;
                    if(!go.TryGetComponent<NpcFeedback>(out var fb))
                        continue;
                    npcFeedbacks.Add(npc, fb);
                }
                break;
        }
        lineIdx++;
        lineLock = false;
    }


    private void ParseEnd(JToken line) {
        ScoreUI.score = scriptVars["score"];
        SceneManager.LoadScene("Scenes/00-Score");
    }


    private void ParseChoice(JToken line) {
        uiBridge.ClearOptions();
        var hint = (string)line["text"] ?? string.Empty;
        uiBridge.SetSubtitle(utterance: hint);

        var choices = new List<(Button, KeyWords)>();
        foreach(var choice in (JArray)line["choice"]) {
            var target = (string)choice["target"];
            var text = (string)choice["text"];
            var score = (int)(choice["score"] ?? 0);

            var button = uiBridge.AddOptions(text);
            button.onClick.AddListener(() => {
                Debug.Log($"Choose: `{text}` Score: ({score})");
                scriptVars["score"] += score;
                LoadScene(target);
                Step();
            });
            choices.Add((button, KeyWords.FromToken(choice)));
        }

#if !SCRIPT_JSON_DEBUG_ONLY
        StartCoroutine(WaitSpeechThenJump(choices));
#endif
    }


    private void ParseJump(JToken line) {
        LoadScene((string)line["target"]);
        Step();
    }


    private void ParseNormalUtterance(JToken line) {
        var speaker = (string)line["speaker"] ?? string.Empty;
        var utterance = (string)line["text"] ?? string.Empty;
        if(GameConfig.showSubtitle)
            uiBridge.SetSubtitle(speaker, utterance);
        else
            uiBridge.SetSubtitle();
        var hasAudio = LoadAudio(line, out var clip);
        StartCoroutine(hasAudio ? WaitAudioStopThenUnlock(line, clip) : WaitSecondsThenUnlock(3));
    }


    private void ParsePlayerUtterance(JToken line) {
        var speaker = string.IsNullOrEmpty(GameConfig.playerName) ? "Speler" : GameConfig.playerName;
        var utterance = (string)line["text"];
        uiBridge.SetSubtitle(speaker, utterance);
#if SCRIPT_JSON_DEBUG_ONLY
        StartCoroutine(WaitSecondsThenUnlock(3));
#else
        StartCoroutine(WaitSpeechThenUnlock(line));
#endif
    }


    private IEnumerator WaitSpeechThenJump(IReadOnlyList<(Button, KeyWords)> choices) {
        var canBreak = false;
        var nextLoop = false;
        var index = -1;

        void Callback(bool ok, string stt) {
            if(ok) {
                var rpl = stt.STTClean().Split(' ');
                foreach(var ((_, kw), idx) in choices.Select((it, id) => (it, id))) {
                    if(kw.Match(rpl)) {
                        canBreak = true;
                        index = idx;
                        break;
                    }
                }
            }
            nextLoop = true;
        }

        recognizer.OnSpeaking += uiBridge.SignalSpeaking;
        recognizer.OnAttempt += Callback;
        while(!canBreak) {
            nextLoop = false;
            recognizer.RequestSession();
            yield return new WaitUntil(() => nextLoop);
        }
        recognizer.OnAttempt -= Callback;
        recognizer.OnSpeaking -= uiBridge.SignalSpeaking;
        recognizer.EndSession();

        choices[index].Item1.onClick.Invoke();
    }


    private IEnumerator WaitSpeechThenUnlock(JToken line) {
        var canBreak = false;
        var nextLoop = false;

        var keywords = KeyWords.FromToken(line);

        void Callback(bool ok, string stt) {
            if(ok) {
                var rpl = stt.STTClean().Split(' ');
                Debug.Log($"Check `{keywords}` ? `{string.Join(' ', rpl)}`");
                if(keywords.Match(rpl))
                    canBreak = true;
            }
            nextLoop = true;
        }

        recognizer.OnSpeaking += uiBridge.SignalSpeaking;
        recognizer.OnAttempt += Callback;
        while(!canBreak) {
            nextLoop = false;
            recognizer.RequestSession();
            yield return new WaitUntil(() => nextLoop);
        }
        recognizer.OnAttempt -= Callback;
        recognizer.OnSpeaking -= uiBridge.SignalSpeaking;
        recognizer.EndSession();

        lineIdx++;
        lineLock = false;
    }


    private IEnumerator WaitAudioStopThenUnlock(JToken line, AudioClip clip, float epsilon = .6f) {
        var speaker = (string)line["speaker"] ?? string.Empty;
        var utterance = (string)line["text"] ?? string.Empty;
        var hasFb = npcFeedbacks.TryGetValue(speaker, out var fb);
        if(hasFb) {
            var info = new NpcFeedback.SpeechInfo { name = speaker, utterance = utterance, clip = clip };
            fb.StartSpeaking(info);
        }
        var source = debugSource; // todo map to npc
        source.clip = clip;
        source.Play();
        yield return new WaitWhile(() => source.isPlaying);
        if(hasFb)
            fb.StopSpeaking();
        yield return new WaitForSeconds(epsilon);
        lineIdx++;
        lineLock = false;
    }


    private IEnumerator WaitSecondsThenUnlock(float seconds) {
        yield return new WaitForSeconds(seconds);
        lineIdx++;
        lineLock = false;
    }


    private static bool LoadAudio(JToken line, out AudioClip clip) {
        clip = null;
        var path = (string)line["audio"];
        if(string.IsNullOrEmpty(path))
            return false;
        clip = Resources.Load<AudioClip>($"Audios/{path.RSplit('.')}");
        return clip != null;
    }


    private bool LoadScene(string path) {
        var json = Resources.Load<TextAsset>($"Scenarios/{path.RSplit('.')}");
        return LoadScene(json);
    }


    private bool LoadScene(TextAsset json) {
        var jobj = JObject.Parse(json.text);
        lines = (JArray)jobj["main"];
        lineIdx = 0;
        lineLock = false;

        debugSource.Stop();
        uiBridge.ClearOptions();
        uiBridge.SetSubtitle();

        return true;
    }


    private void Start() {
        recognizer = new SpeechRecognizer();

        uiBridge.OnMicBtnClick.AddListener(() => {
            // todo skip current line
        });

        scriptVars = new Dictionary<string, int>();
        scriptVars["score"] = ScoreUI.score;

        npcFeedbacks = new Dictionary<string, NpcFeedback>();

        LoadScene(startScript);
        Step();
    }


    private void Update() {
        if(autoStep)
            Step();
    }


    private void OnDestroy() {
        recognizer?.Dispose();
    }
}


internal class KeyWords {
    private readonly string[] words;
    private readonly string type;


    public KeyWords(string[] words, string type) {
        this.words = words ?? new string[] { };
        this.type = type;
    }


    public static KeyWords FromToken(JToken token) {
        var text = token["text"];
        var keywords = token["keywords"];
        if(keywords is null) {
            if(text is null)
                return new KeyWords(null, "empty");
            var words = ((string)text).STTClean().Split(' ');
            //const int num = 3;
            //if(words.Length > num)
            //    words = words.OrderBy(_ => UnityEngine.Random.value).Take(num).ToArray();
            return new KeyWords(words, "all");
        }
        var type = keywords["type"];
        var value = keywords["value"];
        if(value is null) {
            if(text is null)
                return new KeyWords(null, "empty");
            var words = ((string)text).STTClean().Split(' ');
            return new KeyWords(words, (string)type);
        }
        return new KeyWords(value.Select(v => (string)v).ToArray(), (string)type);
    }


    public bool Match(string[] pattern) {
        if(pattern.Contains(Extensions.BypassToken))
            return true;
        return type switch {
            "all" => words.All(pattern.Contains),
            "any" => words.Any(pattern.Contains),
            "empty" => words.Any(w => !string.IsNullOrEmpty(w)),
            _ => false
        };
    }


    public override string ToString() {
        return $"[{type}] {string.Join(' ', words)}";
    }
}
