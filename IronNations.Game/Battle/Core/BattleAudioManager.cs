using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Core.Mathematics;
using Stride.Input;
using Stride.Engine;
using Stride.Audio;
using Vortice.Vulkan;

namespace IronNations.Battle.Core
{
    public class BattleAudioManager : SyncScript
    {
        public Sound drumsSound , trumpetSound;
        public float volume = 1;
        SoundInstance trumpetInstance;

        public override void Start()
        {
            trumpetInstance = trumpetSound.CreateInstance();
        }

        public async void PlayTrumpet ()
        {
            trumpetInstance.Volume = volume;
            await trumpetInstance.ReadyToPlay();
            trumpetInstance.Play();
        }

        public override void Update()
        {
            // Do stuff every new frame
        }
    }
}
