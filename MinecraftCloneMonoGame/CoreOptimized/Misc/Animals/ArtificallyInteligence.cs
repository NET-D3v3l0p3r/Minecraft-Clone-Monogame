using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftCloneMonoGame.CoreOptimized.Misc.Animals
{
    public interface ArtificallyInteligence
    {
        Definition.Typus AITypus { get; set; }
        Stack<Definition.Intention> AIIntentions { get; set; }

        void ThinkAI();
        void ProcessAI();   

    }

    public struct Definition
    {
        public enum Typus
        {
            Aggressive,
            Passive,
            ProvokeRequired
        };

        public enum Intention
        {
            Attack,
            Eat,
            Idle
        };

    }
}
