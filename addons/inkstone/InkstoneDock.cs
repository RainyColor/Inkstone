#if TOOLS
using Godot;
using System;

[Tool]
public partial class InkstoneDock : Control
{
    private bool storyStarted;
    private InkPlayer player;
    private Button loadButton;
    private FileDialog fileDialog;
    private Label storyNameLabel;
    private Button startButton;
    private Button stopButton;
    private Button clearButton;
    private VBoxContainer storyText;
    private VBoxContainer storyChoices;
    private ScrollContainer scroll;

    public override void _Ready()
    {
        // Initialize top.
        loadButton = GetNode<Button>("Container/Left/Top/LoadButton");
        fileDialog = GetNode<FileDialog>("FileDialog");
        storyNameLabel = GetNode<Label>("Container/Left/Top/Label");
        startButton = GetNode<Button>("Container/Left/Top/StartButton");
        stopButton = GetNode<Button>("Container/Left/Top/StopButton");
        clearButton = GetNode<Button>("Container/Left/Top/ClearButton");

        loadButton.Pressed += () => { fileDialog.PopupCenteredRatio(0.5F); };
        fileDialog.FileSelected += LoadStoryResource;
        startButton.Pressed += StartStory;
        stopButton.Pressed += StopStory;
        clearButton.Pressed += () => { ClearStory(false); };

        // Initialize bottom.
        storyText = GetNode<VBoxContainer>("Container/Left/Scroll/Margin/StoryText");
        storyChoices = GetNode<VBoxContainer>("Container/Right/StoryChoices");
        scroll = GetNode<ScrollContainer>("Container/Left/Scroll");

        // Set icons.
        loadButton.Icon = GetThemeIcon("Load", "EditorIcons");
        startButton.Icon = GetThemeIcon("Play", "EditorIcons");
        stopButton.Icon = GetThemeIcon("Stop", "EditorIcons");
        clearButton.Icon = GetThemeIcon("Clear", "EditorIcons");

        UpdateTop();
    }

    private void UpdateTop()
    {
        bool hasStory = player != null;

        // Do not judge me.
        storyNameLabel.Text = hasStory ? player.StoryPath : string.Empty;

        startButton.Visible = hasStory && !storyStarted;
        stopButton.Visible = hasStory && storyStarted;
        clearButton.Visible = hasStory;
        clearButton.Disabled = storyText.GetChildCount() <= 0;

        storyChoices.GetParent<Control>().Visible = hasStory;
    }

    private void LoadStoryResource(string path)
    {
        if (player == null)
        {
            player = new InkPlayer();
            AddChild(player);

            player.InkContinued += OnStoryContinued;
            player.InkChoices += OnStoryChoices;
            player.InkEnded += OnStoryEnded;
        }

        if (!string.IsNullOrEmpty(fileDialog.CurrentFile))
        {
            player.LoadStory(fileDialog.CurrentPath);
        }

        ResetState();
    }

    private void ResetState()
    {
        storyStarted = false;
        ClearStory(true);
    }

    private void StartStory()
    {
        if (player == null) return;

        storyStarted = true;
        player.Continue();

        UpdateTop();
    }

    private void StopStory()
    {
        ResetState();
        player?.LoadStory();
    }

    private void ClearStory(bool clearChoices)
    {
        RemoveAllStoryContent();
        if (clearChoices)
            RemoveAllChoices();

        UpdateTop();
    }

    private void OnStoryContinued(string text, string[] tags)
    {
        text = text.Trim();
        if (text.Length == 0)
        {
            player.Continue();
            return;
        }

        Label newLine = new Label()
        {
            AutowrapMode = TextServer.AutowrapMode.Word,
            Text = text,
        };

        AddToStory(newLine);

        if (tags.Length > 0)
        {
            newLine = new Label()
            {
                AutowrapMode = TextServer.AutowrapMode.Arbitrary,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = $"# {string.Join(", ", tags)}",
            };

            newLine.AddThemeColorOverride("font_color", GetThemeColor("font_color_disabled", "Button"));
            AddToStory(newLine);
        }

        player.Continue();
    }

    private void OnStoryChoices(string[] choices)
    {
        int num = storyChoices.GetChildCount();
        for (int i = 0; i < choices.Length; i++)
        {
            if (i < num)
            {
                ChoiceButton btn = storyChoices.GetChild<ChoiceButton>(i);
                btn.Text = choices[i];
                btn.Idx = i;
                btn.Visible = true;
                continue;
            }

            ChoiceButton button = new ChoiceButton()
            {
                Text = choices[i],
                Idx = i,
            };
            button.ChoicePressed += ClickChoice;
            storyChoices.AddChild(button);
        }
    }

    private void OnStoryEnded()
    {
        CanvasItem endOfStory = new VBoxContainer();
        endOfStory.AddChild(new HSeparator());
        CanvasItem endOfStoryLine = new HBoxContainer();
        endOfStory.AddChild(endOfStoryLine);
        endOfStory.AddChild(new HSeparator());
        Control separator = new HSeparator()
        {
            SizeFlagsHorizontal = (int)(SizeFlags.Fill | SizeFlags.Expand),
        };
        Label endOfStoryText = new Label()
        {
            Text = "End of story"
        };
        endOfStoryLine.AddChild(separator);
        endOfStoryLine.AddChild(endOfStoryText);
        endOfStoryLine.AddChild(separator.Duplicate());
        AddToStory(endOfStory);
    }

    private void ClickChoice(int idx)
    {
        RemoveAllChoices();
        AddToStory(new HSeparator());
        player.ChooseChoiceIndex(idx);
        player.Continue();
    }

    private async void AddToStory(CanvasItem item)
    {
        storyText.AddChild(item);
        await ToSignal(GetTree(), "process_frame");
        await ToSignal(GetTree(), "process_frame");
        scroll.ScrollVertical = (int)scroll.GetVScrollBar().MaxValue;
    }


    private void RemoveAllStoryContent()
    {
        foreach (Node n in storyText.GetChildren())
        {
            storyText.RemoveChild(n);
        }
    }

    private void RemoveAllChoices()
    {
        int num = storyChoices.GetChildCount();
        for (int i = 0; i < num; i++)
        {
            CanvasItem n = storyChoices.GetChild<CanvasItem>(i);
            n.Visible = false;
        }
    }
}
#endif
