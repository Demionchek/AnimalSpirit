namespace Features.Dialogue.Domain
{
    public readonly struct DialogueFinished
    {
        public readonly int DialogueId;

        public DialogueFinished(int id)
        {
            DialogueId = id;
        }
    }

    public readonly struct DialogueAdvancePressed { }

    public readonly struct DialogueRequested
    {
        public readonly int DialogueId;

        public DialogueRequested(int id)
        {
            DialogueId = id;
        }
    }
}