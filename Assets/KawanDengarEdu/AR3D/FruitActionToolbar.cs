using UnityEngine;
using KawanDengarEdu.UI;

namespace KawanDengarEdu.AR3D
{
    public class FruitActionToolbar : MonoBehaviour
    {
        [Header("Controllers")]
        public TrackballFruitRotator rotator;
        public FruitBiteController biteController;
        public FruitDecayTimer decayTimer;
        public FruitAnatomyController anatomyController;

        [Header("Screen Navigation")]
        public ScreenManager screenManager;

        public void OnClickBite()
        {
            if (biteController != null)
            {
                biteController.TakeBite();
            }
        }

        public void OnClickDispose()
        {
            // Buang buah (animasi reset buah baru yang segar)
            ResetEntireFruit();
        }

        public void OnClickPlantSeed()
        {
            // Berpindah ke Mode Menanam Buah (Layar 09)
            if (screenManager != null)
            {
                screenManager.ShowPlantingGameScreen();
            }
        }

        public void OnClickAnatomy()
        {
            if (anatomyController != null)
            {
                anatomyController.ToggleAnatomyModal();
            }
        }

        public void OnClickReset()
        {
            ResetEntireFruit();
        }

        public void OnClickFastForwardDecay()
        {
            if (decayTimer != null)
            {
                decayTimer.FastForwardDecay();
            }
        }

        public void ResetEntireFruit()
        {
            if (rotator != null) rotator.ResetRotation();
            if (biteController != null) biteController.ResetFruit();
            if (decayTimer != null) decayTimer.ResetDecay();
        }
    }
}
