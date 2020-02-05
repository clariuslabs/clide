namespace Clide
{
    public class DialogCreated
    {
        public DialogCreated(IDialogWindow dialog)
        {
            Dialog = dialog;
        }

        public IDialogWindow Dialog { get; }
    }
}
