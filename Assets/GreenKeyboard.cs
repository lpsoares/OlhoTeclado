using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GreenKeyboard : AbstractKeyboard
{
    private readonly DecoderAPI decoderAPI;
    private readonly ContextGazeInteraction contextGazeInteraction;
    private KeyboardState curState = KeyboardState.Initial;
    private KeyboardContext curContext = null;
    private List<WordCandidates> wordSequence = new();
    private Color backgroundColor = new(0.01f, 0.55f, 0.01f);
    private Color keyColor = new(0.01f, 0.25f, 0.01f);

    public GreenKeyboard(ContextGazeInteraction contextGazeInteraction, Func<KeyboardContext> instantiateContext, DecoderAPI decoderAPI, List<ITextChangeListener> textChangeListeners, List<IContextChangeListener> contextChangeListeners, List<IContextPositionsListener> contextPositionListeners)
        : base(KeyboardType.Blue, instantiateContext, textChangeListeners, contextChangeListeners, contextPositionListeners)
    {
        this.contextGazeInteraction = contextGazeInteraction;
        this.decoderAPI = decoderAPI;

        KeyboardState[] states = { KeyboardState.InactiveNext, KeyboardState.Next, KeyboardState.Current, KeyboardState.Previous, KeyboardState.InactivePrevious };
        for (int i = 0; i < states.Length; i++)
        {
            var context = instantiateContext();
            context.InitKeyLayout(new()
            {
                new List<string> { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },
                new List<string> { "A", "S", "D", "F", "G", "H", "J", "K", "L" },
                new List<string> { "Z", "X", "C", "V", "B", "N", "M", "'", "." },
            }, 5, new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "'" });
            keyboardContexts.Add(context);
            context.backgroundColor = backgroundColor;
            context.keyColor = keyColor;
            context.State = states[i];
            context.Depth = context.TargetDepth;

            if (context.State == KeyboardState.Current)
            {
                Dictionary<string, List<float>> keyPositions = new();
                foreach (var key in context.Keys)
                {
                    keyPositions[key.label.ToLower()] = new List<float> { key.X, key.Y, key.Width, key.Height };
                }
                decoderAPI.Keys = keyPositions;
            }
        }

        NotifyContextPositionsListeners();
    }

    public override KeyboardContext Update(out Vector3 gazeInContext, out Vector2 gaze2DInContext)
    {
        KeyboardContext context = contextGazeInteraction.GetCurrentContext(keyboardContexts, out gazeInContext, out gaze2DInContext);

        if (context != null)
        {
            if (curState == KeyboardState.Initial)
            {
                if (context.State == KeyboardState.Current)
                {
                    curState = KeyboardState.Current;
                    curContext = context;
                    NotifyContextChangeListeners(curContext);
                }
            }
            else if (curState != context.State)
            {
                SwitchContext(context);
            }

            if (curContext != null && curContext.State == KeyboardState.Current)
            {
                curContext.CurrentGaze = gazeInContext;
                if (curContext.GazeInKeys)
                {
                    decoderAPI.AddGesturePoint(Time.time * 1000, gaze2DInContext.x, gaze2DInContext.y);
                }
                else
                {
                    decoderAPI.ResetPoints();
                }
            }
        }

        return context;
    }

    private void SwitchContext(KeyboardContext newContext)
    {
        KeyboardContext previousContext = curContext;
        curContext = newContext;

        NotifyContextChangeListeners(newContext);

        KeyboardState previousState = curState;
        curState = newContext.State;

        int stateDiff = 0;
        if (previousState == KeyboardState.Current && curState == KeyboardState.Next)
        {
            ResetAllContextCandidates();
            stateDiff = 1;

            var lastKey = previousContext.LastSelectedKey;
            if (lastKey != null && lastKey.IsKey == true)
            {
                if (lastKey.IsCandidateKey)
                {
                    wordSequence[^1].CurrentWord = lastKey.label.ToLower();
                    curContext.Candidates = wordSequence[^1].Candidates;
                }
                else
                {
                    wordSequence.Add(new WordCandidates(previousContext.LastSelectedKey.label.ToLower(), new List<string>()));
                }
                UpdateTextFromSequence();
            }
            else
            {
                decoderAPI.DecodeGesture((decodedWords) =>
                {
                    // TODO: Stop next context switch until this decoding is done
                    curContext.Candidates = decodedWords;
                    if (decodedWords.Count > 0)
                    {
                        var candidates = new List<string>(decodedWords);
                        wordSequence.Add(new WordCandidates(candidates[0], candidates));
                        Debug.Log($"Decoded words: {string.Join(", ", decodedWords)}");
                        UpdateTextFromSequence();
                    }
                });
            }
        }
        else if (previousState == KeyboardState.Current && curState == KeyboardState.Previous)
        {
            ResetAllContextCandidates();
            stateDiff = -1;
            if (wordSequence.Count > 0)
            {
                wordSequence = wordSequence.GetRange(0, wordSequence.Count - 1);
                if (wordSequence.Count > 0)
                {
                    curContext.Candidates = wordSequence[^1].Candidates;
                }
                UpdateTextFromSequence();
            }
        }
        else if (curState == KeyboardState.Current)
        {
            decoderAPI.SetContext("");
            decoderAPI.ResetPoints();
        }
        if (stateDiff != 0)
        {
            foreach (var ctx in keyboardContexts)
            {
                ctx.State = (KeyboardState)(((int)ctx.State + stateDiff + 5) % 5);
                ctx.CurrentGaze = Vector3.zero;
            }
        }
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
        bool markLast = curContext.Candidates.Count > 0;

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
                // If the last word has candidates, we mark it with a special character
                markedWord = $"<mark=#{keyColor.WithAlpha(0.3f).ToHexString()}>{currentWord}</mark>";
            }
            // If the word has no candidates, it is a single character that came from a key press (e.g. punctuation)
            string preText = word.Candidates.Count > 0 ? " " : "";
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
}

internal class WordCandidates
{
    public List<string> Candidates { get; } = new();
    public string CurrentWord { get; set; } = string.Empty;

    public WordCandidates(string word, List<string> candidates)
    {
        CurrentWord = word;
        Candidates.AddRange(candidates);
    }
}