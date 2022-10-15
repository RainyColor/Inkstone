#if TOOLS
using Godot;
using System;

[Tool]
public partial class Inkstone : EditorPlugin
{
    private const string AddonBasePath = "res://addons/inkstone";
    private readonly string _customTypeIconPath = $"{AddonBasePath}/icon.svg";
    private readonly string _customTypeScriptPath = $"{AddonBasePath}/InkPlayer.cs";
    private readonly string _dockScene = $"{AddonBasePath}/InkstoneDock.tscn";
    private InkstoneDock _dock;

    public override void _EnterTree()
    {
        // Custom types
        Texture2D icon = GD.Load<Texture2D>(_customTypeIconPath);
        CSharpScript customTypeScript = GD.Load<CSharpScript>(_customTypeScriptPath);
        AddCustomType("InkPlayer", "Node", customTypeScript, icon);

        // Editor
        _dock = GD.Load<PackedScene>(_dockScene).Instantiate<InkstoneDock>();
        AddControlToBottomPanel(_dock, "Inkstone");
        // VBoxContainer editorViewport = GetEditorInterface().GetEditorMainScreen();
        // Vector2i minSize = new Vector2i(_dock.CustomMinimumSize.x, (int)(editorViewport.Size.y * 0.3));
        // _dock.CustomMinimumSize = minSize;
    }

    public override void _ExitTree()
    {
        RemoveCustomType("InkPlayer");

        RemoveControlFromBottomPanel(_dock);
    }
}
#endif
