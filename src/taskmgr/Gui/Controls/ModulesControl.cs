using System.Diagnostics;
using Task.Manager.Configuration;
using Task.Manager.System;
using Task.Manager.System.Controls;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public partial class ModulesControl : Control
{
    private readonly Theme _theme;
    private readonly ListView _listView;

    private int _selectedProcessId = -1;
    
    public ModulesControl(ISystemTerminal terminal, Theme theme)
        : base(terminal)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));

        _listView = new ListView(terminal) {
            BackgroundHighlightColour = _theme.BackgroundHighlight,
            ForegroundHighlightColour = _theme.ForegroundHighlight,
            BackgroundColour = _theme.Background,
            ForegroundColour = _theme.Foreground,
            HeaderBackgroundColour = _theme.HeaderBackground,
            HeaderForegroundColour = _theme.HeaderForeground
        };
        
        _listView.ColumnHeaders
            .Add(new ListViewColumnHeader("MODULE"))
            .Add(new ListViewColumnHeader("FILENAME"));
        
        Controls.Add(_listView);
    }

    private void LoadModuleInfos()
    {
        _listView.Items.Clear();

        var modules = ModuleInfo.GetModules(SelectedProcessId)
            .OrderBy(m => m.ModuleName)
            .ToList();
        
        foreach (var moduleInfo in modules) {
            var item = new ModuleListViewItem(moduleInfo, _theme);
            _listView.Items.Add(item);
        }
    }
    
    protected override void OnDraw()
    {
        if (_listView.Items.Count == 0) {
            LoadModuleInfos();
        }

        _listView.Draw();
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo, ref bool handled) =>
        _listView.KeyPressed(keyInfo, ref handled);

    protected override void OnLoad()
    {
        _listView.Load();
    }

    protected override void OnResize()
    {
        _listView.X = X;
        _listView.Y = Y;
        _listView.Width = Width;
        _listView.Height = Height;
        
        _listView.ColumnHeaders[(int)Columns.ModuleName].Width = ColumnModuleNameWidth;

        int total =
            ColumnModuleNameWidth + ColumnMargin;
        
        if (total + ColumnFileNameWidth + ColumnMargin < Width) {
            _listView.ColumnHeaders[(int)Columns.FileName].Width = Width - total - ColumnMargin;    
        }
        else {
            _listView.ColumnHeaders[(int)Columns.FileName].Width = ColumnFileNameWidth;
        }

        for (int i = 0; i < (int)Columns.Count; i++) {
            _listView.ColumnHeaders[i].BackgroundColour = _theme.HeaderBackground;
            _listView.ColumnHeaders[i].ForegroundColour = _theme.HeaderForeground;
        }
        
        _listView.Resize();
    }

    public int SelectedProcessId
    {
        get => _selectedProcessId;
        set => _selectedProcessId = value;
    }
}