using Task.Manager.Configuration;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public partial class ModulesControl
{
    private class ModuleListViewItem : ListViewItem
    {
        public ModuleListViewItem(ModuleInfo moduleInfo, Theme theme)
            : base(moduleInfo.ModuleName ?? string.Empty)
        {
            Theme = theme;

            SubItems.AddRange(
                new ListViewSubItem(this, moduleInfo.FileName));
            
            for (int i = 0; i < (int)Columns.Count; i++) {
                SubItems[i].BackgroundColor = Theme.Background;
                SubItems[i].ForegroundColor = Theme.Foreground;
            }
        }
        
        private Theme Theme { get; }
    }
}