using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features.Core.Settings.Scene
{
    [CreateAssetMenu(menuName = "Dialogue/DialogueDatabase")]
    public sealed class DialogueDatabase : ScriptableObject
    {
        public List<DialogueEntry> Dialogues;
    }

    [Serializable]
    public class DialogueEntry
    {
        public int Id;
        public List<string> Lines;

    }
}