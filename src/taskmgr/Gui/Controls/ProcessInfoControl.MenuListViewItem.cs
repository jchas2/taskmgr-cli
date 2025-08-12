using Task.Manager.System.Controls;
using Task.Manager.System.Controls.ListView;

namespace Task.Manager.Gui.Controls;

public partial class ProcessInfoControl
{
    private class MenuListViewItem : ListViewItem
    {
        private readonly Control associatedControl;
        
        public MenuListViewItem(Control associatedControl, string text)
            : base(text) => this.associatedControl = associatedControl;
        
        public Control AssociatedControl => associatedControl;
    }
}
