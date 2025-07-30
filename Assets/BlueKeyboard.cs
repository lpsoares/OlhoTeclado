using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BlueKeyboard : AbstractKeyboard, IKeyPressListener
{
    private readonly DecoderAPI decoderAPI;
    private readonly ContextGazeInteraction contextGazeInteraction;
    private readonly KeyboardContext context = null;
    private List<WordCandidates> wordSequence = new();
    private Color backgroundColor = Color.clear;
    private Color keyColor = new(1.0f, 1.0f, 1.0f);
    private Color textColor = new(0.0f, 0.0f, 0.0f);
    private Color highlightColor = new(0.7f, 0.7f, 1.0f);
    private Color dwellColor = new(0.6f, 0.6f, 0.8f);
    private bool decodingGesture = false;
    private bool isOutside = true;

    public BlueKeyboard(ContextGazeInteraction contextGazeInteraction, Func<KeyboardContext> instantiateContext, DecoderAPI decoderAPI, List<ITextChangeListener> textChangeListeners, List<IContextChangeListener> contextChangeListeners, List<IContextPositionsListener> contextPositionListeners, List<ICandidateListListener> candidateListListeners, List<IKeyPressListener> keyPressListeners)
        : base(KeyboardType.Blue, instantiateContext, textChangeListeners, contextChangeListeners, contextPositionListeners, candidateListListeners, keyPressListeners)
    {
        this.contextGazeInteraction = contextGazeInteraction;
        this.decoderAPI = decoderAPI;

        context = instantiateContext();
        context.InitKeyLayout(new()
        {
            new List<string> { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },
            new List<string> { "A", "S", "D", "F", "G", "H", "J", "K", "L" },
            new List<string> { "Z", "X", "C", "V", "B", "N", "M", "'", "." },
        }, 5, dwellEnabledKeys: new List<string> { "Candidate_Keys", "Backspace" }, keyPressListener: this, withBackspace: true);
        keyboardContexts.Add(context);
        context.backgroundColor = backgroundColor;
        context.keyColor = keyColor;
        context.highlightColor = highlightColor;
        context.dwellColor = dwellColor;
        context.textColor = textColor;
        context.State = KeyboardState.Current;
        context.Depth = context.TargetDepth;
        context.hasAlpha = true;

        Dictionary<string, List<float>> keyPositions = new();
        foreach (var key in context.Keys)
        {
            keyPositions[key.label.ToLower()] = new List<float> { key.X, key.Y, key.Width, key.Height };
        }
        decoderAPI.Keys = keyPositions;

        NotifyContextPositionsListeners();
    }

    public override KeyboardContext Update(out Vector3 gazeInContext, out Vector2 gaze2DInContext)
    {
        KeyboardContext currentContext = contextGazeInteraction.GetCurrentContext(keyboardContexts, out gazeInContext, out gaze2DInContext);

        if (!currentContext)
        {
            return null;
        }
        
        context.CurrentGaze = gazeInContext;
        if (context.GazeInKeys)
        {
            if (isOutside)
            {
                decoderAPI.ResetPoints();
                isOutside = false;
            }

            decoderAPI.AddGesturePoint(Time.time * 1000, gaze2DInContext.x, gaze2DInContext.y);
        }
        else
        {
            if (!isOutside)
            {
                DecodeGazePath();
            }
            isOutside = true;
        }
        
        return context;
    }

    private void DecodeGazePath()
    {
        ResetAllContextCandidates();
     
        decodingGesture = true;
        context.Candidates = Enumerable.Repeat("...", 10).ToList();
        decoderAPI.DecodeGesture((decodedWords) =>
        {
            decodingGesture = false;
            if (decodedWords.Count > 0)
            {
                context.Candidates = decodedWords;
                var candidates = new List<string>(decodedWords);
                wordSequence.Add(new WordCandidates(string.Empty, candidates));
                Debug.Log($"Decoded words: {string.Join(", ", decodedWords)}");
                UpdateTextFromSequence();
            }
            else
            {
                Debug.Log("No words decoded. Going back to previous state.");
                if (wordSequence.Count > 0)
                {
                    context.Candidates = wordSequence.Last().Candidates;
                }
                else
                {
                    context.Candidates = new List<string>();
                }
            }
            NotifyCandidateListListeners(context.Candidates.ToArray());
        });
    }

    private void ResetAllContextCandidates()
    {
        foreach (var ctx in keyboardContexts)
        {
            ctx.Candidates = new List<string>();
        }
    }

    private void UpdateTextFromSequence()
    {
        bool markLast = context.Candidates.Count > 0;

        string previousText = Text;
        string text = string.Empty;
        string richText = string.Empty;
        for (int i = 0; i < wordSequence.Count; i++)
        {
            var word = wordSequence[i];
            var isLast = i == wordSequence.Count - 1;
            var currentWord = word.CurrentWord;
            var markedWord = currentWord;
            if (isLast && markLast)
            {
                string wordOrSpace = string.IsNullOrEmpty(currentWord) ? " " : currentWord;
                // If the last word has candidates, we mark it with a special character
                markedWord = $"<mark=#{keyColor.WithAlpha(0.3f).ToHexString()}>{wordOrSpace}</mark>";
            }
            // If the word has no candidates, it is a single character that came from a key press (e.g. punctuation)
            string preText = word.Candidates.Count > 0 && !string.IsNullOrEmpty(currentWord) ? " " : "";
            text += preText + currentWord;
            richText += preText + markedWord;
        }
        Text = text.Trim();
        RichText = richText.Trim();
        if (Text != previousText)
        {
            decoderAPI.SetContext(Text);
            NotifyTextChangeListeners(Text);
        }
    }

    public override void ResetText()
    {
        base.ResetText();
        wordSequence.Clear();
        ResetAllContextCandidates();
        decoderAPI.SetContext(Text);
    }

    public void OnKeyPress(Key key)
    {
        Debug.Log($"Key pressed ({key.name}): {key.label}");
        if (key.name.StartsWith("Candidate"))
        {
            if (wordSequence.Count > 0)
            {
                var lastWord = wordSequence.Last();
                lastWord.CurrentWord = key.label.ToLower();
                context.Candidates = lastWord.Candidates;
                NotifyCandidateListListeners(lastWord.Candidates.ToArray());
                UpdateTextFromSequence();
            }
        }
        else if (key.name == "Backspace")
        {
            PopRightWhileEmpty();

            if (wordSequence.Count > 0)
            {
                wordSequence.RemoveAt(wordSequence.Count - 1);
                PopRightWhileEmpty();
                if (wordSequence.Count > 0)
                {
                    context.Candidates = wordSequence.Last().Candidates;
                    NotifyCandidateListListeners(context.Candidates.ToArray());
                }
                else
                {
                    context.Candidates = new List<string>();
                }
                UpdateTextFromSequence();
            }
        }
    }

    private void PopRightWhileEmpty()
    {
        while (wordSequence.Count > 0 && string.IsNullOrEmpty(wordSequence.Last().CurrentWord))
        {
            wordSequence.RemoveAt(wordSequence.Count - 1);
        }
    }
}
