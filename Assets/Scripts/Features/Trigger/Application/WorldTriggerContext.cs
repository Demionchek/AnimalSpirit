using Features.Cutscene.Application;
using Features.Dialogue.Application;
using Features.Interactables.Application;

namespace Features.Trigger.Application
{
    public sealed class WorldTriggerContext
    {
        public readonly CutsceneService Cutscene;
 //       public readonly CheckpointService Checkpoint;
        public readonly DoorService Door;
        public readonly DialogueFacade Dialogue;

        public WorldTriggerContext(
            CutsceneService cutscene,
 //           CheckpointService checkpoint,
            DoorService door,
            DialogueFacade dialogue)
        {
            Cutscene = cutscene;
 //           Checkpoint = checkpoint;
            Door = door;
            Dialogue = dialogue;
        }
    }
}