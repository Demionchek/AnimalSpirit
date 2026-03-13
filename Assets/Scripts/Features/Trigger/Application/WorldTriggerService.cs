using Features.Cutscene.Application;
using Features.Dialogue.Application;
using Features.Interactables.Application;
using Features.Trigger.Domain;
using UnityEngine;
using VContainer;

namespace Features.Trigger.Application
{
    public sealed class WorldTriggerService
    {
        private readonly WorldTriggerContext _context;

        public WorldTriggerService(
            CutsceneService cutscene,
   //         CheckpointService checkpoint,
            DialogueFacade dialogue,
            IObjectResolver resolver)
        {
            resolver.TryResolve(out DoorService door);

            _context = new WorldTriggerContext(
                cutscene,
 //               checkpoint,
                door,
                dialogue);
        }

        public void Execute(TriggerConfigSO config)
        {
            if (config == null)
            {
                Debug.LogError($"WorldTriggerService: Trigger config is null");
                return;
            }

            config.Execute(_context);
        }
    }
}
