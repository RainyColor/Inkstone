using Godot;
using System;
using System.Collections.Generic;
using Ink.Runtime;

#if TOOLS
[Tool]
#endif
public partial class InkPlayer : Node
{
    public delegate void InkContinuedEventHandler(string text, string[] tags);
    public delegate void InkEndedEventHandler();
    public delegate void InkChoicesEventHandler(string[] choices);
    public delegate void InkErrorEventHandler(string message, bool isWarning);

    public event InkContinuedEventHandler InkContinued;
    public event InkEndedEventHandler InkEnded;
    public event InkChoicesEventHandler InkChoices;
    public event InkErrorEventHandler InkError;

    [Export] public bool AutoLoadStory = false;
    [Export(PropertyHint.File, "*.json")]
    public string StoryPath = null;

    public string CurrentText => _inkStory?.currentText ?? default;
    public string[] CurrentTags => _inkStory?.currentTags.ToArray() ?? default;
    public string[] CurrentChoices => _inkStory?.currentChoices.ConvertAll(choice => choice.text).ToArray() ?? default;
    public bool CanContinue => _inkStory?.canContinue ?? false;
    public bool HasChoices => _inkStory?.currentChoices.Count > 0;
    public string[] GlobalTags => _inkStory?.globalTags?.ToArray() ?? default;

    private Story _inkStory = null;
    private string _storyContent = null;

    public override void _Ready()
    {
        if (AutoLoadStory && StoryPath != null)
        {
            LoadStory(StoryPath);
        }
    }

    private void Reset()
    {
        _inkStory = null;
    }

    public Error LoadStory()
    {
        Reset();
        _inkStory = new Story(_storyContent);
        _inkStory.onError += OnStoryError;

        return Error.Ok;
    }

    public Error LoadStory(string path)
    {
        StoryPath = path;

        FileAccess file = FileAccess.Open(StoryPath, FileAccess.ModeFlags.Read);
        Error err = FileAccess.GetOpenError();
        if ( err != Error.Ok)
        {
            return err;
        }

        _storyContent = file.GetAsText();
        return LoadStory();
    }

    public Error LoadStoryAndSetState(string state)
    {
        Error error = LoadStory();
        if (error == Error.Ok)
        {
            SetState(state);
        }
        
        return error;
    }

    public Error LoadStoryAndSetState(string path, string state)
    {
        Error error = LoadStory(path);
        if (error == Error.Ok)
        {
            SetState(state);
        }
        
        return error;
    }

    /// <summary>
    /// Continue the story for one line of content.
    /// </summary>
    /// <returns>The next line of story content.</returns>
    public string Continue()
    {
        string text = null;

        if (CanContinue)
        {
            _inkStory.Continue();
            text = CurrentText;
            InkContinued(CurrentText, CurrentTags);
        }
        else if (HasChoices)
        {
            InkChoices(CurrentChoices);
        }
        else
        {
            InkEnded();
        }

        return text;
    }

    /// <summary>
    /// Choose a choice from the CurrentChoices.
    /// </summary>
    /// <param name="index">The index of the choice to choose from CurrentChoices.</param>
    public void ChooseChoiceIndex(int index)
    {
        if (index < 0 || index >= _inkStory?.currentChoices.Count) return;
        _inkStory.ChooseChoiceIndex(index);
    }

    /// <summary>
    /// Choose a choice from the CurrentChoices and automatically continue the story for one line of content.
    /// </summary>
    /// <param name="index">The index of the choice to choose from CurrentChoices.</param>
    /// <returns>The next line of story content.</returns>
    public string ChooseChoiceIndexAndContinue(int index)
    {
        ChooseChoiceIndex(index);
        return Continue();
    }

    /// <summary>
    /// Change the current position of the story to the given <paramref name="pathString"/>.
    /// </summary>
    /// <param name="pathString">A dot-separated path string.</param>
    /// <returns>False if there was an error during the change, true otherwise.</returns>
    public bool ChoosePathString(string pathString)
    {
        if (_inkStory != null)
        {
            try
            {
                _inkStory.ChoosePathString(pathString);

                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr(e.ToString());
            }
        }

        return false;
    }

    public void SwitchFlow(string flowName)
    {
        _inkStory?.SwitchFlow(flowName);
    }

    public void SwitchToDefaultFlow()
    {
        _inkStory?.SwitchToDefaultFlow();
    }

    public void RemoveFlow(string flowName)
    {
        _inkStory?.RemoveFlow(flowName);
    }

    public object GetVariable(string name)
    {
        return _inkStory?.variablesState[name];
    }

    public void SetVariable(string name, object value)
    {
        if (_inkStory == null) return;

        _inkStory.variablesState[name] = value;
    }

    public void ObserveVariable(string name, Story.VariableObserver observer)
    {
        _inkStory?.ObserveVariable(name, observer);
    }

    private void RemoveVariableObserver(string name)
    {
        _inkStory.RemoveVariableObserver(null, name);
    }

    public int VisitCountAtPathString(string pathString)
    {
        return _inkStory?.state.VisitCountAtPathString(pathString) ?? 0;
    }

    public void BindExternalFunction(string inkFuncName, Callable callable)
    {
        BindExternalFunction(inkFuncName, callable, false);
    }

    public void BindExternalFunction(string inkFuncName, Callable callable, bool lookaheadSafe)
    {
        _inkStory?.BindExternalFunctionGeneral(inkFuncName, (object[] foo) => callable.Call((Variant)foo), lookaheadSafe);
    }

    public void BindExternalFunction(string inkFuncName, Func<object> func, bool lookaheadSafe)
    {
        _inkStory?.BindExternalFunction(inkFuncName, func, lookaheadSafe);
    }

    public void BindExternalFunction(string inkFuncName, Func<object> func)
    {
        BindExternalFunction(inkFuncName, func, false);
    }

    public void BindExternalFunction<T>(string inkFuncName, Func<T, object> func, bool lookaheadSafe)
    {
        _inkStory?.BindExternalFunction(inkFuncName, func, lookaheadSafe);
    }

    public void BindExternalFunction<T>(string inkFuncName, Func<T, object> func)
    {
        BindExternalFunction(inkFuncName, func, false);
    }

    public void BindExternalFunction<T1, T2>(string inkFuncName, Func<T1, T2, object> func, bool lookaheadSafe)
    {
        _inkStory?.BindExternalFunction(inkFuncName, func, lookaheadSafe);
    }

    public void BindExternalFunction<T1, T2>(string inkFuncName, Func<T1, T2, object> func)
    {
        BindExternalFunction(inkFuncName, func, false);
    }

    public void BindExternalFunction<T1, T2, T3>(string inkFuncName, Func<T1, T2, T3, object> func, bool lookaheadSafe)
    {
        _inkStory?.BindExternalFunction(inkFuncName, func, lookaheadSafe);
    }

    public void BindExternalFunction<T1, T2, T3>(string inkFuncName, Func<T1, T2, T3, object> func)
    {
        BindExternalFunction(inkFuncName, func, false);
    }

    public void BindExternalFunction<T1, T2, T3, T4>(string inkFuncName, Func<T1, T2, T3, T4, object> func, bool lookaheadSafe)
    {
        _inkStory?.BindExternalFunction(inkFuncName, func, lookaheadSafe);
    }

    public void BindExternalFunction<T1, T2, T3, T4>(string inkFuncName, Func<T1, T2, object> func)
    {
        BindExternalFunction(inkFuncName, func, false);
    }

    public object EvaluateFunction(string functionName, bool returnTextOutput, params object[] arguments)
    {
        if (!returnTextOutput)
            return _inkStory?.EvaluateFunction(functionName, arguments);

        string textOutput = null;
        object returnValue = _inkStory?.EvaluateFunction(functionName, out textOutput, arguments);
        return new object[] { returnValue, textOutput };
    }

    public string GetState()
    {
        return _inkStory.state.ToJson();
    }

    public void SetState(string state)
    {
        _inkStory.state.LoadJson(state);
    }

    public void SaveStateOnDisk(string path)
    {
        if (!path.StartsWith("res://") && !path.StartsWith("user://"))
            path = $"user://{path}";

        FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (!file.IsOpen()) return;

        file.StoreString(GetState());
    }

    public void LoadStateFromDisk(string path)
    {
        if (!path.StartsWith("res://") && !path.StartsWith("user://"))
            path = $"user://{path}";

        FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (!file.IsOpen()) return;

        file.Seek(0);
        if (file.GetLength() > 0)
            _inkStory.state.LoadJson(file.GetAsText());
    }

    public string[] TagsForContentAtPath(string pathString)
    {
        return _inkStory?.TagsForContentAtPath(pathString)?.ToArray() ?? default;
    }

    private void OnStoryError(string message, Ink.ErrorType errorType)
    {
        if (errorType == Ink.ErrorType.Author) return;  // This should never happen but eh? What's the cost of checking.

        if (GetSignalConnectionList(nameof(InkError)).Count > 0)
            // EmitSignal(nameof(InkError), message, errorType == Ink.ErrorType.Warning);
            InkError(message, errorType == Ink.ErrorType.Warning);
        else
            GD.PrintErr($"Ink had an error. It is strongly suggested that you connect an error handler to InkError. {message}");
    }
}
