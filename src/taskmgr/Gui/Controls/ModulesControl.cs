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
    
    public ModulesControl(
        ISystemTerminal terminal, 
        Theme theme)
        : base(terminal)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        
        _listView = new ListView(terminal);
        
        Controls.Add(_listView);
        
        Theme = theme;
    }

    private void LoadModuleInfos()
    {
        _listView.Items.Clear();

        var modules = ModuleInfo.GetModules(SelectedProcessId)
            .OrderBy(m => m.ModuleName)
            .ToList();
        
        foreach (var moduleInfo in modules) {
            var item = new ModuleListViewItem(moduleInfo, Theme);
            _listView.Items.Add(item);
        }
    }
    
    protected override void OnDraw()
    {
        _listView.X = 0;
        _listView.Y = 0;
        _listView.Width = Width;
        _listView.Height = Terminal.WindowHeight - 1;

        UpdateColumnHeaders();
        LoadModuleInfos();
        
        _listView.Draw();
    }

    protected override void OnKeyPressed(ConsoleKeyInfo keyInfo)
    {
        _listView.KeyPressed(keyInfo);
    }

    protected override void OnLoad()
    {
        _listView.BackgroundHighlightColour = _theme.BackgroundHighlight;
        _listView.ForegroundHighlightColour = _theme.ForegroundHighlight;
        _listView.BackgroundColour = _theme.Background;
        _listView.ForegroundColour = _theme.Foreground;
        _listView.HeaderBackgroundColour = _theme.HeaderBackground;
        _listView.HeaderForegroundColour = _theme.HeaderForeground;
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("MODULE"));
        _listView.ColumnHeaders.Add(new ListViewColumnHeader("FILENAME"));
        _listView.X = 0;
        _listView.Y = 0;
        _listView.Width = Terminal.WindowWidth;
        _listView.Load();
    }
    
    public int SelectedProcessId
    {
        get => _selectedProcessId;
        set => _selectedProcessId = value;
    }

    private Theme Theme { get; }

    private void UpdateColumnHeaders()
    {
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
             _listView.ColumnHeaders[i].BackgroundColour = Theme.HeaderBackground;
             _listView.ColumnHeaders[i].ForegroundColour = Theme.HeaderForeground;
         }
    }
}