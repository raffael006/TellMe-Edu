using System.Collections.Generic;
using TellMe.Core;
using TellMe.Mascot;
using UnityEngine;

namespace TellMe.Modules
{
    /// <summary>
    /// Modul 6: Kartu Digital (Pengguna: Siswa / Raka).
    /// Galeri 2 kolom berisi koleksi Kartu Buah 3D bertekstur frosted glass.
    /// Kartu terbuka menampilkan model buah 3D cerah, label NEW, dan tombol hijau untuk membuka AR Buah.
    /// Kartu belum terbuka tampak terkunci dengan efek kaca buram gelap (locked glass state).
    /// </summary>
    public class DigitalCardGalleryController : MonoBehaviour
    {
        public List<Kartu> GetAllCards()
        {
            if (LocalDataManager.Instance == null) return new List<Kartu>();
            return LocalDataManager.Instance.Data.daftarKartu;
        }

        public void OnCardClicked(Kartu kartu)
        {
            ThemeTokens.TriggerHapticFeedback();

            if (kartu == null) return;

            if (kartu.terbuka)
            {
                kartu.labelBaru = false;
                LocalDataManager.Instance?.SaveDatabase();

                BerryBearsController.Instance?.SetMascotState(
                    BerryBearsState.STATE_ON_TAP,
                    $"Ayo kita lihat objek 3D {kartu.namaBuahId} di dunia nyata lewat Kamera AR!");

                AppNavigationManager.Instance?.OpenARFruitWithCard(kartu);
            }
            else
            {
                BerryBearsController.Instance?.SetMascotState(
                    BerryBearsState.STATE_QUIZ_THINKING,
                    $"Kartu {kartu.namaBuahId} masih terkunci. Yuk dapatkan nilai >= 75 di Kuis {ThemeTokens.GetSubjectDisplayName(kartu.mataPelajaran)} untuk membukanya!");
            }
        }
    }
}
