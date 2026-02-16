using UnityEngine;
using UnityEngine.Video;

public partial class IntroVideoManager : MonoBehaviour {
    public VideoPlayer videoPlayer;
    public GameObject tutorialUI; // ใส่ Group ของคุณยายสอน

    void Start() {
        tutorialUI.SetActive(false);
        videoPlayer.loopPointReached += OnVideoFinished; // เมื่อวิดีโอจบให้เรียกฟังก์ชัน
        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp) {
        videoPlayer.gameObject.SetActive(false); // ปิดวิดีโอ
        tutorialUI.SetActive(true); // เปิดให้คุณยายออกมาสอน
    }
}