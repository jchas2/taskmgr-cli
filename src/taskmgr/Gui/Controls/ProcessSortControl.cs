using Task.Manager.Cli.Utils;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Controls.ListView;

namespace Task.Manager.Gui.Controls;

public sealed class ProcessSortControl : Control
{
    private readonly ListView _listView;

    public ProcessSortControl(ISystemTerminal terminal, Theme theme) : base(terminal)
    {
        _listView = new ListView(terminal);
        _listView.BackgroundHighlightColour = theme.BackgroundHighlight;
        _listView.ForegroundHighlightColour = theme.ForegroundHighlight;
        _listView.BackgroundColour = theme.Background;
        _listView.ForegroundColour = theme.Foreground;
        _listView.HeaderBackgroundColour = theme.HeaderBackground;
        _listView.HeaderForegroundColour = theme.HeaderForeground;
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("SORT BY"));
        
        Controls.Add(_listView);
    }

    private void LoadItems()
    {
        var columns = Enum.GetValues<ProcessControl.Columns>()
            .Where(c => c != ProcessControl.Columns.Count);
        
        foreach (var column in columns) {
            _listView.Items.Add(new ListViewItem(column.GetDescription()));
        }
    }
    
    protected override void OnDraw()
    {
        if (_listView.Items.Count == 0) {
            LoadItems();            
        }
        
        _listView.Draw();
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo) =>
        _listView.KeyPressed(keyInfo);

    protected override void OnLoad() =>
        _listView.Load();

    protected override void OnResize()
    {
        _listView.X = X;
        _listView.Y = Y;
        _listView.Width = Width;
        _listView.Height = Height;

        _listView.ColumnHeaders[0].Width = Width;
        
        _listView.Resize();
    }
}