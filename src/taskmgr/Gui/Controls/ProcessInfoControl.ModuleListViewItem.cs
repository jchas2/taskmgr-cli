using Task.Manager.Configuration;
using Task.Manager.System.Controls.ListView;
using Task.Manager.System.Process;

namespace Task.Manager.Gui.Controls;

public partial class ProcessInfoControl
{
    private class ModuleListViewItem : ListViewItem
    {
        public ModuleListViewItem(ModuleInfo moduleInfo, Theme theme)
            : base(moduleInfo.ModuleName ?? string.Empty)
        {
            ArgumentNullException.ThrowIfNull(theme);
            
            SubItems.AddRange(
                new ListViewSubItem(this, moduleInfo.FileName));
            
            for (int i = 0; i < (int)ModuleColumns.Count; i++) {
                SubItems[i].BackgroundColor = theme.Background;
                SubItems[i].ForegroundColor = theme.Foreground;
            }
        }
    }
}