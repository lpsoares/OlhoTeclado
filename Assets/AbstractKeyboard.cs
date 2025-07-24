using System;
using System.Collections.Generic;
using UnityEngine;

public enum KeyboardType
{
    Red,
    Blue,
    Green
}

public abstract class AbstractKeyboard
{
    public List<KeyboardContext> keyboardContexts;
    protected readonly List<ITextChangeListener> textChangeListeners;
    protected readonly List<IContextChangeListener> contextChangeListeners;
    protected readonly List<IContextPositionsListener> contextPositionListeners;
    protected readonly List<ICandidateListListener> candidateListListeners;
    protected readonly List<IKeyPressListener> keyPressListeners;

    public KeyboardType Type
    {
        get; private set;
    }
    protected Func<KeyboardContext> instantiateContext;
    public string Text
    {
        get;
        protected set;
    }
    public string RichText
    {
        get;
        protected set;
    }

    public AbstractKeyboard(KeyboardType type, Func<KeyboardContext> instantiateContext, List<ITextChangeListener> textChangeListeners, List<IContextChangeListener> contextChangeListeners, List<IContextPositionsListener> contextPositionListeners, List<ICandidateListListener> candidateListListeners, List<IKeyPressListener> keyPressListeners)
    {
        Type = type;
        this.instantiateContext = instantiateContext;
        this.textChangeListeners = textChangeListeners;
        this.contextChangeListeners = contextChangeListeners;
        this.contextPositionListeners = contextPositionListeners;
        this.candidateListListeners = candidateListListeners;
        this.keyPressListeners = keyPressListeners;
        keyboardContexts = new List<KeyboardContext>();
        Text = string.Empty;
    }

    public abstract KeyboardContext Update(out Vector3 gazeInContext, out Vector2 gaze2DInContext);

    public ContextPositionData[] GetContextPositions()
    {
        List<ContextPositionData> contextPositions = new List<ContextPositionData>();
        foreach (var ctx in keyboardContexts)
        {
            var transform = ctx.transform;
            contextPositions.Add(new ContextPositionData(ctx.State.ToString(), transform.position, transform.up, transform.right, transform.forward));
        }
        ContextPositionData[] contextPositionData = contextPositions.ToArray();
        return contextPositionData;
    }

    public virtual void ResetText()
    {
        Text = string.Empty;
        RichText = string.Empty;
        NotifyTextChangeListeners(Text);
    }

    public void NotifyKeyPressListeners(string keyName, string keyLabel, string keyValue)
    {
        foreach (var listener in keyPressListeners)
        {
            listener.OnKeyPress(keyName, keyLabel, keyValue);
        }
    }

    public void NotifyContextChangeListeners(KeyboardContext newContext)
    {
        foreach (var listener in contextChangeListeners)
        {
            listener.OnContextChanged(newContext);
        }
    }

    public void NotifyTextChangeListeners(string newText)
    {
        foreach (var listener in textChangeListeners)
        {
            listener.OnTextChanged(newText);
        }
    }

    public void NotifyContextPositionsListeners()
    {
        ContextPositionData[] contextPositionData = GetContextPositions();
        foreach (var listener in contextPositionListeners)
        {
            listener.OnContextPositionsChanged(contextPositionData);
        }
    }

    public void NotifyCandidateListListeners(string[] candidates)
    {
        foreach (var listener in candidateListListeners)
        {
            listener.OnCandidateListChanged(candidates);
        }
    }
}

public interface IKeyPressListener
{
    void OnKeyPress(string keyName, string keyLabel, string keyValue);
}

public interface ITextChangeListener
{
    void OnTextChanged(string newText);
}

public interface IContextChangeListener
{
    void OnContextChanged(KeyboardContext newContext);
}

public interface IContextPositionsListener
{
    void OnContextPositionsChanged(ContextPositionData[] contextPositionData);
}

public interface ICandidateListListener
{
    void OnCandidateListChanged(string[] candidates);
}

public class ContextPositionData
{
    public string contextName;
    public Vector3 origin;
    public Vector3 upVector;
    public Vector3 rightVector;
    public Vector3 forwardVector;

    public ContextPositionData(string contextName, Vector3 origin, Vector3 upVector, Vector3 rightVector, Vector3 forwardVector)
    {
        this.contextName = contextName;
        this.origin = origin;
        this.upVector = upVector;
        this.rightVector = rightVector;
        this.forwardVector = forwardVector;
    }
}