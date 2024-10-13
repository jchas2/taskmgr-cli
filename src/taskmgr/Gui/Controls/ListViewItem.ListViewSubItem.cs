﻿namespace Task.Manager.Gui.Controls;

public class ListViewSubItem
{
    private readonly ListViewItem _owner;
    private string? _text;
    private SubItemStyle? _style;

    public ListViewSubItem(ListViewItem owner, string? text)
    {
        _owner = owner;
        _text = text;
    }

    public ListViewSubItem(
        ListViewItem owner,
        string? text,
        ConsoleColor backgroundColor,
        ConsoleColor foregroundColor)
    {
        _owner = owner;
        _text = text;
        _style = new SubItemStyle {
            BackgroundColour = backgroundColor,
            ForegroundColour = foregroundColor
        };
    }

    public ConsoleColor BackgroundColor
    {
        get {
            if (_style != null) {
                return _style.BackgroundColour;
            }
            
            return _owner.ListView?.BackgroundColour ?? Console.BackgroundColor;
        }
        set {
            _style ??= new SubItemStyle();
            
            if (_style.BackgroundColour != value) {
                _style.BackgroundColour = value;
            }
        }
    }

    public ConsoleColor ForegroundColor
    {
        get {
            if (_style != null) {
                return _style.ForegroundColour;
            }
            
            return _owner.ListView?.ForegroundColour ?? Console.ForegroundColor;
        }
        set {
            _style ??= new SubItemStyle();
            
            if (_style.ForegroundColour != value) {
                _style.ForegroundColour = value;
            }
        }
    }
}