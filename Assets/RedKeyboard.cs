using System;
using System.Collections.Generic;
using UnityEngine;

public class RedKeyboard : AbstractKeyboard
{
    private readonly ContextGazeInteraction contextGazeInteraction;
    private KeyboardState curState = KeyboardState.Initial;
    private KeyboardContext curContext = null;

    public RedKeyboard(ContextGazeInteraction contextGazeInteraction, Func<KeyboardContext> instantiateContext, List<ITextChangeListener> textChangeListeners, List<IContextChangeListener> contextChangeListeners, List<IContextPositionsListener> contextPositionListeners, List<ICandidateListListener> candidateListListeners, List<IKeyPressListener> keyPressListeners)
        : base(KeyboardType.Red, instantiateContext, textChangeListeners, contextChangeListeners, contextPositionListeners, candidateListListeners, keyPressListeners)
    {
        this.contextGazeInteraction = contextGazeInteraction;
        KeyboardState[] states = { KeyboardState.InactiveNext, KeyboardState.Next, KeyboardState.Current, KeyboardState.Previous, KeyboardState.InactivePrevious };
        for (int i = 0; i < states.Length; i++)
        {
            var context = instantiateContext();
            context.InitKeyLayout(new()
            {
                new List<string> { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P" },
                new List<string> { "A", "S", "D", "F", "G", "H", "J", "K", "L" },
                new List<string> { "Z", "X", "C", "V", "B", "N", "M", "'" },
                new List<string> { " " },
            });
            keyboardContexts.Add(context);
            context.keyColor = new Color(0.55f, 0.01f, 0.01f);
            context.State = states[i];
            context.Depth = context.TargetDepth;
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
                KeyboardContext previousContext = curContext;
                curContext = context;
                NotifyContextChangeListeners(context);
                KeyboardState previousState = curState;
                curState = context.State;

                int stateDiff = 0;
                if (previousState == KeyboardState.Current && curState == KeyboardState.Next)
                {
                    stateDiff = 1;
                    Text += previousContext.LastSelectedKey == null ? "" : previousContext.LastSelectedKey.label.ToLower();
                }
                else if (previousState == KeyboardState.Current && curState == KeyboardState.Previous)
                {
                    stateDiff = -1;
                    if (Text.Length > 0)
                    {
                        Text = Text[..^1];
                    }
                }
                if (stateDiff != 0)
                {
                    NotifyTextChangeListeners(Text);
                    foreach (var ctx in keyboardContexts)
                    {
                        ctx.State = (KeyboardState)(((int)ctx.State + stateDiff + 5) % 5);
                        ctx.CurrentGaze = Vector3.zero;
                    }
                }
            }

            if (curContext != null && curContext.State == KeyboardState.Current)
            {
                curContext.CurrentGaze = gazeInContext;
            }
        }

        return context;
    }
}