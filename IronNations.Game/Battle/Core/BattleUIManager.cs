using Stride.Engine;
using Stride.UI.Controls;

namespace IronNations.Battle.Core
{
    public class BattleUIManager : SyncScript
    {
        private float minutes = 0, seconds = 0;
        private float clock = 0;

        private UIComponent uIComponent;
        private UIPage uIPage;
        public override void Start()
        {
            uIComponent = Entity.Get<UIComponent>();
            uIPage = uIComponent.Page;
        }

        public override void Update()
        {
            if (Counter())
            {
                seconds++;
            }

            Display();
        }

        private bool Counter()
        {
            if (clock >= 1)
            {
                clock = 0;
                return true;
            }

            clock += 1 * BattleManager.deltaTime;

            return false;
        }

        private void Display ()
        {
            //Battle clock
            TextBlock clockLabel = uIPage.RootElement.FindName("Clock") as TextBlock;
            string clockString = minutes.ToString("F0") + " : " + seconds.ToString("F0");
            clockLabel.Text = clockString;
        }
    }
}
